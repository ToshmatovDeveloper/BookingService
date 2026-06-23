using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Hotel.Get;

public class GetHotelByIdHandler(
    ApplicationDbContext dbContext,
    ILogger<GetHotelByIdHandler> logger) : IRequestHandler<GetHotelByIdRequest, Result<HotelDto, string>>
{
    public async Task<Result<HotelDto, string>> Handle(GetHotelByIdRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started reading hotel by id : {request.Id}");

        var result = await dbContext.Hotels
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
        {
            logger.LogError($"Hotel with id : {request.Id} not found");

            return "Hotel not found";
        }
        
        logger.LogInformation($"Hotel with id : {request.Id} found");
        
        return new HotelDto(result.Name, result.Address, result.Floor, result.StarRating);
    }
}