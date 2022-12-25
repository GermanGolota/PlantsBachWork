using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.Services;
using Plants.Presentation.Examples;
using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation.Controllers.v2;

[ApiController]
[Route("v2/auth")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class AuthController : ControllerBase
{
    private readonly IAuthorizer _authorizer;

    public AuthController(IAuthorizer authorizer)
    {
        _authorizer = authorizer;
    }

    [HttpPost("login")]
    [SwaggerRequestExample(typeof(LoginDto), typeof(LoginRequestExamplev2))]
    public async Task<ActionResult> Login(LoginDto dto, CancellationToken token)
    {
        var authorization = await _authorizer.Authorize(dto.Username, dto.Password);
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

public record LoginDto(string Username, string Password);