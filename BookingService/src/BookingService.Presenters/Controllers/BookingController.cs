using BookingService.Application.Booking.Cancel;
using BookingService.Application.Booking.Create;
using BookingService.Application.Booking.Get;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Presenters.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController(
    IMediator mediator): ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateBooking(
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
    
        return Ok(result);
    }
    
    [HttpGet("{bookingId:guid}")]
    public async Task<IActionResult> GetBookingById(
        [FromRoute] Guid bookingId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBookingByIdRequest(bookingId), cancellationToken);
            
        return Ok(result.Value);
    }
    
    [HttpPatch("{bookingId:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(
        [FromRoute] Guid bookingId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CancelBookingRequest(bookingId), cancellationToken);
            
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }
        
        return Ok(result.Value);
    }
    
}