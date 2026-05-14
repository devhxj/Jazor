using System.Reflection;
using ECMAScript.TDesign;

namespace Jazor.RazorVue.Test;

internal static class TDesignTestMetadata
{
    public static string[] RuntimeComponentExportNames { get; } =
        GetRuntimeComponentExportNames(typeof(TDesignComponents));

    public static string[] StrongAuthoringComponentNames { get; } =
        RuntimeComponentExportNames
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

    private static string[] GetRuntimeComponentExportNames(Type exportHost)
        => exportHost
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static property => property.PropertyType == typeof(ITDesignComponent))
            .Select(static property => property.Name)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();
}
