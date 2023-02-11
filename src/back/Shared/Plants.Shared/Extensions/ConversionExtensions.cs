namespace Plants.Shared;

public static class ConversionExtensions
{
    public static Guid ToGuid(this long value)
    {
        byte[] bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    public static long ToLong(this Guid value)
    {
        byte[] b = value.ToByteArray();
        return BitConverter.ToInt64(b, 0);
    }

    public static Guid GetRandomConvertableGuid(this Random rng) =>
        rng.NextInt64().ToGuid();
}
