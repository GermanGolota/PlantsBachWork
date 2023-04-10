using Microsoft.Extensions.DependencyInjection;

namespace Plants.Files.Infrastructure;

public static class DiExtensions
{
    public static IServiceCollection AddFilesServices(this IServiceCollection services)
    {
        services.AddSingleton<IBlobStoragesInitializer, BlobStoragesInitializer>();
        services.AddSingleton<CloudBlobClientFactory>();
        services.AddSingleton<IFileRepository, BlobFileRepository>();
        services.AddHostedService<BlobStoragesInitializationHostedService>();

        services.AddSingleton<IPictureUploader, PictureUploader>();

        return services;
    }
}
