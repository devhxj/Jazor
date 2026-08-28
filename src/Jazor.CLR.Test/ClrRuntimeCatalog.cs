using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Acornima;
using Acornima.Ast;
using ECMAScript;

namespace Jazor.CLR.Test;

internal sealed record ClrRuntimeNamedImport(string ModulePath, string ImportedName);

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

    public IReadOnlySet<string> GetExportedNames()
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var export in Parse().Body.OfType<ExportNamedDeclaration>())
        {
            switch (export.Declaration)
            {
                case FunctionDeclaration function when function.Id is not null:
                    names.Add(function.Id.Name);
                    break;
                case ClassDeclaration @class when @class.Id is not null:
                    names.Add(@class.Id.Name);
                    break;
                case VariableDeclaration variables:
                    foreach (var variable in variables.Declarations)
                    {
                        if (variable.Id is not Identifier identifier)
                            throw new NotSupportedException($"Unsupported exported variable pattern: {variable.Id.Type}");

                        names.Add(identifier.Name);
                    }
                    break;
                case null:
                    foreach (var specifier in export.Specifiers)
                    {
                        names.Add(specifier.Exported switch
                        {
                            Identifier identifier => identifier.Name,
                            StringLiteral literal => literal.Value,
                            _ => throw new NotSupportedException(
                                $"Unsupported named export key: {specifier.Exported.Type}")
                        });
                    }
                    break;
                default:
                    throw new NotSupportedException(
                        $"Unsupported named export declaration: {export.Declaration.Type}");
            }
        }

        return names;
    }

    public IReadOnlySet<string> GetImportedModulePaths()
        => Parse().Body
            .OfType<ImportDeclaration>()
            .Select(static import => import.Source.Value)
            .ToHashSet(StringComparer.Ordinal);

    public IReadOnlyList<ClrRuntimeNamedImport> GetNamedImports()
        => Parse().Body
            .OfType<ImportDeclaration>()
            .SelectMany(static import => import.Specifiers
                .OfType<ImportSpecifier>()
                .Select(specifier => new ClrRuntimeNamedImport(
                    import.Source.Value,
                    specifier.Imported switch
                    {
                        Identifier identifier => identifier.Name,
                        StringLiteral literal => literal.Value,
                        _ => throw new NotSupportedException(
                            $"Unsupported named import key: {specifier.Imported.Type}")
                    })))
            .ToArray();

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
        var catalogType = typeof(Global).Assembly.GetType("Jazor.Artifacts.RuntimeProviderCatalog", throwOnError: true)!;
        var getModules = catalogType.GetMethod("GetModules", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Jazor.Artifacts.RuntimeProviderCatalog.GetModules() was not found.");
        var modules = getModules.Invoke(null, null) as IEnumerable
            ?? throw new InvalidOperationException("Jazor.Artifacts.RuntimeProviderCatalog.GetModules() returned no module collection.");
        var artifacts = new List<ClrRuntimeModuleArtifact>();
        foreach (var module in modules)
        {
            var type = module.GetType();
            artifacts.Add(new ClrRuntimeModuleArtifact(
                ReadProperty("AssemblyName") ?? typeof(Global).Assembly.GetName().Name ?? "ECMAScript",
                ReadRequiredProperty("TypeName"),
                ReadRequiredProperty("Id"),
                ReadRequiredProperty("RelativePath"),
                ReadRequiredProperty("Content"),
                ReadRequiredProperty("Hash")));

            string? ReadProperty(string propertyName)
                => type.GetProperty(propertyName)?.GetValue(module) as string;

            string ReadRequiredProperty(string propertyName)
                => ReadProperty(propertyName)
                    ?? throw new InvalidOperationException($"Catalog module property '{propertyName}' was not found.");
        }

        return artifacts.OrderBy(static artifact => artifact.RelativePath, StringComparer.Ordinal).ToArray();
    }
}
