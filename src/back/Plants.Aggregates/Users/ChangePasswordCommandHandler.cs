using Plants.Aggregates.Services;

namespace Plants.Aggregates.Users;

internal class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IUserUpdater _userUpdater;
    private readonly IRepository<User> _repo;

    public ChangePasswordCommandHandler(IUserUpdater userUpdater, IRepository<User> projection)
    {
        _userUpdater = userUpdater;
        _repo = projection;
    }

    public Task<CommandForbidden?> ShouldForbidAsync(ChangePasswordCommand command, IUserIdentity userIdentity) =>
        userIdentity.HasRole(UserRole.Manager).And(UserPasswordValidator.Validate(command.NewPassword)).ToResultTask();

    public async Task<IEnumerable<Event>> HandleAsync(ChangePasswordCommand command)
    {
        await _userUpdater.UpdatePasswordAsync(command.Login, command.OldPassword, command.NewPassword);
        var user = await _repo.GetByIdAsync(command.Metadata.Aggregate.Id);
        return new[]
        {
            new PasswordChangedEvent(EventFactory.Shared.Create<PasswordChangedEvent>(command))
        };
    }

}
