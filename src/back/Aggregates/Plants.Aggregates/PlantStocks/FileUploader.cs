namespace Plants.Aggregates;

public class FileUploader
{
    private readonly IFileRepository _file;

    private const string _plantImageDirectory = "PlantImages";
    private const string _coverImageDirectory = "InstructionCoverImages";

    public FileUploader(IFileRepository file)
    {
        _file = file;
    }

    public async Task<Picture[]> UploadPlantAsync(Guid stockId, byte[][] images, CancellationToken token = default) =>
        await Task.WhenAll(images.Select(picture => UploadPlantImageAsync(stockId, picture, token)));

    private async Task<Picture> UploadPlantImageAsync(Guid stockId, byte[] picture, CancellationToken token = default)
    {
        var pictureId = Guid.NewGuid();
        var location = await _file.SaveAsync(new(GetNewFileLocation(stockId, _plantImageDirectory, pictureId), picture), token);
        return new Picture(pictureId, _file.GetUrl(location));
    }

    public async Task<string> UploadIntructionCoverAsync(Guid instructionId, byte[] image, CancellationToken token = default)
    {
        var location = await _file.SaveAsync(new(GetNewFileLocation(instructionId, _coverImageDirectory, Guid.NewGuid()), image), token);
        return _file.GetUrl(location);
    }

    private FileLocation GetNewFileLocation(Guid id, string baseDir, Guid pictureId) =>
        new(Path.Combine(baseDir, id.ToString()), pictureId.ToString(), "jpeg");
}
