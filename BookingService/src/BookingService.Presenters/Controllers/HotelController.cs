using BookingService.Application.Hotel.Create;
using BookingService.Application.Hotel.Delete;
using BookingService.Application.Hotel.Get;
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
        CreateHotelRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
    
        return Ok(result);
    }
    
    [HttpGet("{hotelId:guid}")]
    public async Task<IActionResult> GetHotelById(
        [FromRoute] Guid hotelId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetHotelByIdRequest(hotelId), cancellationToken);
           
        var hotel = new HotelDto(result.Name, result.Address, result.Floors, result.StarRating);
        
        return Ok(hotel);
    }
    
    [HttpDelete("{hotelId:guid}")]
    public async Task<IActionResult> DeleteHotel(
        [FromRoute] Guid hotelId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteHotelRequest(hotelId), cancellationToken);
            
        return Ok(result);
    }
}