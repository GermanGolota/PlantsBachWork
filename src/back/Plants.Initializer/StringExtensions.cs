﻿using Plants.Shared;

namespace Plants.Initializer;

public static class StringExtensions
{
    public static string Delimit(this string str) =>
        $"\"{str}\"";
    public static string DelimitList(this IEnumerable<string> strings) =>
        String.Join(", ", strings.Select(x => x.Delimit()));
    public static string DelimitList(this IEnumerable<UserRole> roles) =>
        roles.Select(x => x.ToString()).DelimitList();
}