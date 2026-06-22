using BookingService.Domain.Enum;

namespace BookingService.Domain.Entities;

public class Room(Guid id, uint roomNumber, int floorNumber, Guid hotelId, RoomType roomType)
{
    public Guid Id { get; set; } = id;

    public uint RoomNumber { get; set; } = roomNumber;

    public int FloorNumber { get; set; } = floorNumber;
    
    public bool IsVacant { get; set; } = true;

    public Guid HotelId { get; set; } = hotelId;

    public RoomType RoomType { get; set; } = roomType;
}