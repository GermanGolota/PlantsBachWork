namespace Plants.Domain;

public interface ICommandSender
{
    Task<IEnumerable<Event>> SendCommandAsync(Command command);
}