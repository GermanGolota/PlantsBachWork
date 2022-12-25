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
                    .AddSingleton<IIdentityProvider, ConfigIdentityProvider>()
                    .AddTransient<AdminUserCreator>()
                    .AddTransient<Initializer>();
        })
        .Build();

var sub = host.Services.GetRequiredService<IEventSubscriptionWorker>();
await sub.StartAsync(CancellationToken.None);
var initer = host.Services.GetRequiredService<Initializer>();
await initer.InitializeAsync();
sub.Stop(CancellationToken.None);