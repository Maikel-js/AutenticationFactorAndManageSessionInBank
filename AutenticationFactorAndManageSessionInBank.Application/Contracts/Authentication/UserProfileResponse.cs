namespace AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;

public sealed record UserProfileResponse(
    Guid UserId,
    string Email,
    string FullName,
    string TenantId,
    bool MfaEnabled);
