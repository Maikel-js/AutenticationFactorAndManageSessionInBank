using System.Security.Cryptography;
using System.Text;
using AutenticationFactorAndManageSessionInBank.Application.Abstractions.Security;
using AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;

namespace AutenticationFactorAndManageSessionInBank.Infrastructure.Security;

public sealed class RefreshTokenFactory : IRefreshTokenFactory
{
    public RefreshTokenMaterial Create()
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return new RefreshTokenMaterial(rawToken, Hash(rawToken));
    }

    public string Hash(string rawToken)
    {
        var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashedBytes);
    }
}
