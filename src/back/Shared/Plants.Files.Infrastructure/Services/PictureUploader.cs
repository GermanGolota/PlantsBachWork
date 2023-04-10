using Plants.Aggregates;
using Plants.Shared;
using SixLabors.ImageSharp.Formats.Jpeg;

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
        var finalContent = await GetFinalContent(file.Content);
        var fileId = file.Id;
        var location = new FileLocation("Images", fileId.ToString(), "jpeg");
        await _file.SaveAsync(new(location, finalContent), token);
        return new Picture(fileId, _file.GetUrl(location));
    }

    private static async Task<byte[]> GetFinalContent(byte[] bytes)
    {
        using (var image = Image.Load(bytes))
        {
            const int width = 512;
            const int height = 512;
            image.Mutate(x => x.Resize(width, height));

            using (var ms = new MemoryStream())
            {
                image.Save(ms, new JpegEncoder());
                return await ms.ReadAllBytesAsync();
            }
        }
    }
}
