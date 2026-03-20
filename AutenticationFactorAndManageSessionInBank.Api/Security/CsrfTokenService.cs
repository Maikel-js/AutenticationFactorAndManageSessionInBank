using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace AutenticationFactorAndManageSessionInBank.Api.Security;

public sealed class CsrfTokenService
{
    public string Generate() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
}
