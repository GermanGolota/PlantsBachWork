using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Plants.Domain.Infrastructure;

namespace Plants.Files.Infrastructure;

internal sealed class CloudBlobClientFactory
{
    private readonly BlobServiceConnection _settings;

    public CloudBlobClientFactory(IOptions<ConnectionConfig> settingsProvider) =>
        _settings = settingsProvider.Value.Blob;

    public BlobContainerClient Create() =>
        new(_settings.Template, _settings.ContainerName);
}