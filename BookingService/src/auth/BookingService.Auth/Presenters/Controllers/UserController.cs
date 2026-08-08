using BookingService.Auth.Application.Features;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BookingService.Auth.Presenters.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(
    IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("auth-limit")] 
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(
        UserRegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
    
        return Ok(result);
    }
    
    [AllowAnonymous]
    [EnableRateLimiting("auth-limit")] 
    [HttpPost("login")]
    public async Task<IActionResult> UserLogin(UserLoginCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }
    
    [AllowAnonymous]
    [EnableRateLimiting("auth-limit")] 
    [HttpPost("refresh")]
    public async Task<IActionResult> UserLoginWithRefreshToken(RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    } 
}