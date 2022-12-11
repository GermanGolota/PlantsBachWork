using EventStore.ClientAPI;
using Plants.Aggregates.Services;
using Plants.Core;
using Plants.Presentation;

var host = Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .Build();

using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
var updater = scope.ServiceProvider.GetRequiredService<IUserUpdater>();
await updater.Create("test", "testPass", "theName", new[]
{
    UserRole.Consumer
});

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
host.Run();