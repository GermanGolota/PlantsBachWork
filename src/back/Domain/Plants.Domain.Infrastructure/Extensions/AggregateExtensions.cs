namespace Plants.Domain.Infrastructure;

public static class AggregateExtensions
{
    public static string ToTopic(this AggregateDescription aggregate) =>
        $"{aggregate.Name}_{aggregate.Id}";

    public static string ToIndexName(this string aggregateName) =>
       aggregateName.ToLower();
}
