namespace Plants.Domain;

public interface ICommandSender
{
    Task<OneOf<CommandAcceptedResult, CommandForbidden>> SendCommandAsync(Command command, CommandExecutionOptions options, CancellationToken token = default);
}

public record struct CommandAcceptedResult(CommandDescription Command);
public record struct CommandDescription(Guid Id, string Name, DateTime StartedTime, AggregateDescription Aggregate);
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
    public record Notify(string Username) : CommandExecutionOptions;
}