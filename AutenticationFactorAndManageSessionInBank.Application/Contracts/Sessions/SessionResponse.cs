using AutenticationFactorAndManageSessionInBank.Domain.Enums;

namespace AutenticationFactorAndManageSessionInBank.Application.Contracts.Sessions;

public sealed record SessionResponse(
    Guid SessionId,
    SessionStatus Status,
    decimal RiskScore,
    bool MfaRequired,
    bool MfaVerified,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset LastSeenAtUtc,
    string DeviceId,
    string IpAddress);
