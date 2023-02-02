using Microsoft.Extensions.Options;

namespace Plants.Initializer;

internal class AdminUserCreator
{
    private readonly TempPasswordContext _context;
    private readonly CommandHelper _command;
    private readonly UserConfig _options;

    public AdminUserCreator(IOptionsSnapshot<UserConfig> options, TempPasswordContext context, CommandHelper command)
    {
        _context = context;
        _command = command;
        _options = options.Get(UserConstrants.NewAdmin);
    }

    public async Task<OneOf<CommandAcceptedResult, CommandForbidden>> SendCreateAdminCommandAsync(CancellationToken token = default) =>
        await _command.CreateAndSendAsync(
            factory => factory.Create<CreateUserCommand, User>(_options.Username.ToGuid()),
            meta => new CreateUserCommand(meta,
            new UserCreationDto(
                _options.FirstName,
                _options.LastName,
                "",
                _options.Username,
                "admin@admin.admin",
                "English",
                Enum.GetValues<UserRole>())),
            token);

    public async Task<OneOf<CommandAcceptedResult, CommandForbidden>> SendResetPasswordCommandAsync(CancellationToken token = default) =>
        await _command.CreateAndSendAsync(
            factory => factory.Create<ChangePasswordCommand, User>(_options.Username.ToGuid()),
            meta => new ChangePasswordCommand(meta, _options.Username, _context.TempPassword, _options.Password),
            token);
}
