using System.ComponentModel.DataAnnotations;

namespace Plants.Initializer;

[ConfigSection(UserConstrants.Admin, UserConstrants.Admin)]
[ConfigSection(UserConstrants.NewAdmin, UserConstrants.NewAdmin)]
internal class UserConfig
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}