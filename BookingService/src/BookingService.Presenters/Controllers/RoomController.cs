using BookingService.Application.Features.Commands.Room;
using BookingService.Application.Features.Queries.Room;
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
        CreateRoomCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
    
        return Ok(result);
    }
    
    [HttpGet("{roomId:guid}")]
    public async Task<IActionResult> GetRoomById(
        [FromRoute] Guid roomId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRoomByIdQuery(roomId), cancellationToken);

        var room = new RoomDto(result.HotelId, result.RoomNumber, result.FloorNumber, result.RoomType);
        
        return Ok(room);
    }
    
    [HttpDelete("{roomId:guid}")]
    public async Task<IActionResult> DeleteRoom(
        [FromRoute] Guid roomId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteRoomCommand(roomId), cancellationToken);
            
        return Ok(result);
    }
}