namespace Plants.Presentation;

public record QueryViewResult<T>(bool Exists, T? Item)
{
    public QueryViewResult() : this(false, default)
    {

    }

    public QueryViewResult(T value) : this(true, value)
    {

    }
}

public static class QueryViewResultExtensions
{
    public static QueryViewResult<T> ToQueryResult<T>(this T? result) =>
        result is null
            ? new()
            : new(result);
}