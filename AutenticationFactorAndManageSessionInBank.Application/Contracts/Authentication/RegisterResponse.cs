namespace AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;

public sealed record RegisterResponse(
    Guid UserId,
    string Email,
    string FullName,
    bool MfaEnabled,
    string? TotpSecret,
    string? ProvisioningUri);
