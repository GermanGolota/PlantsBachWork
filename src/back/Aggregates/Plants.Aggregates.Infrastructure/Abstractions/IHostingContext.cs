namespace Plants.Aggregates.Infrastructure;

public interface IHostingContext
{
    public string WebRootPath { get; }
    public string WebRootUrlPath { get; }
}
