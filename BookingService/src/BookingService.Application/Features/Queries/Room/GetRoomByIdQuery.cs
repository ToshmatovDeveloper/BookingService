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

namespace BookingService.Application.Features.Queries.Room;

public record GetRoomByIdQuery(Guid Id) : IRequest<RoomDto>;

public class GetRoomByIdQueryHandler(
    ApplicationDbContext dbContext,
    IOptionsMonitor<CacheSettings> options,
    IDistributedCache cache,
    ILogger<GetRoomByIdQueryHandler> logger) : IRequestHandler<GetRoomByIdQuery, RoomDto>
{
    public async Task<RoomDto> Handle(GetRoomByIdQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started reading room by id : {query.Id}");

        string key = $"room-{query.Id}";
        
        string? cachedRoom = await cache.GetStringAsync(key, cancellationToken);
        
        Domain.Entities.Room? room;
        
        if (string.IsNullOrEmpty(cachedRoom))
        {
            room = await dbContext.Rooms
                .Where(x => x.Id == query.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (room == null)
            {
                logger.LogError($"Room with id : {query.Id} not found");
                
                throw new NotFoundException("Room not found"); 
            }
        
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = options.CurrentValue.TimeToLive
            };

            await cache.SetStringAsync(
                key, 
                JsonConvert.SerializeObject(room), 
                cacheOptions, 
                cancellationToken);
            
            logger.LogInformation($"Room with id : {query.Id} found in db");
        }
        else
        {
            logger.LogInformation($"Room with id : {query.Id} found in cache");
            
            room = JsonConvert.DeserializeObject<Domain.Entities.Room>(cachedRoom);
        }
        
        return new RoomDto(room!.HotelId, room.RoomNumber, room.FloorNumber, room.RoomType);
    }
}