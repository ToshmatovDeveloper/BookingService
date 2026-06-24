using BookingService.Domain.Enum;

namespace BookingService.Domain.DTOs;

public record BookingDto(
    Guid HotelId,
    Guid RoomId, 
    DateTime StartDate,
    DateTime EndDate);