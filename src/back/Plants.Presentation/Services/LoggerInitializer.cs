using Microsoft.Extensions.Options;
using Plants.Domain.Infrastructure.Config;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.SystemConsole.Themes;

namespace Plants.Presentation.Services;

public class LoggerInitializer
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ConnectionConfig _options;

    public LoggerInitializer(IConfiguration configuration, IOptions<ConnectionConfig> options, IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
        _options = options.Value;
    }

    public void Initialize()
    {
        var elasticCreds = _options.GetCreds(_ => _.ElasticSearch);
        var elasticUrl = _options.ElasticSearch.Template.Format(elasticCreds.Username, elasticCreds.Password);
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("ApplicationName", _environment.ApplicationName)
            .Destructure.ToMaximumDepth(4)
            .Destructure.ToMaximumCollectionCount(25)
            .Destructure.ToMaximumStringLength(500)
            .WriteTo.Console(LogEventLevel.Information, theme: AnsiConsoleTheme.Code)
            .WriteTo.File("Logs/log.txt", restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Hour, fileSizeLimitBytes: 1024L * 1024 * 1024 * 10)
            .WriteTo.Elasticsearch(elasticUrl, 
                indexFormat: $"{_environment.EnvironmentName}-{_environment.ApplicationName}-{{0:yyyy.MM.dd}}".ToLower(),
                restrictedToMinimumLevel: LogEventLevel.Information,
                deadLetterIndexName: $"{_environment.EnvironmentName}-deadletter-{{0:yyyy.MM.dd}}",
                typeName: null,
                autoRegisterTemplateVersion: AutoRegisterTemplateVersion.ESv7)
            .ReadFrom.Configuration(_configuration)
            .CreateLogger();

        //Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
    }
}
