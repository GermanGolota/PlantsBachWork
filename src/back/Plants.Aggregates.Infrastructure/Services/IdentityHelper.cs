using Plants.Aggregates.Services;
using Plants.Services.Infrastructure.Encryption;
using Plants.Shared;

namespace Plants.Aggregates.Infrastructure.Services;

internal class IdentityHelper : IIdentityHelper
{
    private readonly SymmetricEncrypter _encrypter;

    public IdentityHelper(SymmetricEncrypter encrypter)
    {
        _encrypter = encrypter;
    }

    public IUserIdentity Build(string password, string username, UserRole[] roles) =>
        new UserIdentity
        {
            Hash = _encrypter.Encrypt(password),
            Roles = roles,
            UserName = username
        };
}
