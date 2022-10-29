namespace Plants.Domain;

public interface ICommandHandler<T> where T : Command
{
    Task<IEnumerable<Event>> HandleAsync(T command);
}

public interface IDomainCommandHandler<T> where T : Command
{
    IEnumerable<Event> Handle(T command);
}