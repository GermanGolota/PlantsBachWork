using System.Collections;

namespace Plants.Shared;

public interface ITwoWayDictionary<T0, T1> : IEnumerable<KeyValuePair<T0, T1>>
{
    bool TryGetFor(T1 t1, out T0 t0);
    bool TryGetFor(T0 t0, out T1 t1);
    bool ContainsKey(T0 t0);
    bool ContainsKey(T1 t1);
}

public static class TwoWayDictionaryExtensions
{
    public static T1 Get<T0, T1>(this ITwoWayDictionary<T0, T1> dict, T0 key)
    {
        if (dict.TryGetFor(key, out T1 result))
        {
            return result;
        }

        throw new ArgumentOutOfRangeException($"Key '{key}' does not exist in the dictionary");
    }

    public static T0 Get<T0, T1>(this ITwoWayDictionary<T0, T1> dict, T1 key)
    {
        if (dict.TryGetFor(key, out T0 result))
        {
            return result;
        }

        throw new ArgumentOutOfRangeException($"Key '{key}' does not exist in the dictionary");
    }
}

public class TwoWayDictionary<T0, T1> : ITwoWayDictionary<T0, T1>
{
    private readonly IDictionary<T0, T1> _forward;
    private readonly IDictionary<T1, T0> _backward;

    public TwoWayDictionary(IDictionary<T0, T1> forward)
    {
        _forward = forward;
        if (_forward.Values.Distinct().Count() != _forward.Values.Count)
        {
            throw new Exception("Cannot construct two-way dictionary, some values are not unique");
        }
        _backward = forward.ToDictionary((i) => i.Value, (i) => i.Key);
    }

    public bool TryGetFor(T1 t1, out T0 t0) => _backward.TryGetValue(t1, out t0);

    public bool TryGetFor(T0 t0, out T1 t1) => _forward.TryGetValue(t0, out t1);

    IEnumerator IEnumerable.GetEnumerator() => _forward.GetEnumerator();
    public IEnumerator<KeyValuePair<T0, T1>> GetEnumerator() => _forward.GetEnumerator();

    public bool ContainsKey(T0 t0) =>
        _forward.ContainsKey(t0);

    public bool ContainsKey(T1 t1) =>
        _backward.ContainsKey(t1);
}
