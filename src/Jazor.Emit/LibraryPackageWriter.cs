using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Jazor.Emit;

/// <summary>
/// Writes the package project consumed by package-aware tools. Embedded resources remain local
/// packages; npm and JSR identities are declared in the root project for DenoHost restore.
/// </summary>
internal static class LibraryPackageWriter
{
    private const string PackageProjectName = "@jazor/generated";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static void WritePackageProject(string workspaceRoot, LibraryAssets libraries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRoot);
        ArgumentNullException.ThrowIfNull(libraries);

        var projections = CreateProjections(libraries);
        var dependencies = new JsonObject();
        var managedPackages = new JsonObject();
        foreach (var projection in projections.Values)
        {
            if (string.Equals(projection.Reference.Source, "embedded-mjs", StringComparison.Ordinal))
            {
                WriteEmbeddedPackage(workspaceRoot, projection);
                dependencies[projection.Reference.Name] = "file:./packages/" + projection.Reference.Name;
            }
            else
            {
                // Keep the external identity in the root project. The selected local projection
                // gives NetPack an immediate offline graph and can be replaced by DenoHost.
                WriteExternalPackageProjection(workspaceRoot, projection);
                dependencies[projection.Reference.Name] = projection.Reference.Version;
            }

            managedPackages[projection.Reference.Name] = new JsonObject
            {
                ["source"] = projection.Reference.Source,
                ["version"] = projection.Reference.Version,
                ["integrity"] = projection.Reference.Integrity
            };
        }

        var rootPackage = new JsonObject
        {
            ["name"] = PackageProjectName,
            ["private"] = true,
            ["type"] = "module",
            ["dependencies"] = dependencies,
            // npm/Deno ignore this namespace. It keeps the source and digest that produced an
            // exact dependency available to DenoHost's frozen restore diagnostics.
            ["jazor"] = new JsonObject { ["packages"] = managedPackages }
        };
        WriteJson(Path.Combine(workspaceRoot, "package.json"), rootPackage);
        WritePackageLock(workspaceRoot, rootPackage, projections);
    }

    private static SortedDictionary<string, PackageProjection> CreateProjections(LibraryAssets libraries)
    {
        var packages = new SortedDictionary<string, PackageProjection>(StringComparer.Ordinal);
        foreach (var projection in libraries.PackageProjections.Values.OrderBy(static value => value.Reference.Name, StringComparer.Ordinal))
        {
            if (!packages.TryGetValue(projection.Reference.Name, out var package))
            {
                package = new PackageProjection(projection.Reference, projection.RootRelativePath);
                packages.Add(projection.Reference.Name, package);
            }
            else if (!Equals(package.Reference, projection.Reference) ||
                     !string.Equals(package.RootRelativePath, projection.RootRelativePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Package '{projection.Reference.Name}' resolves to conflicting materialized providers.");
            }

            foreach (var (exportName, exportTarget) in projection.Exports)
            {
                if (package.Exports.TryGetValue(exportName, out var existingTarget) &&
                    !string.Equals(existingTarget, exportTarget, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Package '{projection.Reference.Name}' declares conflicting export '{exportName}'.");
                }
                package.Exports[exportName] = exportTarget;
            }
        }

        return packages;
    }

    private static void WriteEmbeddedPackage(string workspaceRoot, PackageProjection package)
    {
        var sourceRoot = GetSafePath(workspaceRoot, package.RootRelativePath);
        if (!Directory.Exists(sourceRoot))
            throw new DirectoryNotFoundException($"Materialized package root was not found: '{package.RootRelativePath}'.");

        var localPackageRoot = GetSafePath(workspaceRoot, "packages/" + package.Reference.Name);
        CopyDirectory(sourceRoot, localPackageRoot);
        WritePackageManifest(localPackageRoot, package);

        // NetPack resolves normal Node package locations. DenoHost can replace this with its
        // restored layout while preserving the same root package.json dependency graph.
        var nodeModulesPackageRoot = GetSafePath(workspaceRoot, "node_modules/" + package.Reference.Name);
        CopyDirectory(localPackageRoot, nodeModulesPackageRoot);
    }

    private static void WriteExternalPackageProjection(string workspaceRoot, PackageProjection package)
    {
        var sourceRoot = GetSafePath(workspaceRoot, package.RootRelativePath);
        if (!Directory.Exists(sourceRoot))
            throw new DirectoryNotFoundException($"Materialized package root was not found: '{package.RootRelativePath}'.");

        var nodeModulesPackageRoot = GetSafePath(workspaceRoot, "node_modules/" + package.Reference.Name);
        CopyDirectory(sourceRoot, nodeModulesPackageRoot);
        WritePackageManifest(nodeModulesPackageRoot, package);
    }

    private static void WritePackageManifest(string packageRoot, PackageProjection package)
    {
        var exports = new JsonObject();
        foreach (var (exportName, exportTarget) in package.Exports)
            exports[exportName] = exportTarget;

        var packageJson = new JsonObject
        {
            ["name"] = package.Reference.Name,
            ["version"] = package.Reference.Version,
            ["type"] = "module",
            ["exports"] = exports
        };
        WriteJson(Path.Combine(packageRoot, "package.json"), packageJson);
    }

    private static void CopyDirectory(string sourceRoot, string targetRoot)
    {
        if (Directory.Exists(targetRoot))
            Directory.Delete(targetRoot, recursive: true);
        foreach (var sourcePath in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories)
                     .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relativePath = Path.GetRelativePath(sourceRoot, sourcePath);
            var targetPath = GetSafePath(targetRoot, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            File.Copy(sourcePath, targetPath, overwrite: false);
        }
    }

    private static void WriteJson(string path, JsonObject value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(
            path,
            value.ToJsonString(JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static void WritePackageLock(
        string workspaceRoot,
        JsonObject rootPackage,
        IReadOnlyDictionary<string, PackageProjection> projections)
    {
        var rootDependencies = rootPackage["dependencies"]?.DeepClone() ?? new JsonObject();
        var packageEntries = new JsonObject
        {
            [""] = new JsonObject
            {
                ["name"] = rootPackage["name"]?.GetValue<string>(),
                ["version"] = "0.0.0",
                ["dependencies"] = rootDependencies
            }
        };

        foreach (var projection in projections.Values)
        {
            var packageEntry = new JsonObject
            {
                ["version"] = projection.Reference.Version
            };
            if (!string.IsNullOrWhiteSpace(projection.Reference.Integrity))
                packageEntry["integrity"] = projection.Reference.Integrity;
            packageEntries["node_modules/" + projection.Reference.Name] = packageEntry;
        }

        var lockFile = new JsonObject
        {
            ["name"] = rootPackage["name"]?.GetValue<string>(),
            ["version"] = "0.0.0",
            ["lockfileVersion"] = 3,
            ["requires"] = true,
            ["packages"] = packageEntries
        };
        WriteJson(Path.Combine(workspaceRoot, "package-lock.json"), lockFile);
    }

    private static string GetSafePath(string root, string relativePath)
    {
        var normalizedRoot = Path.GetFullPath(root);
        normalizedRoot = normalizedRoot.EndsWith(Path.DirectorySeparatorChar)
            ? normalizedRoot
            : normalizedRoot + Path.DirectorySeparatorChar;
        var candidate = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!candidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Package path escapes the workspace: '{relativePath}'.");
        return candidate;
    }

    private sealed record PackageProjection(LibraryPackageReference Reference, string RootRelativePath)
    {
        public SortedDictionary<string, string> Exports { get; } = new(StringComparer.Ordinal);
    }
}
