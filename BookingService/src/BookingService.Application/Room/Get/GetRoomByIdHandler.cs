using BookingService.Application.CustomExceptions;
using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Room.Get;

public record GetRoomByIdRequest(Guid Id) : IRequest<RoomDto>;

public class GetRoomByIdHandler(
    ApplicationDbContext dbContext,
    IMemoryCache cache,
    ILogger<GetRoomByIdHandler> logger) : IRequestHandler<GetRoomByIdRequest, RoomDto>
{
    public async Task<RoomDto> Handle(GetRoomByIdRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started reading room by id : {request.Id}");

        if (!cache.TryGetValue(request.Id, out Domain.Entities.Room? room))
        {
            room = await dbContext.Rooms
                .Where(x => x.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (room == null)
            {
                logger.LogError($"Room with id : {request.Id} not found");
                
                throw new NotFoundException("Room not found"); 
            }
        
            cache.Set(request.Id, room,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            
            logger.LogInformation($"Room with id : {request.Id} found in db");
        }
        else
        {
            logger.LogInformation($"Room with id : {request.Id} found in cache");
        }
        
        return new RoomDto(room.HotelId, room.RoomNumber, room.FloorNumber, room.RoomType);
    }
}