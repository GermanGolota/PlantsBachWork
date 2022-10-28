namespace Plants.Domain;

public interface ICommandSender
{
    Task SendExternalCommandAsync(Command command);
    void SendLocalCommand(Command command);
}