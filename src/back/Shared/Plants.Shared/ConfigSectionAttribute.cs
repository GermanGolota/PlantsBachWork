using Microsoft.Extensions.Options;

namespace Plants.Shared;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class ConfigSectionAttribute : Attribute
{
    public string[] SectionNames { get; set; }
    public string OptionName { get; set; }

    public ConfigSectionAttribute()
    {
        SectionNames = new string[] { };
        OptionName = Options.DefaultName;
    }

    public ConfigSectionAttribute(string sectionName)
    {
        SectionNames = new[] { sectionName };
        OptionName = Options.DefaultName;
    }

    public ConfigSectionAttribute(string[] sectionNames)
    {
        SectionNames = sectionNames;
        OptionName = Options.DefaultName;
    }

    public ConfigSectionAttribute(string optionName, string sectionName)
    {
        SectionNames = new[] { sectionName };
        OptionName = optionName;
    }

    public ConfigSectionAttribute(string optionName, string[] sectionNames)
    {
        SectionNames = sectionNames;
        OptionName = optionName;
    }
}
