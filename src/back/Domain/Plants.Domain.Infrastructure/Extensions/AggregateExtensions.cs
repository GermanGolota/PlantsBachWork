namespace Plants.Domain.Infrastructure.Extensions;

public static class AggregateExtensions
{
    public static string ToTopic(this AggregateDescription aggregate) =>
        $"{aggregate.Name}_{aggregate.Id}";
}
