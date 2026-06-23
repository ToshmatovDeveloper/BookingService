using BookingService.Domain.Enum;

namespace BookingService.Domain.Entities;

public class Booking(Guid id, Guid hotelId, Guid roomId, DateTime startDate, DateTime endDate)
{
    public Guid Id { get; set; } = id;

    public Guid HotelId { get; set; } = hotelId;

    public Guid RoomId { get; set; } = roomId;
    
    public Room Room { get; set; }

    public DateTime StartDate { get; set; } = startDate;

    public DateTime EndDate { get; set; } = endDate;

    public BookingStatus Status { get; set; }
}