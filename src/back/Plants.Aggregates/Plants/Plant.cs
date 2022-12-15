namespace Plants.Aggregates.Plants;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
public class Plant : AggregateBase, IDomainCommandHandler<CreatePlantCommand>, IEventHandler<PlantCreatedEvent>
{
    public Plant(Guid id) : base(id)
    {
    }

    public string PlantName { get; private set; }

    public CommandForbidden? ShouldForbid(CreatePlantCommand command, IUserIdentity userIdentity) =>
         userIdentity.HasRole(Producer)
        .And(this.RequireNew);

    public IEnumerable<Event> Handle(CreatePlantCommand command) =>
         new[]
        {
            new PlantCreatedEvent(EventFactory.Shared.Create<PlantCreatedEvent>(command, Version + 1), command.Plant)
        };

    public void Handle(PlantCreatedEvent @event)
    {
        PlantName = @event.Plant.PlantName;
    }
}

public record CreatePlantCommand(CommandMetadata Metadata, PlantCreationDto Plant) : Command(Metadata);
public record PlantCreatedEvent(EventMetadata Metadata, PlantCreationDto Plant) : Event(Metadata);
public record PlantCreationDto(string PlantName);