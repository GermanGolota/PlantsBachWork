namespace Plants.Aggregates;

internal class ChangeRoleCommandHandler : ICommandHandler<ChangeRoleCommand>
{
    private readonly IUserUpdater _updater;
    private readonly IQueryService<User> _userRepo;
    private User _user;

    public ChangeRoleCommandHandler(IUserUpdater updater, IQueryService<User> userRepo)
    {
        _updater = updater;
        _userRepo = userRepo;
    }

    public async Task<CommandForbidden?> ShouldForbidAsync(ChangeRoleCommand command, IUserIdentity identity, CancellationToken token = default)
    {
        var result = identity.HasRole(command.Role);
        _user = await _userRepo.GetByIdAsync(command.Metadata.Aggregate.Id, token: token);
        return result.And((_user.Roles.Length != 1 || _user.Roles[0] != command.Role).ToForbidden("Cannot remove last role of the user"));
    }

    public async Task<IEnumerable<Event>> HandleAsync(ChangeRoleCommand command, CancellationToken token = default)
    {
        _user ??= await _userRepo.GetByIdAsync(command.Metadata.Aggregate.Id, token: token);
        await _updater.ChangeRoleAsync(_user.Login, _user.FullName, _user.Roles, command.Role, token);
        return new[]
        {
            new RoleChangedEvent(EventFactory.Shared.Create<RoleChangedEvent>(command), command.Role)
        };
    }

}
