using Plants.Aggregates.Infrastructure.HealthCheck;
using Plants.Aggregates.Infrastructure.Helper;
using Plants.Presentation;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .UseSerilog()
        .Build();

host.Services.GetRequiredService<ILoggerInitializer>().Initialize();

var check = host.Services.GetRequiredService<IHealthChecker>();
await check.WaitForServicesStartupOrTimeout(CancellationToken.None);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
host.Run();