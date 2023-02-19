namespace Plants.Initializer;

[Allow(UserRole.Manager, AllowType.Read)]
[Allow(UserRole.Manager, AllowType.Write)]
public sealed class StartupStatus : AggregateBase, IEventHandler<InitializedEvent>
{
    public static Guid StartupId = Guid.Parse("c8bf168e-6727-4446-8ae6-89be6648f1d6");

    public StartupStatus(Guid id) : base(id)
    {

    }

    public void Handle(InitializedEvent @event)
    {
        CompletedTime = @event.Time;
    }

    public DateTime? CompletedTime { get; private set; } = null;
}