using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plants.Aggregates.Infrastructure;
using Plants.Domain.Infrastructure;
using Plants.Domain.Services;
using Plants.Infrastructure.Config;
using Plants.Initializer;
using Plants.Shared;

var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((ctx, services) =>
        {
            services
                .Configure<ConnectionConfig>(ctx.Configuration.GetSection("Connection"))
                .Configure<AdminUserConfig>(ctx.Configuration.GetSection("Admin"))
                .AddShared()
                .AddDomainInfrastructure()
                .AddAggregatesInfrastructure();
            services.AddTransient<MongoDbInitializer>()
                    .AddTransient<EventStoreInitializer>()
                    .AddSingleton<IIdentityProvider, ConfigIdentityProvider>()
                    .AddTransient<AdminUserCreator>()
                    .AddTransient<Initializer>();
        })
        .Build();

var initer = host.Services.GetRequiredService<Initializer>();
await initer.InitializeAsync();