using AuthService.Application.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;


namespace AuthService.Presenters.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserRegisterController(
    IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RegisterUser(
        UserRegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
    
        return Ok(result);
    }
}