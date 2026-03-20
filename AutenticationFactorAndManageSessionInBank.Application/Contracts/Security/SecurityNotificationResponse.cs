namespace AutenticationFactorAndManageSessionInBank.Application.Contracts.Security;

public sealed record SecurityNotificationResponse(
    Guid SessionId,
    string Title,
    string Severity,
    string Description,
    DateTimeOffset CreatedAtUtc);
