using Plants.Domain.Services;

namespace Plants.Domain.Infrastructure;

internal class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
