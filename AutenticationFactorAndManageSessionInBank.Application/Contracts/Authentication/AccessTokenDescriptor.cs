namespace AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;

public sealed record AccessTokenDescriptor(string Token, DateTimeOffset ExpiresAtUtc);
