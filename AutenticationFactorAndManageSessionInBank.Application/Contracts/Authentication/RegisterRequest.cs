namespace AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;

public sealed record RegisterRequest(
    string Email,
    string FullName,
    string Password,
    string TenantId,
    bool EnableMfa);
