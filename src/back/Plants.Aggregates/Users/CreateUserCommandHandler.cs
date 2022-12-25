using Plants.Aggregates.Services;
using Plants.Services;
using Plants.Shared;

namespace Plants.Aggregates.Users;

internal class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IProjectionQueryService<User> _userQuery;
    private readonly IEmailer _emailer;
    private readonly IUserUpdater _changer;
    private readonly TempPasswordContext _context;

    public CreateUserCommandHandler(IProjectionQueryService<User> userQuery, IEmailer emailer, IUserUpdater changer, TempPasswordContext context)
    {
        _userQuery = userQuery;
        _emailer = emailer;
        _changer = changer;
        _context = context;
    }

    public async Task<CommandForbidden?> ShouldForbidAsync(CreateUserCommand command, IUserIdentity userIdentity) =>
        userIdentity.HasRoles(UserCheckType.All, command.Data.Roles)
        .And(command.Data.Roles.Any().ToForbidden("Has to have some roles"))
        .And(await UserDontExist(command));

    private async Task<CommandForbidden?> UserDontExist(CreateUserCommand command)
    {
        return await _userQuery.Exists(command.Metadata.Id) switch
        {
            true => new CommandForbidden("User already exists"),
            false => null
        };
    }

    public async Task<IEnumerable<Event>> HandleAsync(CreateUserCommand command)
    {
        const int TempPasswordLength = 8;
        var user = command.Data;
        var tempPassword = StringHelper.GetRandomAlphanumericString(TempPasswordLength);
        _context.TempPassword = tempPassword;
        var lang = user.Language ?? "English";
        await _emailer.SendInvitationEmail(user.Email, user.Login, tempPassword, lang);
        await _changer.Create(user.Login, tempPassword, $"{user.FirstName} {user.LastName}", user.Roles);
        var metadata = EventFactory.Shared.Create<UserCreatedEvent>(command) with { Id = user.Login.ToGuid() };
        return new[]
        {
            new UserCreatedEvent(metadata, user)
        };
    }
}

public class TempPasswordContext
{
    public string TempPassword { get; set; }
}