using Plants.Aggregates.Services;

namespace Plants.Aggregates.PlantStocks;

public class FileUploader
{
    private readonly IFileRepository _file;

    private const string _plantImageDirectory = "PlantImages";

    public FileUploader(IFileRepository file)
	{
        _file = file;
    }

    /// <returns>Urls for uploaded items</returns>
    public async Task<string[]> UploadAsync(Guid stockId, byte[][] images)
    {
        var files = await Task.WhenAll(images.Select(picture => _file.SaveAsync(new(GetNewFileLocation(stockId), picture))));
        return files.Select(_file.GetUrl).ToArray();
    }

    private FileLocation GetNewFileLocation(Guid id) =>
        new(Path.Combine(_plantImageDirectory, id.ToString()), Guid.NewGuid().ToString(), "jpeg");
}
