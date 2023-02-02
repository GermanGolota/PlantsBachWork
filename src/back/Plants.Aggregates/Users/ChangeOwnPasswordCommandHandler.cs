namespace Plants.Aggregates;

public class ChangeOwnPasswordCommandHandler : ICommandHandler<ChangeOwnPasswordCommand>
{
    private readonly IUserUpdater _userUpdater;
    private readonly IQueryService<User> _query;
    private readonly IIdentityProvider _identity;
    private readonly IIdentityHelper _identityHelper;

    public ChangeOwnPasswordCommandHandler(IUserUpdater userUpdater, IQueryService<User> query, IIdentityProvider identity, IIdentityHelper identityHelper)
    {
        _userUpdater = userUpdater;
        _query = query;
        _identity = identity;
        _identityHelper = identityHelper;
    }

    public async Task<CommandForbidden?> ShouldForbidAsync(ChangeOwnPasswordCommand command, IUserIdentity userIdentity, CancellationToken token = default)
    {
        var passwordForbid = (command.OldPassword != command.NewPassword).ToForbidden("Can't change password to the same one");
        var user = await _query.GetByIdAsync(command.Metadata.Aggregate.Id, token: token);
        var loginForbid = (user.Login.CompareTo(userIdentity.UserName) == 0).ToForbidden("You cannot change someone elses password");
        return passwordForbid.And(loginForbid).And(UserPasswordValidator.Validate(command.NewPassword));
    }

    public async Task<IEnumerable<Event>> HandleAsync(ChangeOwnPasswordCommand command, CancellationToken token = default)
    {
        var identity = _identity.Identity!;
        await _userUpdater.UpdatePasswordAsync(identity.UserName, command.OldPassword, command.NewPassword, token);
        var newIdentity = _identityHelper.Build(command.NewPassword, identity.UserName, identity.Roles);
        _identity.UpdateIdentity(newIdentity);
        var user = await _query.GetByIdAsync(command.Metadata.Aggregate.Id, token: token);
        return new[]
        {
            new PasswordChangedEvent(EventFactory.Shared.Create<PasswordChangedEvent>(command))
        };
    }

}
