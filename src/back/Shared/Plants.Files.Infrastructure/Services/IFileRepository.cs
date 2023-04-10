namespace Plants.Files;

public interface IFileRepository
{
    Task<FileLocation> SaveAsync(FileDto file, CancellationToken token = default);
    string GetUrl(FileLocation location);
}

public record FileDto(FileLocation Location, byte[] Content);
public record FileLocation(string Path, string FileName, string FileExtension);