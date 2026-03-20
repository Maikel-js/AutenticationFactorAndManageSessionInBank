using AutenticationFactorAndManageSessionInBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutenticationFactorAndManageSessionInBank.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(x => x.PasswordSalt).HasMaxLength(256).IsRequired();
        builder.Property(x => x.TenantId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TotpSecret).HasMaxLength(128);
        builder.HasIndex(x => new { x.Email, x.TenantId }).IsUnique();
        builder.HasMany(x => x.Sessions)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);
    }
}
