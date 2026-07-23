using BookingService.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Auth.Infrastructure.Configuration;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);
        
        builder.Property(rt => rt.Token).HasMaxLength(200);
        
        builder.HasIndex(rt => rt.Token).IsUnique();
        
        builder.HasOne(rt =>rt.Account).WithMany().HasForeignKey(rt => rt.AccountId);
    }
}