namespace Plants.Infrastructure.Helpers;

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
}
