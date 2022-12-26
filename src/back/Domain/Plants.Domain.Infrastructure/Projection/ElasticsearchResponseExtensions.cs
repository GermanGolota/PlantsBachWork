using Elastic.Transport.Products.Elasticsearch;
using Microsoft.Extensions.Logging;

namespace Plants.Domain.Infrastructure.Projection;

internal static class ElasticsearchResponseExtensions
{
    public static void Process(this ElasticsearchResponse response, ILogger logger, string aggregateName, string operationName)
    {
        if (response.IsValidResponse is false)
        {
            foreach (var warning in response.ElasticsearchWarnings)
            {
                logger.LogWarning("Elastic search warning - '{warning}' during '{op}'", warning, operationName);
            }

            logger.LogError("Elastic error for '{aggName}' with info - '{info}' during '{op}'", aggregateName, response.ApiCallDetails, operationName);

            throw new Exception($"Failed to '{operationName}' '{aggregateName}'");
        }
        else
        {
            logger.LogInformation("Sucessfully performed '{op}' for '{agg}'", operationName, aggregateName);
        }
    }
}
