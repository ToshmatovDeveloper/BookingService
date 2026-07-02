using BookingService.Application.CustomExceptions;
using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Hotel.Get;

public record GetHotelByIdRequest(Guid Id) : IRequest<HotelDto>;

public class GetHotelByIdHandler(
    ApplicationDbContext dbContext,
    IMemoryCache cache,
    ILogger<GetHotelByIdHandler> logger) : IRequestHandler<GetHotelByIdRequest, HotelDto>
{
    public async Task<HotelDto> Handle(GetHotelByIdRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started reading hotel by id : {request.Id}");

        if (!cache.TryGetValue(request.Id, out Domain.Entities.Hotel? hotel))
        {
            hotel = await dbContext.Hotels
                .Where(x => x.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (hotel == null)
            {
                logger.LogError($"Hotel with id : {request.Id} not found");

                throw new NotFoundException("Hotel not found");
            }
        
            cache.Set(request.Id, hotel,
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