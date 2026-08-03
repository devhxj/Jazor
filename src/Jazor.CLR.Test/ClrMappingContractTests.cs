using System.Reflection;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class ClrMappingContractTests
{
    public static IEnumerable<TestDataRow<string>> ImportMappings
        => ClrRuntimeMappingCatalog.Imports.Select(static mapping => new TestDataRow<string>(mapping.Id)
        {
            DisplayName = mapping.Id
        });

    [TestMethod]
    public void ImportMappings_HaveRuntimeModuleAndExecutableBody()
    {
        var failures = new List<string>();
        foreach (var type in typeof(Jazor.CLR.RuntimeModule).Assembly.GetTypes()
            .Where(static type => type.Namespace == "Jazor.CLR")
            .OrderBy(static type => type.FullName, StringComparer.Ordinal))
        {
            var imports = type
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(static member => (Member: member, Mapping: member.GetCustomAttribute<JazorAttribute>()))
                .Where(static entry => entry.Mapping?.Op == Op.Import)
                .ToArray();
            if (imports.Length == 0)
                continue;

            var modulePath = ClrRuntimeMappingCatalog.GetModulePath(type);
            if (string.IsNullOrWhiteSpace(modulePath))
            {
                failures.Add($"{type.FullName}: Import mappings require a non-empty ECMAScript module path.");
                continue;
            }

            foreach (var import in imports)
            {
                var methods = ClrRuntimeMappingCatalog.GetRuntimeMethods(import.Member);
                if (methods.Count == 0)
                    failures.Add($"{type.FullName}.{import.Member.Name}: Import mappings require a method or property accessor implementation.");

                foreach (var method in methods)
                {
                    if (method.GetMethodBody() is null)
                        failures.Add($"{type.FullName}.{method.Name}: Import mappings require an executable method body.");
                }
            }
        }

        Assert.IsEmpty(failures, string.Join(Environment.NewLine, failures));
    }

    [TestMethod]
    public void ImportMappingCatalog_HasUniqueMemberContractsAndStableIds()
    {
        var imports = ClrRuntimeMappingCatalog.Imports;

        Assert.IsNotEmpty(imports);
        Assert.HasCount(imports.Count, imports.Select(static mapping => mapping.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(imports.Count, imports.Select(static mapping => mapping.Member).Distinct(StringComparer.Ordinal));
    }

    [TestMethod]
    [DynamicData(nameof(ImportMappings))]
    public void ImportMapping_HasPublishedCatalogExport(string mappingId)
    {
        var mapping = ClrRuntimeMappingCatalog.GetImportById(mappingId);
        Assert.IsFalse(string.IsNullOrWhiteSpace(mapping.ModulePath));

        var artifact = ClrRuntimeCatalog.Get(mapping.ModulePath);
        Assert.Contains(mapping.ExportName, artifact.GetExportedNames(),
            $"{mapping.Member} must be exported from {mapping.ModulePath} as {mapping.ExportName}.");
    }
}
