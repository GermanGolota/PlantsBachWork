namespace Plants.Domain;

public interface ICommandHandler<T> where T : Command
{
    Task<CommandForbidden?> ShouldForbidAsync(T command, IUserIdentity userIdentity, CancellationToken token = default);
    Task<IEnumerable<Event>> HandleAsync(T command, CancellationToken token = default);
}

public interface IDomainCommandHandler<T> where T : Command
{
    CommandForbidden? ShouldForbid(T command, IUserIdentity userIdentity);
    IEnumerable<Event> Handle(T command);
}