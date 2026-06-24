using BookingService.Domain.Enum;

namespace BookingService.Domain.Entities;

public class Room(Guid id, uint roomNumber, int floorNumber, Guid hotelId, RoomType roomType)
{
    public Guid Id { get; init; } = id;
    
    public Guid HotelId { get; init; } = hotelId;
    
    public uint RoomNumber { get; init; } = roomNumber;
    
    public int FloorNumber { get; init; } = floorNumber;
    
    public bool IsVacant { get; init; } = true;
    
    public RoomType RoomType { get; init; } = roomType;
    
    public Hotel Hotel { get; init; } = null!;
    
    public IEnumerable<Booking>? Bookings { get; init; } 
}