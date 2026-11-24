using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using ECMAScript;

namespace Jazor.Npm;

internal static class WebIdlCarriers
{
    private static readonly Assembly HostAssembly = typeof(Element).Assembly;
    internal static Guid Version => HostAssembly.ManifestModule.ModuleVersionId;
    internal static string Metadata { get; } = JsonSerializer.Serialize(ReadTypes());

    private static SortedDictionary<string, string> ReadTypes()
    {
        var carriers = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var type in HostAssembly.GetExportedTypes())
        {
            if (type.Namespace != "ECMAScript" || !type.IsClass || type.IsNested || type.IsGenericType ||
                type.IsAbstract && type.IsSealed ||
                type.GetCustomAttribute<ECMAScriptAttribute>(inherit: false) is not { Import: null } ||
                type.GetCustomAttribute<DescriptionAttribute>(inherit: false)?.Description is not { } description ||
                !description.StartsWith("@#", StringComparison.Ordinal) || description.Length == 2)
                continue;
            // WebIDL emits the JS name even when a C# name needs to be disambiguated,
            // for example DocumentRef or EventRef. Only one carrier may own a name.
            var name = description[2..];
            if (carriers.TryGetValue(name, out _))
                carriers[name] = "";
            else
                carriers.Add(name, type.Name);
        }
        return carriers;
    }
}
