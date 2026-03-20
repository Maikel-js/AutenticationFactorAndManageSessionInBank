namespace AutenticationFactorAndManageSessionInBank.Api.Contracts;

public sealed record RegisterRequestDto(
    string Email,
    string FullName,
    string Password,
    string TenantId,
    bool EnableMfa);
