using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plants.DbMigrator;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddOptions<MigratorConfig>()
                .BindConfiguration("Configurator")
                .ValidateDataAnnotations()
                .ValidateOnStart();
        services.AddTransient<IFileLoader, FileLoader>();
        services.Decorate<IFileLoader, FileLoaderLoggingDecorator>();
        services.AddTransient<IMigrationApplyer, MigrationApplyer>();
        services.Decorate<IMigrationApplyer, MigrationApplyerLoggingDecorator>();
        services.AddTransient<Migrator>();
    })
    .Build();

var migrator = host.Services.GetRequiredService<Migrator>();
await migrator.MigrateAsync();