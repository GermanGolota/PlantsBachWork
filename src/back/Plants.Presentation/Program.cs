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

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
host.Run();