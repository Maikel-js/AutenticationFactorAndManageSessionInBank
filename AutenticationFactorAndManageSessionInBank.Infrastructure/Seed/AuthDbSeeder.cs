using AutenticationFactorAndManageSessionInBank.Domain.Entities;
using AutenticationFactorAndManageSessionInBank.Infrastructure.Persistence;
namespace AutenticationFactorAndManageSessionInBank.Infrastructure.Seed;

public sealed class AuthDbSeeder
{
    private readonly AuthDbContext _dbContext;
    private readonly Security.PasswordHasher _passwordHasher;
    public AuthDbSeeder(AuthDbContext dbContext, Security.PasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
        if (_dbContext.Users.Any())
        {
            return;
        }

        var password = _passwordHasher.Hash("P@ssword123!");
        var user = new User(
            "admin@fintech.local",
            "Fintech Admin",
            password.Hash,
            password.Salt,
            mfaEnabled: true,
            tenantId: "tenant-demo",
            totpSecret: "JBSWY3DPEHPK3PXP");

        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
