using BookingService.Application.Booking.Create;
using BookingService.Domain.DTOs;
using BookingService.Domain.Entities;
using BookingService.Domain.Enum;
using BookingService.Infrastructure;
using MediatR; 
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.IntegrationTests;

public class BookingTests(BookingTestWebFactory factory) 
    : IClassFixture<BookingTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase = factory.ResetDatabaseAsync;
    private IServiceProvider Services { get; set; } = factory.Services;

    [Fact]
    public async Task CreateBooking_with_valid_data_should_succeed()
    {
        //arrange
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var cancellationToken = CancellationToken.None;

        var hotel = new Hotel(Guid.NewGuid(), "Tower", "A", 12, HotelStarRating.FiveStar);

        dbContext.Hotels.Add(hotel);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        var room = new Room(Guid.NewGuid(), 12, 22, hotel.Id, RoomType.FamilyRoom);
        
        dbContext.Rooms.Add(room);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        //act
        
        var result = await mediator.Send(new CreateBookingRequest(
            new BookingDto(
                hotel.Id,
                room.Id,
                DateTime.UtcNow.AddDays(4),
                DateTime.UtcNow.AddDays(6))), cancellationToken);

        //assert
        
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public async Task CreateBooking_with_overlapping_dates_should_throw_exception()
    {
        //arrange
        
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var cancellationToken = CancellationToken.None;

        var hotel = new Hotel(
            Guid.NewGuid(),
            "Tower",
            "A",
            12, 
            HotelStarRating.FiveStar);

        dbContext.Hotels.Add(hotel);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        var room = new Room(
            Guid.NewGuid(),
            12,
            22,
            hotel.Id,
            RoomType.FamilyRoom);
        
        dbContext.Rooms.Add(room);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        var booking = new Booking(
            Guid.NewGuid(),
            hotel.Id,
            room.Id, 
            DateTime.UtcNow.AddDays(3),
            DateTime.UtcNow.AddDays(7), 
            BookingStatus.Confirmed);
        
        await dbContext.Bookings.AddAsync(booking, cancellationToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        //act

        var message = String.Empty;
        
        try
        {
            await mediator.Send(new CreateBookingRequest(
                new BookingDto(
                        hotel.Id,
                        room.Id,
                        DateTime.UtcNow.AddDays(4),
                        DateTime.UtcNow.AddDays(6))),
                cancellationToken);
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }
        
        //assert

        Assert.Equal("Room is not available for booking in current time range", message);
        
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }
}
