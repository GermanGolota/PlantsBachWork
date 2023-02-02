using Microsoft.Extensions.Options;

namespace Plants.Services.Infrastructure;

public class SymmetricEncrypter
{
    private readonly IOptions<AuthConfig> _options;

    private string Key => _options.Value.AuthKey;

    public SymmetricEncrypter(IOptions<AuthConfig> options)
    {
        _options = options;
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
