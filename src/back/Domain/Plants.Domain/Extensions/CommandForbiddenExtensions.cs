namespace Plants.Domain.Extensions;

public static class CommandForbiddenExtensions
{
    public static CommandForbidden? And(this CommandForbidden? forbidden, Func<CommandForbidden?> forbidChain) =>
        forbidden ?? forbidChain();
}
