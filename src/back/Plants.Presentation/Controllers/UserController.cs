using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

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

    public record FindUsersResult(List<FindUsersResultItem> Items);
    public record FindUsersResultItem(string FullName, string Mobile, string Login)
    {
        //for converter
        public FindUsersResultItem() : this("", "", "")
        {

        }
        private string[] roles;

        public string[] Roles
        {
            get => roles;
            set
            {
                roles = value;
                RoleCodes = value.Select(To).ToArray();

            }
        }

        private static UserRole To(string role)
        {
            return role switch
            {
                "consumer" => UserRole.Consumer,
                "producer" => UserRole.Producer,
                "manager" => UserRole.Manager,
                _ => throw new ArgumentException("Bad role name", role)
            };
        }

        public UserRole[] RoleCodes { get; set; }
    }

    public record AlterRoleResult(bool Successfull);

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
    public record CreateUserResult(bool Success, string Message);


    public record CreateUserCommandView(string Login, List<UserRole> Roles, string Email, string? Language,
        string FirstName, string LastName, string PhoneNumber);


    [HttpPost("create")]
    public async Task<ActionResult<CreateUserResult>> CreateUser(
        [FromBody] CreateUserCommandView command, CancellationToken token = default)
    {
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<Aggregates.CreateUserCommand>(new(command.Login.ToGuid(), nameof(User))),
            meta => new Plants.Aggregates.CreateUserCommand(meta, new(command.FirstName, command.LastName, command.PhoneNumber, command.Login, command.Email, command.Language, command.Roles.ToArray())),
            token
            );
        return result.Match<CreateUserResult>(succ => new(true, "Sucessfull"),
            fail => new(false, String.Join('\n', fail.Reasons)));
    }

    public record ChangePasswordResult(bool Success, string Message)
    {
        public ChangePasswordResult() : this(true, "")
        {

        }

        public ChangePasswordResult(string msg) : this(false, msg)
        {

        }
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
