namespace Plants.Domain.Config;

[ConfigSection(Section)]
public class CommandSenderOptions
{
    public const string Section = "Command";

    public double DefaultTimeoutInSeconds { get; set; } = 60 * 3;
}
