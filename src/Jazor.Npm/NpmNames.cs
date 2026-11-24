namespace Jazor.Npm;

public static class NpmNames
{
    public static string NamespaceFor(string moduleSpecifier) =>
        "Npm." + string.Join(".", moduleSpecifier.TrimStart('@').Split('/').Select(PascalCase));

    public static string TagFor(string moduleSpecifier, string exportName) =>
        exportName == "default" ? PascalCase(moduleSpecifier.Substring(moduleSpecifier.LastIndexOf('/') + 1)) : exportName;

    public static string ValueNameFor(string exportName) => PascalCase(exportName);

    private static string PascalCase(string value) =>
        string.Concat(value.Split(['-', '_', '.'], StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part.Substring(1)));
}
