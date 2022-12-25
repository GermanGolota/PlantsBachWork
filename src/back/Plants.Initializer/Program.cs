using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plants.Aggregates.Infrastructure;
using Plants.Domain.Infrastructure;
using Plants.Domain.Services;
using Plants.Initializer;
using Plants.Services.Infrastructure;
using Plants.Shared;

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

var initer = host.Services.GetRequiredService<Initializer>();
await initer.InitializeAsync();