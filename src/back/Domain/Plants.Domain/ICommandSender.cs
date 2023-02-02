using Plants.Shared.Model;

namespace Plants.Domain;

public interface ICommandSender
{
    Task<OneOf<CommandAcceptedResult, CommandForbidden>> SendCommandAsync(Command command, CommandExecutionOptions options, CancellationToken token = default);
}

public record struct CommandAcceptedResult();
public record struct CommandForbidden(string[] Reasons)
{
    public CommandForbidden(string reason) : this(new[] { reason })
    {

    }
}

public abstract record CommandExecutionOptions
{
    public record NoWait() : CommandExecutionOptions;
    public record Wait(TimeSpan TimeToWait) : CommandExecutionOptions;
}