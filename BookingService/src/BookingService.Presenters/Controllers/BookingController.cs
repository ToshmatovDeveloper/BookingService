using System.Security.Claims;
using BookingService.Application.Features.Commands.Booking;
using BookingService.Application.Features.Queries.Booking;
using BookingService.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BookingService.Presenters.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController(
    IMediator mediator): ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateBooking(
        CreateBookingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
    
        return Ok(result);
    }
    
    [Authorize]
    [HttpGet("{bookingId:guid}")]
    public async Task<IActionResult> GetBookingById(
        [FromRoute] Guid bookingId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBookingByIdQuery(bookingId), cancellationToken);
           
        var booking = new BookingDto(result.HotelId, result.RoomId, result.StartDate, result.EndDate);
        
        return Ok(booking);
    }

    [Authorize]
    [HttpPatch("{bookingId:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(
        [FromRoute] Guid bookingId,
        CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var userId = Guid.Parse(userIdString!);

        var result = await mediator.Send(new CancelBookingCommand(bookingId, userId), cancellationToken);
        
        if (result.dto == null)
        {
            return BadRequest(new { error = result.message });
        }
    
        return Ok(result.dto);
    }
}