using System.Text.Json;
using System.Text.Json.Nodes;
using Jazor.Common;

namespace Jazor.Emit;

/// <summary>
/// Writes the root package project consumed by standard JavaScript tools. Embedded modules are project
/// source files; only external npm/JSR identities become package dependencies.
/// </summary>
internal static class LibraryPackageWriter
{
    private const string PackageProjectName = "@jazor/generated";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static void WritePackageProject(string workspaceRoot, LibraryAssets libraries, bool hasSsrEntry = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRoot);
        ArgumentNullException.ThrowIfNull(libraries);

        var dependencies = new JsonObject();
        var managedPackages = new JsonObject();

        // identity 记录覆盖全部已选绑定（含自有源码 carrier），作为诊断与门禁证据；
        // dependency 只为外部 npm/JSR 包写入。
        foreach (var reference in libraries.PackageReferences.Values
                     .OrderBy(static reference => reference.Name, StringComparer.Ordinal))
        {
            var isExternal = reference.Source is "npm" or "jsr";
            if (isExternal)
            {
                // External packages are resolved by pnpm. Never copy a binding snapshot into
                // node_modules: doing so hides upstream exports/sideEffects and defeats tree
                // shaking.
                AddDependency(dependencies, reference.Name, LibraryPackageIdentity.GetDependencySpecifier(reference));
            }
            // ECMAScript 自有源码 carrier 是项目源码（clr/**、runtime/**、dist/**），按相对
            // import 解析；它不出现在 dependencies 里，但保留 identity 供诊断与门禁比对。
            managedPackages[reference.Name] = new JsonObject
            {
                ["source"] = reference.Source,
                ["version"] = reference.Version,
                ["canonicalName"] = reference.CanonicalName,
                ["specifier"] = reference.Name
            };
            if (!string.IsNullOrWhiteSpace(reference.Integrity))
                managedPackages[reference.Name]! ["integrity"] = reference.Integrity;
        }

        var packagePath = Path.Combine(workspaceRoot, "package.json");
        var existing = File.Exists(packagePath)
            ? JsonNode.Parse(File.ReadAllText(packagePath))!.AsObject()
            : null;
        var rootPackage = existing ?? new JsonObject
        {
            ["name"] = PackageProjectName,
            ["private"] = true,
            ["type"] = "module",
            ["main"] = "./" + ProjectEntryWriter.BrowserEntryFileName,
            ["exports"] = CreateExports(hasSsrEntry),
            ["dependencies"] = dependencies,
            ["devDependencies"] = new JsonObject { ["vite"] = ViteProjectWriter.Version },
            ["scripts"] = new JsonObject
            {
                [JazorArtifactDefaults.DevelopmentTaskName] = "vite",
                ["build"] = "vite build"
            },
            // pnpm ignores this namespace. Preserve the authored source and
            // exact binding identity independently of its npm installation key.
            ["jazor"] = new JsonObject { ["packages"] = managedPackages }
        };
        if (existing is not null)
        {
            // Only binding dependencies are managed by Emit. Authored scripts, tooling and
            // other package fields belong to the standard project and survive regeneration.
            var previousManaged = existing["jazor"]?["packages"]?.AsObject();
            var mergedDependencies = existing["dependencies"]?.AsObject() ?? new JsonObject();
            foreach (var name in previousManaged?.Select(pair => pair.Key).ToArray() ?? [])
                mergedDependencies.Remove(name);
            foreach (var dependency in dependencies)
                mergedDependencies[dependency.Key] = dependency.Value?.DeepClone();
            rootPackage["dependencies"] = mergedDependencies;
            rootPackage["jazor"] = new JsonObject { ["packages"] = managedPackages };
            rootPackage["exports"] = CreateExports(hasSsrEntry);
        }
        if (hasSsrEntry)
        {
            var scripts = rootPackage["scripts"]?.AsObject() ?? new JsonObject();
            // ASP.NET Core imports SSR exports directly through NodeApi. No
            // process-launching ssr/ssr:dev command is needed for new projects.
            scripts["build:ssr"] ??= "vite build --config " + ViteProjectWriter.SsrConfigFileName;
            rootPackage["scripts"] = scripts;
            ViteProjectWriter.WriteSsr(workspaceRoot);
        }
        WriteJson(packagePath, rootPackage);
        // The standard task still owns a missing default config after an SDK upgrade.
        // Authored config files remain untouched; alternate build tools do not acquire Vite files.
        if (rootPackage["scripts"]?["build"]?.GetValue<string>() == "vite build")
            ViteProjectWriter.Write(workspaceRoot);
        if (libraries.PackageReferences.Values.Any(reference => reference.Source == "jsr"))
        {
            var npmrc = Path.Combine(workspaceRoot, ".npmrc");
            var config = File.Exists(npmrc) ? File.ReadAllText(npmrc) : string.Empty;
            if (!config.Split('\n').Any(line => line.TrimStart().StartsWith("@jsr:registry", StringComparison.Ordinal)))
                ProjectFileWriter.Write(npmrc, config.TrimEnd() + "\n@jsr:registry=https://npm.jsr.io\n");
        }
    }

    private static JsonObject CreateExports(bool hasSsrEntry)
    {
        var exports = new JsonObject
        {
            ["."] = "./" + ProjectEntryWriter.BrowserEntryFileName
        };
        if (hasSsrEntry)
        {
            exports["./ssr"] = "./" + ProjectEntryWriter.SsrEntryFileName;
            exports["./hydration"] = "./" + ProjectEntryWriter.HydrationEntryFileName;
        }
        return exports;
    }

    private static void WriteJson(string path, JsonObject value)
        => ProjectFileWriter.Write(path, value.ToJsonString(JsonOptions).ReplaceLineEndings("\n") + "\n");


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

}
