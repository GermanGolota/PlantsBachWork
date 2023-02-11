using System.Collections.Concurrent;

namespace Plants.Shared;

public static class DictionaryExtensions
{
    public static void AddList<TKey, TValue>(this IDictionary<TKey, List<TValue>> dict, TKey key, TValue value)
    {
        if (dict.ContainsKey(key))
        {
            dict[key].Add(value);
        }
        else
        {
            dict[key] = new() { value };
        }
    }

    public static void AddList<TKey, TValue>(this IDictionary<TKey, List<TValue>> dict, TKey key, IEnumerable<TValue> values)
    {
        if (dict.ContainsKey(key))
        {
            dict[key].AddRange(values);
        }
        else
        {
            dict[key] = values.ToList();
        }
    }

    public static void CacheTransformation<TKey, TValue>(this IDictionary<TKey, TValue> dict, TValue value, Func<TValue, TKey> transformation)
    {
        var key = transformation(value);
        if (dict.ContainsKey(key) is false)
        {
            dict[key] = value;
        }
    }

    public static IDictionary<TValue, TKey> ToInverse<TKey, TValue>(this IDictionary<TKey, TValue> dict) where TKey : notnull where TValue : notnull =>
        dict.ToDictionary(_ => _.Value, _ => _.Key);

    public static void RemoveWithRetry<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key) where TKey : notnull
    {
        const int maxRetryCount = 5;
        int retryCount = 0;
        while (dict.Remove(key, out _) is false && retryCount < maxRetryCount)
        {
            retryCount++;
            Thread.Sleep(15);
        }

        if (retryCount == maxRetryCount)
        {
            throw new Exception($"Failed to remove information for '{key}'");
        }
    }
}
