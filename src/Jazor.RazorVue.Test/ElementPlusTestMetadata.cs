using System.Reflection;
using System.Text.Json;
using ECMAScript.ElementPlus;

namespace Jazor.RazorVue.Test;

internal static class ElementPlusTestMetadata
{
    public static string[] RuntimeComponentExportNames { get; } =
        GetRuntimeComponentExportNames(typeof(ElementPlusComponents));

    public static string[] OfficialComponentExportNames { get; } =
        ReadOfficialComponentExportNames();

    public static string[] OfficialDirectiveExportNames { get; } =
        ReadOfficialDirectiveExportNames();

    public static string[] StrongAuthoringComponentNames { get; } =
        OfficialComponentExportNames
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

    private static string[] GetRuntimeComponentExportNames(Type exportHost)
        => exportHost
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static property => property.PropertyType == typeof(IElementPlusComponent))
            .Select(static property => property.Name)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

    private static string[] ReadOfficialComponentExportNames()
        => ReadWebTypesArray("vue-components")
            .Select(static element => ReadExportSymbol(element))
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Where(static name => !string.Equals(name, "ElOwn", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray()!;

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

    private static string? ReadExportSymbol(JsonElement component)
    {
        if (component.TryGetProperty("source", out var source) &&
            source.TryGetProperty("symbol", out var symbol) &&
            symbol.ValueKind == JsonValueKind.String)
        {
            return symbol.GetString();
        }

        if (component.TryGetProperty("name", out var nameProperty) &&
            nameProperty.ValueKind == JsonValueKind.String)
        {
            var tagName = nameProperty.GetString();
            if (!string.IsNullOrWhiteSpace(tagName))
            {
                return string.Concat(
                    tagName!
                        .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(static segment => char.ToUpperInvariant(segment[0]) + segment[1..]));
            }
        }

        return null;
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
