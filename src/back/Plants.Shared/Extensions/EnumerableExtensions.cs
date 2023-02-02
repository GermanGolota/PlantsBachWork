namespace Plants.Shared;

public static class EnumerableExtensions
{
    public static bool In<T>(this T value, IEnumerable<T> source) =>
        source.Contains(value);

    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source) =>
        source.SelectMany(x => x);

    public static T Random<T>(this ICollection<T> source, Random? rng = null)
    {
        rng ??= System.Random.Shared;
        var index = rng.Next(0, source.Count() - 1);
        return source.ElementAt(index);
    }

    public static IEnumerable<T> Random<T>(this ICollection<T> source, int count, Random? rng = null)
    {
        var sourceCount = source.Count();
        if (sourceCount < count)
        {
            throw new ArgumentException("Requested more items than there are in the collection", nameof(count));
        }

        rng ??= System.Random.Shared;
        List<int> indexes = new();
        for (int i = 0; i < count; i++)
        {
            int index;
            do
            {
                index = rng.Next(0, sourceCount - 1);
            }
            while (indexes.Contains(index));

            indexes.Add(index);

            yield return source.ElementAt(index);
        }
    }

    public static IEnumerable<T> Random<T>(this ICollection<T> source, int minCountInclusive, int maxCountExclusive, Random? rng = null)
    {
        rng ??= System.Random.Shared;
        return source.Random(rng.Next(minCountInclusive, maxCountExclusive));
    }
}
