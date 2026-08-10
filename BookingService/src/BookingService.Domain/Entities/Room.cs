using BookingService.Domain.Enum;
using BookingService.Domain.Interfaces;

namespace BookingService.Domain.Entities;

public class Room(uint roomNumber, int floorNumber, Guid hotelId, RoomType roomType) : IAuditableEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    
    public Guid HotelId { get; init; } = hotelId;
    
    public uint RoomNumber { get; init; } = roomNumber;
    
    public int FloorNumber { get; init; } = floorNumber;
    
    public bool IsVacant { get; init; } = true;
    
    public RoomType RoomType { get; init; } = roomType;
    
    public Hotel Hotel { get; init; } = null!;
    
    public IEnumerable<Booking>? Bookings { get; init; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime ModifiedAt { get; set; }
}