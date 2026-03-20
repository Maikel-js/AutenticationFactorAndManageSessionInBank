using AutenticationFactorAndManageSessionInBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutenticationFactorAndManageSessionInBank.Infrastructure.Persistence.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DeviceId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CurrentIpAddress).HasMaxLength(64).IsRequired();
        builder.Property(x => x.UserAgent).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.MfaTicketHash).HasMaxLength(256);
        builder.Property(x => x.RiskScore).HasPrecision(5, 2);
        builder.HasMany(x => x.RefreshTokens)
            .WithOne(x => x.Session)
            .HasForeignKey(x => x.SessionId);
        builder.HasMany(x => x.RiskSignals)
            .WithOne()
            .HasForeignKey(x => x.SessionId);
    }
}
