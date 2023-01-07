using Microsoft.Extensions.Options;
using Plants.Domain.Infrastructure.Config;
using Plants.Domain.Infrastructure.Services;
using Plants.Services.Infrastructure.Encryption;

namespace Plants.Aggregates.Infrastructure.Domain;

internal class ServiceIdentityProvider : IServiceIdentityProvider
{
    private readonly ConnectionConfig _options;
    private readonly SymmetricEncrypter _encrypter;

    public ServiceIdentityProvider(IOptions<ConnectionConfig> options, SymmetricEncrypter encrypter)
	{
        _options = options.Value;
        _encrypter = encrypter;
    }

    public IUserIdentity ServiceIdentity => new UserIdentity
    {
        Hash = _encrypter.Encrypt(_options.DefaultCreds.Password),
        UserName = _options.DefaultCreds.Username,
        Roles = Enum.GetValues<UserRole>()
    };
}
