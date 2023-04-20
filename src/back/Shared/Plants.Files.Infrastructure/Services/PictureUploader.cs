using Plants.Aggregates;

namespace Plants.Files.Infrastructure;

internal sealed class PictureUploader : IPictureUploader
{
    private readonly IFileRepository _file;

    public PictureUploader(IFileRepository file)
    {
        _file = file;
    }

    public async Task<Picture[]> UploadAsync(CancellationToken token, params FileView[] files)
    {
        var tasks = files.Select(file => UploadAsync(file, token)).ToArray();
        return await Task.WhenAll(tasks);
    }

    public async Task<Picture> UploadAsync(FileView file, CancellationToken token)
    {
        var finalContent = await GetFinalContent(file.Content, token);
        var fileId = file.Id;
        var location = new FileLocation("Images", fileId.ToString(), "jpeg");
        await _file.SaveAsync(new(location, finalContent), token);
        return new Picture(fileId, _file.GetUrl(location));
    }

    private static async Task<byte[]> GetFinalContent(byte[] bytes, CancellationToken token)
    {
        var path = GetRandomPath();

        using (var image = Image.Load(bytes))
        {
            const int width = 512;
            const int height = 512;
            image.Mutate(x => x.Resize(width, height));
            await image.SaveAsJpegAsync(path, token);
        }

        var finalBytes = await File.ReadAllBytesAsync(path, token);

        File.Delete(path);

        return finalBytes;
    }

    private static string GetRandomPath()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Images");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, $"{Guid.NewGuid()}.jpeg");
    }
}
