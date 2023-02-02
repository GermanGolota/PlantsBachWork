using Microsoft.Extensions.Options;

namespace Plants.Initializer;

internal class ConfigIdentityProvider : IIdentityProvider
{
    public ConfigIdentityProvider(IOptionsSnapshot<UserConfig> options, SymmetricEncrypter encrypter)
    {
        var adminOptions = options.Get(UserConstrants.Admin);
        _identity = new UserIdentity(Enum.GetValues<UserRole>(), adminOptions.Username, encrypter.Encrypt(adminOptions.Password));
    }

    private readonly UserIdentity _identity;
    public IUserIdentity? Identity => _identity;

    public void UpdateIdentity(IUserIdentity newIdentity)
    {
        _identity.Roles = newIdentity.Roles;
        _identity.Hash = newIdentity.Hash;
        _identity.UserName = newIdentity.UserName;
    }

    private class UserIdentity : IUserIdentity
    {
        public UserIdentity(UserRole[] roles, string userName, string hash)
        {
            Roles = roles;
            UserName = userName;
            Hash = hash;
        }

        public UserRole[] Roles { get; set; }

        public string UserName { get; set; }

        public string Hash { get; set; }
    }

}

