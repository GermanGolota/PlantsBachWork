using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Plants.Presentation.Config
{
    public static class DIExtensions
    {
        /// <summary>
        /// Would bind a section, that corresponds to linear subdivisioning of 
        /// config into sections using <param name="sectionNames"></param>
        /// If no section names is provided, then an entire config would be used
        /// </summary>
        public static IServiceCollection BindConfigSection<T>(this IServiceCollection services,
          IConfiguration config, params string[] sectionNames) where T : class
        {
            services.Configure<T>(options =>
            {
                sectionNames
                    .Aggregate(config, (config, sectionName) => config.GetSection(sectionName))
                    .Bind(options);
            });
            return services;
        }
    }
}
