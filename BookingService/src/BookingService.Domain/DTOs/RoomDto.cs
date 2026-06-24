using BookingService.Domain.Enum;

namespace BookingService.Domain.DTOs;

public record RoomDto(Guid HotelId, uint RoomNumber, int FloorNumber, RoomType RoomType);