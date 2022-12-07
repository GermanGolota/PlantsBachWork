using Plants.Application.Contracts;
using Plants.Core;
using Plants.Domain;
using Plants.Domain.Projection;
using Plants.Domain.Services;
using Plants.Shared;

namespace Plants.Application.Aggregates;

public class User : AggregateBase, IEventHandler<UserCreatedEvent>
{
    public User(Guid id) : base(id)
    {
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Login { get; private set; }
    public UserRole[] Roles { get; private set; }

    public void Handle(UserCreatedEvent @event)
    {
        if (Version is AggregateBase.NewAggregateVersion)
        {
            var user = @event.Data;
            FirstName = user.FirstName;
            LastName = user.LastName;
            PhoneNumber = user.PhoneNumber;
            Login = user.Login;
            Roles = user.Roles;
        }
    }
}

public record UserCreatedEvent(EventMetadata Metadata, UserCreationDto Data) : Event(Metadata);
public record CreateUserCommand(CommandMetadata Metadata, UserCreationDto Data) : Command(Metadata);

public record UserCreationDto(string FirstName, string LastName, string PhoneNumber, string Login, string Email, string Language, UserRole[] Roles);

internal class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IProjectionQueryService<User> _userQuery;
    private readonly IEmailer _emailer;

    public CreateUserCommandHandler(IProjectionQueryService<User> userQuery, IEmailer emailer)
    {
        _userQuery = userQuery;
        _emailer = emailer;
    }

    public async Task<CommandForbidden?> ShouldForbidAsync(CreateUserCommand command)
    {
        return (await _userQuery.Exists(command.Metadata.Id)) switch
        {
            true => new CommandForbidden("Plant already created"),
            false => null
        };
    }

    public async Task<IEnumerable<Event>> HandleAsync(CreateUserCommand command)
    {
        const int TempPasswordLength = 8;
        var user = command.Data;
        var tempPassword = StringHelper.GetRandomAlphanumericString(TempPasswordLength);
        var lang = user.Language ?? "English";
        await _emailer.SendInvitationEmail(user.Email, user.Login, tempPassword, lang);
        var metadata = EventFactory.Shared.Create<UserCreatedEvent>(command, 0) with { Id = user.Login.ToGuid() };
        return new[]
        {
            new UserCreatedEvent(metadata, user)
        };
    }
}
