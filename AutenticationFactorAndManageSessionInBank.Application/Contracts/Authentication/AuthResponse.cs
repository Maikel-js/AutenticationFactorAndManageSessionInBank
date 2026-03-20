namespace AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;

public sealed record AuthResponse(
    string? AccessToken,
    DateTimeOffset? AccessTokenExpiresAtUtc,
    string? RefreshToken,
    DateTimeOffset? RefreshTokenExpiresAtUtc,
    Guid SessionId,
    decimal RiskScore,
    bool RequiresMfa,
    string? MfaTicket,
    DateTimeOffset? MfaTicketExpiresAtUtc,
    UserProfileResponse User);
