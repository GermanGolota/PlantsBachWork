using Microsoft.Extensions.Options;
using Plants.Aggregates.Users;
using Plants.Core;
using Plants.Domain;
using Plants.Domain.Services;
using Plants.Shared;

namespace Plants.Initializer;

internal class AdminUserCreator
{
    private readonly ICommandSender _sender;
    private readonly CommandMetadataFactory _metadataFactory;
    private readonly AdminUserConfig _options;

    public AdminUserCreator(ICommandSender sender, CommandMetadataFactory metadataFactory, IOptions<AdminUserConfig> options)
    {
        _sender = sender;
        _metadataFactory = metadataFactory;
        _options = options.Value;
    }
    public Task<OneOf<CommandAcceptedResult, CommandForbidden>> SendCreateAdminCommandAsync()
    {
        var meta = _metadataFactory.Create<CreateUserCommand, User>(_options.Username.ToGuid());
        var command = new CreateUserCommand(meta,
            new UserCreationDto(
                "admin@admin.admin",
                _options.Name,
                _options.Name,
                "English",
                _options.Username,
                "",
                Enum.GetValues<UserRole>()));
        return _sender.SendCommandAsync(command);
    }
}
