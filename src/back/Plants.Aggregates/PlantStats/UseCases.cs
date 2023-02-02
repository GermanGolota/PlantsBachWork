using Plants.Domain.Aggregate;

namespace Plants.Aggregates.PlantStats;

public record GroupSelectedEvent(EventMetadata Metadata, string GroupName) : Event(Metadata);
