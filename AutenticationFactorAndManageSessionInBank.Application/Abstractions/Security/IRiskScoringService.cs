using AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;
using AutenticationFactorAndManageSessionInBank.Domain.Entities;

namespace AutenticationFactorAndManageSessionInBank.Application.Abstractions.Security;

public interface IRiskScoringService
{
    RiskEvaluation Evaluate(User user, LoginRequest request);
}
