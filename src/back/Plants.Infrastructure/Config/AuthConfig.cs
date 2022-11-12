using Plants.Shared;

namespace Plants.Infrastructure.Config;

[ConfigSection(Section)]
public class AuthConfig
{
    public const string Section = "Auth";
    public string AuthKey { get; set; }
    public double TokenValidityHours { get; set; }
}
