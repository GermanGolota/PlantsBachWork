namespace Plants.Presentation;

public record CommandViewResult(bool Success, string Message);

public static class CommandViewResultExtensions
{
    public static CommandViewResult ToCommandResult(this OneOf<CommandAcceptedResult, CommandForbidden> cmdResult, string? successMessage = null)
    {
        successMessage ??= "Successfull";

        return cmdResult.Match(_ => new CommandViewResult(true, successMessage), fail => new CommandViewResult(false, string.Join('\n', fail.Reasons)));
    }
}