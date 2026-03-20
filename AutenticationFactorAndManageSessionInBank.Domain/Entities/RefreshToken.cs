using AutenticationFactorAndManageSessionInBank.Domain.Common;
using AutenticationFactorAndManageSessionInBank.Domain.Enums;

namespace AutenticationFactorAndManageSessionInBank.Domain.Entities;

public sealed class RefreshToken : Entity
{
    private RefreshToken()
    {
    }

    public RefreshToken(Guid sessionId, string tokenHash, DateTimeOffset expiresAtUtc, string? parentTokenId)
    {
        SessionId = sessionId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        ParentTokenId = parentTokenId;
        TokenId = Guid.NewGuid().ToString("N");
        Status = RefreshTokenStatus.Active;
    }

    public Guid SessionId { get; private set; }
    public Session Session { get; private set; } = null!;
    public string TokenId { get; private set; } = string.Empty;
    public string TokenHash { get; private set; } = string.Empty;
    public string? ParentTokenId { get; private set; }
    public string? ReplacedByTokenId { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public RefreshTokenStatus Status { get; private set; }

    public bool IsActive(DateTimeOffset nowUtc) =>
        Status == RefreshTokenStatus.Active && ExpiresAtUtc > nowUtc;

    public void Rotate(string replacementTokenId, DateTimeOffset nowUtc)
    {
        Status = RefreshTokenStatus.Rotated;
        ReplacedByTokenId = replacementTokenId;
        RevokedAtUtc = nowUtc;
    }

    public void Revoke(DateTimeOffset nowUtc)
    {
        Status = RefreshTokenStatus.Revoked;
        RevokedAtUtc = nowUtc;
    }

    public void MarkReused(DateTimeOffset nowUtc)
    {
        Status = RefreshTokenStatus.Reused;
        RevokedAtUtc = nowUtc;
    }
}
