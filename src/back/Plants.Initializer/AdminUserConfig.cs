using Plants.Shared;

namespace Plants.Initializer;

[ConfigSection("Admin")]
internal class AdminUserConfig
{
	public string Username { get; set; }
	public string Password { get; set; }
	public string Name { get; set; }
}