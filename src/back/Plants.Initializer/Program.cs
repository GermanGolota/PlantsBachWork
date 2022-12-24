using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plants.Aggregates.Infrastructure;
using Plants.Domain.Infrastructure;
using Plants.Domain.Infrastructure.Config;
using Plants.Domain.Services;
using Plants.Initializer;
using Plants.Services.Infrastructure;
using Plants.Services.Infrastructure.Config;
using Plants.Shared;

var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((ctx, services) =>
        {
            var config = ctx.Configuration;
            services
                .Configure<ConnectionConfig>(config.GetSection(ConnectionConfig.Section))
                .Configure<AuthConfig>(config.GetSection(AuthConfig.Section))
                .Configure<UserConfig>(UserConstrants.Admin, config.GetSection(UserConstrants.Admin))
                .Configure<UserConfig>(UserConstrants.NewAdmin, config.GetSection(UserConstrants.NewAdmin))
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