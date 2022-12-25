using Plants.Aggregates.Services;

namespace Plants.Aggregates.Users;

internal class ChangeRoleCommandHandler : ICommandHandler<ChangeRoleCommand>
{
    private readonly IUserUpdater _updater;
    private readonly IRepository<User> _userRepo;

    public ChangeRoleCommandHandler(IUserUpdater updater, IRepository<User> userRepo)
    {
        _updater = updater;
        _userRepo = userRepo;
    }

    public Task<CommandForbidden?> ShouldForbidAsync(ChangeRoleCommand command, IUserIdentity identity) =>
         identity.HasRole(command.Role).ToResultTask();

    public async Task<IEnumerable<Event>> HandleAsync(ChangeRoleCommand command)
    {
        var user = await _userRepo.GetByIdAsync(command.Metadata.Aggregate.Id);
        await _updater.ChangeRole(user.Login, $"{user.FirstName} {user.LastName}", user.Roles, command.Role);
        return new[]
        {
            new RoleChangedEvent(EventFactory.Shared.Create<RoleChangedEvent>(command), command.Role)
        };
    }

}
