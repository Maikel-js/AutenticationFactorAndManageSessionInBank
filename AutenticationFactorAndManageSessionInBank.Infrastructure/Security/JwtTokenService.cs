using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutenticationFactorAndManageSessionInBank.Application.Abstractions.Security;
using AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;
using AutenticationFactorAndManageSessionInBank.Domain.Entities;
using AutenticationFactorAndManageSessionInBank.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AutenticationFactorAndManageSessionInBank.Infrastructure.Security;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string CreateAccessToken(User user, Session session, DateTimeOffset expiresAtUtc) =>
        Describe(user, session, expiresAtUtc.AddMinutes(-_options.AccessTokenMinutes)).Token;

    public AccessTokenDescriptor Describe(User user, Session session, DateTimeOffset nowUtc)
    {
        var expiresAtUtc = nowUtc.AddMinutes(_options.AccessTokenMinutes);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("tenant_id", user.TenantId),
            new("session_id", session.Id.ToString()),
            new("risk_score", session.RiskScore.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)),
            new("amr", session.MfaVerified ? "pwd+mfa" : "pwd")
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: nowUtc.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        var rawToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new AccessTokenDescriptor(rawToken, expiresAtUtc);
    }
}
