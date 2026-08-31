using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
        var manifestPath = FindEcmascriptManifest();
        var packageRoot = Path.GetDirectoryName(manifestPath)!;
        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var root = document.RootElement;
        if (root.GetProperty("schemaVersion").GetInt32() != 2 ||
            !string.Equals(root.GetProperty("libraryId").GetString(), "ecmascript", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("ECMAScript must expose the JS-resource manifest schema.");
        }

        var imports = root.GetProperty("imports");
        var artifacts = new List<ClrRuntimeModuleArtifact>();
        foreach (var import in imports.EnumerateObject())
        {
            var entry = import.Value;
            if (!string.Equals(entry.GetProperty("type").GetString(), "module", StringComparison.Ordinal))
                throw new InvalidOperationException($"ECMAScript import '{import.Name}' must be a module resource.");

            var relativeFile = entry.GetProperty("production").GetString()
                ?? throw new InvalidOperationException($"ECMAScript import '{import.Name}' has no production file.");
            const string distPrefix = "dist/";
            if (!relativeFile.StartsWith(distPrefix, StringComparison.Ordinal))
                throw new InvalidOperationException($"ECMAScript import '{import.Name}' must resolve from dist: '{relativeFile}'.");

            var sourcePath = Path.GetFullPath(Path.Combine(
                packageRoot,
                relativeFile.Replace('/', Path.DirectorySeparatorChar)));
            var normalizedPackageRoot = Path.GetFullPath(packageRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!sourcePath.StartsWith(normalizedPackageRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"ECMAScript import '{import.Name}' escapes its package root.");

            artifacts.Add(new ClrRuntimeModuleArtifact(
                "ECMAScript",
                import.Name,
                import.Name,
                import.Name,
                File.ReadAllText(sourcePath).ReplaceLineEndings("\n"),
                entry.GetProperty("productionHash").GetString()
                    ?? throw new InvalidOperationException($"ECMAScript import '{import.Name}' has no production hash.")));
        }

        return artifacts.OrderBy(static artifact => artifact.RelativePath, StringComparer.Ordinal).ToArray();
    }

    private static string FindEcmascriptManifest()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "ECMAScript", "manifest.json");
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new FileNotFoundException("The ECMAScript JS-resource manifest was not found.");
    }
}
