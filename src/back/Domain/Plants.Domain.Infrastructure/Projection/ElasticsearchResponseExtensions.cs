using Microsoft.Extensions.Logging;
using Nest;

namespace Plants.Domain.Infrastructure.Projection;

internal static class ElasticsearchResponseExtensions
{
    public static void Process(this IResponse response, ILogger logger, string aggregateName, string operationName)
    {
        if (response.IsValid is false)
        {
            logger.LogError(response.OriginalException, "Elastic error for '{aggName}' with info - '{info}' during '{op}'", aggregateName, response.DebugInformation, operationName);

            throw new Exception($"Failed to '{operationName}' '{aggregateName}'");
        }
        else
        {
            logger.LogInformation("Sucessfully performed '{op}' for '{agg}'", operationName, aggregateName);
        }
    }
}
