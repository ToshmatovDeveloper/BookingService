using BookingService.Domain.Enum;

namespace BookingService.Domain.Entities;

public class Booking(Guid hotelId, Guid roomId, DateTime startDate, DateTime endDate, BookingStatus Status)
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid HotelId { get; set; } = hotelId;

    public Guid RoomId { get; set; } = roomId;
    
    public Room? Room { get; set; }

    public DateTime StartDate { get; set; } = startDate;

    public DateTime EndDate { get; set; } = endDate;

    public BookingStatus Status { get; set; }
}