using AutenticationFactorAndManageSessionInBank.Domain.Enums;

namespace AutenticationFactorAndManageSessionInBank.Application.Contracts.Security;

public sealed record DeviceResponse(
    string DeviceId,
    string UserAgent,
    string LastIpAddress,
    DateTimeOffset LastSeenAtUtc,
    SessionStatus Status,
    int SessionCount);
