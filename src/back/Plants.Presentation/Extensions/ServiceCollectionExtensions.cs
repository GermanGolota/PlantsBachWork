using Plants.Shared;

namespace Plants.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection BindConfigSections(this IServiceCollection services, IConfiguration config)
    {
        var attributeType = typeof(ConfigSectionAttribute);
        var types = Helpers.Type.Types.Where(type => Attribute.IsDefined(type, attributeType));

        var binderType = typeof(GenericConfigBinder<>);

        foreach (var type in types)
        {
            var attribute = Attribute.GetCustomAttribute(type, attributeType);
            if (attribute is not null && attribute is ConfigSectionAttribute sectionAttribute)
            {
                var currentBinderType = binderType.MakeGenericType(type);
                var sectionNames = sectionAttribute.SectionNames;
                var section = sectionNames.Aggregate(config, (currentConfig, sectionName) => currentConfig.GetSection(sectionName));
                var bindMethod = currentBinderType.GetMethod(nameof(GenericConfigBinder<IServiceCollection>.Bind))!;
                var binder = Activator.CreateInstance(currentBinderType, services, section);
                bindMethod.Invoke(binder, Array.Empty<object>());
            }
        }
        return services;
    }

    private class GenericConfigBinder<T> where T : class
    {
        private readonly IServiceCollection _services;
        private readonly IConfiguration _configSection;

        public GenericConfigBinder(IServiceCollection services, IConfiguration configSection)
        {
            _services = services;
            _configSection = configSection;
        }

        public void Bind()
        {
            _services.AddOptions<T>()
                .Bind(_configSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }
    }
}
