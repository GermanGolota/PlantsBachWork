using Microsoft.Extensions.Options;

namespace Plants.Domain;

public class CommandHelper
{
    public CommandHelper(
        ICommandSender sender, CommandMetadataFactory factory,
        IIdentityProvider identityProvider, IOptions<CommandSenderOptions> options)
    {
        Sender = sender;
        Factory = factory;
        IdentityProvider = identityProvider;
        Options = options.Value;
    }

    public ICommandSender Sender { get; }
    public CommandMetadataFactory Factory { get; }
    public IIdentityProvider IdentityProvider { get; }
    public CommandSenderOptions Options { get; }
}

public static class CommandHelperExtensions
{
    public static async Task<OneOf<CommandAcceptedResult, CommandForbidden>> CreateAndSendAsync(this CommandHelper helper,
        Func<CommandMetadataFactory, CommandMetadata> metadataFunc,
        Func<CommandMetadata, Command> commandFunc,
        bool wait = false,
        CancellationToken token = default)
    {
        var meta = metadataFunc(helper.Factory);
        var command = commandFunc(meta);
        return await helper.Sender.SendCommandAsync(command, GetOptions(helper, helper.IdentityProvider.Identity!, wait), token);
    }

    public static async Task<OneOf<CommandAcceptedResult, CommandForbidden>> CreateAndSendAsync(this CommandHelper helper,
       Func<CommandMetadataFactory, IUserIdentity, CommandMetadata> metadataFunc,
       Func<CommandMetadata, IUserIdentity, Command> commandFunc,
       bool wait = false,
       CancellationToken token = default)
    {
        var identity = helper.IdentityProvider.Identity!;
        var meta = metadataFunc(helper.Factory, identity);
        var command = commandFunc(meta, identity);
        return await helper.Sender.SendCommandAsync(command, GetOptions(helper, identity, wait), token);
    }

    private static CommandExecutionOptions GetOptions(CommandHelper helper, IUserIdentity identity, bool wait) =>
        wait
            ? new CommandExecutionOptions.Wait(TimeSpan.FromSeconds(helper.Options.DefaultTimeoutInSeconds))
            : new CommandExecutionOptions.Notify(identity.UserName);

}