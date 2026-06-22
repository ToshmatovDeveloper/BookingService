using BookingService.Application.Hotel.Create;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Presenters.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HotelController(
    IMediator mediator): ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateHotel(
        CreateHotelRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(request, cancellationToken);
    
        return Ok();
    }
}