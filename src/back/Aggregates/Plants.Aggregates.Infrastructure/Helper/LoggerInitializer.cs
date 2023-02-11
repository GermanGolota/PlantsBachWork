using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.SystemConsole.Themes;

namespace Plants.Aggregates.Infrastructure;

internal sealed class LoggerInitializer : ILoggerInitializer
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
            .WriteTo.Elasticsearch(
               new ElasticsearchSinkOptions(new Uri(elasticUrl))
               {
                   IndexFormat = $"{_environment.EnvironmentName}-{_environment.ApplicationName}-{{0:yyyy.MM.dd}}".ToLower(),
                   MinimumLogEventLevel = LogEventLevel.Information,
                   DeadLetterIndexName = $"{_environment.EnvironmentName}-deadletter-{{0:yyyy.MM.dd}}",
                   TypeName = null,
                   AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                   ModifyConnectionSettings = x => x.BasicAuthentication(elasticCreds.Username, elasticCreds.Password)
               })
            .ReadFrom.Configuration(_configuration)
            .CreateLogger();

        //Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
    }
}
