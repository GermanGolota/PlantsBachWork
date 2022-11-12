namespace Plants.Domain.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
