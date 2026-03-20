using AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;
using AutenticationFactorAndManageSessionInBank.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AutenticationFactorAndManageSessionInBank.Api.Security;

public sealed class CookieTokenWriter
{
    private readonly SecurityOptions _securityOptions;
    private readonly IWebHostEnvironment _environment;

    public CookieTokenWriter(IOptions<SecurityOptions> securityOptions, IWebHostEnvironment environment)
    {
        _securityOptions = securityOptions.Value;
        _environment = environment;
    }

    public void WriteAuthCookies(HttpContext httpContext, AuthResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.AccessToken) || string.IsNullOrWhiteSpace(response.RefreshToken))
        {
            return;
        }

        httpContext.Response.Cookies.Append(
            _securityOptions.AccessCookieName,
            response.AccessToken,
            BuildCookieOptions(httpOnly: true, response.AccessTokenExpiresAtUtc, sameSite: SameSiteMode.Lax));

        httpContext.Response.Cookies.Append(
            _securityOptions.RefreshCookieName,
            response.RefreshToken,
            BuildCookieOptions(httpOnly: true, response.RefreshTokenExpiresAtUtc, sameSite: SameSiteMode.Strict));
    }

    public void WriteCsrfCookie(HttpContext httpContext, string csrfToken)
    {
        httpContext.Response.Cookies.Append(
            _securityOptions.CsrfCookieName,
            csrfToken,
            BuildCookieOptions(httpOnly: false, DateTimeOffset.UtcNow.AddHours(8), sameSite: SameSiteMode.Lax));
    }

    public void ClearAuthCookies(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(_securityOptions.AccessCookieName, BuildCookieOptions(true, null, SameSiteMode.Lax));
        httpContext.Response.Cookies.Delete(_securityOptions.RefreshCookieName, BuildCookieOptions(true, null, SameSiteMode.Strict));
    }

    private CookieOptions BuildCookieOptions(bool httpOnly, DateTimeOffset? expiresAtUtc, SameSiteMode sameSite) =>
        new()
        {
            HttpOnly = httpOnly,
            Secure = _securityOptions.RequireHttpsCookies && !_environment.IsDevelopment() ? true : _securityOptions.RequireHttpsCookies,
            SameSite = sameSite,
            Expires = expiresAtUtc?.UtcDateTime,
            Path = "/",
            Domain = string.IsNullOrWhiteSpace(_securityOptions.CookieDomain) ? null : _securityOptions.CookieDomain
        };
}
