using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Plants.Aggregates.Infrastructure.Abstractions;
using Plants.Aggregates.Services;

namespace Plants.Aggregates.Infrastructure.Services;

internal class FileRepository : IFileRepository
{
    private const string _baseDirectory = "UserInfo";

    private readonly IFileProvider _fileProvider;
    private readonly IHostingContext _context;

    public FileRepository(IFileProvider fileProvider, IHostingContext context)
    {
        _fileProvider = fileProvider;
        _context = context;
    }

    public async Task<FileLocation> SaveAsync(FileDto file)
    {
        var location = GetLocation(file.Location);

        var info = _fileProvider.GetFileInfo(location);
        if (info.Exists is false)
        {
            var fullPath = Path.Combine(_context.WebRootPath, location);
            var directory = Path.GetDirectoryName(fullPath)!;
            Directory.CreateDirectory(directory);
            await File.WriteAllBytesAsync(fullPath, file.Content);
        }
        else
        {
            throw new Exception("This file already exists");
        }

        return file.Location;
    }

    private static string GetLocation(FileLocation locaton) =>
        Path.ChangeExtension(Path.Combine(_baseDirectory, locaton.Path, locaton.FileName), locaton.FileExtension);

    public string GetUrl(FileLocation location)
    {
        var endPath = new PathString();
        endPath = AppendPath(endPath, _baseDirectory);
        endPath = SplitAndAppendPath(endPath, location.Path);
        endPath = AppendPath(endPath, $"{location.FileName}.{location.FileExtension}");
        return endPath;
    }

    private static PathString SplitAndAppendPath(PathString initialPath, string path)
    {
        foreach (var pathSegment in path.Split(Path.DirectorySeparatorChar))
        {
            initialPath = initialPath.Add(new PathString($"/{pathSegment.TrimStart('/').TrimStart(Path.DirectorySeparatorChar)}"));
        }

        return initialPath;
    }

    private static PathString AppendPath(PathString initialPath, string path)
    {
        initialPath = initialPath.Add(new PathString($"/{path.TrimStart('/').TrimStart(Path.DirectorySeparatorChar)}"));
        return initialPath;
    }
}
