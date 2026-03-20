namespace AutenticationFactorAndManageSessionInBank.Api.Contracts;

public sealed record VerifyMfaRequestDto(
    string MfaTicket,
    string OtpCode);
