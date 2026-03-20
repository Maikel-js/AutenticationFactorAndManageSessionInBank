namespace AutenticationFactorAndManageSessionInBank.Api.Contracts;

public sealed record LoginRequestDto(
    string Email,
    string Password,
    string DeviceId,
    string TenantId);
