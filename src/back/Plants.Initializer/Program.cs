using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Plants.Aggregates.Infrastructure;
using Plants.Aggregates.Infrastructure.Abstractions;
using Plants.Aggregates.Infrastructure.Helper;
using Plants.Initializer.Seeding;
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

            services.AddSingleton<MongoRolesDbInitializer>()
                    .AddSingleton<ElasticSearchRolesInitializer>()
                    .AddSingleton<IIdentityProvider, ConfigIdentityProvider>()
                    .AddSingleton<AdminUserCreator>()
                    .AddSingleton<Initializer>()
                    .AddSingleton<HealthChecker>()
                    .AddSingleton<Seeder>()
                    .AddSingleton<IFileProvider>(factory =>
                    {
                        var options = factory.GetRequiredService<IOptions<WebRootConfig>>().Value;
                        return new PhysicalFileProvider(options.Path);
                    })
                    .AddSingleton<IHostingContext, HostingContext>();
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

var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var provider = scope.ServiceProvider;
    var check = provider.GetRequiredService<HealthChecker>();
    await check.WaitForServicesStartupOrTimeout(cts.Token);
    var sub = provider.GetRequiredService<IEventSubscription>();
    await sub.StartAsync(cts.Token);
    var initer = provider.GetRequiredService<Initializer>();
    await initer.InitializeAsync(cts.Token);
    var seeder = provider.GetRequiredService<Seeder>();
    await seeder.SeedAsync(cts.Token);
    sub.Stop();
    await host.StopAsync(cts.Token);
}

cts.Cancel();