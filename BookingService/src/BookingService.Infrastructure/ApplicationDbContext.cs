using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Hotel> Hotels { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new HotelConfiguration());
        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new BookingConfiguration());
    }
}

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("Hotels");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(x => x.Address)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.HasMany(x => x.RoomIds)
            .WithOne(x => x.Hotel);
        
        builder.Property(x => x.StarRating)
            .IsRequired();
    }
}

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.RoomNumber)
            .IsRequired();
        
        builder.Property(x => x.FloorNumber)
            .IsRequired();
        
        builder.Property(x => x.IsVacant)
            .IsRequired();

        builder.HasOne(x => x.Hotel)
            .WithMany(x => x.RoomIds)
            .HasForeignKey(x => x.HotelId);
        
        builder.Property(x => x.RoomType)
            .IsRequired();
    }
}

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Rooms");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.HotelId)
            .IsRequired();
        
        builder.HasOne(x => x.Room)
            .WithOne(x => x.Booking)
            .HasForeignKey<Room>(r => r.Id);
        
        builder.Property(x => x.RoomId)
            .IsRequired();
        
        builder.Property(x => x.StartDate)
            .IsRequired();
        
        builder.Property(x => x.EndDate)
            .IsRequired();
        
        builder.Property(x => x.Status)
            .IsRequired();
    }
}