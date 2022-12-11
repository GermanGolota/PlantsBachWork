using Microsoft.Extensions.Configuration;

namespace Plants.Aggregates.Infrastructure.Encryption;

public class SymmetricEncrypter
{
    private readonly IConfiguration _config;

    private string Key => _config.GetSection("Auth")["AuthKey"];
    //TODO: Actually add options
    public SymmetricEncrypter(IConfiguration config)
    {
        _config = config;
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
