using BookingService.Domain.Enum;

namespace BookingService.Domain.DTOs;

public record HotelDto(string Name, string Address, int Floors, HotelStarRating StarRating);