namespace Plants.Initializer;

internal class InitializationRequestedCommandHandler : ICommandHandler<InitializationRequestedCommand>
{
    private readonly IQueryService<StartupStatus> _query;
    private readonly Initializer _initializer;
    private readonly Seeder _seeder;
    private readonly IDateTimeProvider _dateTime;

    public InitializationRequestedCommandHandler(
        IQueryService<StartupStatus> query, 
        Initializer initializer, 
        Seeder seeder,
        IDateTimeProvider dateTime)
    {
        _query = query;
        _initializer = initializer;
        _seeder = seeder;
        _dateTime = dateTime;
    }

    private StartupStatus? _status = null;

    public async Task<CommandForbidden?> ShouldForbidAsync(InitializationRequestedCommand command, IUserIdentity userIdentity, CancellationToken token = default)
    {
        _status ??= await _query.GetByIdAsync(command.Metadata.Aggregate.Id, token: token);
        return (_status.CompletedTime is null).ToForbidden("Already processed");
    }

    public async Task<IEnumerable<Event>> HandleAsync(InitializationRequestedCommand command, CancellationToken token = default)
    {
        await _initializer.InitializeAsync(token);
        await _seeder.SeedAsync(token);

        return new[]
        {
            new InitializedEvent(EventFactory.Shared.Create<InitializedEvent>(command), _dateTime.UtcNow)
        };
    }

}
