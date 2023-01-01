using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.Services;
using Plants.Application.Commands;
using Plants.Presentation.Examples;
using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("auth")]
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [SwaggerRequestExample(typeof(LoginCommand), typeof(LoginRequestExample))]
    public async Task<ActionResult> Login(LoginCommand command, CancellationToken token)
    {
        var res = await _mediator.Send(command, token);
        ActionResult result;
        if (res.IsSuccessfull)
        {
            result = Ok(res);
        }
        else
        {
            result = Unauthorized();
        }
        return result;
    }
}

[ApiController]
[Route("v2/auth")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class AuthControllerV2 : ControllerBase
{
    private readonly IAuthorizer _authorizer;

    public AuthControllerV2(IAuthorizer authorizer)
    {
        _authorizer = authorizer;
    }

    [HttpPost("login")]
    [SwaggerRequestExample(typeof(LoginCommand), typeof(LoginRequestExampleV2))]
    public async Task<ActionResult> Login(LoginCommand command, CancellationToken token)
    {
        var authorization = await _authorizer.AuthorizeAsync(command.Login, command.Password);
        ActionResult result;
        if (authorization is not null)
        {
            result = Ok(authorization);
        }
        else
        {
            result = Unauthorized();
        }
        return result;
    }
}
