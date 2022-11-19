using System.Security.Cryptography;
using System.Text;

namespace Plants.Shared;

public static class StringExtensions
{
    public static Guid ToGuid(this string str)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            return new(hash);
        }
    }
}
