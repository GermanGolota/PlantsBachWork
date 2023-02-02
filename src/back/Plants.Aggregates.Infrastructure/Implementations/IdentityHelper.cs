namespace Plants.Aggregates.Infrastructure;

internal class IdentityHelper : IIdentityHelper
{
    private readonly SymmetricEncrypter _encrypter;

    public IdentityHelper(SymmetricEncrypter encrypter)
    {
        _encrypter = encrypter;
    }

    public IUserIdentity Build(string password, string username, UserRole[] roles) =>
        new UserIdentity(roles, username, _encrypter.Encrypt(password));
}
