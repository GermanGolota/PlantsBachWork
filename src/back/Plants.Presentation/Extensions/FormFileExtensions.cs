namespace Plants.Presentation.Extensions;

public static class FormFileExtensions
{
    public static byte[] ReadBytes(this IFormFile file)
    {
        using (var fileStream = file.OpenReadStream())
        {
            byte[] bytes = new byte[file.Length];
            fileStream.Read(bytes, 0, (int)file.Length);
            return bytes;
        }
    }
}
