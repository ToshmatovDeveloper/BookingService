using BookingService.Application.Hotel.Create;
using BookingService.Application.Hotel.Delete;
using BookingService.Application.Hotel.Get;
using BookingService.Domain.DTOs;
using BookingService.Domain.Entities;
using BookingService.Domain.Enum;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.IntegrationTests;

public class HotelTests(BookingServiceTestWebFactory factory) 
    : IClassFixture<BookingServiceTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase = factory.ResetDatabaseAsync;
    private IServiceProvider Services { get; set; } = factory.Services;

    [Fact]
    public async Task Create_hotel_with_valid_data_should_succeed()
    {
        //arrange
        await using var scope = Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var cancellationToken = CancellationToken.None;

        var hotelDto = new HotelDto("Hotel A", "Qatar", 12, HotelStarRating.FiveStar);
        
        //act
        
        var result = await mediator.Send(new CreateHotelRequest(hotelDto), cancellationToken);

        //Assert

        Assert.NotEqual(Guid.Empty, result);
    }
    
    [Fact]
    public async Task Get_request_with_valid_id_should_succeed()
    {
        //arrange
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var cancellationToken = CancellationToken.None;

        var hotelId = Guid.NewGuid();
        
        var hotel = new Hotel(hotelId, "Hotel A", "Qatar", 12, HotelStarRating.FiveStar);
        
        dbContext.Hotels.Add(hotel);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        //act
        
        var result = await mediator.Send(new GetHotelByIdRequest(hotelId), cancellationToken);

        //Assert

        Assert.Equal("Hotel A", result.Name);
        Assert.Equal(hotel.Floor, result.Floors);
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
        
        var hotel = new Hotel(hotelId, "Hotel A", "Qatar", 12, HotelStarRating.FiveStar);
        
        dbContext.Hotels.Add(hotel);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        var message = String.Empty;
        
        //act
        
        var result = await mediator.Send(new DeleteHotelRequest(Guid.NewGuid()), cancellationToken);
        
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