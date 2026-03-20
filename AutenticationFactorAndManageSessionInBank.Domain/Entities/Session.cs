using AutenticationFactorAndManageSessionInBank.Domain.Common;
using AutenticationFactorAndManageSessionInBank.Domain.Enums;

namespace AutenticationFactorAndManageSessionInBank.Domain.Entities;

public sealed class Session : Entity
{
    private readonly List<RefreshToken> _refreshTokens = [];
    private readonly List<RiskSignal> _riskSignals = [];

    private Session()
    {
    }

    public Session(
        Guid userId,
        string tenantId,
        string deviceId,
        string ipAddress,
        string userAgent,
        decimal riskScore,
        bool mfaRequired,
        bool mfaVerified,
        DateTimeOffset expiresAtUtc)
    {
        UserId = userId;
        TenantId = tenantId;
        DeviceId = deviceId;
        CurrentIpAddress = ipAddress;
        UserAgent = userAgent;
        RiskScore = riskScore;
        MfaRequired = mfaRequired;
        MfaVerified = mfaVerified;
        ExpiresAtUtc = expiresAtUtc;
        LastSeenAtUtc = DateTimeOffset.UtcNow;
        Status = mfaRequired && !mfaVerified ? SessionStatus.PendingMfa : SessionStatus.Active;
    }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string TenantId { get; private set; } = string.Empty;
    public string DeviceId { get; private set; } = string.Empty;
    public string CurrentIpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public decimal RiskScore { get; private set; }
    public bool MfaRequired { get; private set; }
    public bool MfaVerified { get; private set; }
    public SessionStatus Status { get; private set; }
    public DateTimeOffset LastSeenAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public string? MfaTicketHash { get; private set; }
    public DateTimeOffset? MfaTicketExpiresAtUtc { get; private set; }
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;
    public IReadOnlyCollection<RiskSignal> RiskSignals => _riskSignals;

    public bool IsEligibleForTokenIssuance(DateTimeOffset nowUtc) =>
        Status is SessionStatus.Active && ExpiresAtUtc > nowUtc;

    public RefreshToken IssueRefreshToken(
        string tokenHash,
        DateTimeOffset expiresAtUtc,
        string? parentTokenId = null)
    {
        var refreshToken = new RefreshToken(Id, tokenHash, expiresAtUtc, parentTokenId);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public void RegisterRiskSignal(string type, decimal contribution, string metadata)
    {
        RiskScore = Math.Clamp(RiskScore + contribution, 0m, 100m);
        _riskSignals.Add(new RiskSignal(Id, type, contribution, metadata));

        if (RiskScore >= 80m)
        {
            Status = SessionStatus.Suspicious;
        }
    }

    public void MarkMfaVerified()
    {
        MfaVerified = true;
        MfaTicketHash = null;
        MfaTicketExpiresAtUtc = null;
        if (Status == SessionStatus.PendingMfa)
        {
            Status = SessionStatus.Active;
        }
    }

    public void BeginMfaChallenge(string ticketHash, DateTimeOffset expiresAtUtc)
    {
        MfaTicketHash = ticketHash;
        MfaTicketExpiresAtUtc = expiresAtUtc;
        Status = SessionStatus.PendingMfa;
    }

    public bool CanAcceptMfaTicket(string ticketHash, DateTimeOffset nowUtc) =>
        Status == SessionStatus.PendingMfa &&
        MfaTicketHash == ticketHash &&
        MfaTicketExpiresAtUtc.HasValue &&
        MfaTicketExpiresAtUtc > nowUtc;

    public void Touch(string ipAddress, DateTimeOffset nowUtc)
    {
        CurrentIpAddress = ipAddress;
        LastSeenAtUtc = nowUtc;

        if (ExpiresAtUtc <= nowUtc && Status is not SessionStatus.Revoked)
        {
            Status = SessionStatus.Expired;
        }
    }

    public void Revoke(DateTimeOffset nowUtc)
    {
        Status = SessionStatus.Revoked;
        LastSeenAtUtc = nowUtc;
    }
}
