using EventStore.ClientAPI;
using Plants.Presentation;

var host = Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .Build();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
var connection = host.Services.GetRequiredService<IEventStoreConnection>();
await connection.ConnectAsync();
host.Run();