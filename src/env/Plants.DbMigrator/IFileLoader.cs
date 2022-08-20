using Microsoft.Extensions.Logging;

namespace Plants.DbMigrator;

public interface IFileLoader
{
    Task<List<string>> GetFullFileNames(string folder, string? pattern = null);
    Task<string> LoadFileAsync(string fullFileName);
}

public class FileLoader : IFileLoader
{
    public Task<List<string>> GetFullFileNames(string folder, string? pattern = null)
    {
        DirectoryInfo directoryInfo = new(folder);
        FileInfo[] files = pattern is null
            ? directoryInfo.GetFiles()
            : directoryInfo.GetFiles(pattern);
        var names = files.Select(x => x.FullName).ToList();
        return Task.FromResult(names);
    }

    public Task<string> LoadFileAsync(string fullFileName) => File.ReadAllTextAsync(fullFileName);
}

public class FileLoaderLoggingDecorator : IFileLoader
{
    private readonly IFileLoader _inner;
    private readonly ILogger<FileLoaderLoggingDecorator> _logger;

    public FileLoaderLoggingDecorator(IFileLoader inner, ILogger<FileLoaderLoggingDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<List<string>> GetFullFileNames(string folder, string? pattern = null)
    {
        var result = await _inner.GetFullFileNames(folder, pattern);
        _logger.LogInformation("Loaded '{cnt}' file names from '{fld}' folder for '{pattern}' pattern", result.Count, folder, pattern);
        return result;
    }

    public async Task<string> LoadFileAsync(string fullFileName)
    {
        var result = await _inner.LoadFileAsync(fullFileName);
        _logger.LogInformation("Loaded '{content}' content for '{name}' file", result, fullFileName);
        return result;
    }
}