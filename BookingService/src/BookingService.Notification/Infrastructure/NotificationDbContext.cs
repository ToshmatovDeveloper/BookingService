using BookingService.Notification.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Notification.Infrastructure;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Mail> Mails { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MailConfiguration());
    }
}

public class MailConfiguration : IEntityTypeConfiguration<Mail>
{
    public void Configure(EntityTypeBuilder<Mail> builder)
    {
        builder.ToTable("Mails");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ReceiverAddress)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(x => x.Subject)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(x => x.Text)
            .IsRequired();
        
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();
    }
}
            