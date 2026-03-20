namespace AutenticationFactorAndManageSessionInBank.Infrastructure.Configuration;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    public string FrontendOrigin { get; init; } = "https://app.fintechshield.local";
    public string CookieDomain { get; init; } = "";
    public bool RequireHttpsCookies { get; init; } = true;
    public string AccessCookieName { get; init; } = "__Host-fsa-at";
    public string RefreshCookieName { get; init; } = "__Host-fsa-rt";
    public string CsrfCookieName { get; init; } = "XSRF-TOKEN";
}
