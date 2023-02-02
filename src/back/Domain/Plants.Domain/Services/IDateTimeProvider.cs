namespace Plants.Domain;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
