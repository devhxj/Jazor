#!/usr/bin/env dotnet run

using System.Formats.Tar;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Nodes;

// Refresh only the inputs and shipped assets. Run the binding generators afterwards
// so reviewed C# type projections are never replaced by guessed TypeScript mappings.
// 仅更新输入与资源；组件契约仍由各自生成器生成并检查。
if (args.Length != 2)
    throw new ArgumentException("Usage: update-vue-binding-inputs.cs -- <vuetify|element-plus|tdesign-vue-next|vue-data-ui> <version>");
var package = args[0];
var version = args[1];
var project = package switch
{
    "vuetify" => "ECMAScript.Vuetify",
    "element-plus" => "ECMAScript.ElementPlus",
    "tdesign-vue-next" => "ECMAScript.TDesign",
    "vue-data-ui" => "ECMAScript.VueDataUi",
    _ => throw new ArgumentException("Unsupported binding package: " + package)
};
var root = Directory.GetCurrentDirectory();
if (!File.Exists(Path.Combine(root, "Jazor.slnx")))
    throw new InvalidOperationException("Run from the repository root.");
var projectRoot = Path.Combine(root, "src", project);
var manifestPath = Path.Combine(projectRoot, "manifest.json");
var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!;
var previousVersion = manifest["version"]!.GetValue<string>();
var upstream = Path.Combine(root, "src", "ECMAScript.Vue.Generator", "upstream", package);
var previousSnapshot = Path.Combine(upstream, previousVersion);
var snapshot = Path.Combine(upstream, version);
var cache = Path.Combine(root, ".tmp", "packages", $"{package}-{version}.tgz");
Directory.CreateDirectory(Path.GetDirectoryName(cache)!);

using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
using var metadata = JsonDocument.Parse(await client.GetStringAsync($"https://registry.npmjs.org/{package}/{version}"));
if (metadata.RootElement.GetProperty("version").GetString() != version)
    throw new InvalidOperationException("Registry returned a different package version.");
var distribution = metadata.RootElement.GetProperty("dist");
var integrity = distribution.GetProperty("integrity").GetString()!;
if (!integrity.StartsWith("sha512-", StringComparison.Ordinal))
    throw new InvalidOperationException("Expected npm SHA-512 integrity metadata.");
var expectedHash = integrity[7..];
if (!File.Exists(cache) || HashArchive(cache) != expectedHash)
{
    Console.WriteLine($"Downloading {package}@{version}");
    await using (var input = await client.GetStreamAsync(distribution.GetProperty("tarball").GetString()!))
    await using (var output = File.Create(cache))
        await input.CopyToAsync(output);
}
if (HashArchive(cache) != expectedHash)
    throw new InvalidOperationException("Downloaded archive does not match npm integrity metadata.");

var files = new Dictionary<string, string>(StringComparer.Ordinal);
if (package is not "tdesign-vue-next" and not "vue-data-ui")
{
    foreach (var file in Directory.EnumerateFiles(previousSnapshot, "*", SearchOption.AllDirectories))
    {
        var relative = Path.GetRelativePath(previousSnapshot, file).Replace('\\', '/');
        if (relative != "contracts.json")
            files.Add(package == "vuetify" && relative == "web-types.json" ? "dist/json/web-types.json" : relative, Path.Combine(snapshot, relative));
    }
}
var assets = package switch
{
    "vuetify" => new[] { ("LICENSE.md", "licenses/LICENSE.md") },
    "element-plus" => [("dist/index.full.min.mjs", "dist/index.full.min.mjs"), ("dist/index.css", "dist/index.css"), ("LICENSE", "licenses/LICENSE")],
    "vue-data-ui" => Array.Empty<(string, string)>(),
    _ => [("LICENSE", "licenses/LICENSE")]
};
foreach (var (source, destination) in assets)
    files.Add(source, Path.Combine(projectRoot, destination));

using (var archive = File.OpenRead(cache))
using (var gzip = new GZipStream(archive, CompressionMode.Decompress))
using (var reader = new TarReader(gzip))
{
    while (reader.GetNextEntry() is { } entry)
    {
        if (!entry.Name.StartsWith("package/", StringComparison.Ordinal) || entry.DataStream is null ||
            !files.Remove(entry.Name[8..], out var destination))
            continue;
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        using var output = File.Create(destination);
        entry.DataStream.CopyTo(output);
    }
}
if (files.Count > 0)
    throw new InvalidOperationException("Missing upstream inputs: " + string.Join(", ", files.Keys));
if (package == "tdesign-vue-next")
{
    var buildRoot = Path.Combine(root, "src", "ECMAScript.Vue.Generator", "tdesign-runtime");
    var buildPackage = JsonNode.Parse(File.ReadAllText(Path.Combine(buildRoot, "package.json")))!;
    if (buildPackage["dependencies"]![package]!.GetValue<string>() != version)
        throw new InvalidOperationException("Update tdesign-runtime/package.json and package-lock.json to the requested version first.");
    await RunAsync(OperatingSystem.IsWindows() ? "cmd.exe" : "npm", buildRoot,
        OperatingSystem.IsWindows() ? ["/d", "/c", "npm.cmd", "ci", "--ignore-scripts", "--no-audit", "--no-fund"] : ["ci", "--ignore-scripts", "--no-audit", "--no-fund"]);
    await BuildTDesignEntriesAsync(root, projectRoot, buildRoot, snapshot, version);
}
if (package == "element-plus")
{
    var buildRoot = Path.Combine(root, "src", "ECMAScript.Vue.Generator", "element-plus-runtime");
    var buildPackage = JsonNode.Parse(File.ReadAllText(Path.Combine(buildRoot, "package.json")))!;
    if (buildPackage["dependencies"]![package]!.GetValue<string>() != version)
        throw new InvalidOperationException("Update element-plus-runtime/package.json and package-lock.json to the requested version first.");
    await RunAsync(OperatingSystem.IsWindows() ? "cmd.exe" : "npm", buildRoot,
        OperatingSystem.IsWindows() ? ["/d", "/c", "npm.cmd", "ci", "--ignore-scripts", "--no-audit", "--no-fund"] : ["ci", "--ignore-scripts", "--no-audit", "--no-fund"]);
    await BuildElementPlusEntriesAsync(root, projectRoot, buildRoot, snapshot, version);
}
if (package == "vuetify")
{
    var contracts = File.ReadAllText(Path.Combine(previousSnapshot, "contracts.json"));
    File.WriteAllText(Path.Combine(snapshot, "contracts.json"), contracts.Replace(
        $"\"upstreamVersion\": \"{previousVersion}\"", $"\"upstreamVersion\": \"{version}\"", StringComparison.Ordinal));

    var buildRoot = Path.Combine(root, "src", "ECMAScript.Vue.Generator", "vuetify-runtime");
    var buildPackage = JsonNode.Parse(File.ReadAllText(Path.Combine(buildRoot, "package.json")))!;
    if (buildPackage["dependencies"]![package]!.GetValue<string>() != version)
        throw new InvalidOperationException("Update vuetify-runtime/package.json and package-lock.json to the requested version first.");
    await RunAsync(OperatingSystem.IsWindows() ? "cmd.exe" : "npm", buildRoot,
        OperatingSystem.IsWindows() ? ["/d", "/c", "npm.cmd", "ci", "--ignore-scripts", "--no-audit", "--no-fund"] : ["ci", "--ignore-scripts", "--no-audit", "--no-fund"]);
    await BuildVuetifyEntriesAsync(root, projectRoot, buildRoot, snapshot, version);
}
if (package == "vue-data-ui")
{
    if (!metadata.RootElement.TryGetProperty("gitHead", out var gitHeadElement) ||
        gitHeadElement.GetString() is not { } gitHead ||
        !Regex.IsMatch(gitHead, "^[0-9a-f]{40}$", RegexOptions.CultureInvariant))
    {
        throw new InvalidOperationException($"{package}@{version} does not declare a valid npm gitHead.");
    }

    var repositoryUrl = metadata.RootElement.GetProperty("repository").GetProperty("url").GetString();
    if (!string.Equals(repositoryUrl, "git+https://github.com/graphieros/vue-data-ui.git", StringComparison.Ordinal))
        throw new InvalidOperationException($"Unexpected vue-data-ui repository metadata: '{repositoryUrl}'.");

    var buildRoot = Path.Combine(root, "src", "ECMAScript.Vue.Generator", "vue-data-ui-runtime");
    await RunAsync(OperatingSystem.IsWindows() ? "cmd.exe" : "npm", buildRoot,
        OperatingSystem.IsWindows() ? ["/d", "/c", "npm.cmd", "ci", "--ignore-scripts", "--no-audit", "--no-fund"] : ["ci", "--ignore-scripts", "--no-audit", "--no-fund"]);
    await BuildVueDataUiEntriesAsync(root, projectRoot, buildRoot, cache, version, gitHead, client);
}
// Entry builders write complete manifests from esbuild's graph. Reload before the common
// version/hash refresh so the final pass validates the generated paths as well.
if (package is "tdesign-vue-next" or "element-plus" or "vuetify" or "vue-data-ui")
    manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!;
