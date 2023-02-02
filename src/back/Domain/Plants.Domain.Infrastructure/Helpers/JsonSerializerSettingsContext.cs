using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Plants.Domain.Infrastructure;

public static class JsonSerializerSettingsContext
{
    private static JsonSerializerSettings _settings = new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver() };

    public static JsonSerializerSettings Settings => _settings;

    private class PrivateSetterContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(
            MemberInfo member,
            MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;
        }
    }
}
