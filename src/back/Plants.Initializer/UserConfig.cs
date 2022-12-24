using Plants.Shared;
using System.ComponentModel.DataAnnotations;

namespace Plants.Initializer;

//TODO: Figure out multiple named registrations
//[ConfigSection("Admin")]
internal class UserConfig
{
	[Required]
	public string Username { get; set; }
    [Required]
    public string Password { get; set; }
	public string FirstName { get; set; }
    public string LastName { get; set; }
}