namespace Plants.Presentation;

public record ResultDto(bool Success, string Message);

public static class ResultDtoExtensions
{
    public static ResultDto ToResult(this OneOf<CommandAcceptedResult, CommandForbidden> cmdResult, string? successMessage = null)
    {
        successMessage ??= "Successfull";

        return cmdResult.Match(_ => new ResultDto(true, successMessage), fail => new ResultDto(false, string.Join('\n', fail.Reasons)));
    }
}