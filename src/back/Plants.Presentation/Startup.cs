using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

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
                options.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            opt.AddPolicy(ProdPolicyName, options =>
            {
                var config = Configuration["AllowedHosts"]!;
                options.WithOrigins(config)
                                    .AllowAnyMethod()
                                    .AllowAnyHeader();
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

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseStaticFiles();

        app.UseExceptionHandler(handler => handler.UseCustomErrors(env));

        app.UseRouting();

        app.UseAuthorization();
        app.UseMiddleware<UrlAuthMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
