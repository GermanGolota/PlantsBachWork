using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plants.Initializer;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((ctx, services) =>
        {
            services
                .BindConfigSections(ctx.Configuration)
                .AddShared()
                .AddSharedServices()
                .AddDomainInfrastructure()
                .AddAggregatesInfrastructure();

            services.AddHealthChecks()
                .AddDomainHealthChecks(ctx.Configuration)
                .AddAggregatesHealthChecks(ctx.Configuration);

            services.AddSingleton<MongoRolesDbInitializer>()
                    .AddSingleton<ElasticSearchRolesInitializer>()
                    .AddSingleton<IIdentityProvider, ConfigIdentityProvider>()
                    .AddSingleton<AdminUserCreator>()
                    .AddSingleton<Initializer>()
                    .AddSingleton<Seeder>()
                    .AddSingleton<INotificationSender, MockNotificationSender>();
        })
        .UseSerilog()
        .Build();

host.Services.GetRequiredService<ILoggerInitializer>().Initialize();
await host.Services.GetRequiredService<IBlobStoragesInitializer>().Initialize(CancellationToken.None);

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("Canceling...");
    cts.Cancel();
    e.Cancel = true;
};

var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var provider = scope.ServiceProvider;
    var check = provider.GetRequiredService<IHealthChecker>();
    await check.WaitForServicesStartupOrTimeout(cts.Token);
    var sub = provider.GetRequiredService<IEventSubscription>();

    await sub.StartAsync(cts.Token);

    var command = provider.GetRequiredService<CommandHelper>();
    var result = await command.SendAndWaitAsync(
        factory => factory.Create<InitializationRequestedCommand, StartupStatus>(StartupStatus.StartupId),
        meta => new InitializationRequestedCommand(meta),
        cts.Token);

    sub.Stop();

    var logger = provider.GetRequiredService<ILogger<Program>>();
    result.Match(succ =>
    {
        logger.LogInformation("Successfully initialized");
    }, fail =>
    {
        if (fail.Reasons.All(_ => _.Contains("Already processed")))
        {
            logger.LogInformation("Already initialized - skipping initialization");
        }
        else
        {
            throw new Exception(String.Join('\n', fail.Reasons));
        }
    });

}

cts.Cancel();