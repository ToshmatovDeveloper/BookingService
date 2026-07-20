using BookingService.Application.CustomExceptions;
using BookingService.Application.Settings.Cache;
using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookingService.Application.Features.Queries.Room;

public record GetRoomByIdQuery(Guid Id) : IRequest<RoomDto>;

public class GetRoomByIdQueryHandler(
    ApplicationDbContext dbContext,
    IOptionsMonitor<CacheSettings> options,
    IMemoryCache cache,
    ILogger<GetRoomByIdQueryHandler> logger) : IRequestHandler<GetRoomByIdQuery, RoomDto>
{
    public async Task<RoomDto> Handle(GetRoomByIdQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started reading room by id : {query.Id}");

        if (!cache.TryGetValue(query.Id, out Domain.Entities.Room? room))
        {
            room = await dbContext.Rooms
                .Where(x => x.Id == query.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (room == null)
            {
                logger.LogError($"Room with id : {query.Id} not found");
                
                throw new NotFoundException("Room not found"); 
            }
        
            cache.Set(query.Id, room, options.CurrentValue.TimeToLive);
            
            logger.LogInformation($"Room with id : {query.Id} found in db");
        }
        else
        {
            logger.LogInformation($"Room with id : {query.Id} found in cache");
        }
        
        return new RoomDto(room.HotelId, room.RoomNumber, room.FloorNumber, room.RoomType);
    }
}