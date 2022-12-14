using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.Users;
using Plants.Core;
using Plants.Domain;
using Plants.Domain.Projection;
using Plants.Domain.Services;
using Plants.Presentation.Dtos;
using Plants.Shared;

namespace Plants.Presentation.Controllers.v2;

[ApiController]
[Route("v2/users")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class UserController : ControllerBase
{
    private readonly ICommandSender _sender;
    private readonly IProjectionQueryService<User> _query;
    private readonly IIdentityProvider _identity;
    private readonly CommandMetadataFactory _metadataFactory;

    public UserController(ICommandSender sender, IProjectionQueryService<User> query, IIdentityProvider identity, CommandMetadataFactory metadataFactory)
    {
        _sender = sender;
        _query = query;
        _identity = identity;
        _metadataFactory = metadataFactory;
    }

    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<UserDto>>> Search(
        [FromQuery] string? name, [FromQuery] string? phone, [FromQuery] UserRole[]? roles)
    {
        var currentUserRoles = _identity.Identity.Roles;
        var allRoles = Enum.GetValues<UserRole>();
        var rolesToFetch = currentUserRoles.Intersect(roles ?? allRoles).ToList();
        //TODO: Abstract away
        var usersDb = (await ((name, phone) switch
        {
            (null, null) => _query.FindAllAsync(_ => true),
            (null, var number) => _query.FindAllAsync(user => user.PhoneNumber == number),
            (var aName, null) => _query.FindAllAsync(user => user.FirstName.Contains(aName) || user.LastName.Contains(aName)),
            (var aName, var number) => _query.FindAllAsync(user => (user.FirstName.Contains(aName) || user.LastName.Contains(aName)) && user.PhoneNumber == number)
        })).ToList();
        return usersDb.Where(x => x.Roles.All(role => rolesToFetch.Contains(role)))
            .Select(user => new UserDto($"{user.FirstName} {user.LastName}", user.PhoneNumber, user.Roles)).ToList();
    }

    [HttpPost("{login}/change/{role}")]
    public async Task<ActionResult<ResultDto>> AddRole(
       [FromRoute] string login, [FromRoute] UserRole role)
    {
        var meta = _metadataFactory.Create<ChangeRoleCommand>(new(login.ToGuid(), nameof(User)));
        var command = new ChangeRoleCommand(meta, role);
        var cmdResult = await _sender.SendCommandAsync(command);
        return cmdResult.ToResult();
    }

    [HttpPost("create")]
    public async Task<ActionResult<ResultDto>> CreateUser(
        [FromBody] UserCreationDto user)
    {
        var meta = _metadataFactory.Create<CreateUserCommand>(new(user.Login.ToGuid(), nameof(User)));
        var command = new CreateUserCommand(meta, user);
        var result = await _sender.SendCommandAsync(command);
        return result.ToResult();
    }

    [HttpPost("changePass")]
    public async Task<ActionResult<ResultDto>> ChangePassword([FromBody] PasswordChangeDto password)
    {
        var identity = _identity.Identity;
        var meta = _metadataFactory.Create<ChangeOwnPasswordCommand>(new(identity.UserName.ToGuid(), nameof(User)));
        var command = new ChangeOwnPasswordCommand(meta, password.OldPassword, password.NewPassword);
        var result = await _sender.SendCommandAsync(command);
        return result.ToResult();
    }
}

public record PasswordChangeDto(string OldPassword, string NewPassword);

public record UserDto(string FullName, string PhoneNumber, UserRole[] Roles);
