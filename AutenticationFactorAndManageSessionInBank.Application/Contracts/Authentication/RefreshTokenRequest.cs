namespace AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;

public sealed record RefreshTokenRequest(
    string RefreshToken,
    string IpAddress);
