using Plants.Aggregates.Infrastructure.Abstractions;
using Plants.Presentation;

var host = Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .Build();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
var context = host.Services.GetRequiredService<IHostingContext>();
host.Run();