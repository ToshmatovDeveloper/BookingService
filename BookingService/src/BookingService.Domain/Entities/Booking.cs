using BookingService.Domain.Enum;
using BookingService.Domain.Interfaces;

namespace BookingService.Domain.Entities;

public class Booking(Guid hotelId, Guid roomId, Guid userId, DateTime startDate, DateTime endDate, BookingStatus Status)
    : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid HotelId { get; set; } = hotelId;

    public Guid RoomId { get; set; } = roomId;

    public Guid UserId { get; set; } = userId;
    
    public Room? Room { get; set; }

    public DateTime StartDate { get; set; } = startDate;

    public DateTime EndDate { get; set; } = endDate;

    public BookingStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime ModifiedAt { get; set; }
}