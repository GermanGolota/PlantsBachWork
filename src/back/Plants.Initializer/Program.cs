using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plants.Aggregates.Infrastructure;
using Plants.Aggregates.Infrastructure.Helper;
using Plants.Services.Infrastructure;
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
                .AddDomain(ctx.Configuration);

            services.AddTransient<MongoRolesDbInitializer>()
                    .AddTransient<ElasticSearchRolesInitializer>()
                    .AddSingleton<IIdentityProvider, ConfigIdentityProvider>()
                    .AddTransient<AdminUserCreator>()
                    .AddTransient<Initializer>()
                    .AddTransient<HealthChecker>();
        })
        .UseSerilog()
        .Build();

host.Services.GetRequiredService<ILoggerInitializer>().Initialize();

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("Canceling...");
    cts.Cancel();
    e.Cancel = true;
};

var check = host.Services.GetRequiredService<HealthChecker>();
await check.WaitForServicesStartupOrTimeout(cts.Token);

var sub = host.Services.GetRequiredService<IEventSubscriptionWorker>();
await sub.StartAsync(cts.Token);
var initer = host.Services.GetRequiredService<Initializer>();
await initer.InitializeAsync(cts.Token);
sub.Stop();
await host.StopAsync(cts.Token);

cts.Cancel();