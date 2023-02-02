using System.Text.Json.Serialization;

namespace Plants.Aggregates.Infrastructure;


public class UserCreateDefinition
{
    public required string Password { get; set; }
    public required List<string> Roles { get; set; }
    [JsonPropertyName("full_name")]
    public required string FullName { get; set; }
}


public class UserUpdateDefinition
{
    public required List<string> Roles { get; set; }
    [JsonPropertyName("full_name")]
    public required string FullName { get; set; }
}
