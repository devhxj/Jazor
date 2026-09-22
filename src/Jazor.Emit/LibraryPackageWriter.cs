using System.Text.Json;
using System.Text.Json.Nodes;

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
                // External packages are resolved by Deno/npm. Never copy a binding snapshot into
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
                ["dev"] = "vite",
                ["build"] = "vite build"
            },
            // npm/Deno ignore this namespace. It keeps the source and digest that produced an
            // exact dependency available to DenoHost's frozen restore diagnostics.
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
            const string ssrArguments = "--node-modules-dir=manual --frozen-lockfile --no-remote --no-prompt --allow-env=NODE_ENV --allow-read=. --allow-net=127.0.0.1 ssr-entry.js";
            // SSR tasks are ordinary project configuration. Seed missing tasks when enabling
            // SSR, but preserve authored commands and their choice of runtime/watch options.
            scripts["ssr"] ??= "deno run " + ssrArguments;
            scripts["ssr:dev"] ??= "deno run --watch=. --watch-exclude=node_modules,dist --no-clear-screen " + ssrArguments;
            rootPackage["scripts"] = scripts;
        }
        WriteJson(packagePath, rootPackage);
        if (existing is null)
            ViteProjectWriter.Write(workspaceRoot);
        PreserveCompatibleNpmLock(workspaceRoot, rootPackage);
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

    private static void PreserveCompatibleNpmLock(string workspaceRoot, JsonObject package)
    {
        var lockPath = Path.Combine(workspaceRoot, "package-lock.json");
        if (!File.Exists(lockPath))
            return;

        try
        {
            var lockJson = JsonNode.Parse(File.ReadAllText(lockPath))?.AsObject();
            var root = lockJson?["packages"]?[""]?.AsObject();
            if (lockJson is null || root is null ||
                lockJson["lockfileVersion"] is null ||
                !DependenciesEqual(package["dependencies"]?.AsObject(), root["dependencies"]?.AsObject()) ||
                !DependenciesEqual(package["devDependencies"]?.AsObject(), root["devDependencies"]?.AsObject()))
            {
                File.Delete(lockPath);
            }
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            File.Delete(lockPath);
        }
    }

    private static bool DependenciesEqual(JsonObject? expected, JsonObject? actual)
    {
        expected ??= new JsonObject();
        actual ??= new JsonObject();
        return expected.Count == actual.Count && expected.All(pair =>
            actual[pair.Key]?.GetValue<string>() == pair.Value?.GetValue<string>());
    }


}
