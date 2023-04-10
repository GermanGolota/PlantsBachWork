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
        var fileId = file.Id;
        var location = new FileLocation("Images", fileId.ToString(), "jpeg");
        await _file.SaveAsync(new(location, file.Content), token);
        return new Picture(fileId, _file.GetUrl(location));
    }
}
