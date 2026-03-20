using AutenticationFactorAndManageSessionInBank.Domain.Entities;

namespace AutenticationFactorAndManageSessionInBank.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, string tenantId, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
}
