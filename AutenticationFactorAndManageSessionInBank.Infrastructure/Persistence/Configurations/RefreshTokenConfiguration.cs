using AutenticationFactorAndManageSessionInBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutenticationFactorAndManageSessionInBank.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.TokenHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ParentTokenId).HasMaxLength(64);
        builder.Property(x => x.ReplacedByTokenId).HasMaxLength(64);
        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}
