using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Configuration;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        
        builder.Property(a => a.UserName)
            .HasMaxLength(AccountConstants.UserNameMaxLength)
            .IsRequired();
        
        builder.Property(a => a.Email)
            .HasMaxLength(AccountConstants.MaxEmailLength)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .ValueGeneratedOnAdd();
         
        builder.Property(a => a.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate();
    }
}