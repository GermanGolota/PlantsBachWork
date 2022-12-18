using Plants.Shared;

namespace Plants.Initializer;

//TODO: Figure out multiple named registrations
//[ConfigSection("Admin")]
internal class UserConfig
{
	public string Username { get; set; }
	public string Password { get; set; }
	public string Name { get; set; }
}