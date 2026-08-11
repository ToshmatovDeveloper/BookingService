using BookingService.Application.CustomExceptions;
using BookingService.Application.Settings.Cache;
using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BookingService.Application.Features.Queries.Hotel;

public record GetHotelByIdQuery(Guid Id) : IRequest<HotelDto>;

public class GetHotelByIdQueryHandler(
    ApplicationDbContext dbContext,
    IOptionsMonitor<CacheSettings> options,
    IDistributedCache cache,
    ILogger<GetHotelByIdQueryHandler> logger) : IRequestHandler<GetHotelByIdQuery, HotelDto>
{
    public async Task<HotelDto> Handle(GetHotelByIdQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started reading hotel by id : {query.Id}");

        string key = $"hotel-{query.Id}";
        
        string? cachedHotel = await cache.GetStringAsync(key, cancellationToken);
        
        Domain.Entities.Hotel? hotel;
        
        if (string.IsNullOrEmpty(cachedHotel))
        {
            hotel = await dbContext.Hotels
                .Where(x => x.Id == query.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (hotel == null)
            {
                logger.LogError($"Hotel with id : {query.Id} not found");

                throw new NotFoundException("Hotel not found");
            }
        
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = options.CurrentValue.TimeToLive
            };

            await cache.SetStringAsync(
                key, 
                JsonConvert.SerializeObject(hotel), 
                cacheOptions, 
                cancellationToken);
            
            logger.LogInformation($"Hotel with id : {query.Id} found in db");
        }
        else
        {
            logger.LogInformation($"Hotel with id : {query.Id} found in cache");
            
            hotel = JsonConvert.DeserializeObject<Domain.Entities.Hotel>(cachedHotel);
        }
        
        return new HotelDto(hotel!.Name, hotel.Address, hotel.Floor, hotel.StarRating);
    }
}