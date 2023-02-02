using Microsoft.Extensions.Options;
using Plants.Domain.Abstractions;
using Plants.Domain.Aggregate;
using Plants.Domain.Config;
using Plants.Domain.Identity;
using Plants.Shared.Model;

namespace Plants.Domain.Services;

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
        CancellationToken token = default)
    {
        var meta = metadataFunc(helper.Factory);
        var command = commandFunc(meta);
        return await helper.Sender.SendCommandAsync(command, GetOptions(helper), token);
    }

    public static async Task<OneOf<CommandAcceptedResult, CommandForbidden>> CreateAndSendAsync(this CommandHelper helper,
       Func<CommandMetadataFactory, IUserIdentity, CommandMetadata> metadataFunc,
       Func<CommandMetadata, IUserIdentity, Command> commandFunc,
       CancellationToken token = default)
    {
        var identity = helper.IdentityProvider.Identity!;
        var meta = metadataFunc(helper.Factory, identity);
        var command = commandFunc(meta, identity);
        return await helper.Sender.SendCommandAsync(command, GetOptions(helper), token);
    }

    private static CommandExecutionOptions.Wait GetOptions(CommandHelper helper) =>
        new CommandExecutionOptions.Wait(TimeSpan.FromSeconds(helper.Options.DefaultTimeoutInSeconds));

}