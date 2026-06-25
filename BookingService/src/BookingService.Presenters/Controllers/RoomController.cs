using BookingService.Application.Room.Create;
using BookingService.Application.Room.Delete;
using BookingService.Application.Room.Get;
using BookingService.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Presenters.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomController(
    IMediator mediator): ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateRoom(
        CreateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
    
        return Ok(result);
    }
    
    [HttpGet("{roomId:guid}")]
    public async Task<IActionResult> GetRoomById(
        [FromRoute] Guid roomId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRoomByIdRequest(roomId), cancellationToken);

        var room = new RoomDto(result.HotelId, result.RoomNumber, result.FloorNumber, result.RoomType);
        
        return Ok(room);
    }
    
    [HttpDelete("{roomId:guid}")]
    public async Task<IActionResult> DeleteRoom(
        [FromRoute] Guid roomId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteRoomRequest(roomId), cancellationToken);
            
        return Ok(result);
    }
}