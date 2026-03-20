namespace AutenticationFactorAndManageSessionInBank.Domain.Enums;

public enum SessionStatus
{
    PendingMfa = 1,
    Active = 2,
    Revoked = 3,
    Expired = 4,
    Suspicious = 5
}
