using AutenticationFactorAndManageSessionInBank.Application.Abstractions.Persistence;
using AutenticationFactorAndManageSessionInBank.Domain.Entities;
using AutenticationFactorAndManageSessionInBank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutenticationFactorAndManageSessionInBank.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AuthDbContext _dbContext;

    public UserRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByEmailAsync(string email, string tenantId, CancellationToken cancellationToken) =>
        _dbContext.Users
            .Include(x => x.Sessions)
            .FirstOrDefaultAsync(x => x.Email == email && x.TenantId == tenantId, cancellationToken);

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken) =>
        _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken) =>
        await _dbContext.Users.AddAsync(user, cancellationToken);
}
