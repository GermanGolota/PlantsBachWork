using Plants.Aggregates;

namespace Plants.Files;

public interface IPictureUploader
{
    Task<Picture[]> UploadAsync(CancellationToken token, params FileView[] files);
}

public record FileView(Guid Id, byte[] Content);