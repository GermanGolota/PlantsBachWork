namespace Plants.Domain;

public static class CommandForbiddenExtensions
{
    public static CommandForbidden? Or(this CommandForbidden? forbidden, CommandForbidden? forbiddenSecond) =>
        forbidden is not null && forbiddenSecond is not null
            ? new CommandForbidden(forbidden.Value.Reasons.Union(forbiddenSecond.Value.Reasons).ToArray())
            : null;

    public static CommandForbidden? And(this CommandForbidden? forbidden, Func<CommandForbidden?> forbidChain) =>
        forbidden.And(forbidChain());

    public static CommandForbidden? And(this CommandForbidden? forbidden, CommandForbidden? forbiddenSecond) =>
      forbidden ?? forbiddenSecond;

    public static CommandForbidden? ToForbidden(this bool success, string forbiddenReason) =>
        success 
            ? null 
            : new CommandForbidden(forbiddenReason);
}
