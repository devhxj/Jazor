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
        var embeddedProjections = new Dictionary<string, PackageProjection>(StringComparer.Ordinal);
        foreach (var projection in projections.Values)
        {
            if (string.Equals(projection.Reference.Source, "embedded-mjs", StringComparison.Ordinal))
            {
                WriteEmbeddedPackage(workspaceRoot, projection);
                var localPath = GetEmbeddedDependencyPath(projection);
                AddDependency(dependencies, projection.Reference.Name, "file:./" + localPath);
                embeddedProjections[projection.Reference.Name] = projection;
            }
            else
            {
                // External packages are resolved by Deno/npm. Never copy a binding snapshot into
                // node_modules: doing so hides upstream exports/sideEffects and defeats tree
                // shaking.
                AddDependency(dependencies, projection.Reference.Name, GetDependencySpecifier(projection.Reference));
            }

            managedPackages[projection.Reference.Name] = new JsonObject
            {
                ["source"] = projection.Reference.Source,
                ["version"] = projection.Reference.Version
            };
            if (!string.IsNullOrWhiteSpace(projection.Reference.Integrity))
                managedPackages[projection.Reference.Name]! ["integrity"] = projection.Reference.Integrity;
        }

        // Dependencies that are declared by an external entry but have no local manifest still
        // belong in the root project. They are intentionally absent from PackageProjections.
        foreach (var reference in libraries.PackageReferences.Values
                     .Where(static reference => reference.Source is "npm" or "jsr")
                     .OrderBy(static reference => reference.Name, StringComparer.Ordinal))
        {
            AddDependency(dependencies, reference.Name, GetDependencySpecifier(reference));
            if (managedPackages[reference.Name] is null)
            {
                managedPackages[reference.Name] = new JsonObject
                {
                    ["source"] = reference.Source,
                    ["version"] = reference.Version
                };
                if (!string.IsNullOrWhiteSpace(reference.Integrity))
                    managedPackages[reference.Name]! ["integrity"] = reference.Integrity;
            }
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
        // package-lock.json is an npm lock graph, not a second hand-written description of
        // the Deno graph. Emit it only when every dependency is local and therefore complete
        // from the materialized project itself. External npm/JSR graphs are resolved by Deno
        // from package.json and frozen in deno.lock; a partial lock would make Deno reject the
        // project before it can resolve the real transitive package graph.
        if (CanWriteCompleteNpmLock(libraries.PackageReferences.Values))
        {
            WritePackageLock(
                workspaceRoot,
                rootPackage,
                embeddedProjections,
                libraries.PackageReferences.Values);
        }
        else
        {
            DeleteFile(Path.Combine(workspaceRoot, "package-lock.json"));
        }
    }

    private static bool CanWriteCompleteNpmLock(IEnumerable<LibraryPackageReference> references)
        => references.All(static reference => reference.Source == "embedded-mjs");

    private static void DeleteFile(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
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

        var localPackageRoot = GetSafePath(workspaceRoot, GetEmbeddedPackagePath(package));
        if (!string.Equals(
                Path.GetFullPath(sourceRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                Path.GetFullPath(localPackageRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase))
        {
            CopyDirectory(sourceRoot, localPackageRoot);
        }
        WritePackageManifest(localPackageRoot, package);
    }

    private static string GetEmbeddedPackagePath(PackageProjection package)
        => package.RootRelativePath;

    private static string GetEmbeddedDependencyPath(PackageProjection package)
        => GetEmbeddedPackagePath(package).Replace('\\', '/');

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
        IReadOnlyDictionary<string, PackageProjection> projections,
        IEnumerable<LibraryPackageReference> references)
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
            if (projection.Reference.Source == "embedded-mjs")
                packageEntry["resolved"] = "file:./" + GetEmbeddedDependencyPath(projection);
            if (!string.IsNullOrWhiteSpace(projection.Reference.Integrity))
                packageEntry["integrity"] = projection.Reference.Integrity;
            packageEntries["node_modules/" + projection.Reference.Name] = packageEntry;
        }

        // Keep npm package entries complete enough for npm consumers while Deno remains the
        // authoritative resolver. JSR identities use a Deno protocol and therefore have no
        // npm lock representation; they stay in package.json and the `jazor.packages` record.
        foreach (var reference in references
                     .Where(static reference => reference.Source == "npm")
                     .OrderBy(static reference => reference.Name, StringComparer.Ordinal))
        {
            var key = "node_modules/" + reference.Name;
            if (packageEntries[key] is not null)
                continue;

            var packageEntry = new JsonObject
            {
                ["version"] = reference.Version
            };
            if (LooksLikeConcreteVersion(reference.Version))
            {
                var simpleName = reference.Name[(reference.Name.LastIndexOf('/') + 1)..];
                packageEntry["resolved"] =
                    $"https://registry.npmjs.org/{reference.Name}/-/{simpleName}-{reference.Version}.tgz";
            }

            if (!string.IsNullOrWhiteSpace(reference.Integrity))
                packageEntry["integrity"] = reference.Integrity;
            packageEntries[key] = packageEntry;
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

    private static bool LooksLikeConcreteVersion(string value)
        => value.Length > 0 &&
           char.IsDigit(value[0]) &&
           value.Split('.', StringSplitOptions.RemoveEmptyEntries).Length >= 2;

    private static string GetDependencySpecifier(LibraryPackageReference reference)
    {
        if (reference.Source == "jsr")
        {
            if (reference.Version.StartsWith("jsr:", StringComparison.OrdinalIgnoreCase))
                return reference.Version;
            return "jsr:" + reference.Name + "@" + reference.Version;
        }

        return reference.Version;
    }

    private static void AddDependency(JsonObject dependencies, string name, string value)
    {
        if (dependencies[name] is { } existing)
        {
            if (!string.Equals(existing.GetValue<string>(), value, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Package '{name}' is declared with conflicting dependency identities '{existing.GetValue<string>()}' and '{value}'.");
            }

            return;
        }

        dependencies[name] = value;
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
