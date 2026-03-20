using AutenticationFactorAndManageSessionInBank.Domain.Common;

namespace AutenticationFactorAndManageSessionInBank.Domain.Entities;

public sealed class RiskSignal : Entity
{
    private RiskSignal()
    {
    }

    public RiskSignal(Guid sessionId, string signalType, decimal scoreContribution, string metadata)
    {
        SessionId = sessionId;
        SignalType = signalType;
        ScoreContribution = scoreContribution;
        Metadata = metadata;
    }

    public Guid SessionId { get; private set; }
    public string SignalType { get; private set; } = string.Empty;
    public decimal ScoreContribution { get; private set; }
    public string Metadata { get; private set; } = string.Empty;
}
