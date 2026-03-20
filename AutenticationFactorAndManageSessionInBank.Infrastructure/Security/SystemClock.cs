using AutenticationFactorAndManageSessionInBank.Application.Abstractions.Security;

namespace AutenticationFactorAndManageSessionInBank.Infrastructure.Security;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
