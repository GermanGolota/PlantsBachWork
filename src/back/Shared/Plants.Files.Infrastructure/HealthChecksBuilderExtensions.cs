using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plants.Domain.Infrastructure;

namespace Plants.Files.Infrastructure;

public static class HealthChecksBuilderExtensions
{
    public static IHealthChecksBuilder AddFilesHealthChecks(this IHealthChecksBuilder builder, IConfiguration configuration)
    {
        var blobSettings = configuration.GetSection(ConnectionConfig.Section).Get<ConnectionConfig>()!.Blob;
        builder.AddAzureBlobStorage(blobSettings.Template, blobSettings.ContainerName);

        return builder;
    }
}
