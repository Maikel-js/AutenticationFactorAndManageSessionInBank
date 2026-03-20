namespace AutenticationFactorAndManageSessionInBank.Application.Abstractions.Security;

public interface ITotpService
{
    string GenerateSecret();
    bool ValidateCode(string secret, string code, DateTimeOffset nowUtc);
    string BuildProvisioningUri(string issuer, string email, string secret);
}
