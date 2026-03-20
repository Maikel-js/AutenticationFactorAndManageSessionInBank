using AutenticationFactorAndManageSessionInBank.Application.Abstractions.Persistence;
using AutenticationFactorAndManageSessionInBank.Application.Abstractions.Security;
using AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;
using AutenticationFactorAndManageSessionInBank.Domain.Entities;
using AutenticationFactorAndManageSessionInBank.Domain.Enums;

namespace AutenticationFactorAndManageSessionInBank.Application.Services;

public sealed class AuthService
{
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(14);
    private static readonly TimeSpan MfaTicketLifetime = TimeSpan.FromMinutes(5);
    private const string TotpIssuer = "FintechShield";

    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IAuthDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenFactory _refreshTokenFactory;
    private readonly IRiskScoringService _riskScoringService;
    private readonly ITotpService _totpService;
    private readonly IClock _clock;

    public AuthService(
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        IAuthDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenFactory refreshTokenFactory,
        IRiskScoringService riskScoringService,
        ITotpService totpService,
        IClock clock)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenFactory = refreshTokenFactory;
        _riskScoringService = riskScoringService;
        _totpService = totpService;
        _clock = clock;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var tenantId = request.TenantId.Trim();
        var fullName = request.FullName.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(tenantId))
        {
            throw new InvalidOperationException("Invalid registration payload.");
        }

        var existing = await _userRepository.GetByEmailAsync(email, tenantId, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("User already exists.");
        }

        var password = _passwordHasher.Hash(request.Password);
        var totpSecret = request.EnableMfa ? _totpService.GenerateSecret() : null;
        var user = new User(email, fullName, password.Hash, password.Salt, request.EnableMfa, tenantId, totpSecret);

        await _userRepository.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.MfaEnabled,
            user.TotpSecret,
            user.MfaEnabled ? _totpService.BuildProvisioningUri(TotpIssuer, user.Email, user.TotpSecret!) : null);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var tenantId = request.TenantId.Trim();
        var user = await _userRepository.GetByEmailAsync(NormalizeEmail(request.Email), tenantId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        var nowUtc = _clock.UtcNow;
        var risk = _riskScoringService.Evaluate(user, request);
        var session = user.StartSession(
            request.DeviceId.Trim(),
            request.IpAddress.Trim(),
            request.UserAgent.Trim(),
            risk.Score,
            mfaVerified: !user.MfaEnabled,
            expiresAtUtc: nowUtc.Add(SessionLifetime));

        foreach (var signal in risk.Signals)
        {
            session.RegisterRiskSignal(signal, 0m, $"signal:{signal}");
        }

        await _sessionRepository.AddAsync(session, cancellationToken);

        if (user.MfaEnabled)
        {
            var mfaMaterial = _refreshTokenFactory.Create();
            session.BeginMfaChallenge(mfaMaterial.Hash, nowUtc.Add(MfaTicketLifetime));
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new AuthResponse(
                null,
                null,
                null,
                null,
                session.Id,
                session.RiskScore,
                true,
                mfaMaterial.RawToken,
                nowUtc.Add(MfaTicketLifetime),
                ToUserProfile(user));
        }

        var tokenSet = IssueSessionTokens(user, session, nowUtc);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return tokenSet;
    }

    public async Task<AuthResponse> VerifyMfaAsync(VerifyMfaRequest request, CancellationToken cancellationToken)
    {
        var nowUtc = _clock.UtcNow;
        var ticketHash = _refreshTokenFactory.Hash(request.MfaTicket);
        var session = await _sessionRepository.GetByMfaTicketHashAsync(ticketHash, cancellationToken)
            ?? throw new InvalidOperationException("MFA challenge is invalid.");

        if (!session.CanAcceptMfaTicket(ticketHash, nowUtc))
        {
            throw new InvalidOperationException("MFA challenge expired.");
        }

        var user = session.User;
        if (string.IsNullOrWhiteSpace(user.TotpSecret) || !_totpService.ValidateCode(user.TotpSecret, request.OtpCode.Trim(), nowUtc))
        {
            session.RegisterRiskSignal("mfa-failed", 10m, "totp-invalid");
            await _dbContext.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Invalid MFA code.");
        }

        session.Touch(request.IpAddress.Trim(), nowUtc);
        session.MarkMfaVerified();

        var tokenSet = IssueSessionTokens(user, session, nowUtc);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return tokenSet;
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var nowUtc = _clock.UtcNow;
        var tokenHash = _refreshTokenFactory.Hash(request.RefreshToken);
        var storedToken = await _sessionRepository.GetRefreshTokenGraphAsync(tokenHash, cancellationToken);

        if (storedToken is null)
        {
            throw new InvalidOperationException("Refresh token is invalid.");
        }

        var session = storedToken.Session;
        var user = session.User;

        if (!storedToken.IsActive(nowUtc))
        {
            storedToken.MarkReused(nowUtc);
            session.Revoke(nowUtc);
            await _dbContext.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Refresh token is no longer active.");
        }

        session.Touch(request.IpAddress.Trim(), nowUtc);
        if (!session.IsEligibleForTokenIssuance(nowUtc))
        {
            throw new InvalidOperationException("Session is no longer active.");
        }

        var nextRefreshMaterial = _refreshTokenFactory.Create();
        var nextRefreshToken = session.IssueRefreshToken(
            nextRefreshMaterial.Hash,
            nowUtc.Add(RefreshTokenLifetime),
            storedToken.TokenId);

        storedToken.Rotate(nextRefreshToken.TokenId, nowUtc);

        var accessToken = _jwtTokenService.Describe(user, session, nowUtc);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            nextRefreshMaterial.RawToken,
            nextRefreshToken.ExpiresAtUtc,
            session.Id,
            session.RiskScore,
            false,
            null,
            null,
            ToUserProfile(user));
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        return ToUserProfile(user);
    }

    public async Task RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByOwnedIdAsync(userId, sessionId, cancellationToken)
            ?? throw new InvalidOperationException("Session not found.");

        var nowUtc = _clock.UtcNow;
        session.Revoke(nowUtc);
        foreach (var refreshToken in session.RefreshTokens.Where(static token => token.Status == RefreshTokenStatus.Active))
        {
            refreshToken.Revoke(nowUtc);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private AuthResponse IssueSessionTokens(User user, Session session, DateTimeOffset nowUtc)
    {
        var refreshMaterial = _refreshTokenFactory.Create();
        var refreshToken = session.IssueRefreshToken(refreshMaterial.Hash, nowUtc.Add(RefreshTokenLifetime));
        var accessToken = _jwtTokenService.Describe(user, session, nowUtc);

        return new AuthResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshMaterial.RawToken,
            refreshToken.ExpiresAtUtc,
            session.Id,
            session.RiskScore,
            false,
            null,
            null,
            ToUserProfile(user));
    }

    private static UserProfileResponse ToUserProfile(User user) =>
        new(user.Id, user.Email, user.FullName, user.TenantId, user.MfaEnabled);

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();
}
