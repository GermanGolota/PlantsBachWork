using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Commands;
using Plants.Application.Requests;
using Plants.Core;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("users")]
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "v1")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<ActionResult<FindUsersResult>> Search(
        [FromQuery] string? name, [FromQuery] string? phone, [FromQuery] UserRole[]? roles)
    {
        return await _mediator.Send(new FindUsersRequest(name, phone, roles));
    }

    [HttpPost("{login}/add/{role}")]
    public async Task<ActionResult<AlterRoleResult>> AddRole(
       [FromRoute] string login, [FromRoute] UserRole role)
    {
        return await _mediator.Send(new AlterRoleCommand(login, role, AlterType.Add));
    }

    [HttpPost("{login}/remove/{role}")]
    public async Task<ActionResult<AlterRoleResult>> RemoveRole(
       [FromRoute] string login, [FromRoute] UserRole role)
    {
        return await _mediator.Send(new AlterRoleCommand(login, role, AlterType.Remove));
    }

    [HttpPost("create")]
    public async Task<ActionResult<CreateUserResult>> RemoveRole(
        [FromBody] CreateUserCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpPost("changePass")]
    public async Task<ActionResult<ChangePasswordResult>> ChangePassword([FromBody] PasswordChangeDto password)
    {
        return await _mediator.Send(new ChangePasswordCommand(password.Password));
    }
}

public record PasswordChangeDto(string Password);
