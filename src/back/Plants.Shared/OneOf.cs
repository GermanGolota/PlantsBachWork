using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plants.Shared;

public class OneOf<T0, T1>
{
    private readonly bool _isT0;
    // It is better to store both fields instead of having an object field
    // because of boxing when assigning and casting when using value types
    private readonly T0 _t0;
    private readonly T1 _t1;

    public OneOf(T0 value)
    {
        _t0 = value;
        _t1 = default!;
        _isT0 = true;
    }

    public OneOf(T1 value)
    {
        _t0 = default!;
        _t1 = value;
        _isT0 = false;
    }

    public TResult Match<TResult>(Func<T0, TResult> func0, Func<T1, TResult> func1) =>
        _isT0
            ? func0(_t0)
            : func1(_t1);

    public void Match(Action<T0> func0, Action<T1> func1)
    {
        if (_isT0)
        {
            func0(_t0);
        }
        else
        {
            func1(_t1);
        }
    }

    public static implicit operator OneOf<T0, T1>(T0 first) => new(first);
    public static implicit operator OneOf<T0, T1>(T1 second) => new(second);

    public override string ToString() => 
        Match(_ => _?.ToString(), _ => _?.ToString()) ?? "";
}

public static class OneOfExtensions
{
    public static IList<JsonConverter> AddOneOfConverter(this IList<JsonConverter> list)
    {
        list.Add(new OneOfJsonConverterFactory());

        return list;
    }

    public static Task MatchAsync<T0, T1>(this OneOf<T0, T1> oneOf, Func<T0, Task> func0, Func<T1, Task> func1) =>
          oneOf.Match(func0, func1);

    public static Task<TResult> MatchAsync<T0, T1, TResult>(this OneOf<T0, T1> oneOf, Func<T0, Task<TResult>> func0, Func<T1, Task<TResult>> func1) =>
          oneOf.Match(func0, func1);

    public static OneOf<T0, T1> ToOneOf<T0, T1>(this T0 value) =>
        new(value);

    public static OneOf<T0, T1> ToOneOf<T0, T1>(this T1 value) =>
        new(value);

    public static OneOf<T1, T0> Swap<T0, T1>(this OneOf<T0, T1> value) =>
        value.Match(x => new OneOf<T1, T0>(x), x => new OneOf<T1, T0>(x));

    public static bool IsFirst<T0, T1>(this OneOf<T0, T1> value) =>
        value.Match(_ => true, _ => false);

    public static bool IsSecond<T0, T1>(this OneOf<T0, T1> value) =>
        value.Match(_ => false, _ => true);

    public static IEnumerable<T0> SelectFirsts<T0, T1>(this IEnumerable<OneOf<T0, T1>> values)
    {
        foreach (var value in values)
        {
            if (value.IsFirst())
            {
                T0? current = value.Match(x => x, x => default(T0));
                if (current is not null)
                {
                    yield return current;
                }
            }
        }
    }

    public static IEnumerable<T1> SelectSeconds<T0, T1>(this IEnumerable<OneOf<T0, T1>> values)
    {
        foreach (var value in values)
        {
            if (value.IsSecond())
            {
                T1? current = value.Match(x => default(T1), x => x);
                if (current is not null)
                {
                    yield return current;
                }
            }
        }
    }
}

internal sealed class OneOfJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsAssignableToGenericType(typeof(OneOf<,>));

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        JsonConverter? converter;
        if (typeToConvert.IsAssignableToGenericType(typeof(OneOf<,>)))
        {
            Type firstType = typeToConvert.GetGenericArguments()[0];
            Type secondType = typeToConvert.GetGenericArguments()[1];

            converter = (JsonConverter)Activator.CreateInstance(
                typeof(OneOfConverter<,>).MakeGenericType(
                    new Type[] { firstType, secondType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: Array.Empty<object>(),
                culture: null)!;
        }
        else
        {
            converter = null;
        }
        return converter;
    }

    private sealed class OneOfConverter<T0, T1> : JsonConverter<OneOf<T0, T1>>
    {
        public override OneOf<T0, T1>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                var first = (T0?)JsonSerializer.Deserialize(ref reader, typeof(T0), options);
                if (first is not null)
                {
                    return new(first);
                }
            }
            catch
            {
            }

            try
            {
                var second = (T1?)JsonSerializer.Deserialize(ref reader, typeof(T1), options);
                if (second is not null)
                {
                    return new(second);
                }
            }
            catch
            {
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, OneOf<T0, T1> value, JsonSerializerOptions options)
        {
            value.Match(
                x =>
                {
                    JsonSerializer.Serialize(writer, x, typeof(T0), options);
                },
                x =>
                {
                    JsonSerializer.Serialize(writer, x, typeof(T1), options);
                });
        }
    }
}