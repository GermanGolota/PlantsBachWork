using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Plants.Shared;

public static class DiExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services)
    {
        services.AddSingleton(_ => Helpers.Type);

        return services;
    }

    public static IServiceCollection BindConfigSections(this IServiceCollection services, IConfiguration config, Type? binderType = null)
    {
        var attributeType = typeof(ConfigSectionAttribute);

        var baseBinderType = typeof(GenericConfigBinder<>);
        binderType ??= baseBinderType;

        if (binderType.IsAssignableToGenericType(baseBinderType) is false)
        {
            throw new InvalidOperationException($"Improper binder type - '{binderType.FullName}' was provided");
        }

        var types = Helpers.Type.Types.Where(type => Attribute.IsDefined(type, attributeType));
        foreach (var type in types)
        {
            var attributes = Attribute.GetCustomAttributes(type, attributeType);
            if (attributes is not null)
            {
                foreach (var sectionAttribute in attributes.Where(_ => _.GetType() == attributeType).Select(_ => (ConfigSectionAttribute)_))
                {
                    var currentBinderType = binderType.MakeGenericType(type);
                    var sectionNames = sectionAttribute.SectionNames;
                    var section = sectionNames.Aggregate(config, (currentConfig, sectionName) => currentConfig.GetSection(sectionName));
                    var bindMethod = currentBinderType.GetMethod(nameof(GenericConfigBinder<IServiceCollection>.Bind))!;
                    var binder = Activator.CreateInstance(currentBinderType, services, section, sectionAttribute.OptionName);
                    bindMethod.Invoke(binder, Array.Empty<object>());
                }
            }
        }
        return services;
    }

}

public class GenericConfigBinder<T> where T : class
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configSection;
    private readonly string _optionName;

    public GenericConfigBinder(IServiceCollection services, IConfiguration configSection, string optionName)
    {
        _services = services;
        _configSection = configSection;
        _optionName = optionName;
    }

    public void Bind()
    {
        var builder = _services.AddOptions<T>(_optionName)
            .Bind(_configSection)
            .ValidateDataAnnotations();
        AdditionalBinding(builder);
    }

    public virtual void AdditionalBinding(OptionsBuilder<T> builder)
    {

    }
}
