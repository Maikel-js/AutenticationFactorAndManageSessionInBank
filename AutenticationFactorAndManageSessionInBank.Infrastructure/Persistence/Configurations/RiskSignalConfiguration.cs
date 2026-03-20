using AutenticationFactorAndManageSessionInBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutenticationFactorAndManageSessionInBank.Infrastructure.Persistence.Configurations;

public sealed class RiskSignalConfiguration : IEntityTypeConfiguration<RiskSignal>
{
    public void Configure(EntityTypeBuilder<RiskSignal> builder)
    {
        builder.ToTable("risk_signals");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SignalType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Metadata).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ScoreContribution).HasPrecision(5, 2);
    }
}
