using BookingService.Application.Features.Commands.Booking;
using BookingService.Application.Features.Queries.Booking;
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
        CreateBookingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
    
        return Ok(result);
    }
    
    [HttpGet("{bookingId:guid}")]
    public async Task<IActionResult> GetBookingById(
        [FromRoute] Guid bookingId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBookingByIdQuery(bookingId), cancellationToken);
           
        var booking = new BookingDto(result.HotelId, result.RoomId, result.StartDate, result.EndDate);
        
        return Ok(booking);
    }
    
    [HttpPatch("{bookingId:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(
        [FromRoute] Guid bookingId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CancelBookingCommand(bookingId), cancellationToken);
            
        if (result.dto == null)
        {
            return BadRequest(new { error = result.message });
        }
        
        return Ok(result.dto);
    }
    
}