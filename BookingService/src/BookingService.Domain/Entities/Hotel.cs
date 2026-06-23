namespace BookingService.Domain.Entities;

public class Hotel
{
    public Hotel() { }
    
    public Hotel(Guid id, string name, string address, string starRating)
    {
        Id = id;
        Name = name;
        Address = address;
        StarRating = starRating;
    }
    
    public Guid Id { get; init; }

    public string Name { get; init; } 

    public string Address { get; init; }

    public IEnumerable<Room>? RoomIds { get; init; } 

    public string StarRating { get; init; }
}