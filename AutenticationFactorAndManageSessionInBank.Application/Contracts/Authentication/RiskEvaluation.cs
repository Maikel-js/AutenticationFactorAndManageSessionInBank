namespace AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;

public sealed record RiskEvaluation(decimal Score, IReadOnlyList<string> Signals);
