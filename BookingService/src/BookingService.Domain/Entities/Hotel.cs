using BookingService.Domain.Enum;

namespace BookingService.Domain.Entities;

public class Hotel
{
    public Hotel() { }
    
    public Hotel(string name, string address, int floors, HotelStarRating starRating)
    {
        Name = name;
        Address = address;
        Floor =  floors;
        StarRating = starRating;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();

    public string Name { get; init; } 

    public string Address { get; init; }

    public int Floor { get; init; }
    
    public HotelStarRating StarRating { get; init; }
    
    public IEnumerable<Room>? RoomIds { get; init; } 
}