namespace Plants.Initializer;

public sealed record InitializationRequestedCommand(CommandMetadata Metadata) : Command(Metadata);
public sealed record InitializedEvent(EventMetadata Metadata, DateTime Time) : Event(Metadata);