using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.Search;
using Plants.Aggregates.Users;
using Plants.Application.Commands;
using Plants.Application.Requests;
using Plants.Services.Infrastructure.Encryption;
using Plants.Shared.Model;

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
    public async Task<ActionResult<CreateUserResult>> CreateUser(
        [FromBody] Plants.Application.Commands.CreateUserCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpPost("changePass")]
    public async Task<ActionResult<ChangePasswordResult>> ChangePassword([FromBody] PasswordChangeDto password)
    {
        return await _mediator.Send(new Plants.Application.Commands.ChangePasswordCommand(password.Password));
    }
}

public record PasswordChangeDto(string Password);

[ApiController]
[Route("v2/users")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class UserControllerV2 : ControllerBase
{
    private readonly CommandHelper _command;
    private readonly SymmetricEncrypter _encrypter;
    private readonly ISearchQueryService<User, UserSearchParams> _search;

    public UserControllerV2(CommandHelper command, SymmetricEncrypter encrypter, ISearchQueryService<User, UserSearchParams> search)
    {
        _command = command;
        _encrypter = encrypter;
        _search = search;
    }

    [HttpGet("")]
    public async Task<ActionResult<FindUsersResult>> Search(
       [FromQuery] string? name, [FromQuery] string? phone, [FromQuery] UserRole[]? roles, CancellationToken token)
    {
        var currentUserRoles = _command.IdentityProvider.Identity!.Roles;
        var allRoles = Enum.GetValues<UserRole>();
        var rolesToFetch = currentUserRoles.Intersect(roles ?? allRoles).ToArray();
        var results = await _search.SearchAsync(new(name, phone, roles), new SearchAll(), token);
        return new FindUsersResult(
            results.Select(user => new FindUsersResultItem(user.FullName, user.PhoneNumber, user.Login)
            {
                RoleCodes = user.Roles
            }).ToList()
            );
    }

    [HttpPost("{login}/add/{role}")]
    public async Task<ActionResult<AlterRoleResult>> AddRole(
       [FromRoute] string login, [FromRoute] UserRole role, CancellationToken token)
    {
        return await ChangeRole(login, role, token);
    }

    private async Task<ActionResult<AlterRoleResult>> ChangeRole(string login, UserRole role, CancellationToken token = default)
    {
        var result = await _command.CreateAndSendAsync(
                    factory => factory.Create<ChangeRoleCommand>(new(login.ToGuid(), nameof(User))),
                    meta => new ChangeRoleCommand(meta, role),
                    token
                    );
        return new AlterRoleResult(result.IsFirst());
    }

    [HttpPost("{login}/remove/{role}")]
    public async Task<ActionResult<AlterRoleResult>> RemoveRole(
       [FromRoute] string login, [FromRoute] UserRole role, CancellationToken token)
    {
        return await ChangeRole(login, role, token);
    }

    [HttpPost("create")]
    public async Task<ActionResult<CreateUserResult>> CreateUser(
        [FromBody] Plants.Application.Commands.CreateUserCommand command, CancellationToken token = default)
    {
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<Plants.Aggregates.Users.CreateUserCommand>(new(command.Login.ToGuid(), nameof(User))),
            meta => new Plants.Aggregates.Users.CreateUserCommand(meta, new(command.FirstName, command.LastName, command.PhoneNumber, command.Login, command.Email, command.Language, command.Roles.ToArray())),
            token
            );
        return result.Match<CreateUserResult>(succ => new(true, "Sucessfull"),
            fail => new(false, String.Join('\n', fail.Reasons)));
    }

    [HttpPost("changePass")]
    public async Task<ActionResult<ChangePasswordResult>> ChangePassword([FromBody] PasswordChangeDto password, CancellationToken token)
    {
        var oldPassword = _encrypter.Decrypt(_command.IdentityProvider.Identity!.Hash);
        var result = await _command.CreateAndSendAsync(
            (factory, identity) => factory.Create<ChangeOwnPasswordCommand>(new(identity.UserName.ToGuid(), nameof(User))),
            (meta, identity) => new ChangeOwnPasswordCommand(meta, oldPassword, password.Password),
            token
            );
        return result.Match<ChangePasswordResult>(_ => new(),
            fail => new(String.Join('\n', fail.Reasons)));
    }

}
