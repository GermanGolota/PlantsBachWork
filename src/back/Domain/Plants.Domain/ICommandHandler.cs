namespace Plants.Domain;

public interface ICommandHandler<T> where T : Command
{
    Task<CommandForbidden?> ShouldForbidAsync(T command);
    Task<IEnumerable<Event>> HandleAsync(T command);
}

public interface IDomainCommandHandler<T> where T : Command
{
    CommandForbidden? ShouldForbid(T command);
    IEnumerable<Event> Handle(T command);
}