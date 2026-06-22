using BookingService.Domain.Enum;

namespace BookingService.Domain.Entities;

public class Hotel(Guid id, string name, string address, HotelStarRating rating, List<Guid> roomsId)
{
    public Guid Id { get; set; } = id;

    public string Name { get; set; } = name;

    public string Address { get; set; } = address;

    public List<Guid> RoomsId { get; set; } = roomsId;

    public HotelStarRating StarRating { get; set; } = rating;
}