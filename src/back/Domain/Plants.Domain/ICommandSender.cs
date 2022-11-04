namespace Plants.Domain;

public interface ICommandSender
{
    Task SendCommandAsync(Command command);
}