using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace Plants.Domain.Infrastructure;

internal static class ObjectExtensions
{
    internal static object RemoveProperties(this object obj, params string[] names)
    {
        var jobj = JObject.FromObject(obj);
        foreach (var name in names)
        {
            jobj.Property(name)!.Remove();
        }
        var expanded = jobj.ToObject<ExpandoObject>()!;
        dynamic dynamicData = expanded;
        return dynamicData;
    }
}
