﻿using System.Security.Cryptography;
using System.Text;

namespace Plants.Shared.Extensions;

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

    public static string QuoteDelimit(this string str) =>
      $"\"{str}\"";

    public static string QuoteDelimitList(this IEnumerable<string> strings) =>
        string.Join(", ", strings.Select(x => x.QuoteDelimit()));

    public static string QuoteDelimitList(this IEnumerable<UserRole> roles) =>
        roles.Select(x => x.ToString()).QuoteDelimitList();
}