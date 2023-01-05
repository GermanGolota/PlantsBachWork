using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plants.Aggregates.Infrastructure;
using Plants.Initializer;
using Plants.Services.Infrastructure;

var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((ctx, services) =>
        {
            services
                .BindConfigSections(ctx.Configuration)
                .AddShared()
                .AddSharedServices()
                .AddDomainInfrastructure()
                .AddAggregatesInfrastructure();
            services.AddTransient<MongoRolesDbInitializer>()
                    .AddTransient<ElasticSearchRolesInitializer>()
                    .AddSingleton<IIdentityProvider, ConfigIdentityProvider>()
                    .AddTransient<AdminUserCreator>()   
                    .AddTransient<Initializer>();
        })
        .Build();

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("Canceling...");
    cts.Cancel();
    e.Cancel = true;
};

var sub = host.Services.GetRequiredService<IEventSubscriptionWorker>();
await sub.StartAsync(cts.Token);
var initer = host.Services.GetRequiredService<Initializer>();
await initer.InitializeAsync(cts.Token);
sub.Stop();
await host.StopAsync(cts.Token);