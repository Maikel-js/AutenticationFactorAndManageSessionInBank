namespace AutenticationFactorAndManageSessionInBank.Application.Abstractions.Security;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
