namespace AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;

public sealed record RefreshTokenMaterial(string RawToken, string Hash);
