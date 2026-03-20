namespace AutenticationFactorAndManageSessionInBank.Infrastructure.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "fintech-auth";
    public string Audience { get; init; } = "fintech-api";
    public string SecretKey { get; init; } = "CHANGE_ME_WITH_A_MINIMUM_32_CHAR_SECRET";
    public int AccessTokenMinutes { get; init; } = 15;
}
