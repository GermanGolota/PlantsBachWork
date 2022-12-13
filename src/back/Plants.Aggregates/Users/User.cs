using Plants.Core;

namespace Plants.Aggregates.Users;

public class User : AggregateBase, IEventHandler<UserCreatedEvent>, IEventHandler<RoleChangedEvent>
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
        var user = @event.Data;
        FirstName = user.FirstName;
        LastName = user.LastName;
        PhoneNumber = user.PhoneNumber;
        Login = user.Login;
        Roles = user.Roles;
    }

    public void Handle(RoleChangedEvent @event)
    {
        if (Roles.Contains(@event.Role))
        {
            Roles = Roles.Where(x => x != @event.Role).ToArray();
        }
        else
        {
            Roles = Roles.Append(@event.Role).ToArray();
        }
    }

}

public record UserCreationDto(string FirstName, string LastName, string PhoneNumber, string Login, string Email, string Language, UserRole[] Roles);

public record UserCreatedEvent(EventMetadata Metadata, UserCreationDto Data) : Event(Metadata);
public record CreateUserCommand(CommandMetadata Metadata, UserCreationDto Data) : Command(Metadata);

public record ChangeRoleCommand(CommandMetadata Metadata, UserRole Role) : Command(Metadata);
public record RoleChangedEvent(EventMetadata Metadata, UserRole Role) : Event(Metadata);
