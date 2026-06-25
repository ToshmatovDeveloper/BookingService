using BookingService.Application.Booking.Cancel;
using BookingService.Application.Booking.Create;
using BookingService.Application.Booking.Get;
using BookingService.Domain.DTOs;
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
           
        var booking = new BookingDto(result.HotelId, result.RoomId, result.StartDate, result.EndDate);
        
        return Ok(booking);
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