using AutenticationFactorAndManageSessionInBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutenticationFactorAndManageSessionInBank.Application.Abstractions.Persistence;

public interface IAuthDbContext
{
    DbSet<User> Users { get; }
    DbSet<Session> Sessions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<RiskSignal> RiskSignals { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
