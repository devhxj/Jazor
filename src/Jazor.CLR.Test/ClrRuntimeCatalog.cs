using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Acornima;
using Acornima.Ast;
using ECMAScript;

namespace Jazor.CLR.Test;

internal sealed record ClrRuntimeModuleArtifact(
    string AssemblyName,
    string TypeName,
    string Id,
    string RelativePath,
    string Content,
    string Hash)
{
    public Program Parse() => new Parser().ParseModule(Content);

    public IReadOnlySet<string> GetExportedFunctionNames()
        => Parse().Body
            .OfType<ExportNamedDeclaration>()
            .Select(static export => export.Declaration)
            .OfType<FunctionDeclaration>()
            .Select(static function => function.Id?.Name)
            .Where(static name => name is not null)
            .Select(static name => name!)
            .ToHashSet(StringComparer.Ordinal);

    public IReadOnlySet<string> GetImportedModulePaths()
        => Parse().Body
            .OfType<ImportDeclaration>()
            .Select(static import => import.Source.Value)
            .ToHashSet(StringComparer.Ordinal);

    public FunctionDeclaration GetExportedFunction(string exportName)
        => Parse().Body
            .OfType<ExportNamedDeclaration>()
            .Select(static export => export.Declaration)
            .OfType<FunctionDeclaration>()
            .Single(function => function.Id?.Name == exportName);

    public string ComputeHash()
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Content))).ToLowerInvariant();
}

internal static class ClrRuntimeCatalog
{
    private static readonly IReadOnlyList<ClrRuntimeModuleArtifact> Artifacts = ReadArtifacts();
    private static readonly IReadOnlyDictionary<string, ClrRuntimeModuleArtifact> ArtifactsByPath = Artifacts
        .ToDictionary(static artifact => artifact.RelativePath, StringComparer.Ordinal);

    public static IReadOnlyList<ClrRuntimeModuleArtifact> All => Artifacts;

    public static ClrRuntimeModuleArtifact Get(string relativePath) => ArtifactsByPath[relativePath];

    private static IReadOnlyList<ClrRuntimeModuleArtifact> ReadArtifacts()
    {
        var catalogType = typeof(Global).Assembly.GetType("ECMAScript.Catalog", throwOnError: true)!;
        var getModules = catalogType.GetMethod("GetModules", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ECMAScript.Catalog.GetModules() was not found.");
        var modules = getModules.Invoke(null, null) as IEnumerable
            ?? throw new InvalidOperationException("ECMAScript.Catalog.GetModules() returned no module collection.");
        var artifacts = new List<ClrRuntimeModuleArtifact>();
        foreach (var module in modules)
        {
            var type = module.GetType();
            artifacts.Add(new ClrRuntimeModuleArtifact(
                ReadProperty("AssemblyName"),
                ReadProperty("TypeName"),
                ReadProperty("Id"),
                ReadProperty("RelativePath"),
                ReadProperty("Content"),
                ReadProperty("Hash")));

            string ReadProperty(string propertyName)
                => type.GetProperty(propertyName)?.GetValue(module) as string
                    ?? throw new InvalidOperationException($"Catalog module property '{propertyName}' was not found.");
        }

        return artifacts.OrderBy(static artifact => artifact.RelativePath, StringComparer.Ordinal).ToArray();
    }
}
