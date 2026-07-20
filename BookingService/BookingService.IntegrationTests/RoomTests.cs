using BookingService.Application.Features.Commands.Room;
using BookingService.Application.Features.Queries.Room;
using BookingService.Domain.DTOs;
using BookingService.Domain.Entities;
using BookingService.Domain.Enum;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.IntegrationTests;

public class RoomTests(BookingServiceTestWebFactory factory) 
    : IClassFixture<BookingServiceTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase = factory.ResetDatabaseAsync;
    private IServiceProvider Services { get; set; } = factory.Services;

    [Fact]
    public async Task Create_room_with_valid_data_should_succeed()
    {
        //arrange
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var cancellationToken = CancellationToken.None;

        var hotel = new Hotel("Tower", "A", 12, HotelStarRating.FiveStar);

        dbContext.Hotels.Add(hotel);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        var roomDto = new RoomDto(hotel.Id, 22, 12, RoomType.OneBedroom);
        
        //act
        
        var result = await mediator.Send(new CreateRoomCommand(roomDto), cancellationToken);

        //Assert

        Assert.NotNull(result);
    }
    
    [Fact]
    public async Task Get_request_with_valid_id_should_succeed()
    {
        //arrange
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var cancellationToken = CancellationToken.None;

        var hotel = new Hotel("Tower", "A", 12, HotelStarRating.FiveStar);

        dbContext.Hotels.Add(hotel);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        var room = new Room(33, 12, hotel.Id, RoomType.FamilyRoom);
        
        dbContext.Rooms.Add(room);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        //act
        
        var result = await mediator.Send(new GetRoomByIdQuery(room.Id), cancellationToken);

        //Assert

        Assert.Equal((uint)33, result.RoomNumber);
        Assert.Equal(hotel.Id, result.HotelId);
    } 
    
    [Fact]
    public async Task Delete_request_with_non_existed_id_should_failed()
    {
        //arrange
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var cancellationToken = CancellationToken.None;

        var hotelId = Guid.NewGuid();
        
        var hotel = new Hotel("Hotel A", "Qatar", 12, HotelStarRating.FiveStar);
        
        dbContext.Hotels.Add(hotel);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        var room = new Room(33, 12, hotel.Id, RoomType.FamilyRoom);
        
        dbContext.Rooms.Add(room);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        //act
        
        var result = await mediator.Send(new DeleteRoomCommand(Guid.NewGuid()), cancellationToken);
        
        //Assert

        Assert.Equal(false, result);
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