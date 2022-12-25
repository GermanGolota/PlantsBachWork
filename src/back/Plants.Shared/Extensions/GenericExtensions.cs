namespace Plants.Shared.Extensions;

public static class GenericExtensions
{
    public static Task<T> ToResultTask<T>(this T result) =>
        Task.FromResult(result);

    public static Task<R> ToResultTask<T, R>(this T result) where T : R =>
        Task.FromResult<R>(result);
}
