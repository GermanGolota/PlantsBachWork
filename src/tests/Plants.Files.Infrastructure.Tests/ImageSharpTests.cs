using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Reflection;

namespace Plants.Files.Infrastructure.Tests;

public class ImageSharpTests
{
    [Fact(Skip = "Should run this test manually")]
    public async Task CanConvertImage()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var cherry = assembly.GetManifestResourceStream(
            $"{assembly.GetName().Name}.Cherry4.png")!;

        using (var image = Image.Load(cherry))
        {
            const int width = 512;
            const int height = 512;
            image.Mutate(x => x.Resize(width, height));

            using (var ms = new MemoryStream())
            {
                var folder = Path.Combine(Path.GetTempPath(), "Test");
                Directory.CreateDirectory(folder);
                var path = Path.Combine(folder, $"{Guid.NewGuid()}.jpeg");
                await image.SaveAsJpegAsync(path);
            }
        }
    }
}