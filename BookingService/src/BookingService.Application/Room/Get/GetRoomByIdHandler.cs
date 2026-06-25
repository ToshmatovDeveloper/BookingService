using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Room.Get;

public class GetRoomByIdHandler(
    ApplicationDbContext dbContext,
    IMemoryCache cache,
    ILogger<GetRoomByIdHandler> logger) : IRequestHandler<GetRoomByIdRequest, Result<RoomDto, string>>
{
    public async Task<Result<RoomDto, string>> Handle(GetRoomByIdRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started reading room by id : {request.Id}");

        if (!cache.TryGetValue(request.Id, out Domain.Entities.Room? room))
        {
            var result = await dbContext.Rooms
                .Where(x => x.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
            {
                logger.LogError($"Room with id : {request.Id} not found");

                return "Room not found";
            }
        
            cache.Set(request.Id,
                room,
         new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            
            logger.LogInformation($"Room with id : {request.Id} found in db");
        }
        else
        {
            logger.LogInformation($"Room with id : {request.Id} found in cache");
        }
        
        return new RoomDto(room!.HotelId, room.RoomNumber, room.FloorNumber, room.RoomType);
    }
}