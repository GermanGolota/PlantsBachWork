using Microsoft.Extensions.Options;
using Plants.Domain.Identity;
using Plants.Domain.Infrastructure.Config;
using Plants.Domain.Infrastructure.Services;
using Plants.Services.Infrastructure.Encryption;
using Plants.Shared.Model;

namespace Plants.Aggregates.Infrastructure.Domain;

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
        var serviceIdentity = new UserIdentity
        {
            Hash = _encrypter.Encrypt(_options.DefaultCreds.Password),
            UserName = _options.DefaultCreds.Username,
            Roles = Enum.GetValues<UserRole>()
        };
        _identity.UpdateIdentity(serviceIdentity);
    }
}
