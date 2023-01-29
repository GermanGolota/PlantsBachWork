namespace Plants.Domain.Extensions;

public static class AggregateBaseExtensions
{
    public static CommandForbidden? RequireNew(this AggregateBase aggregate) =>
        (aggregate.Metadata.CommandsProcessed is 0 || aggregate.Metadata.CommandsProcessed is 1) switch
        {
            true => null,
            false => new CommandForbidden($"This '{aggregate.Metadata.Name}' already exists")
        };

    public static AggregateDescription GetDescription(this AggregateBase aggregate) =>
        new(aggregate.Id, aggregate.Metadata.Name);
}
