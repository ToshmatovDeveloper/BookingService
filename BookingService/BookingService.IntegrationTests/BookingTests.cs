using BookingService.Application.Features.Commands.Booking;
using BookingService.Application.Features.Queries.Booking;
using BookingService.Domain.DTOs;
using BookingService.Domain.Entities;
using BookingService.Domain.Enum;
using BookingService.Infrastructure;
using MediatR; 
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.IntegrationTests;

public class BookingTests(BookingServiceTestWebFactory factory) 
    : IClassFixture<BookingServiceTestWebFactory>, IAsyncLifetime
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

        var hotel = new Hotel("Tower", "A", 12, HotelStarRating.FiveStar);

        dbContext.Hotels.Add(hotel);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        var room = new Room(12, 22, hotel.Id, RoomType.FamilyRoom);
        
        dbContext.Rooms.Add(room);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        //act
        
        var result = await mediator.Send(new CreateBookingCommand(
            new BookingDto(
                hotel.Id,
                room.Id,
                DateTime.UtcNow.AddDays(4),
                DateTime.UtcNow.AddDays(6))), cancellationToken);

        //assert
        
        Assert.NotNull(result);
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
            "Tower",
            "A",
            12, 
            HotelStarRating.FiveStar);

        dbContext.Hotels.Add(hotel);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        var room = new Room(
            12,
            22,
            hotel.Id,
            RoomType.FamilyRoom);
        
        dbContext.Rooms.Add(room);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        var booking = new Booking(
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
            await mediator.Send(new CreateBookingCommand(
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
    
    [Fact]
    public async Task GetRequest_with_valid_booking_id_should_succeed()
    {
        //arrange
        
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var cancellationToken = CancellationToken.None;

        var hotel = new Hotel(
            "Tower",
            "A",
            12, 
            HotelStarRating.FiveStar);

        dbContext.Hotels.Add(hotel);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        var room = new Room(
            12,
            22,
            hotel.Id,
            RoomType.FamilyRoom);
        
        dbContext.Rooms.Add(room);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        var booking = new Booking(
            hotel.Id,
            room.Id, 
            DateTime.UtcNow.AddDays(3),
            DateTime.UtcNow.AddDays(7), 
            BookingStatus.Confirmed);
        
        await dbContext.Bookings.AddAsync(booking, cancellationToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        //act

        var getResult = await mediator.Send(new GetBookingByIdQuery(booking.Id), cancellationToken);
        
        //assert

        Assert.Equal(getResult, new BookingDto(hotel.Id, room.Id, booking.StartDate, booking.EndDate));
    }
    
    [Fact]
    public async Task Cancel_request_with_valid_booking_id_should_succeed()
    {
        //arrange
        
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var cancellationToken = CancellationToken.None;

        var hotel = new Hotel(
            "Tower",
            "A",
            12, 
            HotelStarRating.FiveStar);

        dbContext.Hotels.Add(hotel);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        var room = new Room(
            12,
            22,
            hotel.Id,
            RoomType.FamilyRoom);
        
        dbContext.Rooms.Add(room);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        var booking = new Booking(
            hotel.Id,
            room.Id, 
            DateTime.UtcNow.AddDays(3),
            DateTime.UtcNow.AddDays(7), 
            BookingStatus.Confirmed);
        
        await dbContext.Bookings.AddAsync(booking, cancellationToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        //act

        var getResult = await mediator
            .Send(new CancelBookingCommand(booking.Id), cancellationToken);
        
        //assert

        Assert.Equal("Booking cancelled successfully", getResult.message);
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
