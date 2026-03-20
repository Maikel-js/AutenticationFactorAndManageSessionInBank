using AutenticationFactorAndManageSessionInBank.Application.Abstractions.Persistence;
using AutenticationFactorAndManageSessionInBank.Application.Contracts.Security;
using AutenticationFactorAndManageSessionInBank.Application.Contracts.Sessions;
using AutenticationFactorAndManageSessionInBank.Domain.Enums;

namespace AutenticationFactorAndManageSessionInBank.Application.Services;

public sealed class SessionQueryService
{
    private readonly ISessionRepository _sessionRepository;

    public SessionQueryService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<SessionResponse> GetByIdAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByOwnedIdAsync(userId, sessionId, cancellationToken)
            ?? throw new InvalidOperationException("Session not found.");

        return ToResponse(session);
    }

    public async Task<IReadOnlyList<SessionResponse>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sessions = await _sessionRepository.ListByUserIdAsync(userId, cancellationToken);
        return sessions
            .OrderByDescending(x => x.LastSeenAtUtc)
            .Select(ToResponse)
            .ToList();
    }

    public async Task<IReadOnlyList<SecurityNotificationResponse>> GetNotificationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sessions = await _sessionRepository.ListByUserIdAsync(userId, cancellationToken);

        return sessions
            .SelectMany(session =>
            {
                var notifications = session.RiskSignals.Select(signal =>
                    new SecurityNotificationResponse(
                        session.Id,
                        $"Risk signal: {signal.SignalType}",
                        session.RiskScore >= 70m ? "high" : "medium",
                        signal.Metadata,
                        signal.CreatedAtUtc));

                if (session.Status == SessionStatus.Suspicious)
                {
                    notifications = notifications.Append(
                        new SecurityNotificationResponse(
                            session.Id,
                            "Suspicious session detected",
                            "high",
                            $"Device {session.DeviceId} exceeded risk threshold.",
                            session.LastSeenAtUtc));
                }

                return notifications;
            })
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(20)
            .ToList();
    }

    public async Task<IReadOnlyList<DeviceResponse>> GetDevicesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sessions = await _sessionRepository.ListByUserIdAsync(userId, cancellationToken);

        return sessions
            .GroupBy(x => x.DeviceId)
            .Select(group =>
            {
                var latest = group.OrderByDescending(x => x.LastSeenAtUtc).First();
                return new DeviceResponse(
                    group.Key,
                    latest.UserAgent,
                    latest.CurrentIpAddress,
                    latest.LastSeenAtUtc,
                    latest.Status,
                    group.Count());
            })
            .OrderByDescending(x => x.LastSeenAtUtc)
            .ToList();
    }

    private static SessionResponse ToResponse(Domain.Entities.Session session) =>
        new(
            session.Id,
            session.Status,
            session.RiskScore,
            session.MfaRequired,
            session.MfaVerified,
            session.ExpiresAtUtc,
            session.LastSeenAtUtc,
            session.DeviceId,
            session.CurrentIpAddress);
}
