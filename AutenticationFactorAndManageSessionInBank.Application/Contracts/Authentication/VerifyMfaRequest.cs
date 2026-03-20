namespace AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;

public sealed record VerifyMfaRequest(
    string MfaTicket,
    string OtpCode,
    string IpAddress);
