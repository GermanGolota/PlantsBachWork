using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Plants.Aggregates.Infrastructure;

public static class HealthChecksBuilderExtensions
{
    public static IHealthChecksBuilder AddAggregatesHealthChecks(this IHealthChecksBuilder builder, IConfiguration configuration)
    {
        var blobSettings = configuration.GetSection(ConnectionConfig.Section).Get<ConnectionConfig>()!.Blob;
        builder.AddAzureBlobStorage(blobSettings.Template, blobSettings.ContainerName);

        return builder;
    }
}
