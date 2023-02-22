namespace Plants.Aggregates;

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

    public async Task<CommandForbidden?> ShouldForbidAsync(CreateUserCommand command, IUserIdentity userIdentity, CancellationToken token = default) =>
        (userIdentity.Roles.Any(_ => (int)_ >= command.Data.Roles.Max(_ => (int)_)))
        .ToForbidden("Cannot create user that would have more access than you do")
        .And(command.Data.Roles.Any().ToForbidden("Has to have some roles"))
        .And(await UserDontExistAsync(command, token));

    private async Task<CommandForbidden?> UserDontExistAsync(CreateUserCommand command, CancellationToken token = default)
    {
        return await _userQuery.ExistsAsync(command.Metadata.Id, token) switch
        {
            true => new CommandForbidden("User already exists"),
            false => null
        };
    }

    public async Task<IEnumerable<Event>> HandleAsync(CreateUserCommand command, CancellationToken token = default)
    {
        var tempPassword = GetTempPassword();
        var user = command.Data;
        var lang = user.Language ?? "English";
        await _emailer.SendInvitationEmailAsync(user.Email, user.Login, tempPassword, lang, token);
        await _changer.CreateAsync(user.Login, tempPassword, $"{user.FirstName} {user.LastName}", user.Roles, token);
        var metadata = EventFactory.Shared.Create<UserCreatedEvent>(command) with { Id = user.Login.ToGuid() };
        return new[]
        {
            new UserCreatedEvent(metadata, user)
        };
    }

    private string GetTempPassword()
    {
        const int TempPasswordLength = 12;
        var tempPassword = StringHelper.GetRandomAlphanumericString(TempPasswordLength);
        var iterationCount = 0;
        while (UserPasswordValidator.Validate(tempPassword) is not null && iterationCount < 100)
        {
            tempPassword = StringHelper.GetRandomAlphanumericString(TempPasswordLength);
        }
        _context.TempPassword = tempPassword;
        if (UserPasswordValidator.Validate(tempPassword) is not null)
        {
            throw new Exception("Cannot create user, please try again later");
        }

        return tempPassword;
    }
}

public class TempPasswordContext
{
    public string TempPassword { get; set; }
}