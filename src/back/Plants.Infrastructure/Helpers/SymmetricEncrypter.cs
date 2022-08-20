using Microsoft.Extensions.Options;
using Plants.Infrastructure.Config;

namespace Plants.Infrastructure.Helpers
{
    public class SymmetricEncrypter
    {
        private readonly AuthConfig _config;
        private string Key => _config.AuthKey;

        public SymmetricEncrypter(IOptions<AuthConfig> config)
        {
            _config = config.Value;
        }

        public string Encrypt(string str)
        {
            return AesOperation.EncryptString(Key, str);
        }

        public string Decrypt(string str)
        {
            return AesOperation.DecryptString(Key, str);
        }
    }
}
