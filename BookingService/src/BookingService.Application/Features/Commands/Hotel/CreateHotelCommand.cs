using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Commands.Hotel;

public record CreateHotelCommand(HotelDto Dto) : IRequest<HotelDto>;

public class CreateHotelCommandHandler(
    ILogger<CreateHotelCommandHandler> logger,
    ApplicationDbContext dbContext,
    IValidator<CreateHotelCommand> validator) : IRequestHandler<CreateHotelCommand, HotelDto>
{
    public async Task<HotelDto> Handle(CreateHotelCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Started creating hotel");
        var dto = command.Dto;

        await validator.ValidateAndThrowAsync(command, cancellationToken);
        
        var hotel = new Domain.Entities.Hotel(
            dto.Name, dto.Address, dto.Floors, dto.StarRating);
        
        try
        {
            await dbContext.Hotels.AddAsync(hotel, cancellationToken);
            
            logger.LogInformation($"Hotel created successfully. Hotel id : {hotel.Id}");
            
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            throw;
        }
        
        return  new HotelDto(hotel.Name, hotel.Address, hotel.Floor, hotel.StarRating);
    }
}