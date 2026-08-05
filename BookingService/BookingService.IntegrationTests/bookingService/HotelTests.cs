using BookingService.Application.Features.Commands.Hotel;
using BookingService.Application.Features.Queries.Hotel;
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
        
        var result = await mediator.Send(new CreateHotelCommand(hotelDto), cancellationToken);

        //Assert

        Assert.Equal(hotelDto.Name, result.Name);
        Assert.Equal(hotelDto.Address, result.Address);
        Assert.Equal(hotelDto.Floors, result.Floors);
        Assert.Equal(hotelDto.StarRating, result.StarRating);
    }
    
    [Fact]
    public async Task Get_request_with_valid_id_should_succeed()
    {
        //arrange
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var cancellationToken = CancellationToken.None;
        
        var hotel = new Hotel("Hotel A", "Qatar", 12, HotelStarRating.FiveStar);
        
        dbContext.Hotels.Add(hotel);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        //act
        
        var result = await mediator.Send(new GetHotelByIdQuery(hotel.Id), cancellationToken);

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
        
        var hotel = new Hotel("Hotel A", "Qatar", 12, HotelStarRating.FiveStar);
        
        dbContext.Hotels.Add(hotel);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        var message = String.Empty;
        
        //act
        
        var result = await mediator.Send(new DeleteHotelCommand(Guid.NewGuid()), cancellationToken);
        
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