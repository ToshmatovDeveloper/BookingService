using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Hotel.Create;

public record CreateHotelRequest(HotelDto Dto) : IRequest<Guid>;

public class CreateHotelHandler(
    ILogger<CreateHotelHandler> logger,
    ApplicationDbContext dbContext,
    IValidator<CreateHotelRequest> validator) : IRequestHandler<CreateHotelRequest, Guid>
{
    public async Task<Guid> Handle(CreateHotelRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Started creating hotel");
        var dto = request.Dto;

        await validator.ValidateAndThrowAsync(request, cancellationToken);
        
        var hotel = new Domain.Entities.Hotel(
            Guid.NewGuid(), dto.Name, dto.Address, dto.Floors, dto.StarRating);
        
        try
        {
            await dbContext.Hotels.AddAsync(hotel, cancellationToken);
            
            logger.LogInformation($"Hotel created successfully. Hotel id : {hotel.Id}");
            
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
        
        return  hotel.Id;
    }
}