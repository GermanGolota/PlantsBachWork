namespace Plants.Shared;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ConfigSectionAttribute : Attribute
{
    public string[] SectionNames { get; set; }
    public ConfigSectionAttribute(params string[] sectionNames)
    {
        SectionNames = sectionNames;
    }
}
