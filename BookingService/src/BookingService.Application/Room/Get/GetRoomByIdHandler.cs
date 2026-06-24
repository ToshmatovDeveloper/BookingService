using BookingService.Domain.DTOs;
using BookingService.Infrastructure;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Room.Get;

public class GetRoomByIdHandler(
    ApplicationDbContext dbContext,
    ILogger<GetRoomByIdHandler> logger) : IRequestHandler<GetRoomByIdRequest, Result<RoomDto, string>>
{
    public async Task<Result<RoomDto, string>> Handle(GetRoomByIdRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started reading room by id : {request.Id}");

        var result = await dbContext.Rooms
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
        {
            logger.LogError($"Room with id : {request.Id} not found");

            return "Room not found";
        }
        
        logger.LogInformation($"Room with id : {request.Id} found");
        
        return new RoomDto(result.HotelId, result.RoomNumber, result.FloorNumber, result.RoomType);
    }
}