manifest["version"] = version;
RefreshHashes(manifest);
File.WriteAllText(manifestPath, manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"Updated {package}: {previousVersion} -> {version}. Run the generators and review contract drift.");

static string HashArchive(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToBase64String(SHA512.HashData(stream));
}

static async Task RunAsync(
    string executable,
    string workingDirectory,
    string[] arguments,
    IReadOnlyDictionary<string, string>? environment = null)
{
    var start = new ProcessStartInfo(executable) { WorkingDirectory = workingDirectory, UseShellExecute = false, CreateNoWindow = true };
    foreach (var argument in arguments) start.ArgumentList.Add(argument);
    if (environment is not null)
    {
        foreach (var (name, value) in environment)
            start.Environment[name] = value;
    }
    using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start " + executable);
    await process.WaitForExitAsync();
    if (process.ExitCode != 0) throw new InvalidOperationException(executable + " failed: " + process.ExitCode);
}

static async Task BuildVueDataUiEntriesAsync(
    string repositoryRoot,
    string projectRoot,
    string buildRoot,
    string npmArchivePath,
    string version,
    string gitHead,
    HttpClient client)
{
    // The npm package exposes component-level JS facades but only one aggregate stylesheet.
    // Build the exact npm gitHead from source so Vite can retain the component's recursive
    // static/dynamic JS closure and its real CSS closure. Netpack then performs the final graph
    // shake; the manifest controls which source graph is allowed to enter that build.
    // npm 包只有全局 CSS，因此必须从其精确 gitHead 重建组件级 JS/CSS 闭包。
    var sourceArchive = Path.Combine(repositoryRoot, ".tmp", "packages", $"vue-data-ui-{gitHead}.source.tgz");
    if (!File.Exists(sourceArchive))
    {
        Console.WriteLine($"Downloading vue-data-ui source at {gitHead}");
        var temporaryArchive = sourceArchive + ".tmp";
        await using (var input = await client.GetStreamAsync(
                         $"https://codeload.github.com/graphieros/vue-data-ui/tar.gz/{gitHead}"))
        await using (var output = File.Create(temporaryArchive))
            await input.CopyToAsync(output);
        File.Move(temporaryArchive, sourceArchive, overwrite: true);
    }

    var sourceRoot = Path.Combine(repositoryRoot, ".tmp", "vue-data-ui-source-" + gitHead);
    ExtractVueDataUiSource(sourceArchive, sourceRoot, gitHead);
    using (var sourcePackage = JsonDocument.Parse(File.ReadAllText(Path.Combine(sourceRoot, "package.json"))))
    {
        if (sourcePackage.RootElement.GetProperty("name").GetString() != "vue-data-ui" ||
            sourcePackage.RootElement.GetProperty("version").GetString() != version)
        {
            throw new InvalidOperationException(
                $"vue-data-ui source {gitHead} does not match npm version {version}.");
        }
    }

    var buildOutput = Path.Combine(repositoryRoot, ".tmp", "vue-data-ui-build-" + version);
    if (Directory.Exists(buildOutput))
        Directory.Delete(buildOutput, recursive: true);
    await RunAsync(
        "node",
        buildRoot,
        ["node_modules/vite/bin/vite.js", "build", "--config", "vite.config.mjs"],
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["JAZOR_VUE_DATA_UI_SOURCE_ROOT"] = sourceRoot,
            ["JAZOR_VUE_DATA_UI_OUTPUT_ROOT"] = buildOutput,
        });

    var viteManifestPath = Path.Combine(buildOutput, ".vite", "manifest.json");
    if (!File.Exists(viteManifestPath))
        throw new InvalidOperationException("Vite did not emit the vue-data-ui build manifest.");

    using var viteManifest = JsonDocument.Parse(File.ReadAllText(viteManifestPath));
    var records = viteManifest.RootElement.EnumerateObject()
        .ToDictionary(static property => property.Name, static property => property.Value, StringComparer.Ordinal);
    var entryRecords = records
        .Where(static pair => pair.Key.StartsWith("src/entries/vue-ui-", StringComparison.Ordinal) &&
                              pair.Key.EndsWith(".js", StringComparison.Ordinal) &&
                              pair.Value.TryGetProperty("isEntry", out var isEntry) && isEntry.GetBoolean())
        .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
        .ToArray();
    var sourceEntries = Directory.EnumerateFiles(Path.Combine(sourceRoot, "src", "entries"), "vue-ui-*.js")
        .Select(Path.GetFileNameWithoutExtension)
        .OrderBy(static name => name, StringComparer.Ordinal)
        .ToArray();
    var builtEntries = entryRecords
        .Select(static pair => Path.GetFileNameWithoutExtension(pair.Key))
        .OrderBy(static name => name, StringComparer.Ordinal)
        .ToArray();
    if (!sourceEntries.SequenceEqual(builtEntries, StringComparer.Ordinal))
    {
        throw new InvalidOperationException(
            "Vite component entries differ from the vue-data-ui source catalog. " +
            $"Missing: {string.Join(", ", sourceEntries.Except(builtEntries, StringComparer.Ordinal))}; " +
            $"extra: {string.Join(", ", builtEntries.Except(sourceEntries, StringComparer.Ordinal))}.");
    }

    var oldManifestPath = Path.Combine(projectRoot, "manifest.json");
    var oldManifest = JsonNode.Parse(File.ReadAllText(oldManifestPath))!.AsObject();
    var oldImports = oldManifest["imports"]!.AsObject();
    var reviewedSpecifiers = oldImports
        .Select(static pair => pair.Key)
        .Where(static specifier => specifier.StartsWith("vue-data-ui/", StringComparison.Ordinal))
        .OrderBy(static specifier => specifier, StringComparer.Ordinal)
        .ToArray();
    var builtSpecifiers = builtEntries
        .Select(static name => "vue-data-ui/" + name)
        .OrderBy(static specifier => specifier, StringComparer.Ordinal)
        .ToArray();
    if (!reviewedSpecifiers.SequenceEqual(builtSpecifiers, StringComparer.Ordinal))
    {
        throw new InvalidOperationException(
            "The vue-data-ui source entry catalog changed. Review the C# binding catalog before updating runtime assets. " +
            $"Missing bindings: {string.Join(", ", builtSpecifiers.Except(reviewedSpecifiers, StringComparer.Ordinal))}; " +
            $"removed upstream: {string.Join(", ", reviewedSpecifiers.Except(builtSpecifiers, StringComparer.Ordinal))}.");
    }

    var stagedDist = Path.Combine(repositoryRoot, ".tmp", "vue-data-ui-dist-" + version);
    if (Directory.Exists(stagedDist))
        Directory.Delete(stagedDist, recursive: true);
    Directory.CreateDirectory(stagedDist);
    var stagedRuntime = Path.Combine(stagedDist, "runtime");
    CopyTree(buildOutput, stagedRuntime, path =>
        path.StartsWith(Path.Combine(buildOutput, ".vite") + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));

    var stagedLicense = Path.Combine(repositoryRoot, ".tmp", "VUE-DATA-UI-LICENSE-" + version);
    ExtractVueDataUiNpmFiles(npmArchivePath, stagedDist, stagedLicense);

    foreach (var preserved in new[] { "jspdf.browser.mjs", "jspdf.browser.mjs.LEGAL.txt" })
    {
        var source = Path.Combine(projectRoot, "dist", preserved);
        if (!File.Exists(source))
            throw new InvalidOperationException($"Required preserved vue-data-ui asset is missing: '{source}'.");
        File.Copy(source, Path.Combine(stagedDist, preserved), overwrite: true);
    }

    var imports = new JsonObject
    {
        ["jspdf"] = oldImports["jspdf"]?.DeepClone()
            ?? throw new InvalidOperationException("The existing vue-data-ui manifest does not provide jspdf."),
    };
    var allReachableOutputs = new HashSet<string>(StringComparer.Ordinal);
    foreach (var (entryKey, entryRecord) in entryRecords)
    {
        var closure = CollectViteClosure(entryKey, records);
        var entryFile = ReadRequiredString(entryRecord, "file", entryKey);
        var componentName = Path.GetFileNameWithoutExtension(entryKey);
        var importSpecifier = "vue-data-ui/" + componentName;
        var modulePaths = new SortedSet<string>(StringComparer.Ordinal);
        var stylePaths = new SortedSet<string>(StringComparer.Ordinal);
        var staticPaths = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var recordKey in closure)
        {
            var record = records[recordKey];
            var file = ReadRequiredString(record, "file", recordKey);
            allReachableOutputs.Add(file);
            AddOutput(file);
            AddStringArray(record, "css", stylePaths);
            AddStringArray(record, "assets", staticPaths);
        }

        foreach (var style in stylePaths)
            allReachableOutputs.Add(style);
        foreach (var asset in staticPaths)
            allReachableOutputs.Add(asset);
        modulePaths.Remove(entryFile);

        var entryRelativePath = "dist/runtime/" + entryFile;
        var entryAbsolutePath = GetViteOutputPath(buildOutput, entryFile);
        var styleFiles = stylePaths
            .Select(style => CreateFile("style", "dist/runtime/" + style, GetViteOutputPath(buildOutput, style)))
            .ToArray();
        var files = modulePaths
            .Select(module => CreateFile(
                "module",
                "dist/runtime/" + module,
                GetViteOutputPath(buildOutput, module),
                "dist/runtime/" + module))
            .Concat(staticPaths.Select(asset => CreateFile(
                "static",
                "dist/runtime/" + asset,
                GetViteOutputPath(buildOutput, asset))))
            .OrderBy(static file => file["path"]!.GetValue<string>(), StringComparer.Ordinal)
            .ToArray();
        var moduleDependencies = new JsonArray(modulePaths
            .Select(static module => JsonValue.Create("dist/runtime/" + module))
            .ToArray());
        var hash = HashFile(entryAbsolutePath);
        imports[importSpecifier] = new JsonObject
        {
            ["type"] = "module",
            ["development"] = entryRelativePath,
            ["production"] = entryRelativePath,
            ["developmentHash"] = hash,
            ["productionHash"] = hash,
            ["developmentDependencies"] = new JsonArray(JsonValue.Create("jspdf"), JsonValue.Create("vue")),
            ["productionDependencies"] = new JsonArray(JsonValue.Create("jspdf"), JsonValue.Create("vue")),
            ["developmentModuleDependencies"] = moduleDependencies.DeepClone(),
            ["productionModuleDependencies"] = moduleDependencies,
            ["developmentStyles"] = new JsonArray(styleFiles.Select(static style => style.DeepClone()).ToArray()),
            ["productionStyles"] = new JsonArray(styleFiles.Select(static style => style.DeepClone()).ToArray()),
            ["files"] = new JsonArray(files.Select(static file => file.DeepClone()).ToArray()),
        };

        void AddOutput(string output)
        {
            var extension = Path.GetExtension(output);
            if (extension.Equals(".js", StringComparison.OrdinalIgnoreCase))
                modulePaths.Add(output);
            else if (extension.Equals(".css", StringComparison.OrdinalIgnoreCase))
                stylePaths.Add(output);
            else
                staticPaths.Add(output);
        }
    }

    var emittedOutputs = Directory.EnumerateFiles(buildOutput, "*", SearchOption.AllDirectories)
        .Where(path => !path.StartsWith(Path.Combine(buildOutput, ".vite") + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        .Select(path => Path.GetRelativePath(buildOutput, path).Replace('\\', '/'))
        .OrderBy(static path => path, StringComparer.Ordinal)
        .ToArray();
    var orphanedOutputs = emittedOutputs.Except(allReachableOutputs, StringComparer.Ordinal).ToArray();
    if (orphanedOutputs.Length > 0)
        throw new InvalidOperationException("Vite emitted outputs outside every component closure: " + string.Join(", ", orphanedOutputs));

    var licenseRelativePath = "licenses/VUE-DATA-UI-LICENSE";
    var manifest = new JsonObject
    {
        ["schemaVersion"] = 2,
        ["libraryId"] = "vue-data-ui",
        ["version"] = version,
        ["imports"] = imports,
        ["requires"] = oldManifest["requires"]?.DeepClone() ?? new JsonObject { ["vue3"] = "^3.5.0" },
        ["styles"] = new JsonArray(),
        ["files"] = new JsonArray(CreateFile("license", licenseRelativePath, stagedLicense)),
    };

    var stagedManifest = Path.Combine(repositoryRoot, ".tmp", "vue-data-ui-manifest-" + version + ".json");
    File.WriteAllText(
        stagedManifest,
        manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n",
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

    ReplaceDirectory(stagedDist, Path.Combine(projectRoot, "dist"));
    File.Copy(stagedLicense, Path.Combine(projectRoot, licenseRelativePath.Replace('/', Path.DirectorySeparatorChar)), overwrite: true);
    File.Move(stagedManifest, oldManifestPath, overwrite: true);

    void AddStringArray(JsonElement record, string propertyName, ISet<string> destination)
    {
        if (!record.TryGetProperty(propertyName, out var values))
            return;
        foreach (var value in values.EnumerateArray())
        {
            var path = value.GetString()
                ?? throw new InvalidOperationException($"Vite manifest '{propertyName}' contains a non-string value.");
            _ = GetViteOutputPath(buildOutput, path);
            destination.Add(path);
        }
    }

    static JsonObject CreateFile(string type, string relativePath, string absolutePath, string? moduleId = null)
    {
        var result = new JsonObject
        {
            ["type"] = type,
            ["path"] = relativePath,
            ["hash"] = HashFile(absolutePath),
        };
        if (moduleId is not null)
            result["moduleId"] = moduleId;
        return result;
    }
}

static HashSet<string> CollectViteClosure(
    string entryKey,
    IReadOnlyDictionary<string, JsonElement> records)
{
    var closure = new HashSet<string>(StringComparer.Ordinal);
    void Visit(string key)
    {
        if (!closure.Add(key))
            return;
        if (!records.TryGetValue(key, out var record))
            throw new InvalidOperationException($"Vite manifest dependency '{key}' does not resolve to an output record.");

        VisitEdges(record, "imports");
        VisitEdges(record, "dynamicImports");
    }

    void VisitEdges(JsonElement record, string propertyName)
    {
        if (!record.TryGetProperty(propertyName, out var dependencies))
            return;
        foreach (var dependency in dependencies.EnumerateArray())
        {
            Visit(dependency.GetString()
                ?? throw new InvalidOperationException($"Vite manifest '{propertyName}' contains a non-string value."));
        }
    }

    Visit(entryKey);
    return closure;
}

static string ReadRequiredString(JsonElement element, string propertyName, string owner)
{
    if (!element.TryGetProperty(propertyName, out var property) || property.GetString() is not { Length: > 0 } value)
        throw new InvalidOperationException($"Vite manifest record '{owner}' does not declare '{propertyName}'.");
    return value;
}

static string GetViteOutputPath(string outputRoot, string relativePath)
{
    var normalizedRoot = Path.GetFullPath(outputRoot) + Path.DirectorySeparatorChar;
    var fullPath = Path.GetFullPath(Path.Combine(outputRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
    if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        throw new InvalidOperationException($"Vite manifest output is missing or escapes its output root: '{relativePath}'.");
    return fullPath;
}

static void ExtractVueDataUiSource(string archivePath, string destinationRoot, string gitHead)
{
    if (Directory.Exists(destinationRoot))
        Directory.Delete(destinationRoot, recursive: true);
    Directory.CreateDirectory(destinationRoot);
    var prefix = "vue-data-ui-" + gitHead + "/";
    var extracted = 0;

    using var archive = File.OpenRead(archivePath);
    using var gzip = new GZipStream(archive, CompressionMode.Decompress);
    using var reader = new TarReader(gzip);
    while (reader.GetNextEntry() is { } entry)
    {
        var name = entry.Name.Replace('\\', '/');
        if (!name.StartsWith(prefix, StringComparison.Ordinal) || entry.DataStream is null)
            continue;
        var relative = name[prefix.Length..];
        if (relative != "package.json" && !relative.StartsWith("src/", StringComparison.Ordinal))
            continue;
        var destination = GetExtractionPath(destinationRoot, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        using var output = File.Create(destination);
        entry.DataStream.CopyTo(output);
        extracted++;
    }

    if (extracted == 0 || !File.Exists(Path.Combine(destinationRoot, "package.json")) ||
        !Directory.Exists(Path.Combine(destinationRoot, "src", "entries")))
    {
        throw new InvalidOperationException($"Source archive does not contain vue-data-ui commit {gitHead}.");
    }
}

static void ExtractVueDataUiNpmFiles(string archivePath, string stagedDist, string stagedLicense)
{
    var foundLicense = false;
    var foundTypes = false;
    var foundLlms = false;
    using var archive = File.OpenRead(archivePath);
    using var gzip = new GZipStream(archive, CompressionMode.Decompress);
    using var reader = new TarReader(gzip);
    while (reader.GetNextEntry() is { } entry)
    {
        if (entry.DataStream is null)
            continue;
        var name = entry.Name.Replace('\\', '/');
        string? destination = null;
        if (name == "package/LICENSE")
        {
            destination = stagedLicense;
            foundLicense = true;
        }
        else if (name == "package/dist/llms.txt")
        {
            destination = Path.Combine(stagedDist, "llms.txt");
            foundLlms = true;
        }
        else if (name.StartsWith("package/dist/types/", StringComparison.Ordinal))
        {
            destination = GetExtractionPath(stagedDist, name["package/dist/".Length..]);
            foundTypes = true;
        }

        if (destination is null)
            continue;
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        using var output = File.Create(destination);
        entry.DataStream.CopyTo(output);
    }

    if (!foundLicense || !foundTypes || !foundLlms)
        throw new InvalidOperationException("The vue-data-ui npm archive is missing its license, types, or llms documentation.");
}

static string GetExtractionPath(string root, string relativePath)
{
    var normalizedRoot = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
    var fullPath = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
    if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException($"Archive path escapes its extraction root: '{relativePath}'.");
    return fullPath;
}

static void CopyTree(string sourceRoot, string destinationRoot, Func<string, bool> skip)
{
    foreach (var source in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
    {
        if (skip(source))
            continue;
        var relative = Path.GetRelativePath(sourceRoot, source);
        var destination = Path.Combine(destinationRoot, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(source, destination, overwrite: true);
    }
}

static void ReplaceDirectory(string stagedDirectory, string destinationDirectory)
{
    var backup = destinationDirectory + ".jazor-backup-" + Guid.NewGuid().ToString("N");
    try
    {
        if (Directory.Exists(destinationDirectory))
            Directory.Move(destinationDirectory, backup);
        Directory.Move(stagedDirectory, destinationDirectory);
        if (Directory.Exists(backup))
            Directory.Delete(backup, recursive: true);
    }
    catch
    {
        if (!Directory.Exists(destinationDirectory) && Directory.Exists(backup))
            Directory.Move(backup, destinationDirectory);
        throw;
    }
    finally
    {
        if (Directory.Exists(backup))
            Directory.Delete(backup, recursive: true);
    }
}

static string HashFile(string path)
    => Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)));

static async Task BuildVuetifyEntriesAsync(
    string repositoryRoot,
    string projectRoot,
    string buildRoot,
    string snapshotRoot,
    string version)
{
    // Vuetify publishes component-group subpaths rather than one subpath per exported component.
    // Resolve every reviewed binding to its official group, then let esbuild record the exact
    // JS/CSS closure for that export. Netpack consumes these modules as one graph and performs
    // the final application-level tree shaking.
    // Vuetify 组件内部依赖必须留在入口闭包内，但未选中的组件及 CSS 不应进入 Netpack 图。
    var packageRoot = Path.Combine(buildRoot, "node_modules", "vuetify");
    var contractsPath = Path.Combine(snapshotRoot, "contracts.json");
    if (!File.Exists(contractsPath))
        throw new InvalidOperationException("Vuetify contracts.json is required before building component entries.");

    using var contractsDocument = JsonDocument.Parse(File.ReadAllText(contractsPath));
    var componentBindings = contractsDocument.RootElement.GetProperty("components")
        .EnumerateArray()
        .Select(component => (
            Family: NormalizeComponentFamily(component.GetProperty("module").GetString()!),
            Export: component.GetProperty("export").GetString()!))
        .Distinct()
        .OrderBy(static entry => entry.Family, StringComparer.Ordinal)
        .ThenBy(static entry => entry.Export, StringComparer.Ordinal)
        .ToArray();
    if (componentBindings.Length == 0)
        throw new InvalidOperationException("Vuetify contracts.json does not contain any component entry.");

    var stableGroups = ReadGroupedExports(Path.Combine(packageRoot, "lib", "components"), "vuetify/components");
    var labsGroups = ReadGroupedExports(Path.Combine(packageRoot, "lib", "labs"), "vuetify/labs");
    var directiveGroups = ReadDirectiveExports(
        Path.Combine(packageRoot, "lib", "directives", "index.js"),
        "vuetify/directives");

    var entries = new List<(string ImportSpecifier, string SourceSpecifier, string Export, string Name)>();
    foreach (var binding in componentBindings)
    {
        var groups = binding.Family == "vuetify/components" ? stableGroups : labsGroups;
        if (!groups.TryGetValue(binding.Export, out var sourceSpecifier))
        {
            // Vuetify can promote a Labs component without changing this binding's public
            // compatibility path. Resolve that path to the one official current source group.
            var alternateGroups = binding.Family == "vuetify/components" ? labsGroups : stableGroups;
            if (!alternateGroups.TryGetValue(binding.Export, out sourceSpecifier))
                throw new InvalidOperationException($"Vuetify {version} does not export '{binding.Export}' from any component group.");
        }

        entries.Add((
            binding.Family + "/" + binding.Export,
            sourceSpecifier,
            binding.Export,
            (binding.Family == "vuetify/components" ? "component-" : "labs-") + binding.Export));
    }
    foreach (var directive in directiveGroups.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
    {
        entries.Add((
            "vuetify/directives/" + directive.Key,
            directive.Value,
            directive.Key,
            "directive-" + directive.Key));
    }

    var staging = Path.Combine(repositoryRoot, ".tmp", "vuetify-build-" + version);
    if (Directory.Exists(staging))
        Directory.Delete(staging, recursive: true);
    var virtualEntries = Path.Combine(buildRoot, ".jazor-vuetify-entries");
    if (Directory.Exists(virtualEntries))
        Directory.Delete(virtualEntries, recursive: true);
    Directory.CreateDirectory(virtualEntries);

    try
    {
        var rootName = "root";
        File.WriteAllText(
            Path.Combine(virtualEntries, rootName + ".mjs"),
            "import \"vuetify/styles\";\nexport * from \"vuetify/framework\";\n");
        foreach (var entry in entries)
        {
            File.WriteAllText(
                Path.Combine(virtualEntries, entry.Name + ".mjs"),
                $"export {{ {entry.Export} }} from \"{entry.SourceSpecifier}\";\n");
        }

        var metaPath = Path.Combine(staging, "entries.meta.json");
        var entryArguments = new[] { Path.Combine(virtualEntries, rootName + ".mjs") }
            .Concat(entries.Select(entry => Path.Combine(virtualEntries, entry.Name + ".mjs")))
            .Concat([
                "--bundle",
                "--format=esm",
                "--splitting",
                "--platform=browser",
                // Netpack 0.8.2 cannot parse an async object method after a newline. Lower only
                // that syntax boundary here; Netpack still owns graph traversal and shaking.
                "--target=es2016",
                "--external:vue",
                "--charset=utf8",
                "--entry-names=entries/[name]",
                "--chunk-names=chunks/[name]-[hash]",
                "--asset-names=assets/[name]-[hash]",
                "--outdir=" + staging,
                "--metafile=" + metaPath]);
        await RunAsync(
            "node",
            buildRoot,
            ["node_modules/esbuild/bin/esbuild", ..entryArguments]);

        using var meta = JsonDocument.Parse(File.ReadAllText(metaPath));
        var outputs = meta.RootElement.GetProperty("outputs")
            .EnumerateObject()
            .ToDictionary(
                property => Path.GetFullPath(property.Name, buildRoot),
                static property => property.Value,
                StringComparer.OrdinalIgnoreCase);

        var distRoot = Path.Combine(projectRoot, "dist");
        if (Directory.Exists(distRoot))
            Directory.Delete(distRoot, recursive: true);
        Directory.CreateDirectory(distRoot);
        CopyTree(staging, distRoot, static path =>
            path.EndsWith("entries.meta.json", StringComparison.OrdinalIgnoreCase));

        var imports = new JsonObject();
        AddManifestEntry("vuetify", rootName);
        foreach (var entry in entries.OrderBy(static entry => entry.ImportSpecifier, StringComparer.Ordinal))
            AddManifestEntry(entry.ImportSpecifier, entry.Name);

        var manifestPath = Path.Combine(projectRoot, "manifest.json");
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
        manifest["version"] = version;
        manifest["imports"] = imports;
        manifest["styles"] = new JsonArray();
        manifest["files"] = new JsonArray(CreateFile(
            "license",
            "licenses/LICENSE.md",
            Path.Combine(projectRoot, "licenses", "LICENSE.md")));
        RefreshHashes(manifest, projectRoot);
        File.WriteAllText(manifestPath, manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");

        void AddManifestEntry(string importSpecifier, string name)
        {
            var outputPath = Path.GetFullPath(Path.Combine(staging, "entries", name + ".js"));
            if (!outputs.TryGetValue(outputPath, out var output))
                throw new InvalidOperationException($"esbuild did not emit Vuetify entry '{importSpecifier}'.");

            var files = CollectOutputClosure(outputPath, output, outputs, buildRoot)
                .Where(path => !string.Equals(path, outputPath, StringComparison.OrdinalIgnoreCase))
                .Where(static path => Path.GetExtension(path).Equals(".js", StringComparison.OrdinalIgnoreCase))
                .Select(path =>
                {
                    var relative = Path.GetRelativePath(staging, path).Replace('\\', '/');
                    return CreateFile("module", "dist/" + relative, Path.Combine(projectRoot, "dist", relative), "dist/" + relative);
                })
                .ToList();

            var styles = new List<JsonObject>();
            if (output.TryGetProperty("cssBundle", out var cssValue))
            {
                var cssPath = Path.GetFullPath(cssValue.GetString()!, buildRoot);
                if (!outputs.TryGetValue(cssPath, out var cssOutput) || !File.Exists(cssPath))
                    throw new InvalidOperationException($"Vuetify entry '{importSpecifier}' CSS bundle '{cssPath}' is missing.");
                var cssRelative = Path.GetRelativePath(staging, cssPath).Replace('\\', '/');
                styles.Add(CreateFile("style", "dist/" + cssRelative, Path.Combine(projectRoot, "dist", cssRelative)));

                files.AddRange(CollectOutputClosure(cssPath, cssOutput, outputs, buildRoot)
                    .Where(path => !string.Equals(path, cssPath, StringComparison.OrdinalIgnoreCase))
                    .Where(static path => Path.GetExtension(path) is not ".css" and not ".js")
                    .Select(path =>
                    {
                        var relative = Path.GetRelativePath(staging, path).Replace('\\', '/');
                        return CreateFile("static", "dist/" + relative, Path.Combine(projectRoot, "dist", relative));
                    }));
            }

            imports[importSpecifier] = CreateEntry(
                "dist/entries/" + name + ".js",
                Path.Combine(distRoot, "entries", name + ".js"),
                styles,
                files.OrderBy(static file => file["path"]!.GetValue<string>(), StringComparer.Ordinal).ToArray());
        }
    }
    finally
    {
        if (Directory.Exists(virtualEntries))
            Directory.Delete(virtualEntries, recursive: true);
    }

    static string NormalizeComponentFamily(string module)
    {
        if (module == "vuetify/components" || module.StartsWith("vuetify/components/", StringComparison.Ordinal))
            return "vuetify/components";
        if (module == "vuetify/labs/components" || module.StartsWith("vuetify/labs/components/", StringComparison.Ordinal))
            return "vuetify/labs/components";
        throw new InvalidOperationException($"Unsupported Vuetify component family '{module}'.");
    }

    static Dictionary<string, string> ReadGroupedExports(string groupsRoot, string sourcePrefix)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var groupDirectory in Directory.EnumerateDirectories(groupsRoot).OrderBy(static path => path, StringComparer.Ordinal))
        {
            var indexPath = Path.Combine(groupDirectory, "index.js");
            if (!File.Exists(indexPath))
                continue;
            var group = Path.GetFileName(groupDirectory);
            foreach (var runtimeExport in ReadNamedReExports(File.ReadAllText(indexPath)))
            {
                var sourceSpecifier = sourcePrefix + "/" + group;
                if (!result.TryAdd(runtimeExport, sourceSpecifier) &&
                    !string.Equals(result[runtimeExport], sourceSpecifier, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Vuetify export '{runtimeExport}' is declared by multiple groups.");
                }
            }
        }
        return result;
    }

    static Dictionary<string, string> ReadDirectiveExports(string indexPath, string sourcePrefix)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var source = File.ReadAllText(indexPath);
        foreach (Match match in Regex.Matches(
                     source,
                     @"(?m)^export\s*\{(?<exports>[^}]*)\}\s*from\s*""\./(?<group>[^/""]+)/index\.js"";",
                     RegexOptions.CultureInvariant))
        {
            foreach (var runtimeExport in ParseExportList(match.Groups["exports"].Value))
                result.Add(runtimeExport, sourcePrefix + "/" + match.Groups["group"].Value);
        }
        if (result.Count == 0)
            throw new InvalidOperationException("Vuetify directive index does not contain any named exports.");
        return result;
    }

    static IEnumerable<string> ReadNamedReExports(string source)
    {
        foreach (Match match in Regex.Matches(
                     source,
                     @"export\s*\{(?<exports>[^}]*)\}\s*from\s*""[^""]+"";",
                     RegexOptions.CultureInvariant))
        {
            foreach (var runtimeExport in ParseExportList(match.Groups["exports"].Value))
                yield return runtimeExport;
        }
        foreach (Match match in Regex.Matches(
                     source,
                     @"export\s+(?:async\s+)?(?:const|let|var|function|class)\s+(?<name>[$A-Za-z_][$\w]*)\b",
                     RegexOptions.CultureInvariant))
        {
            yield return match.Groups["name"].Value;
        }
    }

    static IEnumerable<string> ParseExportList(string exports)
    {
        foreach (var item in exports.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = Regex.Split(item.Trim(), @"\s+as\s+", RegexOptions.CultureInvariant);
            var runtimeExport = parts[^1].Trim();
            if (Regex.IsMatch(runtimeExport, @"^[$A-Za-z_][$\w]*$", RegexOptions.CultureInvariant))
                yield return runtimeExport;
        }
    }

    static JsonObject CreateEntry(
        string relativePath,
        string absolutePath,
        IReadOnlyList<JsonObject> styles,
        IReadOnlyList<JsonObject> files)
    {
        var hash = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(absolutePath)));
        return new JsonObject
        {
            ["type"] = "module",
            ["development"] = relativePath,
            ["production"] = relativePath,
            ["developmentHash"] = hash,
            ["productionHash"] = hash,
            ["developmentDependencies"] = new JsonArray(JsonValue.Create("vue")),
            ["productionDependencies"] = new JsonArray(JsonValue.Create("vue")),
            ["developmentModuleDependencies"] = new JsonArray(),
            ["productionModuleDependencies"] = new JsonArray(),
            ["developmentStyles"] = new JsonArray(styles.Select(static style => style.DeepClone()).ToArray()),
            ["productionStyles"] = new JsonArray(styles.Select(static style => style.DeepClone()).ToArray()),
            ["files"] = new JsonArray(files.Select(static file => file.DeepClone()).ToArray())
        };
    }

    static JsonObject CreateFile(
        string type,
        string relativePath,
        string absolutePath,
        string? moduleId = null)
    {
        var result = new JsonObject
        {
            ["type"] = type,
            ["path"] = relativePath,
            ["hash"] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(absolutePath)))
        };
        if (moduleId is not null)
            result["moduleId"] = moduleId;
        return result;
    }

    static HashSet<string> CollectOutputClosure(
        string outputPath,
        JsonElement output,
        IReadOnlyDictionary<string, JsonElement> outputs,
        string pathBase)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void Visit(string path, JsonElement node)
        {
            if (!result.Add(path) || !node.TryGetProperty("imports", out var importsElement))
                return;
            foreach (var import in importsElement.EnumerateArray())
            {
                if (import.TryGetProperty("external", out var external) && external.GetBoolean())
                    continue;
                if (!import.TryGetProperty("path", out var pathElement))
                    continue;
                var importedPath = Path.GetFullPath(pathElement.GetString()!, pathBase);
                if (outputs.TryGetValue(importedPath, out var importedOutput))
                    Visit(importedPath, importedOutput);
            }
        }
        Visit(outputPath, output);
        return result;
    }

    static void CopyTree(string sourceRoot, string destinationRoot, Func<string, bool> skip)
    {
        foreach (var source in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            if (skip(source))
                continue;
            var relative = Path.GetRelativePath(sourceRoot, source);
            var destination = Path.Combine(destinationRoot, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(source, destination, overwrite: true);
        }
    }

    static void RefreshHashes(JsonNode node, string packageRoot)
    {
        if (node is JsonObject obj)
        {
            foreach (var (key, pathKey) in new[] { ("hash", "path"), ("developmentHash", "development"), ("productionHash", "production") })
            {
                if (!obj.ContainsKey(key) || obj[pathKey] is not JsonValue pathValue)
                    continue;
                obj[key] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(packageRoot, pathValue.GetValue<string>()))));
            }
            foreach (var child in obj.ToArray())
                if (child.Value is not null)
                    RefreshHashes(child.Value, packageRoot);
        }
        else if (node is JsonArray array)
        {
            foreach (var child in array)
                if (child is not null)
                    RefreshHashes(child, packageRoot);
        }
    }
}

