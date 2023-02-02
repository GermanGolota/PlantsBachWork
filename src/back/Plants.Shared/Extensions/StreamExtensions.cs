namespace Plants.Shared;

public static class StreamExtensions
{
    public static async Task<byte[]> ReadAllBytesAsync(this Func<Stream> streamFunc)
    {
        using(var stream = streamFunc())
        {
            return await stream.ReadAllBytesAsync();
        }
    }

    public static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
    {
        var streamLength = Convert.ToInt32(stream.Length);
        byte[] data = new byte[streamLength + 1];

        //convert to to a byte array
        await stream.ReadAsync(data, 0, streamLength);

        return data;
    }
}
