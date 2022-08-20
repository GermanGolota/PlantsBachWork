using System.ComponentModel.DataAnnotations;

namespace Plants.DbMigrator;

public class MigratorConfig
{
    [Required]
    public string ScriptsLocation { get; set; } = null!;
    [Required]
    public string DbConnectionString { get; set; } = null!;
}