using Plants.Presentation;

var host = CreateHostBuilder(args).Build();
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
host.Run();

static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
}