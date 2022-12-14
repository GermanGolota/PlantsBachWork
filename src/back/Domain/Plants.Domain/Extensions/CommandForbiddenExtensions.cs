namespace Plants.Domain.Extensions;

public static class CommandForbiddenExtensions
{
    public static CommandForbidden? And(this CommandForbidden? forbidden, Func<CommandForbidden?> forbidChain) =>
        forbidden.And(forbidChain());

    public static CommandForbidden? And(this CommandForbidden? forbidden, CommandForbidden? forbiddenSecond) =>
      forbidden ?? forbiddenSecond;

    public static CommandForbidden? ToForbidden(this bool isValid, string forbiddenReason) =>
        isValid switch
        {
            true => null,
            false => new CommandForbidden(forbiddenReason)
        };
}
