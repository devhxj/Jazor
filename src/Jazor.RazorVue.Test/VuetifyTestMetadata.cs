using System.Reflection;
using ECMAScript.Vuetify;

namespace Jazor.RazorVue.Test;

internal static class VuetifyTestMetadata
{
    public static string[] NormalRuntimeComponentExportNames { get; } =
        GetRuntimeComponentExportNames(typeof(VuetifyComponents));

    public static string[] LabsRuntimeComponentExportNames { get; } =
        GetRuntimeComponentExportNames(typeof(VuetifyLabsComponents));

    public static string[] RuntimeComponentExportNames { get; } =
        NormalRuntimeComponentExportNames
            .Concat(LabsRuntimeComponentExportNames)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

    public static string[] RuntimeOnlyAuthoringComponentNames { get; } =
    [
    ];

    public static string[] StrongAuthoringComponentNames { get; } =
        RuntimeComponentExportNames
            .Except(RuntimeOnlyAuthoringComponentNames, StringComparer.Ordinal)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

    private static string[] GetRuntimeComponentExportNames(Type exportHost)
        => exportHost
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static property => property.PropertyType == typeof(IVuetifyComponent))
            .Select(static property => property.Name)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();
}
