namespace Plants.Aggregates;

// Commands

public record CreateUserCommand(CommandMetadata Metadata, UserCreationDto Data) : Command(Metadata);
public record UserCreatedEvent(EventMetadata Metadata, UserCreationDto Data) : Event(Metadata);

public record ChangeRoleCommand(CommandMetadata Metadata, UserRole Role) : Command(Metadata);
public record RoleChangedEvent(EventMetadata Metadata, UserRole Role) : Event(Metadata);

public record ChangeOwnPasswordCommand(CommandMetadata Metadata, string OldPassword, string NewPassword) : Command(Metadata);
public record ChangePasswordCommand(CommandMetadata Metadata, string Login, string OldPassword, string NewPassword) : Command(Metadata);
public record PasswordChangedEvent(EventMetadata Metadata) : Event(Metadata);

// Queries

public record SearchUsers(UserSearchParams Parameters, QueryOptions Options) : IRequest<IEnumerable<FindUsersResultItem>>;
public record GetOwnUsedAddresses : IRequest<AddressViewResult>;

// Types

public record UserCreationDto(string FirstName, string LastName, string PhoneNumber, string Login, string Email, string Language, UserRole[] Roles);

public record AddressViewResult(List<DeliveryAddress> Addresses);
public record UserSearchParams(string Name, string Phone, UserRole[] Roles) : ISearchParams;
public record FindUsersResultItem(Guid Id, string FullName, string Mobile, string Login, UserRole[] RoleCodes);

