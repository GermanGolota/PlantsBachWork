using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Plants.DbMigrator;

public interface IMigrationApplyer
{
    /// <returns> The version of migration, if there is any or null if there are none </returns>
    Task<int?> CheckMigration();
    Task ApplyMigration(int version);
}

public class MigrationApplyer : IMigrationApplyer
{
    private const string _migrationTableName = "$$migration";
    private const string _versionColumnName = "version";
    private const string _checkTableQuery = @$"
SELECT EXISTS (
    SELECT FROM 
        pg_tables
    WHERE
        tablename  = '{_migrationTableName}'
    );";
    private const string _getVersionQuery = $@"
SELECT {_versionColumnName}
FROM {_migrationTableName};
";
    private const string _createTableCommand = $@"
CREATE TABLE {_migrationTableName}(
);
";
    private const string _insertVersionCommand = $@"
INSERT INTO {_migrationTableName}({_versionColumnName}) VALUES (@version);
";

    private readonly IOptions<MigratorConfig> _options;

    public MigrationApplyer(IOptions<MigratorConfig> options)
    {
        _options = options;
    }

    public async Task ApplyMigration(int version)
    {
        var options = _options.Value;
        await using NpgsqlConnection connection = new(options.DbConnectionString);
        connection.Open();
        await connection.ExecuteAsync(_createTableCommand);
        var param = new
        {
            version
        };
        await connection.ExecuteAsync(_insertVersionCommand, param);
    }

    public async Task<int?> CheckMigration()
    {
        var options = _options.Value;
        await using NpgsqlConnection connection = new(options.DbConnectionString);
        connection.Open();
        var tableExists = await connection.QueryFirstAsync<bool>(_checkTableQuery);
        int? result;
        if (tableExists)
        {
            result = await connection.QueryFirstAsync<int>(_getVersionQuery);
        }
        else
        {
            result = null;
        }
        return result;
    }
}

public class MigrationApplyerLoggingDecorator : IMigrationApplyer
{
    private readonly IMigrationApplyer _inner;
    private readonly ILogger<MigrationApplyerLoggingDecorator> _logger;

    public MigrationApplyerLoggingDecorator(IMigrationApplyer inner, ILogger<MigrationApplyerLoggingDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public Task ApplyMigration(int version) => _inner.ApplyMigration(version);

    public async Task<int?> CheckMigration()
    {
        var result = await _inner.CheckMigration();
        _logger.LogInformation("Check migration returned '{version}' version", result);
        return result;
    }
}
