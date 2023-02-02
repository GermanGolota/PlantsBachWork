using Microsoft.Extensions.Options;

namespace Plants.Aggregates.Infrastructure;

internal class ServiceIdentityProvider : IServiceIdentityProvider
{
    private readonly ConnectionConfig _options;
    private readonly SymmetricEncrypter _encrypter;
    private readonly IIdentityProvider _identity;

    public ServiceIdentityProvider(IOptions<ConnectionConfig> options, SymmetricEncrypter encrypter, IIdentityProvider identity)
    {
        _options = options.Value;
        _encrypter = encrypter;
        _identity = identity;
    }

    public void SetServiceIdentity()
    {
        var serviceIdentity = new UserIdentity(Enum.GetValues<UserRole>(), _options.DefaultCreds.Username, _encrypter.Encrypt(_options.DefaultCreds.Password));
        _identity.UpdateIdentity(serviceIdentity);
    }
}
