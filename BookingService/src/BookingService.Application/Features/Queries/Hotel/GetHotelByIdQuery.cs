using BookingService.Application.CustomExceptions;
using BookingService.Application.Settings.Cache;
using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookingService.Application.Features.Queries.Hotel;

public record GetHotelByIdQuery(Guid Id) : IRequest<HotelDto>;

public class GetHotelByIdQueryHandler(
    ApplicationDbContext dbContext,
    IOptionsMonitor<CacheSettings> options,
    IMemoryCache cache,
    ILogger<GetHotelByIdQueryHandler> logger) : IRequestHandler<GetHotelByIdQuery, HotelDto>
{
    public async Task<HotelDto> Handle(GetHotelByIdQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started reading hotel by id : {query.Id}");

        if (!cache.TryGetValue(query.Id, out Domain.Entities.Hotel? hotel))
        {
            hotel = await dbContext.Hotels
                .Where(x => x.Id == query.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (hotel == null)
            {
                logger.LogError($"Hotel with id : {query.Id} not found");

                throw new NotFoundException("Hotel not found");
            }
        
            cache.Set(query.Id, hotel, options.CurrentValue.TimeToLive);
            
            logger.LogInformation($"Hotel with id : {query.Id} found in db");
        }
        else
        {
            logger.LogInformation($"Hotel with id : {query.Id} found in cache");
        }
        
        return new HotelDto(hotel!.Name, hotel.Address, hotel.Floor, hotel.StarRating);
    }
}