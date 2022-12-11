namespace Plants.Domain.Extensions;

public static class AggregateBaseExtensions
{
    public static CommandForbidden? IsNew(this AggregateBase aggregate) =>
        (aggregate.Version is AggregateBase.NewAggregateVersion) switch
        {
            true => null,
            false => new CommandForbidden($"This '{aggregate.Name}' already exists")
        };
}
