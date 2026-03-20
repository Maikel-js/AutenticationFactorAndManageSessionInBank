namespace AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;

public sealed record LoginRequest(
    string Email,
    string Password,
    string IpAddress,
    string UserAgent,
    string DeviceId,
    string TenantId);
