using Microsoft.Extensions.Options;
using Plants.Aggregates.Users;
using Plants.Domain;
using Plants.Domain.Services;
using Plants.Shared;

namespace Plants.Initializer;

internal class AdminUserCreator
{
    private readonly ICommandSender _sender;
    private readonly CommandMetadataFactory _metadataFactory;
    private readonly TempPasswordContext _context;
    private readonly UserConfig _options;

    public AdminUserCreator(ICommandSender sender, CommandMetadataFactory metadataFactory, IOptionsSnapshot<UserConfig> options, TempPasswordContext context)
    {
        _sender = sender;
        _metadataFactory = metadataFactory;
        _context = context;
        _options = options.Get(UserConstrants.NewAdmin);
    }

    public async Task<OneOf<CommandAcceptedResult, CommandForbidden>> SendCreateAdminCommandAsync()
    {
        var meta = _metadataFactory.Create<CreateUserCommand, User>(_options.Username.ToGuid());
        var command = new CreateUserCommand(meta,
            new UserCreationDto(
                _options.FirstName,
                _options.LastName,
                "",
                _options.Username,
                "admin@admin.admin",
                "English",
                Enum.GetValues<UserRole>()));
        return await _sender.SendCommandAsync(command);
    }

    public async Task<OneOf<CommandAcceptedResult, CommandForbidden>> SendResetPasswordCommand()
    {
        var meta = _metadataFactory.Create<ChangePasswordCommand, User>(_options.Username.ToGuid());
        var command = new ChangePasswordCommand(meta, _options.Username, _context.TempPassword, _options.Password);
        return await _sender.SendCommandAsync(command);
    }
}
