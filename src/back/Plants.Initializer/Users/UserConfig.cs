using System.ComponentModel.DataAnnotations;

namespace Plants.Initializer;

[ConfigSection(UserConstrants.Admin, UserConstrants.Admin)]
[ConfigSection(UserConstrants.NewAdmin, UserConstrants.NewAdmin)]
internal class UserConfig
{
    [Required]
    public string Username { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}