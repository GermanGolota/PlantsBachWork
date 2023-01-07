using Plants.Presentation;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .UseSerilog()
        .Build();

var config = host.Services.GetRequiredService<IConfiguration>();
Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();

//Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
host.Run();