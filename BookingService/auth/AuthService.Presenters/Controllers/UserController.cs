using AuthService.Application.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;


namespace AuthService.Presenters.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(
    IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(
        UserRegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
    
        return Ok(result);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> UserLogin(UserLoginRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(request, ct);
        return Ok(result);
    }

}