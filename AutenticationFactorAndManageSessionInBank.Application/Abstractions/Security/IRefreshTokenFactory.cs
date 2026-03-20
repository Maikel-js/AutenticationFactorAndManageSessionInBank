namespace AutenticationFactorAndManageSessionInBank.Application.Abstractions.Security;

public interface IRefreshTokenFactory
{
    RefreshTokenMaterial Create();
    string Hash(string rawToken);
}
