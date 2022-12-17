﻿namespace Plants.Domain.Extensions;

public static class AggregateBaseExtensions
{
    public static CommandForbidden? RequireNew(this AggregateBase aggregate) =>
        (aggregate.CommandsProcessed is 0 || aggregate.CommandsProcessed is 1) switch
        {
            true => null,
            false => new CommandForbidden($"This '{aggregate.Name}' already exists")
        };
}