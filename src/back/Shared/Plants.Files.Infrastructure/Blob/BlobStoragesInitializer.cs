using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Plants.Files.Infrastructure;

internal sealed class BlobStoragesInitializer : IBlobStoragesInitializer
{
    private readonly BlobContainerClient _blobClient;

    public BlobStoragesInitializer(CloudBlobClientFactory blobClientFactory) =>
        _blobClient = blobClientFactory.Create();

    public async Task Initialize(CancellationToken token)
    {
        var result = await _blobClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: token);
        // TODO: Actually look at result
    }
}