using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Hotel.Get;

public class GetHotelByIdHandler(
    ApplicationDbContext dbContext,
    IMemoryCache cache,
    ILogger<GetHotelByIdHandler> logger) : IRequestHandler<GetHotelByIdRequest, Result<HotelDto, string>>
{
    public async Task<Result<HotelDto, string>> Handle(GetHotelByIdRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started reading hotel by id : {request.Id}");

        if (!cache.TryGetValue(request.Id, out Domain.Entities.Hotel? hotel))
        {
            var result = await dbContext.Hotels
                .Where(x => x.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
            {
                logger.LogError($"Hotel with id : {request.Id} not found");

                return "Hotel not found";
            }
        
            cache.Set(request.Id, 
                hotel, 
         new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            
            logger.LogInformation($"Hotel with id : {request.Id} found in db");
        }
        else
        {
            logger.LogInformation($"Hotel with id : {request.Id} found in cache");
        }
        
        return new HotelDto(hotel!.Name, hotel.Address, hotel.Floor, hotel.StarRating);
    }
}