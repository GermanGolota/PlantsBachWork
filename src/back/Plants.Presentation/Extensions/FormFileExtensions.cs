namespace Plants.Presentation;

public static class FormFileExtensions
{
    public static async Task<byte[]> ReadBytesAsync(this IFormFile? file, CancellationToken token = default)
    {
        if(file is null)
        {
            return Array.Empty<byte>();
        }

        using (var fileStream = file.OpenReadStream())
        {
            byte[] bytes = new byte[file.Length];
            await fileStream.ReadAsync(bytes, 0, (int)file.Length, token);
            return bytes;
        }
    }
}
