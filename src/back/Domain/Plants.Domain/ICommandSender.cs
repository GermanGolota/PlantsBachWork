namespace Plants.Domain;

public interface ICommandSender
{
    Task<OneOf<CommandAcceptedResult, CommandForbidden>> SendCommandAsync(Command command, CancellationToken token = default);
}

public record struct CommandAcceptedResult();
public record struct CommandForbidden(string[] Reasons)
{
	public CommandForbidden(string reason) : this(new[] {reason})
	{

	}
}