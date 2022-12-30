namespace Plants.Aggregates.Services;

public interface IFileRepository
{
    Task<FileLocation> SaveAsync(FileDto file);
    Task<FileDto> GetAsync(FileLocation location);
    string GetUrl(FileLocation location);
}

public record FileDto(FileLocation Location, byte[] Content);
public record FileLocation(string Path, string FileName, string FileExtension);