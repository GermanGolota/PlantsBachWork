namespace Plants.Shared.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source) =>
        source.SelectMany(x => x);
}
