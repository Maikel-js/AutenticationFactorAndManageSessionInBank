using AutenticationFactorAndManageSessionInBank.Application.Abstractions.Persistence;
using AutenticationFactorAndManageSessionInBank.Domain.Entities;
using AutenticationFactorAndManageSessionInBank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutenticationFactorAndManageSessionInBank.Infrastructure.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly AuthDbContext _dbContext;

    public SessionRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Session session, CancellationToken cancellationToken) =>
        await _dbContext.Sessions.AddAsync(session, cancellationToken);

    public Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken) =>
        _dbContext.Sessions
            .Include(x => x.User)
            .Include(x => x.RefreshTokens)
            .Include(x => x.RiskSignals)
            .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

    public Task<Session?> GetByOwnedIdAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken) =>
        _dbContext.Sessions
            .Include(x => x.User)
            .Include(x => x.RefreshTokens)
            .Include(x => x.RiskSignals)
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Session>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        await _dbContext.Sessions
            .Include(x => x.User)
            .Include(x => x.RefreshTokens)
            .Include(x => x.RiskSignals)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task<RefreshToken?> GetRefreshTokenGraphAsync(string tokenHash, CancellationToken cancellationToken) =>
        _dbContext.RefreshTokens
            .Include(x => x.Session)
            .ThenInclude(x => x.User)
            .Include(x => x.Session)
            .ThenInclude(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    public Task<Session?> GetByMfaTicketHashAsync(string mfaTicketHash, CancellationToken cancellationToken) =>
        _dbContext.Sessions
            .Include(x => x.User)
            .Include(x => x.RefreshTokens)
            .Include(x => x.RiskSignals)
            .FirstOrDefaultAsync(x => x.MfaTicketHash == mfaTicketHash, cancellationToken);
}
