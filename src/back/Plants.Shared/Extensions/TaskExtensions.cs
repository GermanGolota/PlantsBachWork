using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants.Shared.Extensions;

public static class TaskExtensions
{
    /// <returns>
    /// Value indicating success of execution:
    /// true - task executed
    /// false - timed out
    /// </returns>
    public static async Task<bool> ExecuteWithTimeoutAsync(this Task task, TimeSpan timeout)
    {
        var timeoutTask = Task.Delay(timeout);
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
