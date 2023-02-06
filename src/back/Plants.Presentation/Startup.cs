using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Plants.Domain.Presentation;

namespace Plants.Presentation;

public class Startup
{
    private const string DevPolicyName = "dev";
    private const string ProdPolicyName = "prod";

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton<IHostingContext, HostingContext>()
            .AddPlantsConfiguration(Configuration)
            .AddShared()
            .AddSharedServices()
            .AddDomainInfrastructure()
            .AddAggregatesInfrastructure()
            .AddHostedService<EventStoreHostedService>()
            .AddJwtAuthorization(Configuration)
            .AddWebRootFileProvider()
            .AddPlantsSwagger()
            .AddSignalR()
            .Services
            .AddTransient<INotificationSender, NotificationSender>()
            .AddControllers()
            .AddJsonOptions(_ =>
            {
                _.JsonSerializerOptions.Converters.AddOneOfConverter();
            });

        services.AddHealthChecks()
            .AddDomainHealthChecks(Configuration);

        services.AddHealthChecksUI()
            .AddInMemoryStorage();

        services.AddCors(opt =>
        {
            opt.AddPolicy(DevPolicyName, options =>
            {
                options.AllowAnyMethod()
                       .AllowAnyHeader()
                       .SetIsOriginAllowed(origin => true)
                       .AllowCredentials();
            });

            opt.AddPolicy(ProdPolicyName, options =>
            {
                var config = Configuration["AllowedHosts"]!;
                if (config == "*")
                {
                    options.SetIsOriginAllowed(_ => true);
                }
                else
                {
                    options.WithOrigins(config);
                }
                options
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UsePlantsSwagger();
            app.UseCors(DevPolicyName);
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
            app.UseCors(ProdPolicyName);
        }

        app.UseHealthChecks("/health", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseHealthChecksUI(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-ui-api";
        });

#if !DEBUG
        app.UseHttpsRedirection();
#endif
        app.UseAuthentication();
        app.UseStaticFiles();

        app.UseExceptionHandler(handler => handler.UseCustomErrors(env));

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHub<NotificationHub>("/commandsnotifications");
        });
    }
}
