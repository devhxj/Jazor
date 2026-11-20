using System.IO.Enumeration;
using System.Text.Json;
using System.Text.Json.Nodes;
using Jolt.Compiler;
using Jolt.Vite;

internal static class JoltLibrary
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly string[] VueGroups = ["peerDependencies", "dependencies", "optionalDependencies", "devDependencies"];

    public static async Task<JsonObject> ReadManifestAsync(string project, CancellationToken token)
    {
        var path = Path.Combine(project, "package.json");
        var manifest = JsonNode.Parse(await File.ReadAllTextAsync(path, token))!.AsObject();
        foreach (var property in new[] { "name", "version" })
            if (manifest[property] is not JsonValue value || !value.TryGetValue<string>(out var text) ||
                string.IsNullOrWhiteSpace(text))
                throw new InvalidDataException($"Jolt library packaging requires '{property}' in {path}.");

        var vue = VueGroups.Select(group => manifest[group]?["vue"]).FirstOrDefault(version => version is not null)
            ?? throw new InvalidDataException($"Declare the Vue version range in {path}.");
        manifest["peerDependencies"] ??= new JsonObject();
        manifest["peerDependencies"]!["vue"] = vue.DeepClone();
        foreach (var group in VueGroups.Skip(1))
            (manifest[group] as JsonObject)?.Remove("vue");
        return manifest;
    }

    public static async Task WriteAsync(string project, string output, JsonObject manifest,
        IReadOnlyList<JoltModule> modules, CancellationToken token, string? bundled = null)
    {
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        var assets = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var styles = modules.Where(module => bundled is null && module.ScopedCss is not null)
            .ToDictionary(module => Path.ChangeExtension(module.Path, ".jazor"), module => module.ScopedCss!, StringComparer.Ordinal);
        var compiledStyles = styles.Count == 0 ? [] : await JoltViteToolchain.CompileScopedStylesAsync(project, styles, token);

        var exports = manifest["exports"] as JsonObject ?? new JsonObject();
        if (manifest["exports"] is not null && (manifest["exports"] is not JsonObject ||
            exports.Any(pair => !pair.Key.StartsWith("./", StringComparison.Ordinal))))
            throw new InvalidDataException("Jolt library packaging generates the root package export. Keep authored exports under explicit subpaths.");

        var authoredFiles = (manifest["files"] as JsonArray)?.Select(item => item!.GetValue<string>()) ?? [];
        foreach (var pattern in authoredFiles)
            await IncludeAssetsAsync(pattern.TrimEnd('/'), required: false);
        foreach (var (_, target) in exports)
            foreach (var path in ExportTargets(target))
                await IncludeAssetsAsync(path, required: true);

        var duplicateNames = modules.GroupBy(module => module.Name, StringComparer.Ordinal)
            .Where(group => group.Count() > 1).Select(group => group.Key).ToHashSet(StringComparer.Ordinal);
        var entry = new List<string>();
        foreach (var module in modules.OrderBy(module => module.Path, StringComparer.Ordinal))
        {
            var modulePath = "components/" + module.Path;
            var declarationPath = Path.ChangeExtension(modulePath, ".d.mts");
            var subpath = "./" + Path.ChangeExtension(module.Path, null);
            if (!exports.TryAdd(subpath, EntryPoint(modulePath, declarationPath)))
                throw new InvalidDataException($"The authored export '{subpath}' overlaps a Jolt component export.");
            if (!duplicateNames.Contains(module.Name))
                entry.Add($"export {{ default as {module.Name} }} from {JsonSerializer.Serialize("./" + modulePath)};");

            var code = module.Content;
            if (compiledStyles.TryGetValue(Path.ChangeExtension(module.Path, ".jazor"), out var css))
            {
                files.Add(Path.ChangeExtension(modulePath, ".css"), css.Code);
                foreach (var asset in css.Assets)
                {
                    var assetPath = "components/" + asset;
                    if (!assets.ContainsKey(assetPath))
                        assets.Add(assetPath, await File.ReadAllBytesAsync(Path.Combine(project, asset), token));
                }
                // The compiler emits the stylesheet import first and maps only the
                // statements after it. Preserve those lines when changing its suffix.
                var suffix = code.IndexOf(".jolt.css", StringComparison.Ordinal);
                code = code.Remove(suffix, ".jolt".Length);
            }
            var sourcePath = Path.ChangeExtension(module.Path, ".jazor");
            var packagedSource = assets.ContainsKey(sourcePath)
                ? Path.GetRelativePath(Path.GetDirectoryName(Path.GetFullPath(modulePath, output))!,
                    Path.GetFullPath(sourcePath, output)).Replace(Path.DirectorySeparatorChar, '/')
                : null;
            if (bundled is null)
            {
                files.Add(modulePath, code);
                var map = JsonNode.Parse(module.SourceMap)!.AsObject();
                map["sources"] = new JsonArray(JsonValue.Create(packagedSource ?? Path.GetFileName(sourcePath)));
                files.Add(modulePath + ".map", map.ToJsonString());
            }
            JoltOutput.AddDeclaration(files, declarationPath, module, packagedSource);
        }
        if (bundled is null)
        {
            foreach (var (path, content) in JoltClrRuntime.ReadModules(modules.SelectMany(module => module.ClrImports)))
                files.Add("components/" + path, content);
        }
        else
        {
            foreach (var file in Directory.EnumerateFiles(bundled, "*", Enumeration()))
            {
                var path = Path.GetRelativePath(bundled, file).Replace(Path.DirectorySeparatorChar, '/');
                if (!assets.TryAdd(path, await File.ReadAllBytesAsync(file, token)))
                    throw new InvalidDataException($"The authored package asset '{path}' overlaps a generated file.");
            }
        }

        var barrel = entry.Count == 0 ? "export {};\n" : string.Join('\n', entry) + "\n";
        files.Add("index.mjs", barrel);
        files.Add("index.d.mts", barrel);

        if (compiledStyles.Count != 0 || assets.Keys.Any(path => path.EndsWith(".css", StringComparison.Ordinal)))
        {
            if (manifest["sideEffects"]?.ToJsonString() == "false")
                manifest["sideEffects"] = new JsonArray();
            if (manifest["sideEffects"] is JsonArray sideEffects)
                sideEffects.Add("**/*.css");
        }
        foreach (var key in new[] { "devDependencies", "scripts", "pnpm", "jolt", "module", "typings" })
            manifest.Remove(key);
        manifest["type"] = "module";
        manifest["main"] = "./index.mjs";
        manifest["types"] = "./index.d.mts";
        exports.Add(".", EntryPoint("index.mjs", "index.d.mts"));
        if (manifest["exports"] is null)
            manifest["exports"] = exports;
        manifest["files"] = new JsonArray(files.Keys.Concat(assets.Keys).Order(StringComparer.Ordinal)
            .Select(path => (JsonNode?)JsonValue.Create(path)).ToArray());
        files.Add("package.json", manifest.ToJsonString(JsonOptions) + "\n");
        foreach (var path in assets.Keys)
            if (files.ContainsKey(path) || path is ".jolt-build.lock" or ".jolt-library.json")
                throw new InvalidDataException($"The authored package asset '{path}' overlaps a generated file.");
        await JoltOutput.WriteFilesAsync(output, ".jolt-library.json", files, assets, token);

        async Task IncludeAssetsAsync(string pattern, bool required)
        {
            var relative = pattern.StartsWith("./", StringComparison.Ordinal) ? pattern[2..] : pattern;
            var searchPath = Path.GetFullPath(relative, project);
            if (Path.IsPathRooted(relative) || Path.GetRelativePath(project, searchPath).Split(Path.DirectorySeparatorChar)[0] == "..")
                throw new InvalidDataException($"Package asset '{pattern}' must be relative to the project.");

            IEnumerable<string> matches;
            var wild = relative.IndexOfAny(['*', '?']);
            if (wild >= 0)
            {
                var directory = relative[..wild].LastIndexOf('/');
                var searchRoot = directory >= 0 ? Path.Combine(project, relative[..directory]) : project;
                matches = Directory.Exists(searchRoot) ? EnumerateAssets(searchRoot)
                    .Where(file => FileSystemName.MatchesSimpleExpression(relative, Relative(file), ignoreCase: false)) : [];
            }
            else if (File.Exists(searchPath))
                matches = [searchPath];
            else
                matches = Directory.Exists(searchPath) ? EnumerateAssets(searchPath) : [];

            var found = false;
            foreach (var file in matches)
            {
                var name = Relative(file);
                var outputRelative = Path.GetRelativePath(output, file);
                if (name == "package.json" || (!Path.IsPathRooted(outputRelative) &&
                    outputRelative.Split(Path.DirectorySeparatorChar)[0] != ".."))
                    continue;
                found = true;
                if (!assets.ContainsKey(name))
                    assets.Add(name, await File.ReadAllBytesAsync(file, token));
            }
            if (required && !found)
                throw new FileNotFoundException($"No package asset matches the authored export target '{pattern}'.");
        }

        IEnumerable<string> EnumerateAssets(string directory)
        {
            var relative = Path.GetRelativePath(output, directory);
            if (!Path.IsPathRooted(relative) && relative.Split(Path.DirectorySeparatorChar)[0] != "..")
                yield break;

            foreach (var file in Directory.EnumerateFiles(directory))
                yield return file;
            foreach (var child in new DirectoryInfo(directory).EnumerateDirectories())
            {
                if (child.Name is "node_modules" or ".git" or ".jolt" or ".jazor" ||
                    child.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    continue;
                foreach (var file in EnumerateAssets(child.FullName))
                    yield return file;
            }
        }

        string Relative(string file) => Path.GetRelativePath(project, file).Replace(Path.DirectorySeparatorChar, '/');
    }

    private static EnumerationOptions Enumeration() => new() { RecurseSubdirectories = true };

    private static JsonObject EntryPoint(string module, string declaration) => new()
    {
        ["types"] = "./" + declaration,
        ["import"] = "./" + module,
    };

    private static IEnumerable<string> ExportTargets(JsonNode? target) => target switch
    {
        JsonObject conditions => conditions.SelectMany(pair => ExportTargets(pair.Value)),
        JsonArray alternatives => alternatives.SelectMany(ExportTargets),
        JsonValue value => [value.GetValue<string>()],
        _ => [],
    };
}
