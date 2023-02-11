using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;


[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly CommandHelper _command;
    private readonly SymmetricEncrypter _encrypter;
    private readonly ISearchQueryService<User, UserSearchParams> _search;

    public UserController(CommandHelper command, SymmetricEncrypter encrypter, ISearchQueryService<User, UserSearchParams> search)
    {
        _command = command;
        _encrypter = encrypter;
        _search = search;
    }

    public record FindUsersResultItem(string FullName, string Mobile, string Login, UserRole[] RoleCodes);

    [HttpGet("")]
    public async Task<ActionResult<ListViewResult<FindUsersResultItem>>> Search(
       [FromQuery] string? name, [FromQuery] string? phone, [FromQuery] UserRole[]? roles, CancellationToken token)
    {
        var currentUserRoles = _command.IdentityProvider.Identity!.Roles;
        var allRoles = Enum.GetValues<UserRole>();
        var rolesToFetch = currentUserRoles.Intersect(roles ?? allRoles).ToArray();
        var results = await _search.SearchAsync(new(name, phone, roles), new SearchAll(), token);
        return new ListViewResult<FindUsersResultItem>(
            results.Select(user => new FindUsersResultItem(user.FullName, user.PhoneNumber, user.Login, user.Roles))
            );
    }

    [HttpPost("{login}/add/{role}")]
    public async Task<ActionResult<CommandViewResult>> AddRole(
       [FromRoute] string login, [FromRoute] UserRole role, CancellationToken token)
    {
        return await ChangeRole(login, role, token);
    }

    private async Task<ActionResult<CommandViewResult>> ChangeRole(string login, UserRole role, CancellationToken token = default)
    {
        var result = await _command.SendAndWaitAsync(
                    factory => factory.Create<ChangeRoleCommand>(new(login.ToGuid(), nameof(User))),
                    meta => new ChangeRoleCommand(meta, role),
                    token
                    );
        return result.ToCommandResult();
    }

    [HttpPost("{login}/remove/{role}")]
    public async Task<ActionResult<CommandViewResult>> RemoveRole(
       [FromRoute] string login, [FromRoute] UserRole role, CancellationToken token)
    {
        return await ChangeRole(login, role, token);
    }

    public record CreateUserViewRequest(string Login, List<UserRole> Roles, string Email, string? Language,
        string FirstName, string LastName, string PhoneNumber);

    [HttpPost("create")]
    public async Task<ActionResult<CommandViewResult>> CreateUser(
        [FromBody] CreateUserViewRequest command, CancellationToken token = default)
    {
        var result = await _command.SendAndWaitAsync(
            factory => factory.Create<CreateUserCommand>(new(command.Login.ToGuid(), nameof(User))),
            meta => new CreateUserCommand(meta, new(command.FirstName, command.LastName, command.PhoneNumber, command.Login, command.Email, command.Language, command.Roles.ToArray())),
            token
            );
        return result.ToCommandResult();
    }

    public record ChangePasswordViewRequest(string Password);

    [HttpPost("changePass")]
    public async Task<ActionResult<CommandViewResult>> ChangePassword([FromBody] ChangePasswordViewRequest password, CancellationToken token)
    {
        var oldPassword = _encrypter.Decrypt(_command.IdentityProvider.Identity!.Hash);
        var result = await _command.SendAndWaitAsync(
            (factory, identity) => factory.Create<ChangeOwnPasswordCommand>(new(identity.UserName.ToGuid(), nameof(User))),
            (meta, identity) => new ChangeOwnPasswordCommand(meta, oldPassword, password.Password),
            token
            );
        return result.ToCommandResult();
    }

}
