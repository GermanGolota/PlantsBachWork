namespace Plants.Presentation;

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
