using BookingService.Application.Features.Commands.Hotel;
using BookingService.Application.Features.Queries.Hotel;
using BookingService.Domain.DTOs;
using CSharpFunctionalExtensions;
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
        CreateHotelCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
    
        return Ok(result);
    }
    
    [HttpGet("{hotelId:guid}")]
    public async Task<IActionResult> GetHotelById(
        [FromRoute] Guid hotelId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetHotelByIdQuery(hotelId), cancellationToken);
           
        var hotel = new HotelDto(result.Name, result.Address, result.Floors, result.StarRating);
        
        return Ok(hotel);
    }
    
    [HttpDelete("{hotelId:guid}")]
    public async Task<IActionResult> DeleteHotel(
        [FromRoute] Guid hotelId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteHotelCommand(hotelId), cancellationToken);
            
        return Ok(result);
    }
}