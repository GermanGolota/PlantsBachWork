namespace Plants.Shared;

public static class TaskExtensions
{
    /// <returns>
    /// Value indicating success of execution:
    /// true - task executed
    /// false - timed out
    /// </returns>
    public static async Task<bool> ExecuteWithTimeoutAsync(this Task task, TimeSpan timeout, CancellationToken token)
    {
        var timeoutTask = Task.Delay(timeout, token);
        var resultingTask = await Task.WhenAny(timeoutTask, task);
        bool success;
        if (resultingTask == timeoutTask)
        {
            success = false;
        }
        else
        {
            success = true;
        }
        return success;
    }
}
