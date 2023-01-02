using Plants.Aggregates.Services;

namespace Plants.Aggregates.PlantStocks;

public class FileUploader
{
    private readonly IFileRepository _file;

    private const string _plantImageDirectory = "PlantImages";
    private const string _coverImageDirectory = "InstructionCoverImages";

    public FileUploader(IFileRepository file)
	{
        _file = file;
    }

    /// <returns>Urls for uploaded items</returns>
    public async Task<string[]> UploadPlantAsync(Guid stockId, byte[][] images)
    {
        var files = await Task.WhenAll(images.Select(picture => _file.SaveAsync(new(GetNewFileLocation(stockId, _plantImageDirectory), picture))));
        return files.Select(_file.GetUrl).ToArray();
    }

    public async Task<string> UploadIntructionCover(Guid instructionId, byte[] image)
    {
        var location = await _file.SaveAsync(new(GetNewFileLocation(instructionId, _coverImageDirectory), image));
        return _file.GetUrl(location);
    }

    private FileLocation GetNewFileLocation(Guid id, string baseDir) =>
        new(Path.Combine(baseDir, id.ToString()), Guid.NewGuid().ToString(), "jpeg");
}
