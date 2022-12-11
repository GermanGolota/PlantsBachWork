using Plants.Aggregates.Services;
using Plants.Shared;

namespace Plants.Aggregates.User;

internal class ChangeRoleCommandHandler : ICommandHandler<ChangeRoleCommand>
{
    private readonly IUserUpdater _updater;
    private readonly IProjectionQueryService<User> _userRepo;

    public ChangeRoleCommandHandler(IUserUpdater updater, IProjectionQueryService<User> userRepo)
    {
        _updater = updater;
        _userRepo = userRepo;
    }

    public Task<CommandForbidden?> ShouldForbidAsync(ChangeRoleCommand command, IUserIdentity identity) =>
         identity.HasRole(command.Role).ToResultTask();

    public async Task<IEnumerable<Event>> HandleAsync(ChangeRoleCommand command)
    {
        var user = await _userRepo.GetByIdAsync(command.Metadata.Aggregate.Id);
        await _updater.Change(user.Login, command.Role);
        return new[]
        {
            new RoleChangedEvent(EventFactory.Shared.Create<RoleChangedEvent>(command, user.Version + 1), command.Role)
        };
    }

}
