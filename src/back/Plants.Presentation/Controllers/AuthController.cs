using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthorizer _authorizer;

    public AuthController(IAuthorizer authorizer)
    {
        _authorizer = authorizer;
    }

    [HttpPost("login")]
    [SwaggerRequestExample(typeof(LoginCommand), typeof(LoginRequestExample))]
    public async Task<ActionResult> Login(LoginCommand command, CancellationToken token)
    {
        var authorization = await _authorizer.AuthorizeAsync(command.Login, command.Password, token);
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
