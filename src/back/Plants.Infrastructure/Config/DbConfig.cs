using Plants.Shared;

namespace Plants.Infrastructure.Config;

[ConfigSection]
public class DbConfig
{
    public string DatabaseConnectionTemplate { get; set; } = null!;
}
