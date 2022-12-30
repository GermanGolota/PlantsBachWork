using Plants.Aggregates.Services;

namespace Plants.Aggregates.Users;

internal class ChangeRoleCommandHandler : ICommandHandler<ChangeRoleCommand>
{
    private readonly IUserUpdater _updater;
    private readonly IRepository<User> _userRepo;
    private User _user;

    public ChangeRoleCommandHandler(IUserUpdater updater, IRepository<User> userRepo)
    {
        _updater = updater;
        _userRepo = userRepo;
    }

    public async Task<CommandForbidden?> ShouldForbidAsync(ChangeRoleCommand command, IUserIdentity identity)
    {
        var result = identity.HasRole(command.Role);
        _user = await _userRepo.GetByIdAsync(command.Metadata.Aggregate.Id);
        return result.And((_user.Roles.Length != 1 || _user.Roles[0] != command.Role).ToForbidden("Cannot remove last role of the user"));
    }

    public async Task<IEnumerable<Event>> HandleAsync(ChangeRoleCommand command)
    {
        _user ??= await _userRepo.GetByIdAsync(command.Metadata.Aggregate.Id);
        await _updater.ChangeRoleAsync(_user.Login, $"{_user.FirstName} {_user.LastName}", _user.Roles, command.Role);
        return new[]
        {
            new RoleChangedEvent(EventFactory.Shared.Create<RoleChangedEvent>(command), command.Role)
        };
    }

}
