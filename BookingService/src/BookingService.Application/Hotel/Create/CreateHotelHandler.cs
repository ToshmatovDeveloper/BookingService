using BookingService.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Hotel.Create;

public class CreateHotelHandler(
    ILogger<CreateHotelHandler> logger,
    ApplicationDbContext dbContext) : IRequestHandler<CreateHotelRequest, Guid>
{
    public async Task<Guid> Handle(CreateHotelRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Started creating hotel");
        var dto = request.Dto;

        var hotel = new Domain.Entities.Hotel(
            Guid.NewGuid(), dto.Name, dto.Address, dto.StarRating);
        
        try
        {
            await dbContext.Hotels.AddAsync(hotel, cancellationToken);

            logger.LogInformation("Hotel created successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
        
        return  hotel.Id;
    }
}