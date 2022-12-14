using Plants.Aggregates.Services;

namespace Plants.Aggregates.Users;

public class ChangeOwnPasswordCommandHandler : ICommandHandler<ChangeOwnPasswordCommand>
{
    private readonly IUserUpdater _userUpdater;
    private readonly IProjectionQueryService<User> _query;
    private readonly IIdentityProvider _identity;

    public ChangeOwnPasswordCommandHandler(IUserUpdater userUpdater, IProjectionQueryService<User> query, IIdentityProvider identity)
    {
        _userUpdater = userUpdater;
        _query = query;
        _identity = identity;
    }

    public async Task<CommandForbidden?> ShouldForbidAsync(ChangeOwnPasswordCommand command, IUserIdentity userIdentity)
    {
        var passwordForbid = (command.OldPassword == command.NewPassword).ToForbidden("Can't change password to the same one");
        var user = await _query.GetByIdAsync(command.Metadata.Aggregate.Id);
        var loginForbid = (user.Login == userIdentity.UserName).ToForbidden("You cannot change someone elses password");
        return passwordForbid.And(loginForbid);
    }

    public async Task<IEnumerable<Event>> HandleAsync(ChangeOwnPasswordCommand command)
    {
        var identity = _identity.Identity;
        await _userUpdater.UpdatePassword(identity.UserName, command.OldPassword, command.NewPassword);
        var user = await _query.GetByIdAsync(command.Metadata.Aggregate.Id);
        return new[]
        {
            new PasswordChangedEvent(EventFactory.Shared.Create<PasswordChangedEvent>(command, user.Version + 1))
        };
    }

}
