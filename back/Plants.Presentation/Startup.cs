using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plants.Core;
using Plants.Infrastructure;
using Plants.Presentation.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plants.Presentation
{
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
            services.AddControllers();
            services.AddInfrastructure(Configuration);
            services.AddSwagger();

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

            services.AddAuthorization(options =>
            {
                UserRole[] allRoles = (UserRole[])Enum.GetValues(typeof(UserRole));
                UserRole[] passedRoles = new UserRole[allRoles.Length];
                for (int i = 0; i < allRoles.Length; i++)
                {
                    var policyName = allRoles[i].ToString();
                    options.AddPolicy(policyName, (policy) => policy.RequireClaim(policyName));
                }
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Plants v1"));
                app.UseCors(DevPolicyName);
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseCors(ProdPolicyName);
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
