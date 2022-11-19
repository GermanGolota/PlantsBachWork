using EventStore.ClientAPI;
using Plants.Presentation;

var host = Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .Build();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
host.Run();