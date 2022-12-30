namespace Plants.Presentation.Extensions;

public static class FormFileExtensions
{
    public static async Task<byte[]> ReadBytesAsync(this IFormFile file)
    {
        using (var fileStream = file.OpenReadStream())
        {
            byte[] bytes = new byte[file.Length];
            await fileStream.ReadAsync(bytes, 0, (int)file.Length);
            return bytes;
        }
    }
}
