using BookingService.Application.Room.Create;
using BookingService.Application.Room.Delete;
using BookingService.Application.Room.Get;
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
        [FromRoute] Guid hotelId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRoomByIdRequest(hotelId), cancellationToken);
            
        return Ok(result.Value);
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