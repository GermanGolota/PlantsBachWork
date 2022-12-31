using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.Search;
using Plants.Aggregates.Users;
using Plants.Presentation.Dtos;

namespace Plants.Presentation.Controllers.v2;

[ApiController]
[Route("v2/users")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class UserController : ControllerBase
{
    private readonly CommandHelper _command;
    private readonly ISearchQueryService<User, UserSearchParams> _search;

    public UserController(CommandHelper command, ISearchQueryService<User, UserSearchParams> search)
    {
        _command = command;
        _search = search;
    }

    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<UserDto>>> Search(
        [FromQuery] string? name, [FromQuery] string? phone, [FromQuery] UserRole[]? roles)
    {
        var currentUserRoles = _command.IdentityProvider.Identity!.Roles;
        var allRoles = Enum.GetValues<UserRole>();
        var rolesToFetch = currentUserRoles.Intersect(roles ?? allRoles).ToArray();
        var results = await _search.SearchAsync(new(name, phone, roles), new SearchAll());
        return results.Select(user => new UserDto($"{user.FirstName} {user.LastName}", user.PhoneNumber, user.Login, user.Roles)).ToList();
    }

    [HttpPost("{login}/change/{role}")]
    public async Task<ActionResult<ResultDto>> AddRole(
       [FromRoute] string login, [FromRoute] UserRole role) =>
        (await _command.CreateAndSendAsync(
            factory => factory.Create<ChangeRoleCommand>(new(login.ToGuid(), nameof(User))),
            meta => new ChangeRoleCommand(meta, role)
            )).ToResult();

    [HttpPost("create")]
    public async Task<ActionResult<ResultDto>> CreateUser(
        [FromBody] UserCreationDto user) =>
        (await _command.CreateAndSendAsync(
            factory => factory.Create<CreateUserCommand>(new(user.Login.ToGuid(), nameof(User))),
            meta => new CreateUserCommand(meta, user)
            )).ToResult();

    [HttpPost("changePass")]
    public async Task<ActionResult<ResultDto>> ChangePassword([FromBody] PasswordChangeDto password) =>
        (await _command.CreateAndSendAsync(
            (factory, identity) => factory.Create<ChangeOwnPasswordCommand>(new(identity.UserName.ToGuid(), nameof(User))),
            (meta, identity) => new ChangeOwnPasswordCommand(meta, password.OldPassword, password.NewPassword)
            )).ToResult();
}

public record PasswordChangeDto(string OldPassword, string NewPassword);

public record UserDto(string FullName, string PhoneNumber, string Login, UserRole[] Roles);
