using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Plants.Aggregates.Infrastructure;
using Plants.Aggregates.Infrastructure.Abstractions;
using Plants.Aggregates.Infrastructure.Helper;
using Plants.Infrastructure;
using Plants.Presentation.Extensions;
using Plants.Presentation.HostedServices;
using Plants.Presentation.Middleware;
using Plants.Presentation.Services;
using Plants.Services.Infrastructure;

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
        services.AddMediatR(typeof(Plants.Application.AssemblyTag).Assembly);
        services
            .AddSingleton<IHostingContext, HostingContext>()
            .AddPlantsConfiguration(Configuration)
            .AddShared()
            .AddSharedServices()
            .AddInfrastructure(Configuration)
            .AddDomainInfrastructure()
            .AddAggregatesInfrastructure()
            .AddHostedService<EventStoreHostedService>()
            .AddWebRootFileProvider()
            .AddPlantsSwagger()
            .AddControllers()
            .AddJsonOptions(_ =>
            {
                _.JsonSerializerOptions.Converters.AddOneOfConverter();
            });

        services.AddHealthChecks()
            .AddDomain(Configuration);

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
                var config = Configuration["AllowedHosts"];
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
