using Plants.Presentation;
using Plants.Presentation.Services;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .UseSerilog()
        .Build();

host.Services.GetRequiredService<LoggerInitializer>().Initialize();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
host.Run();