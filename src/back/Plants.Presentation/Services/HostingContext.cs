using Plants.Aggregates.Infrastructure.Abstractions;

namespace Plants.Presentation.Services;

public class HostingContext : IHostingContext
{
    public HostingContext(IWebHostEnvironment webHost)
    {
        WebRootPath = webHost.WebRootPath;
        WebRootUrlPath = $"/{Path.GetFileName(webHost.WebRootPath)}";
    }

    public string WebRootPath { get; }

    public string WebRootUrlPath { get; }
}
