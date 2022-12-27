using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.Search;
using Plants.Aggregates.Users;
using Plants.Domain.Projection;
using Plants.Presentation.Dtos;

namespace Plants.Presentation.Controllers.v2;

[ApiController]
[Route("v2/users")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class UserController : ControllerBase
{
    private readonly ICommandSender _sender;
    private readonly IIdentityProvider _identity;
    private readonly CommandMetadataFactory _metadataFactory;
    private readonly ISearchQueryService<User, UserSearchParams> _search;

    public UserController(ICommandSender sender,  IIdentityProvider identity, CommandMetadataFactory metadataFactory, ISearchQueryService<User, UserSearchParams> search)
    {
        _sender = sender;
        _identity = identity;
        _metadataFactory = metadataFactory;
        _search = search;
    }

    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<UserDto>>> Search(
        [FromQuery] string? name, [FromQuery] string? phone, [FromQuery] UserRole[]? roles)
    {
        var currentUserRoles = _identity.Identity!.Roles;
        var allRoles = Enum.GetValues<UserRole>();
        var rolesToFetch = currentUserRoles.Intersect(roles ?? allRoles).ToArray();
        var results = await _search.Search(new(name, phone, roles), new SearchAll());
        return results.Select(user => new UserDto($"{user.FirstName} {user.LastName}", user.PhoneNumber, user.Login, user.Roles)).ToList();
    }

    private static bool CanBeSeen(UserRole[] roles, UserRole[] visibleRoles)
    {
        var result = true;
        foreach (var role in roles)
        {
            if (visibleRoles.Contains(role) is false && visibleRoles.All(visible => visible < role))
            {
                result = false;
                break;
            }
        }
        return result;
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
        var identity = _identity.Identity!;
        var meta = _metadataFactory.Create<ChangeOwnPasswordCommand>(new(identity.UserName.ToGuid(), nameof(User)));
        var command = new ChangeOwnPasswordCommand(meta, password.OldPassword, password.NewPassword);
        var result = await _sender.SendCommandAsync(command);
        return result.ToResult();
    }
}

public record PasswordChangeDto(string OldPassword, string NewPassword);

public record UserDto(string FullName, string PhoneNumber, string Login, UserRole[] Roles);
