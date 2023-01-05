namespace Plants.Domain.Services;

public class CommandHelper
{
	public CommandHelper(ICommandSender sender, CommandMetadataFactory factory, IIdentityProvider identityProvider)
	{
        Sender = sender;
        Factory = factory;
        IdentityProvider = identityProvider;
    }

    public ICommandSender Sender { get; }
    public CommandMetadataFactory Factory { get; }
    public IIdentityProvider IdentityProvider { get; }
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
        return await helper.Sender.SendCommandAsync(command, token);
    }

    public static async Task<OneOf<CommandAcceptedResult, CommandForbidden>> CreateAndSendAsync(this CommandHelper helper,
       Func<CommandMetadataFactory, IUserIdentity, CommandMetadata> metadataFunc,
       Func<CommandMetadata, IUserIdentity, Command> commandFunc,
       CancellationToken token = default)
    {
        var identity = helper.IdentityProvider.Identity!;
        var meta = metadataFunc(helper.Factory, identity);
        var command = commandFunc(meta, identity);
        return await helper.Sender.SendCommandAsync(command, token);
    }
}