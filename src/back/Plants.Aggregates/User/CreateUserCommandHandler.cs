using Plants.Aggregates.Services;
using Plants.Shared;

namespace Plants.Aggregates.User;

internal class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IProjectionQueryService<User> _userQuery;
    private readonly IEmailer _emailer;
    private readonly IUserChanger _changer;

    public CreateUserCommandHandler(IProjectionQueryService<User> userQuery, IEmailer emailer, IUserChanger changer)
    {
        _userQuery = userQuery;
        _emailer = emailer;
        _changer = changer;
    }

    public async Task<CommandForbidden?> ShouldForbidAsync(CreateUserCommand command, IUserIdentity userIdentity) =>
        userIdentity.HasRoles(UserCheckType.All, command.Data.Roles)
        ?? await _userQuery.Exists(command.Metadata.Id) switch
        {
            true => new CommandForbidden("Plant already created"),
            false => null
        };

    public async Task<IEnumerable<Event>> HandleAsync(CreateUserCommand command)
    {
        const int TempPasswordLength = 8;
        var user = command.Data;
        var tempPassword = StringHelper.GetRandomAlphanumericString(TempPasswordLength);
        var lang = user.Language ?? "English";
        await _emailer.SendInvitationEmail(user.Email, user.Login, tempPassword, lang);
        await _changer.Create(user.Login, user.Roles);
        var metadata = EventFactory.Shared.Create<UserCreatedEvent>(command, 0) with { Id = user.Login.ToGuid() };
        return new[]
        {
            new UserCreatedEvent(metadata, user)
        };
    }
}
