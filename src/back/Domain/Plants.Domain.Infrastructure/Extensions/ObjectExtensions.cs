using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace Plants.Domain.Infrastructure;

internal static class ObjectExtensions
{
    internal static object RemoveProperty(this object obj, string name)
    {
        var jobj = JObject.FromObject(obj);
        jobj.Property(name)!.Remove();
        var expanded = jobj.ToObject<ExpandoObject>()!;
        dynamic dynamicData = expanded;
        return dynamicData;
    }
}
