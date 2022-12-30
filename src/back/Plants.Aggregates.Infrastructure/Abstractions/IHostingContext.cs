namespace Plants.Aggregates.Infrastructure.Abstractions;

public interface IHostingContext
{
    public string WebRootPath { get; }
    public string WebRootUrlPath { get; }
}
