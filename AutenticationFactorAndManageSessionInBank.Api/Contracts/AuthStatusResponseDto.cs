using AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;

namespace AutenticationFactorAndManageSessionInBank.Api.Contracts;

public sealed record AuthStatusResponseDto(
    bool Authenticated,
    bool RequiresMfa,
    Guid SessionId,
    decimal RiskScore,
    string? MfaTicket,
    DateTimeOffset? MfaTicketExpiresAtUtc,
    UserProfileResponse User);
