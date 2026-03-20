using AutenticationFactorAndManageSessionInBank.Application.Abstractions.Security;
using AutenticationFactorAndManageSessionInBank.Application.Contracts.Authentication;
using AutenticationFactorAndManageSessionInBank.Domain.Entities;

namespace AutenticationFactorAndManageSessionInBank.Infrastructure.Security;

public sealed class RiskScoringService : IRiskScoringService
{
    public RiskEvaluation Evaluate(User user, LoginRequest request)
    {
        var signals = new List<string>();
        decimal score = 5m;

        if (!string.Equals(user.TenantId, request.TenantId, StringComparison.Ordinal))
        {
            score += 40m;
            signals.Add("tenant-mismatch");
        }

        if (request.UserAgent.Contains("curl", StringComparison.OrdinalIgnoreCase))
        {
            score += 10m;
            signals.Add("non-browser-agent");
        }

        if (request.IpAddress.StartsWith("10.") || request.IpAddress.StartsWith("192.168."))
        {
            score += 5m;
            signals.Add("private-network-origin");
        }

        if (request.DeviceId.Length < 8)
        {
            score += 15m;
            signals.Add("weak-device-fingerprint");
        }

        return new RiskEvaluation(Math.Clamp(score, 0m, 100m), signals);
    }
}
