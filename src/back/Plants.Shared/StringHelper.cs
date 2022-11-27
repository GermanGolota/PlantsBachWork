using System.Security.Cryptography;
using System.Text;

namespace Plants.Shared;

public static class StringHelper
{
    public static string GetRandomAlphanumericString(int length)
    {
        const string alphanumericCharacters =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "abcdefghijklmnopqrstuvwxyz" +
            "0123456789";
        return GetRandomString(length, alphanumericCharacters);
    }

    private static string GetRandomString(int length, IEnumerable<char> characterSet)
    {
        if (length < 0)
            throw new ArgumentException("length must not be negative", nameof(length));
        if (length > int.MaxValue / 8)
            throw new ArgumentException("length is too big", nameof(length));
        if (characterSet == null)
            throw new ArgumentNullException(nameof(characterSet));
        var characterArray = characterSet.Distinct().ToArray();
        if (characterArray.Length == 0)
            throw new ArgumentException("characterSet must not be empty", nameof(characterSet));

        var bytes = RandomNumberGenerator.GetBytes(length);
        StringBuilder result = new();
        for (int i = 0; i < length; i++)
        {
            ulong value = BitConverter.ToUInt64(bytes, i * 8);
            var character = characterArray[value % (uint)characterArray.Length];
            result.Append(character);
        }
        return result.ToString();
    }
}
