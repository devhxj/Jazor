using System.Reflection;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

internal sealed record ClrImportMappingCase(
    string Id,
    string Member,
    string ModulePath,
    string ExportName)
{
    public bool IsExternalRuntime => ModulePath.StartsWith("@", StringComparison.Ordinal);
}

internal static class ClrRuntimeMappingCatalog
{
    private const string ModuleAttributeName = "ECMAScript.ECMAScriptModuleAttribute";

    public static IReadOnlyList<ClrImportMappingCase> Imports { get; } = ReadImports();

    public static ClrImportMappingCase GetImport(string member)
    {
        var matches = Imports
            .Where(mapping => string.Equals(mapping.Member, member, StringComparison.Ordinal))
            .ToArray();
        if (matches.Length == 1)
            return matches[0];

        throw new InvalidOperationException(
            $"Import mapping lookup for '{member}' expected exactly one match but found {matches.Length}.");
    }

    public static ClrImportMappingCase GetImportById(string id)
        => Imports.Single(mapping => string.Equals(mapping.Id, id, StringComparison.Ordinal));

    public static string? GetModulePath(Type type)
        => type.CustomAttributes
            .SingleOrDefault(static attribute => attribute.AttributeType.FullName == ModuleAttributeName)?
            .ConstructorArguments
            .FirstOrDefault()
            .Value as string;

    private static IReadOnlyList<ClrImportMappingCase> ReadImports()
    {
        var imports = new List<ClrImportMappingCase>();
        foreach (var type in typeof(Jazor.CLR.RuntimeModule).Assembly.GetTypes()
            .Where(static type => type.Namespace == "Jazor.CLR")
            .OrderBy(static type => type.FullName, StringComparer.Ordinal))
        {
            var modulePath = GetModulePath(type);
            foreach (var member in type
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .OrderBy(static member => member.MetadataToken))
            {
                var mapping = member.GetCustomAttribute<JazorAttribute>();
                if (mapping?.Op != Op.Import)
                    continue;

                foreach (var method in GetRuntimeMethods(member))
                {
                    var resolvedModulePath = mapping.ModulePath ?? modulePath ?? string.Empty;
                    // Op.Import may publish a deliberate JavaScript export name. The adapter
                    // method name is only the generator fallback when no name was authored.
                    var exportName = string.IsNullOrEmpty(mapping.Value) ? method.Name : mapping.Value;
                    imports.Add(new ClrImportMappingCase(
                        $"{mapping.Member} -> {resolvedModulePath}#{exportName}",
                        mapping.Member,
                        resolvedModulePath,
                        exportName));
                }
            }
        }

        return imports
            .OrderBy(static mapping => mapping.Member, StringComparer.Ordinal)
            .ThenBy(static mapping => mapping.ExportName, StringComparer.Ordinal)
            .ToArray();
    }

    internal static IReadOnlyList<MethodInfo> GetRuntimeMethods(MemberInfo member)
        => member switch
        {
            MethodInfo method => [method],
            PropertyInfo property => property.GetAccessors(nonPublic: true),
            _ => []
        };
}
