using System.ComponentModel.DataAnnotations;

namespace Plants.Services.Infrastructure;

[ConfigSection(Section)]
public class AuthConfig
{
    public const string Section = "Auth";
    [Required]
    public string AuthKey { get; set; } = null!;
    [Range(0.1, double.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public double TokenValidityHours { get; set; }
}
