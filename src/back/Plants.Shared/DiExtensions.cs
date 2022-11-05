using Microsoft.Extensions.DependencyInjection;

namespace Plants.Shared;

public static class DiExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services)
    {
        services.AddSingleton(_ => Helpers.Type);

        return services;
    }
}