static async Task BuildTDesignEntriesAsync(
    string repositoryRoot,
    string projectRoot,
    string buildRoot,
    string snapshotRoot,
    string version)
{
    // Each generated binding points at one logical component export. Build those exports as
    // separate ESM entry points so an import of Button cannot retain Alert (or its CSS). The
    // shared chunks are copied through the manifest entry closure and are never materialized
    // unless their owning component entry is selected.
    // 每个绑定都对应一个独立入口；共享 chunk 通过 manifest 的入口闭包按需复制。
    var bindingsPath = Path.Combine(snapshotRoot, "bindings.json");
    if (!File.Exists(bindingsPath))
        throw new InvalidOperationException("TDesign bindings.json is required before building component entries.");

    using var bindingsDocument = JsonDocument.Parse(File.ReadAllText(bindingsPath));
    var componentEntries = bindingsDocument.RootElement.GetProperty("components")
        .EnumerateArray()
        .Select(component => (
            Module: component.GetProperty("module").GetString()!,
            Export: component.GetProperty("runtimeExport").GetString()!))
        .Distinct()
        .OrderBy(static entry => entry.Module, StringComparer.Ordinal)
        .ThenBy(static entry => entry.Export, StringComparer.Ordinal)
        .ToArray();
    if (componentEntries.Length == 0)
        throw new InvalidOperationException("TDesign bindings.json does not contain any component entry.");

    var staging = Path.Combine(repositoryRoot, ".tmp", "tdesign-build-" + version);
    if (Directory.Exists(staging))
        Directory.Delete(staging, recursive: true);
    var virtualEntries = Path.Combine(buildRoot, ".jazor-tdesign-entries");
    if (Directory.Exists(virtualEntries))
        Directory.Delete(virtualEntries, recursive: true);
    var componentOutput = Path.Combine(staging, "components");
    Directory.CreateDirectory(virtualEntries);
    Directory.CreateDirectory(componentOutput);

    var names = new Dictionary<(string Module, string Export), string>();
    foreach (var entry in componentEntries)
    {
        var name = SanitizeEntryName(entry.Module + "-" + entry.Export);
        if (!names.TryAdd(entry, name))
            throw new InvalidOperationException($"Duplicate TDesign component entry '{entry.Module}/{entry.Export}'.");
        File.WriteAllText(
            Path.Combine(virtualEntries, name + ".mjs"),
            $"export {{ {entry.Export} }} from \"tdesign-vue-next/es/{entry.Module}/index.mjs\";\n");
    }

    var metaPath = Path.Combine(staging, "components.meta.json");
    var entryArguments = componentEntries
        .Select(entry => Path.Combine(virtualEntries, names[entry] + ".mjs"))
        .Concat([
            "--bundle",
            "--format=esm",
            "--splitting",
            "--platform=browser",
            // Netpack 0.8.2 currently misparses async object methods. Lowering package inputs to
            // ES2016 keeps the checked-in ESM portable while Netpack still owns graph shaking.
            "--target=es2016",
            "--external:vue",
            "--charset=utf8",
            "--entry-names=components/[name]",
            "--chunk-names=chunks/[name]-[hash]",
            "--asset-names=assets/[name]-[hash]",
            "--outdir=" + staging,
            "--metafile=" + metaPath]);
    await RunAsync(
        "node",
        buildRoot,
        ["node_modules/esbuild/bin/esbuild", ..entryArguments]);

    var rootOutput = Path.Combine(staging, "root");
    Directory.CreateDirectory(rootOutput);
    await RunAsync(
        "node",
        buildRoot,
        [
            "node_modules/esbuild/bin/esbuild",
            "node_modules/tdesign-vue-next/es/index.mjs",
            "--bundle",
            "--format=esm",
            "--platform=browser",
            "--target=es2016",
            "--external:vue",
            "--charset=utf8",
            "--outfile=" + Path.Combine(rootOutput, "index.js")
        ]);

    var distRoot = Path.Combine(projectRoot, "dist");
    if (Directory.Exists(distRoot))
        Directory.Delete(distRoot, recursive: true);
    Directory.CreateDirectory(distRoot);
    CopyTree(staging, distRoot, static path =>
        path.EndsWith("components.meta.json", StringComparison.OrdinalIgnoreCase));

    using var meta = JsonDocument.Parse(File.ReadAllText(metaPath));
    var outputs = meta.RootElement.GetProperty("outputs")
        .EnumerateObject()
        .ToDictionary(
            property => Path.GetFullPath(property.Name, buildRoot),
            static property => property.Value,
            StringComparer.OrdinalIgnoreCase);

    var imports = new JsonObject
    {
        ["tdesign-vue-next"] = CreateEntry(
            "dist/root/index.js",
            Path.Combine(distRoot, "root", "index.js"),
            [CreateStyle("dist/root/index.css", Path.Combine(distRoot, "root", "index.css"))],
            []),
    };
    foreach (var entry in componentEntries)
    {
        var name = names[entry];
        var outputPath = Path.GetFullPath(Path.Combine(staging, "components", name + ".js"));
        if (!outputs.TryGetValue(outputPath, out var output))
            throw new InvalidOperationException($"esbuild did not emit TDesign entry '{entry.Module}/{entry.Export}'.");

        var cssBundle = output.TryGetProperty("cssBundle", out var cssValue)
            ? Path.GetFullPath(cssValue.GetString()!, buildRoot)
            : null;
        if (cssBundle is not null && !File.Exists(cssBundle))
            throw new InvalidOperationException($"TDesign entry '{entry.Module}/{entry.Export}' CSS bundle '{cssBundle}' is missing.");

        var staticFiles = CollectOutputClosure(outputPath, output, outputs, buildRoot)
            .Where(path => !string.Equals(path, outputPath, StringComparison.OrdinalIgnoreCase))
            .Select(path =>
            {
                var relative = Path.GetRelativePath(staging, path).Replace('\\', '/');
                return CreateModuleFile("dist/" + relative, Path.Combine(projectRoot, "dist", relative));
            })
            .OrderBy(static file => file["path"]!.GetValue<string>(), StringComparer.Ordinal)
            .ToArray();
        var importSpecifier = "tdesign-vue-next/" + entry.Module + "/" + entry.Export;
        IReadOnlyList<JsonObject> styleFiles = cssBundle is null
            ? []
            : [CreateStyle("dist/components/" + name + ".css", Path.Combine(distRoot, "components", name + ".css"))];
        imports[importSpecifier] = CreateEntry(
            "dist/components/" + name + ".js",
            Path.Combine(distRoot, "components", name + ".js"),
            styleFiles,
            staticFiles);

        if (cssBundle is not null)
        {
            var componentCssPath = Path.Combine(distRoot, "components", name + ".css");
            Directory.CreateDirectory(Path.GetDirectoryName(componentCssPath)!);
            File.Copy(cssBundle, componentCssPath, overwrite: true);
        }
    }

    var manifestPath = Path.Combine(projectRoot, "manifest.json");
    var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
    manifest["version"] = version;
    manifest["imports"] = imports;
    manifest["styles"] = new JsonArray();
    RefreshHashes(manifest, projectRoot);
    File.WriteAllText(manifestPath, manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
    Directory.Delete(virtualEntries, recursive: true);

    static string SanitizeEntryName(string value)
        => string.Concat(value.Select(character => char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '-'));

    static JsonObject CreateEntry(
        string relativePath,
        string absolutePath,
        IReadOnlyList<JsonObject> styles,
        IReadOnlyList<JsonObject> files)
    {
        var hash = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(absolutePath)));
        var entry = new JsonObject
        {
            ["type"] = "module",
            ["development"] = relativePath,
            ["production"] = relativePath,
            ["developmentHash"] = hash,
            ["productionHash"] = hash,
            ["developmentDependencies"] = new JsonArray(JsonValue.Create("vue")),
            ["productionDependencies"] = new JsonArray(JsonValue.Create("vue")),
            ["developmentModuleDependencies"] = new JsonArray(),
            ["productionModuleDependencies"] = new JsonArray(),
            ["developmentStyles"] = new JsonArray(styles.Select(static style => style.DeepClone()).ToArray()),
            ["productionStyles"] = new JsonArray(styles.Select(static style => style.DeepClone()).ToArray()),
            ["files"] = new JsonArray(files.Select(static file => file.DeepClone()).ToArray())
        };
        return entry;
    }

    static JsonObject CreateStyle(string relativePath, string absolutePath)
        => new()
        {
            ["type"] = "style",
            ["path"] = relativePath,
            ["hash"] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(absolutePath)))
        };

    static JsonObject CreateModuleFile(string relativePath, string absolutePath)
        => new()
        {
            ["type"] = "module",
            ["path"] = relativePath,
            ["hash"] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(absolutePath))),
            ["moduleId"] = relativePath
        };

    static HashSet<string> CollectOutputClosure(
        string outputPath,
        JsonElement output,
        IReadOnlyDictionary<string, JsonElement> outputs,
        string pathBase)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void Visit(string path, JsonElement node)
        {
            if (!result.Add(path))
                return;
            if (!node.TryGetProperty("imports", out var importsElement))
                return;
            foreach (var import in importsElement.EnumerateArray())
            {
                if (import.TryGetProperty("external", out var external) && external.GetBoolean())
                    continue;
                if (!import.TryGetProperty("path", out var pathElement))
                    continue;
                var importedPath = Path.GetFullPath(pathElement.GetString()!, pathBase);
                if (outputs.TryGetValue(importedPath, out var importedOutput))
                    Visit(importedPath, importedOutput);
            }
        }

        Visit(outputPath, output);
        return result;
    }

    static void CopyTree(string sourceRoot, string destinationRoot, Func<string, bool> skip)
    {
        foreach (var source in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            if (skip(source))
                continue;
            var relative = Path.GetRelativePath(sourceRoot, source);
            var destination = Path.Combine(destinationRoot, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(source, destination, overwrite: true);
        }
    }

    static void RefreshHashes(JsonNode node, string packageRoot)
    {
        if (node is JsonObject obj)
        {
            foreach (var (key, pathKey) in new[] { ("hash", "path"), ("developmentHash", "development"), ("productionHash", "production") })
            {
                if (!obj.ContainsKey(key) || obj[pathKey] is not JsonValue pathValue)
                    continue;
                var path = pathValue.GetValue<string>();
                obj[key] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(packageRoot, path))));
            }

            foreach (var child in obj.ToArray())
                if (child.Value is not null)
                    RefreshHashes(child.Value, packageRoot);
        }
        else if (node is JsonArray array)
        {
            foreach (var child in array)
                if (child is not null)
                    RefreshHashes(child, packageRoot);
        }
    }
}

