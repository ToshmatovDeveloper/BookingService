using BookingService.Domain.Enum;

namespace BookingService.Domain.Entities;

public class Hotel(Guid id, string name, string address, string rating)
{
    public Guid Id { get; set; } = id;

    public string Name { get; set; } = name;

    public string Address { get; set; } = address;

    public IEnumerable<Room>? RoomIds { get; set; } 

    public string StarRating { get; set; } = rating;
}