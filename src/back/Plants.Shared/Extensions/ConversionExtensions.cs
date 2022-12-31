namespace Plants.Shared.Extensions;

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
        int bint = BitConverter.ToInt32(b, 0);
        return bint;
    }

}
