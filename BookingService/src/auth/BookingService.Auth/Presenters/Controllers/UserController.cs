using BookingService.Auth.Application.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Auth.Presenters.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(
    IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(
        UserRegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
    
        return Ok(result);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> UserLogin(UserLoginCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

}