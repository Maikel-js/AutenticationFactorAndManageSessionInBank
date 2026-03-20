using AutenticationFactorAndManageSessionInBank.Domain.Entities;

namespace AutenticationFactorAndManageSessionInBank.Application.Abstractions.Persistence;

public interface ISessionRepository
{
    Task AddAsync(Session session, CancellationToken cancellationToken);
    Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<Session?> GetByOwnedIdAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Session>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<RefreshToken?> GetRefreshTokenGraphAsync(string tokenHash, CancellationToken cancellationToken);
    Task<Session?> GetByMfaTicketHashAsync(string mfaTicketHash, CancellationToken cancellationToken);
}
