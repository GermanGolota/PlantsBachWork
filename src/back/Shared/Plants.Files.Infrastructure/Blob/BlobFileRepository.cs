using Azure.Storage.Blobs;

namespace Plants.Files.Infrastructure;

internal sealed class BlobFileRepository : IFileRepository
{
    private readonly BlobContainerClient _client;

    public BlobFileRepository(CloudBlobClientFactory clientFactory)
    {
        _client = clientFactory.Create();
    }

    public string GetUrl(FileLocation location)
    {
        var blob = _client.GetBlobClient(location.GetBlobPath());
        return blob.Uri.ToString();
    }

    public async Task<FileLocation> SaveAsync(FileDto file, CancellationToken token = default)
    {
        var blob = _client.GetBlobClient(file.Location.GetBlobPath());
        await blob.UploadAsync(new BinaryData(file.Content), token);
        return file.Location;
    }
}

internal static class FileLocationExtensions
{
    public static string GetBlobPath(this FileLocation locaton) =>
        Path.ChangeExtension(Path.Combine(locaton.Path, locaton.FileName), locaton.FileExtension);
}
