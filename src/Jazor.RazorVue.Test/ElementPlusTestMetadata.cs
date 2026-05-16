using System.Reflection;
using System.Text.Json;
using ECMAScript.ElementPlus;
using ECMAScriptNameAttribute = ECMAScript.ECMAScriptNameAttribute;

namespace Jazor.RazorVue.Test;

internal static class ElementPlusTestMetadata
{
    public static string[] RuntimeComponentExportNames { get; } =
        GetRuntimeComponentExportNames(typeof(ElementPlusComponents));

    public static string[] OfficialComponentExportNames { get; } =
        ReadOfficialInstallableComponentExportNames();

    public static string[] OfficialDirectiveExportNames { get; } =
        ReadOfficialDirectiveExportNames();

    public static string[] StrongAuthoringComponentNames { get; } =
        OfficialComponentExportNames
            .Select(NormalizeAuthoringComponentName)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

    private static string[] GetRuntimeComponentExportNames(Type exportHost)
        => exportHost
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static property => property.PropertyType == typeof(IElementPlusComponent))
            .Select(GetComponentExportName)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

    private static string[] ReadOfficialInstallableComponentExportNames()
    {
        var filePath = FindRepositoryFile(Path.Combine(".tmp", "elementplus-inspect", "package", "es", "component.mjs"));
        var content = File.ReadAllText(filePath);
        var match = System.Text.RegularExpressions.Regex.Match(
            content,
            @"var\s+component_default\s*=\s*\[(?<items>[\s\S]*?)\];",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        if (!match.Success)
            throw new InvalidOperationException("Could not locate the Element Plus installable component baseline.");

        return System.Text.RegularExpressions.Regex.Matches(
                match.Groups["items"].Value,
                @"\bEl[A-Z][A-Za-z0-9]*\b",
                System.Text.RegularExpressions.RegexOptions.CultureInvariant)
            .Select(static item => item.Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray()!;
    }

    private static string[] ReadOfficialDirectiveExportNames()
        => ReadWebTypesArray("attributes")
            .Select(static element => element.TryGetProperty("source", out var source) &&
                                      source.TryGetProperty("symbol", out var symbol)
                ? NormalizeDirectiveExportSymbol(symbol.GetString())
                : null)
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray()!;

    private static string? NormalizeDirectiveExportSymbol(string? symbol)
        => symbol switch
        {
            "ElLoading" => "ElLoadingDirective",
            _ => symbol
        };

    private static string NormalizeAuthoringComponentName(string runtimeExportName)
        => runtimeExportName switch
        {
            "ElSelectV2" => "ElVirtualizedSelect",
            _ => runtimeExportName
        };

    private static string GetComponentExportName(PropertyInfo property)
    {
        foreach (var attribute in property.CustomAttributes)
        {
            if (attribute.AttributeType != typeof(ECMAScriptNameAttribute))
                continue;

            if (attribute.ConstructorArguments.Count == 1 &&
                attribute.ConstructorArguments[0].ArgumentType == typeof(string) &&
                attribute.ConstructorArguments[0].Value is string explicitName &&
                !string.IsNullOrWhiteSpace(explicitName))
            {
                return explicitName;
            }
        }

        return property.Name;
    }

    private static IEnumerable<JsonElement> ReadWebTypesArray(string propertyName)
    {
        var filePath = FindRepositoryFile(Path.Combine(".tmp", "elementplus-inspect", "package", "web-types.json"));
        using var document = JsonDocument.Parse(File.ReadAllText(filePath));
        return document.RootElement
            .GetProperty("contributions")
            .GetProperty("html")
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(static element => element.Clone())
            .ToArray();
    }

    private static string FindRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository file: " + relativePath);
    }
}
