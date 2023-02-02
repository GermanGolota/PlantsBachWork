using Plants.Aggregates.Abstractions;
using Plants.Shared.Model;

namespace Plants.Aggregates.Users;

internal class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IUserUpdater _userUpdater;
    private readonly IQueryService<User> _repo;

    public ChangePasswordCommandHandler(IUserUpdater userUpdater, IQueryService<User> projection)
    {
        _userUpdater = userUpdater;
        _repo = projection;
    }

    public Task<CommandForbidden?> ShouldForbidAsync(ChangePasswordCommand command, IUserIdentity userIdentity, CancellationToken token = default) =>
        userIdentity.HasRole(UserRole.Manager).And(UserPasswordValidator.Validate(command.NewPassword)).ToResultTask();

    public async Task<IEnumerable<Event>> HandleAsync(ChangePasswordCommand command, CancellationToken token = default)
    {
        await _userUpdater.UpdatePasswordAsync(command.Login, command.OldPassword, command.NewPassword, token);
        var user = await _repo.GetByIdAsync(command.Metadata.Aggregate.Id, token: token);
        return new[]
        {
            new PasswordChangedEvent(EventFactory.Shared.Create<PasswordChangedEvent>(command))
        };
    }

}
