using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Plants.DbMigrator;

public class Migrator
{
    private readonly IOptions<MigratorConfig> _options;
    private readonly ILogger<Migrator> _logger;
    private readonly IFileLoader _loader;
    private readonly IMigrationApplyer _migration;

    public Migrator(IOptions<MigratorConfig> options, ILogger<Migrator> logger, IFileLoader loader, IMigrationApplyer migration)
    {
        _options = options;
        _logger = logger;
        _loader = loader;
        _migration = migration;
    }

    public async Task MigrateAsync()
    {
        var options = _options.Value;
        string[] scripts = await GetScripts();
        var currentVersion = await _migration.CheckMigration();
        if (currentVersion is null)
        {
            await ApplyScripts(options.DbConnectionString, scripts);
        }
        else
        {
            //TODO: Add update logic
        }
    }

    private async Task ApplyScripts(string connString, string[] scripts)
    {
        await using NpgsqlConnection connection = new(connString);
        connection.Open();
        foreach (var script in scripts)
        {
            _logger.LogInformation("Executing script: \n '{script}'", script);
            await connection.ExecuteAsync(script);
        }
    }

    private async Task<string[]> GetScripts()
    {
        var options = _options.Value;
        var scriptFiles = await _loader.GetFullFileNames(options.ScriptsLocation, "*.sql");
        var loadTasks = scriptFiles
            //load files in order
            .OrderBy(x => x.Split('_').FirstOrDefault())
            .Select(file => _loader.LoadFileAsync(file));
        return await Task.WhenAll(loadTasks);
    }
}