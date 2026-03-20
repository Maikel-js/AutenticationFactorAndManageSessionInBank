using AutenticationFactorAndManageSessionInBank.Domain.Common;

namespace AutenticationFactorAndManageSessionInBank.Domain.Entities;

public sealed class User : Entity
{
    private readonly List<Session> _sessions = [];

    private User()
    {
    }

    public User(
        string email,
        string fullName,
        string passwordHash,
        string passwordSalt,
        bool mfaEnabled,
        string tenantId,
        string? totpSecret = null)
    {
        Email = email;
        FullName = fullName;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        MfaEnabled = mfaEnabled;
        TenantId = tenantId;
        TotpSecret = totpSecret;
        LastPasswordChangedAtUtc = DateTimeOffset.UtcNow;
    }

    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string PasswordSalt { get; private set; } = string.Empty;
    public string TenantId { get; private set; } = string.Empty;
    public string? TotpSecret { get; private set; }
    public bool MfaEnabled { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset LastPasswordChangedAtUtc { get; private set; }
    public IReadOnlyCollection<Session> Sessions => _sessions;

    public Session StartSession(
        string deviceId,
        string ipAddress,
        string userAgent,
        decimal riskScore,
        bool mfaVerified,
        DateTimeOffset expiresAtUtc)
    {
        var session = new Session(
            Id,
            TenantId,
            deviceId,
            ipAddress,
            userAgent,
            riskScore,
            MfaEnabled,
            mfaVerified,
            expiresAtUtc);

        _sessions.Add(session);
        return session;
    }

    public void EnableMfa(string totpSecret)
    {
        TotpSecret = totpSecret;
        MfaEnabled = true;
    }

    public void DisableMfa()
    {
        TotpSecret = null;
        MfaEnabled = false;
    }
}
