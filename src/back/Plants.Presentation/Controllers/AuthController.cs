using IdentityModel.OidcClient;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation;

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

    public record LoginCommand(string Login, string Password) : IRequest<LoginResult>;

    [HttpPost("login")]
    [SwaggerRequestExample(typeof(LoginCommand), typeof(LoginRequestExampleV2))]
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