static async Task BuildElementPlusEntriesAsync(
    string repositoryRoot,
    string projectRoot,
    string buildRoot,
    string snapshotRoot,
    string version)
{
    // Element Plus publishes a large root ESM module plus component-local entry points. Keep
    // the root for plugin/version/directive APIs, and materialize each component from its own
    // package entry so JS and CSS for an unused component never enter the selected closure.
    // Element Plus 的 style/index.mjs imports Sass sources; collect the published CSS edges
    // explicitly so the reproducible esbuild step does not require a Sass compiler.
    var baselinePath = Path.Combine(snapshotRoot, "es", "component.mjs");
    if (!File.Exists(baselinePath))
        throw new InvalidOperationException("Element Plus component.mjs is required before building component entries.");

    var componentModules = ReadElementPlusComponentModules(File.ReadAllText(baselinePath));
    if (componentModules.Count == 0)
        throw new InvalidOperationException("Element Plus component.mjs does not contain component module imports.");

    var staging = Path.Combine(repositoryRoot, ".tmp", "element-plus-build-" + version);
    if (Directory.Exists(staging))
        Directory.Delete(staging, recursive: true);
    var virtualEntries = Path.Combine(buildRoot, ".jazor-element-plus-entries");
    if (Directory.Exists(virtualEntries))
        Directory.Delete(virtualEntries, recursive: true);

    Directory.CreateDirectory(virtualEntries);
    try
    {
        var names = new Dictionary<(string Module, string Export), string>();
        foreach (var entry in componentModules.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            var component = (Module: entry.Value, Export: entry.Key);
            var name = SanitizeEntryName(component.Module + "-" + component.Export);
            if (!names.TryAdd(component, name))
                throw new InvalidOperationException($"Duplicate Element Plus component entry '{component.Module}/{component.Export}'.");

            var cssImports = CollectElementPlusCssImports(buildRoot, component.Module);
            var source = new StringBuilder();
            source.Append("export { ").Append(component.Export).Append(" } from \"element-plus/es/components/")
                .Append(component.Module).AppendLine("/index.mjs\";");
            foreach (var cssImport in cssImports)
                source.Append("import \"").Append(cssImport).AppendLine("\";");
            File.WriteAllText(Path.Combine(virtualEntries, name + ".mjs"), source.ToString());
        }

        var metaPath = Path.Combine(staging, "components.meta.json");
        var entryArguments = componentModules
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .Select(entry => Path.Combine(virtualEntries, names[(entry.Value, entry.Key)] + ".mjs"))
            .Concat([
                "--bundle",
                "--format=esm",
                "--splitting",
                "--platform=browser",
                "--target=es2016",
                "--external:vue",
                "--charset=utf8",
                "--entry-names=components/[name]",
                "--chunk-names=chunks/[name]-[hash]",
                "--asset-names=assets/[name]-[hash]",
                "--outdir=" + staging,
                "--metafile=" + metaPath]);
        await RunAsync(
            "node",
            buildRoot,
            ["node_modules/esbuild/bin/esbuild", ..entryArguments]);

        var rootOutput = Path.Combine(staging, "root");
        Directory.CreateDirectory(rootOutput);
        await RunAsync(
            "node",
            buildRoot,
            [
                "node_modules/esbuild/bin/esbuild",
                "node_modules/element-plus/es/index.mjs",
                "--bundle",
                "--format=esm",
                "--platform=browser",
                "--target=es2016",
                "--external:vue",
                "--charset=utf8",
                "--outfile=" + Path.Combine(rootOutput, "index.js")
            ]);

        var distRoot = Path.Combine(projectRoot, "dist");
        if (Directory.Exists(distRoot))
            Directory.Delete(distRoot, recursive: true);
        Directory.CreateDirectory(distRoot);
        CopyTree(staging, distRoot, static path =>
            path.EndsWith("components.meta.json", StringComparison.OrdinalIgnoreCase));

        var upstreamCss = Path.Combine(buildRoot, "node_modules", "element-plus", "dist", "index.css");
        if (!File.Exists(upstreamCss))
            throw new InvalidOperationException("Element Plus dist/index.css is missing from the npm package.");
        var rootCss = Path.Combine(distRoot, "root", "index.css");
        Directory.CreateDirectory(Path.GetDirectoryName(rootCss)!);
        File.Copy(upstreamCss, rootCss, overwrite: true);

        using var meta = JsonDocument.Parse(File.ReadAllText(metaPath));
        var outputs = meta.RootElement.GetProperty("outputs")
            .EnumerateObject()
            .ToDictionary(
                property => Path.GetFullPath(property.Name, buildRoot),
                static property => property.Value,
                StringComparer.OrdinalIgnoreCase);

        var imports = new JsonObject
        {
            ["element-plus"] = CreateEntry(
                "dist/root/index.js",
                Path.Combine(distRoot, "root", "index.js"),
                [CreateStyle("dist/root/index.css", rootCss)],
                [])
        };
        foreach (var (runtimeExport, module) in componentModules.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            var name = names[(module, runtimeExport)];
            var outputPath = Path.GetFullPath(Path.Combine(staging, "components", name + ".js"));
            if (!outputs.TryGetValue(outputPath, out var output))
                throw new InvalidOperationException($"esbuild did not emit Element Plus entry '{module}/{runtimeExport}'.");

            var cssBundle = output.TryGetProperty("cssBundle", out var cssValue)
                ? Path.GetFullPath(cssValue.GetString()!, buildRoot)
                : null;
            if (cssBundle is not null && !File.Exists(cssBundle))
                throw new InvalidOperationException($"Element Plus entry '{module}/{runtimeExport}' CSS bundle '{cssBundle}' is missing.");

            var staticFiles = CollectOutputClosure(outputPath, output, outputs, buildRoot)
                .Where(path => !string.Equals(path, outputPath, StringComparison.OrdinalIgnoreCase))
                .Select(path =>
                {
                    var relative = Path.GetRelativePath(staging, path).Replace('\\', '/');
                    return CreateModuleFile("dist/" + relative, Path.Combine(projectRoot, "dist", relative));
                })
                .OrderBy(static file => file["path"]!.GetValue<string>(), StringComparer.Ordinal)
                .ToArray();
            var importSpecifier = "element-plus/" + module + "/" + runtimeExport;
            IReadOnlyList<JsonObject> styleFiles = cssBundle is null
                ? []
                : [CreateStyle("dist/components/" + name + ".css", Path.Combine(distRoot, "components", name + ".css"))];
            imports[importSpecifier] = CreateEntry(
                "dist/components/" + name + ".js",
                Path.Combine(distRoot, "components", name + ".js"),
                styleFiles,
                staticFiles);

            if (cssBundle is not null)
            {
                var componentCssPath = Path.Combine(distRoot, "components", name + ".css");
                Directory.CreateDirectory(Path.GetDirectoryName(componentCssPath)!);
                File.Copy(cssBundle, componentCssPath, overwrite: true);
            }
        }

        var manifestPath = Path.Combine(projectRoot, "manifest.json");
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
        manifest["version"] = version;
        manifest["imports"] = imports;
        manifest["styles"] = new JsonArray();
        RefreshHashes(manifest, projectRoot);
        File.WriteAllText(manifestPath, manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
    }
    finally
    {
        if (Directory.Exists(virtualEntries))
            Directory.Delete(virtualEntries, recursive: true);
    }

    static Dictionary<string, string> ReadElementPlusComponentModules(string content)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Match match in Regex.Matches(
                     content,
                     @"import\s*\{(?<exports>[^}]*)\}\s*from\s*""\.\/components\/(?<module>[^""\/]+)\/index\.mjs"";",
                     RegexOptions.CultureInvariant))
        {
            foreach (var exportText in match.Groups["exports"].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var runtimeExport = exportText.Split(" as ", StringSplitOptions.TrimEntries)[0].Trim();
                if (!runtimeExport.StartsWith("El", StringComparison.Ordinal))
                    continue;
                var module = match.Groups["module"].Value;
                if (!result.TryAdd(runtimeExport, module) && !string.Equals(result[runtimeExport], module, StringComparison.Ordinal))
                    throw new InvalidOperationException($"Element Plus export '{runtimeExport}' maps to multiple modules.");
            }
        }

        var componentList = Regex.Match(content, @"var\s+component_default\s*=\s*\[(?<items>[\s\S]*?)\];", RegexOptions.CultureInvariant);
        if (!componentList.Success)
            throw new InvalidOperationException("Element Plus component.mjs has no installable component list.");
        foreach (Match match in Regex.Matches(componentList.Groups["items"].Value, @"\bEl[A-Z][A-Za-z0-9]*\b", RegexOptions.CultureInvariant))
            if (!result.ContainsKey(match.Value))
                throw new InvalidOperationException($"Element Plus installable export '{match.Value}' has no module import.");
        return result;
    }

    static IReadOnlyList<string> CollectElementPlusCssImports(string buildRoot, string module)
    {
        var packageRoot = Path.Combine(buildRoot, "node_modules", "element-plus");
        var styleEntry = Path.Combine(packageRoot, "es", "components", module, "style", "css.mjs");
        if (!File.Exists(styleEntry))
            throw new InvalidOperationException($"Element Plus component '{module}' has no published CSS entry.");
        var pending = new Queue<string>([styleEntry]);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var css = new HashSet<string>(StringComparer.Ordinal);
        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            if (!visited.Add(current) || !File.Exists(current))
                continue;
            var directory = Path.GetDirectoryName(current)!;
            foreach (Match match in Regex.Matches(File.ReadAllText(current), "(?:import\\s+(?:[^\\\"']+?)?from\\s+)?[\\\"'](?<specifier>[^\\\"']+)[\\\"']", RegexOptions.CultureInvariant))
            {
                var specifier = match.Groups["specifier"].Value;
                if (specifier.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                {
                    css.Add(specifier);
                    continue;
                }
                if (!specifier.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase))
                    continue;
                var next = specifier.StartsWith("element-plus/", StringComparison.Ordinal)
                    ? Path.Combine(packageRoot, specifier["element-plus/".Length..].Replace('/', Path.DirectorySeparatorChar))
                    : Path.GetFullPath(Path.Combine(directory, specifier.Replace('/', Path.DirectorySeparatorChar)));
                pending.Enqueue(next);
            }
        }
        css.Add("element-plus/theme-chalk/base.css");
        return css.OrderBy(static path => path, StringComparer.Ordinal).ToArray();
    }

    static string SanitizeEntryName(string value)
        => string.Concat(value.Select(character => char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '-'));

    static JsonObject CreateEntry(
        string relativePath,
        string absolutePath,
        IReadOnlyList<JsonObject> styles,
        IReadOnlyList<JsonObject> files)
    {
        var hash = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(absolutePath)));
        return new JsonObject
        {
            ["type"] = "module",
            ["development"] = relativePath,
            ["production"] = relativePath,
            ["developmentHash"] = hash,
            ["productionHash"] = hash,
            ["developmentDependencies"] = new JsonArray(JsonValue.Create("vue")),
            ["productionDependencies"] = new JsonArray(JsonValue.Create("vue")),
            ["developmentModuleDependencies"] = new JsonArray(),
            ["productionModuleDependencies"] = new JsonArray(),
            ["developmentStyles"] = new JsonArray(styles.Select(static style => style.DeepClone()).ToArray()),
            ["productionStyles"] = new JsonArray(styles.Select(static style => style.DeepClone()).ToArray()),
            ["files"] = new JsonArray(files.Select(static file => file.DeepClone()).ToArray())
        };
    }

    static JsonObject CreateStyle(string relativePath, string absolutePath)
        => new()
        {
            ["type"] = "style",
            ["path"] = relativePath,
            ["hash"] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(absolutePath)))
        };

    static JsonObject CreateModuleFile(string relativePath, string absolutePath)
        => new()
        {
            ["type"] = "module",
            ["path"] = relativePath,
            ["hash"] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(absolutePath))),
            ["moduleId"] = relativePath
        };

    static HashSet<string> CollectOutputClosure(
        string outputPath,
        JsonElement output,
        IReadOnlyDictionary<string, JsonElement> outputs,
        string pathBase)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void Visit(string path, JsonElement node)
        {
            if (!result.Add(path) || !node.TryGetProperty("imports", out var importsElement))
                return;
            foreach (var import in importsElement.EnumerateArray())
            {
                if (import.TryGetProperty("external", out var external) && external.GetBoolean())
                    continue;
                if (!import.TryGetProperty("path", out var pathElement))
                    continue;
                var importedPath = Path.GetFullPath(pathElement.GetString()!, pathBase);
                if (outputs.TryGetValue(importedPath, out var importedOutput))
                    Visit(importedPath, importedOutput);
            }
        }
        Visit(outputPath, output);
        return result;
    }

    static void CopyTree(string sourceRoot, string destinationRoot, Func<string, bool> skip)
    {
        foreach (var source in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            if (skip(source))
                continue;
            var relative = Path.GetRelativePath(sourceRoot, source);
            var destination = Path.Combine(destinationRoot, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(source, destination, overwrite: true);
        }
    }

    static void RefreshHashes(JsonNode node, string packageRoot)
    {
        if (node is JsonObject obj)
        {
            foreach (var (key, pathKey) in new[] { ("hash", "path"), ("developmentHash", "development"), ("productionHash", "production") })
            {
                if (!obj.ContainsKey(key) || obj[pathKey] is not JsonValue pathValue)
                    continue;
                obj[key] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(packageRoot, pathValue.GetValue<string>()))));
            }
            foreach (var child in obj.ToArray())
                if (child.Value is not null)
                    RefreshHashes(child.Value, packageRoot);
        }
        else if (node is JsonArray array)
        {
            foreach (var child in array)
                if (child is not null)
                    RefreshHashes(child, packageRoot);
        }
    }
}

void RefreshHashes(JsonNode node)
{
    if (node is JsonObject obj)
    {
        foreach (var (key, pathKey) in new[] { ("hash", "path"), ("developmentHash", "development"), ("productionHash", "production") })
            if (obj.ContainsKey(key))
                obj[key] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(projectRoot, obj[pathKey]!.GetValue<string>()))));
        foreach (var child in obj.ToArray())
            if (child.Value is not null) RefreshHashes(child.Value);
    }
    else if (node is JsonArray array)
        foreach (var child in array)
            if (child is not null) RefreshHashes(child);
}
