#!/usr/bin/env dotnet run

using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Nodes;

// Refresh npm identity and generator snapshots only. Runtime JavaScript/CSS remains in
// the upstream package so the final consumer bundler can tree-shake it by entry.
// 仅更新 npm 身份与生成器快照；运行时 JS/CSS 留在上游包中，由最终 bundler 按入口裁剪。
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
var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))?.AsObject()
    ?? throw new InvalidOperationException($"Manifest '{manifestPath}' must contain a JSON object.");
var previousVersion = manifest["version"]?.GetValue<string>()
    ?? throw new InvalidOperationException($"Manifest '{manifestPath}' does not declare a version.");

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
var integrity = distribution.GetProperty("integrity").GetString()
    ?? throw new InvalidOperationException("npm metadata does not contain dist.integrity.");
if (!integrity.StartsWith("sha512-", StringComparison.Ordinal))
    throw new InvalidOperationException("Expected npm SHA-512 integrity metadata.");

var expectedHash = integrity[7..];
if (!File.Exists(cache) || HashArchive(cache) != expectedHash)
{
    Console.WriteLine($"Downloading {package}@{version}");
    var temporaryCache = cache + ".download";
    File.Delete(temporaryCache);
    await using (var input = await client.GetStreamAsync(distribution.GetProperty("tarball").GetString()!))
    await using (var output = File.Create(temporaryCache))
        await input.CopyToAsync(output);
    File.Move(temporaryCache, cache, overwrite: true);
}

if (HashArchive(cache) != expectedHash)
    throw new InvalidOperationException("Downloaded archive does not match npm integrity metadata.");

var archive = ReadArchiveMetadata(cache, package, version);
ValidatePackageMetadata(metadata.RootElement, package, version, archive.PackageJson);
PrepareGeneratorSnapshot(package, previousSnapshot, snapshot, cache, version, archive);
manifest = UpdateExternalManifest(manifest, package, version, integrity, archive);
File.WriteAllText(manifestPath, manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"Updated {package}: {previousVersion} -> {version}. Run the generators and review contract drift.");

static string HashArchive(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToBase64String(SHA512.HashData(stream));
}

static NpmArchive ReadArchiveMetadata(string archivePath, string packageName, string version)
{
    JsonObject? packageJson = null;
    var files = new HashSet<string>(StringComparer.Ordinal);
    var textFiles = new Dictionary<string, string>(StringComparer.Ordinal);

    using var archive = File.OpenRead(archivePath);
    using var gzip = new GZipStream(archive, CompressionMode.Decompress);
    using var reader = new TarReader(gzip);
    while (reader.GetNextEntry() is { } entry)
    {
        if (entry.EntryType is not TarEntryType.RegularFile ||
            entry.DataStream is null)
            continue;

        var relative = NormalizeArchivePath(entry.Name);
        if (relative is null)
            continue;

        files.Add(relative);
        if (relative == "package.json" ||
            IsVuetifyIndex(relative))
        {
            using var buffer = new MemoryStream();
            entry.DataStream.CopyTo(buffer);
            var text = Encoding.UTF8.GetString(buffer.ToArray());
            if (relative == "package.json")
                packageJson = JsonNode.Parse(text)?.AsObject()
                    ?? throw new InvalidOperationException($"{packageName}@{version} contains an invalid package.json.");
            else
                textFiles[relative] = text;
        }
    }

    if (packageJson is null)
        throw new InvalidOperationException($"{packageName}@{version} archive does not contain package.json.");

    return new NpmArchive(packageJson, files, textFiles);
}

static string? NormalizeArchivePath(string name)
{
    var normalized = name.Replace('\\', '/');
    if (!normalized.StartsWith("package/", StringComparison.Ordinal))
        return null;

    var relative = normalized["package/".Length..];
    if (string.IsNullOrWhiteSpace(relative) ||
        relative.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(static segment => segment is "." or ".."))
    {
        throw new InvalidOperationException($"npm archive contains an unsafe path: '{name}'.");
    }

    return relative;
}

static bool IsVuetifyIndex(string relative)
    => relative == "lib/directives/index.js" ||
       (relative.StartsWith("lib/directives/", StringComparison.Ordinal) &&
        relative.EndsWith("/index.js", StringComparison.Ordinal)) ||
       (relative.StartsWith("lib/components/", StringComparison.Ordinal) &&
        relative.EndsWith("/index.js", StringComparison.Ordinal)) ||
       (relative.StartsWith("lib/labs/", StringComparison.Ordinal) &&
        relative.EndsWith("/index.js", StringComparison.Ordinal));

static void ValidatePackageMetadata(
    JsonElement registryMetadata,
    string packageName,
    string version,
    JsonObject packageJson)
{
    if (!string.Equals(packageJson["name"]?.GetValue<string>(), packageName, StringComparison.Ordinal) ||
        !string.Equals(packageJson["version"]?.GetValue<string>(), version, StringComparison.Ordinal))
    {
        throw new InvalidOperationException(
            $"npm archive package identity does not match {packageName}@{version}.");
    }

    if (packageName != "vue-data-ui")
        return;

    var gitHead = registryMetadata.TryGetProperty("gitHead", out var gitHeadElement)
        ? gitHeadElement.GetString()
        : null;
    if (gitHead is null || !Regex.IsMatch(gitHead, "^[0-9a-f]{40}$", RegexOptions.CultureInvariant))
        throw new InvalidOperationException($"{packageName}@{version} does not declare a valid npm gitHead.");

    var repositoryUrl = registryMetadata.TryGetProperty("repository", out var repository) &&
                        repository.ValueKind == JsonValueKind.Object &&
                        repository.TryGetProperty("url", out var url)
        ? url.GetString()
        : null;
    if (!string.Equals(repositoryUrl, "git+https://github.com/graphieros/vue-data-ui.git", StringComparison.Ordinal))
    {
        throw new InvalidOperationException($"Unexpected vue-data-ui repository metadata: '{repositoryUrl}'.");
    }
}

static void PrepareGeneratorSnapshot(
    string packageName,
    string previousSnapshot,
    string snapshot,
    string archivePath,
    string version,
    NpmArchive archive)
{
    if (packageName == "vue-data-ui")
    {
        RequireArchiveFiles(
            archive,
            "dist/style.css",
            "dist/types/vue-data-ui.d.ts");
        return;
    }

    var staging = snapshot + ".staging-" + Guid.NewGuid().ToString("N");
    if (Directory.Exists(staging))
        Directory.Delete(staging, recursive: true);
    Directory.CreateDirectory(staging);

    try
    {
        switch (packageName)
        {
            case "tdesign-vue-next":
                PrepareTDesignSnapshot(previousSnapshot, staging, archivePath, archive);
                break;
            case "element-plus":
                ExtractArchiveFiles(
                    archivePath,
                    staging,
                    static relative => relative == "package.json" ||
                        relative == "attributes.json" ||
                        relative == "web-types.json" ||
                        relative == "es/component.mjs" ||
                        relative == "es/constants/event.d.ts");
                RequireSnapshotFiles(
                    staging,
                    "package.json",
                    "attributes.json",
                    "web-types.json",
                    "es/component.mjs",
                    "es/constants/event.d.ts");
                break;
            case "vuetify":
                PrepareVuetifySnapshot(previousSnapshot, staging, archivePath, version, archive);
                break;
            default:
                throw new InvalidOperationException($"Unsupported snapshot package '{packageName}'.");
        }

        ReplaceDirectory(staging, snapshot);
    }
    finally
    {
        if (Directory.Exists(staging))
            Directory.Delete(staging, recursive: true);
    }
}

static void PrepareTDesignSnapshot(
    string previousSnapshot,
    string staging,
    string archivePath,
    NpmArchive archive)
{
    foreach (var generated in new[]
             {
                 "bindings.json",
                 "components.json",
                 "contracts.json",
                 "documentation.json",
                 "snapshot.json"
             })
    {
        CopyRequiredFile(previousSnapshot, staging, generated);
    }

    var external = Path.Combine(previousSnapshot, "external");
    if (Directory.Exists(external))
        CopyDirectory(external, Path.Combine(staging, "external"));

    ExtractArchiveFiles(
        archivePath,
        staging,
        static relative => relative == "package.json" ||
            (relative.StartsWith("helper/", StringComparison.Ordinal) &&
             relative.EndsWith(".json", StringComparison.Ordinal)) ||
            (relative.StartsWith("es/", StringComparison.Ordinal) &&
             relative.EndsWith(".d.ts", StringComparison.Ordinal)));

    RequireSnapshotFiles(
        staging,
        "package.json",
        "helper/web-types.json",
        "es/index.d.ts",
        "components.json",
        "bindings.json",
        "contracts.json",
        "documentation.json",
        "snapshot.json");

    var snapshotMetadata = JsonNode.Parse(File.ReadAllText(Path.Combine(staging, "snapshot.json")))?.AsObject()
        ?? throw new InvalidOperationException("TDesign snapshot.json is invalid.");
    snapshotMetadata["version"] = archive.PackageJson["version"]!.GetValue<string>();
    File.WriteAllText(
        Path.Combine(staging, "snapshot.json"),
        snapshotMetadata.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
}

static void PrepareVuetifySnapshot(
    string previousSnapshot,
    string staging,
    string archivePath,
    string version,
    NpmArchive archive)
{
    CopyRequiredFile(previousSnapshot, staging, "contracts.json");
    ExtractArchiveFiles(
        archivePath,
        staging,
        static relative => relative == "package.json" || relative == "dist/json/web-types.json",
        static relative => relative == "dist/json/web-types.json" ? "web-types.json" : relative);

    RequireSnapshotFiles(staging, "package.json", "web-types.json", "contracts.json");
    var contractsPath = Path.Combine(staging, "contracts.json");
    var contracts = JsonNode.Parse(File.ReadAllText(contractsPath))?.AsObject()
        ?? throw new InvalidOperationException("Vuetify contracts.json is invalid.");
    contracts["upstreamVersion"] = version;
    File.WriteAllText(
        contractsPath,
        contracts.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
}

static void ExtractArchiveFiles(
    string archivePath,
    string destinationRoot,
    Func<string, bool> include,
    Func<string, string>? map = null)
{
    var found = new HashSet<string>(StringComparer.Ordinal);
    using var archive = File.OpenRead(archivePath);
    using var gzip = new GZipStream(archive, CompressionMode.Decompress);
    using var reader = new TarReader(gzip);
    while (reader.GetNextEntry() is { } entry)
    {
        if (entry.EntryType is not TarEntryType.RegularFile ||
            entry.DataStream is null)
            continue;

        var relative = NormalizeArchivePath(entry.Name);
        if (relative is null || !include(relative))
            continue;

        var destinationRelative = map?.Invoke(relative) ?? relative;
        var destination = GetSafePath(destinationRoot, destinationRelative);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        using var output = File.Create(destination);
        entry.DataStream.CopyTo(output);
        found.Add(relative);
    }
}

static void RequireArchiveFiles(NpmArchive archive, params string[] paths)
{
    var missing = paths.Where(path => !archive.Files.Contains(path)).ToArray();
    if (missing.Length > 0)
        throw new InvalidOperationException(
            "npm archive is missing required files: " + string.Join(", ", missing));
}

static void RequireSnapshotFiles(string root, params string[] paths)
{
    var missing = paths
        .Where(path => !File.Exists(GetSafePath(root, path)))
        .ToArray();
    if (missing.Length > 0)
        throw new InvalidOperationException(
            "Generated upstream snapshot is missing required files: " + string.Join(", ", missing));
}

static void CopyRequiredFile(string sourceRoot, string destinationRoot, string relativePath)
{
    var source = GetSafePath(sourceRoot, relativePath);
    if (!File.Exists(source))
        throw new InvalidOperationException($"Previous upstream snapshot is missing '{relativePath}'.");
    var destination = GetSafePath(destinationRoot, relativePath);
    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
    File.Copy(source, destination, overwrite: true);
}

static void CopyDirectory(string sourceRoot, string destinationRoot)
{
    foreach (var source in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
    {
        var relative = Path.GetRelativePath(sourceRoot, source).Replace('\\', '/');
        var destination = GetSafePath(destinationRoot, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(source, destination, overwrite: true);
    }
}

static string GetSafePath(string root, string relativePath)
{
    var normalizedRoot = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
    var fullPath = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
    if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException($"Path escapes its root: '{relativePath}'.");
    return fullPath;
}

static void ReplaceDirectory(string stagedDirectory, string destinationDirectory)
{
    Directory.CreateDirectory(Path.GetDirectoryName(destinationDirectory)!);
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

static JsonObject UpdateExternalManifest(
    JsonObject original,
    string packageName,
    string version,
    string integrity,
    NpmArchive archive)
{
    var manifest = original.DeepClone().AsObject();
    manifest["source"] = "npm";
    manifest["version"] = version;
    // Keep the binding-level provider constraint alongside the npm package identity. The
    // package's peer dependency is resolved by the shared vue3 manifest/provider rather than
    // silently becoming an unconstrained second Vue instance in the generated project.
    var requires = manifest["requires"] as JsonObject ?? new JsonObject();
    if (archive.PackageJson["peerDependencies"] is JsonObject peerDependencies &&
        peerDependencies["vue"] is JsonValue vuePeer &&
        vuePeer.TryGetValue<string>(out var vueRange) &&
        !string.IsNullOrWhiteSpace(vueRange))
    {
        requires["vue3"] = vueRange;
    }
    manifest["requires"] = requires;
    manifest["styles"] = new JsonArray();
    manifest["files"] = new JsonArray();

    var packages = manifest["packages"] as JsonObject ?? new JsonObject();
    manifest["packages"] = packages;
    var packageMetadata = packages[packageName] as JsonObject ?? new JsonObject();
    packageMetadata["source"] = "npm";
    packageMetadata["version"] = version;
    packageMetadata["integrity"] = integrity;
    packages[packageName] = packageMetadata;

    var imports = manifest["imports"]?.AsObject()
        ?? throw new InvalidOperationException("Manifest must contain an imports object.");
    var touched = 0;
    foreach (var pair in imports.ToArray())
    {
        var logicalSpecifier = pair.Key;
        var entry = pair.Value?.AsObject()
            ?? throw new InvalidOperationException($"Manifest import '{logicalSpecifier}' must be an object.");

        var isTargetEntry = logicalSpecifier == packageName ||
                            logicalSpecifier.StartsWith(packageName + "/", StringComparison.Ordinal);
        if (!isTargetEntry)
        {
            // A binding can keep a second npm entry (for example jspdf in VueDataUi).
            // Convert its old binding-owned path to the package specifier as well.
            if (logicalSpecifier == "jspdf")
            {
                entry["development"] = logicalSpecifier;
                entry["production"] = logicalSpecifier;
            }
            RemoveEmbeddedEntryFields(entry);
            RemoveExternalStyleFields(entry);
            NormalizeDependencyArray(entry, "developmentDependencies");
            NormalizeDependencyArray(entry, "productionDependencies");
            continue;
        }

        touched++;
        var externalSpecifier = MapExternalEntry(logicalSpecifier, packageName, archive);
        ValidateExternalTarget(externalSpecifier, packageName, archive);

        entry["type"] = "module";
        entry["development"] = externalSpecifier;
        entry["production"] = externalSpecifier;
        RemoveEmbeddedEntryFields(entry);
        NormalizeDependencyArray(entry, "developmentDependencies");
        NormalizeDependencyArray(entry, "productionDependencies");

        var styles = GetStyleEdges(logicalSpecifier, externalSpecifier, packageName, archive);
        ValidateStyleEdges(packageName, styles, archive);
        foreach (var field in new[]
                 {
                     "developmentStyleImports",
                     "productionStyleImports",
                     "developmentStylesheetImports",
                     "productionStylesheetImports"
                 })
        {
            entry.Remove(field);
        }

        var moduleStyles = styles.Where(static edge => edge.Module).Select(static edge => edge.Specifier).ToArray();
        var stylesheetStyles = styles.Where(static edge => !edge.Module).Select(static edge => edge.Specifier).ToArray();
        if (moduleStyles.Length > 0)
        {
            entry["developmentStyleImports"] = ToJsonArray(moduleStyles);
            entry["productionStyleImports"] = ToJsonArray(moduleStyles);
        }
        if (stylesheetStyles.Length > 0)
        {
            entry["developmentStylesheetImports"] = ToJsonArray(stylesheetStyles);
            entry["productionStylesheetImports"] = ToJsonArray(stylesheetStyles);
        }
    }

    if (touched == 0)
        throw new InvalidOperationException($"Manifest contains no entries for npm package '{packageName}'.");

    return manifest;
}

static void RemoveEmbeddedEntryFields(JsonObject entry)
{
    foreach (var field in new[]
             {
                 "developmentHash",
                 "productionHash",
                 "developmentModuleDependencies",
                 "productionModuleDependencies",
                 "developmentStyles",
                 "productionStyles",
                 "files"
             })
    {
        entry.Remove(field);
    }
}

static void RemoveExternalStyleFields(JsonObject entry)
{
    foreach (var field in new[]
             {
                 "developmentStyleImports",
                 "productionStyleImports",
                 "developmentStylesheetImports",
                 "productionStylesheetImports"
             })
    {
        entry.Remove(field);
    }
}

static void NormalizeDependencyArray(JsonObject entry, string field)
{
    var values = entry[field] as JsonArray;
    var normalized = new JsonArray();
    var seen = new HashSet<string>(StringComparer.Ordinal);
    if (values is not null)
    {
        foreach (var value in values)
        {
            if (value?.GetValue<string>() is { Length: > 0 } dependency &&
                seen.Add(dependency))
            {
                normalized.Add(JsonValue.Create(dependency));
            }
        }
    }
    entry[field] = normalized;
}

static JsonArray ToJsonArray(IEnumerable<string> values)
{
    var result = new JsonArray();
    foreach (var value in values)
        result.Add(JsonValue.Create(value));
    return result;
}

static string MapExternalEntry(string logicalSpecifier, string packageName, NpmArchive archive)
{
    if (logicalSpecifier == packageName)
    {
        return packageName switch
        {
            "tdesign-vue-next" => packageName + "/es/index.mjs",
            "element-plus" => packageName + "/es/index.mjs",
            "vuetify" => packageName,
            "vue-data-ui" => throw new InvalidOperationException(
                "vue-data-ui aggregate root is not a component-safe tree-shaking entry."),
            _ => throw new InvalidOperationException($"Unsupported npm package '{packageName}'.")
        };
    }

    var prefix = packageName + "/";
    if (!logicalSpecifier.StartsWith(prefix, StringComparison.Ordinal))
        return logicalSpecifier;

    var remainder = logicalSpecifier[prefix.Length..];
    return packageName switch
    {
        "tdesign-vue-next" => MapTDesignEntry(packageName, remainder),
        "element-plus" => MapElementPlusEntry(packageName, remainder),
        "vuetify" => MapVuetifyEntry(packageName, remainder, archive),
        "vue-data-ui" => MapVueDataUiEntry(packageName, remainder, archive),
        _ => throw new InvalidOperationException($"Unsupported npm package '{packageName}'.")
    };
}

static string MapTDesignEntry(string packageName, string remainder)
{
    var slash = remainder.LastIndexOf('/');
    if (slash <= 0 || slash == remainder.Length - 1)
        throw new InvalidOperationException($"Invalid TDesign logical entry '{packageName}/{remainder}'.");
    return packageName + "/es/" + remainder[..slash] + "/index.mjs";
}

static string MapElementPlusEntry(string packageName, string remainder)
{
    var slash = remainder.LastIndexOf('/');
    if (slash <= 0 || slash == remainder.Length - 1)
        throw new InvalidOperationException($"Invalid Element Plus logical entry '{packageName}/{remainder}'.");
    return packageName + "/es/components/" + remainder[..slash] + "/index.mjs";
}

static string MapVuetifyEntry(string packageName, string remainder, NpmArchive archive)
{
    if (remainder.StartsWith("components/", StringComparison.Ordinal))
    {
        var exportName = remainder["components/".Length..];
        var group = FindVuetifyGroup(archive, "components", exportName);
        return packageName + "/components/" + group;
    }

    if (remainder.StartsWith("labs/components/", StringComparison.Ordinal))
    {
        var exportName = remainder["labs/components/".Length..];
        // Vuetify has moved several lab components into the stable components export
        // between releases. Preserve the binding's logical path while resolving it to the
        // package export that the selected upstream version actually publishes.
        var labGroup = TryFindVuetifyGroup(archive, "labs", exportName);
        if (labGroup is not null)
            return packageName + "/labs/" + labGroup;

        var componentGroup = TryFindVuetifyGroup(archive, "components", exportName);
        if (componentGroup is not null &&
            HasPackageExport(archive.PackageJson, "./components/" + componentGroup))
        {
            return packageName + "/components/" + componentGroup;
        }

        throw new InvalidOperationException(
            $"Vuetify labs package entry does not export '{exportName}' in the requested version.");
    }

    if (remainder.StartsWith("directives/", StringComparison.Ordinal))
    {
        var exportName = remainder["directives/".Length..];
        var group = FindVuetifyDirectiveGroup(archive, exportName);
        return packageName + "/directives/" + group;
    }

    return logicalSpecifierForVuetify(packageName, remainder);
}

static string logicalSpecifierForVuetify(string packageName, string remainder)
    => packageName + "/" + remainder;

static string MapVueDataUiEntry(string packageName, string remainder, NpmArchive archive)
{
    if (!remainder.StartsWith("vue-ui-", StringComparison.Ordinal) ||
        !archive.Files.Contains("dist/components/" + remainder + ".js"))
    {
        throw new InvalidOperationException(
            $"vue-data-ui entry '{packageName}/{remainder}' is not a published component entry.");
    }

    return packageName + "/" + remainder;
}

static string FindVuetifyGroup(NpmArchive archive, string family, string exportName)
{
    return TryFindVuetifyGroup(archive, family, exportName)
        ?? throw new InvalidOperationException(
            $"Vuetify {family} package entry does not export '{exportName}' in the requested version.");
}

static string? TryFindVuetifyGroup(NpmArchive archive, string family, string exportName)
{
    var prefix = family == "components" ? "lib/components/" : "lib/labs/";
    const string suffix = "/index.js";
    var direct = prefix + exportName + "/index.js";
    if (archive.TextFiles.TryGetValue(direct, out var directText) &&
        HasNamedExport(directText, exportName))
    {
        return exportName;
    }

    foreach (var pair in archive.TextFiles
                 .Where(pair => pair.Key.StartsWith(prefix, StringComparison.Ordinal) &&
                                pair.Key.EndsWith("/index.js", StringComparison.Ordinal) &&
                                !string.Equals(pair.Key, prefix + "index.js", StringComparison.Ordinal))
                 .OrderBy(pair => pair.Key, StringComparer.Ordinal))
    {
        if (HasNamedExport(pair.Value, exportName))
        {
            var relative = pair.Key[prefix.Length..];
            if (!relative.EndsWith(suffix, StringComparison.Ordinal))
                continue;
            return relative[..^suffix.Length];
        }
    }

    return null;
}

static string FindVuetifyDirectiveGroup(NpmArchive archive, string exportName)
{
    const string prefix = "lib/directives/";
    const string suffix = "/index.js";
    foreach (var pair in archive.TextFiles
                 .Where(pair => pair.Key.StartsWith(prefix, StringComparison.Ordinal) &&
                                pair.Key.EndsWith("/index.js", StringComparison.Ordinal))
                 .OrderBy(pair => pair.Key, StringComparer.Ordinal))
    {
        if (HasNamedExport(pair.Value, exportName))
        {
            var relative = pair.Key[prefix.Length..];
            if (!relative.EndsWith(suffix, StringComparison.Ordinal))
                continue;
            return relative[..^suffix.Length];
        }
    }

    throw new InvalidOperationException(
        $"Vuetify directives package entry does not export '{exportName}' in the requested version.");
}

static bool HasNamedExport(string source, string exportName)
    => Regex.IsMatch(
        source,
        @"(?m)^\s*export\b[^\r\n]*\b" + Regex.Escape(exportName) + @"\b",
        RegexOptions.CultureInvariant);

static void ValidateExternalTarget(string specifier, string packageName, NpmArchive archive)
{
    ValidateSpecifier(specifier);
    if (specifier == packageName)
    {
        if (packageName == "vuetify" && !HasPackageExport(archive.PackageJson, "."))
            throw new InvalidOperationException("Vuetify package does not publish its root export.");
        return;
    }

    var remainder = specifier[(packageName + "/").Length..];
    if (packageName == "vuetify")
    {
        if (!HasPackageExport(archive.PackageJson, "./" + remainder))
            throw new InvalidOperationException(
                $"Vuetify does not publish export '{specifier}'.");
        var relative = remainder switch
        {
            _ when remainder.StartsWith("components/", StringComparison.Ordinal)
                => "lib/components/" + remainder["components/".Length..] + "/index.js",
            _ when remainder.StartsWith("labs/", StringComparison.Ordinal)
                => "lib/labs/" + remainder["labs/".Length..] + "/index.js",
            _ when remainder.StartsWith("directives/", StringComparison.Ordinal)
                => "lib/directives/" + remainder["directives/".Length..] + "/index.js",
            _ => null
        };
        if (relative is not null && !archive.Files.Contains(relative))
            throw new InvalidOperationException(
                $"Vuetify export '{specifier}' resolves to missing '{relative}'.");
        return;
    }

    if (packageName == "vue-data-ui")
    {
        if (!remainder.StartsWith("vue-ui-", StringComparison.Ordinal) ||
            !archive.Files.Contains("dist/components/" + remainder + ".js"))
        {
            throw new InvalidOperationException(
                $"vue-data-ui does not publish component entry '{specifier}'.");
        }

        return;
    }

    if (!archive.Files.Contains(remainder))
        throw new InvalidOperationException(
            $"npm package '{packageName}' does not publish entry '{specifier}'.");
}

static void ValidateSpecifier(string specifier)
{
    if (specifier.Contains('\\', StringComparison.Ordinal) ||
        specifier.Split('/').Any(static part => part is "." or ".." or ""))
    {
        throw new InvalidOperationException($"Invalid external package specifier '{specifier}'.");
    }
}

static bool HasPackageExport(JsonObject packageJson, string subpath)
{
    var exports = packageJson["exports"];
    return exports is not null && ExportMapContains(exports, subpath);
}

static bool ExportMapContains(JsonNode node, string subpath)
{
    if (node is JsonObject obj)
    {
        var subpathMap = obj.Any(pair => pair.Key.StartsWith(".", StringComparison.Ordinal));
        if (subpathMap)
        {
            return obj.Any(pair => pair.Key.StartsWith(".", StringComparison.Ordinal) &&
                                   ExportKeyMatches(pair.Key, subpath));
        }

        return obj.Any(pair => pair.Value is not null && ExportMapContains(pair.Value, subpath));
    }

    if (node is JsonArray array)
        return array.Any(child => child is not null && ExportMapContains(child, subpath));

    return node is JsonValue;
}

static bool ExportKeyMatches(string key, string subpath)
{
    if (key == subpath)
        return true;

    var wildcard = key.IndexOf('*');
    if (wildcard < 0)
        return false;

    var prefix = key[..wildcard];
    var suffix = key[(wildcard + 1)..];
    return subpath.StartsWith(prefix, StringComparison.Ordinal) &&
           subpath.EndsWith(suffix, StringComparison.Ordinal) &&
           subpath.Length >= prefix.Length + suffix.Length;
}

static IReadOnlyList<StyleEdge> GetStyleEdges(
    string logicalSpecifier,
    string externalSpecifier,
    string packageName,
    NpmArchive archive)
{
    if (packageName == "vue-data-ui")
        return [new StyleEdge(false, packageName + "/style.css")];

    if (packageName == "tdesign-vue-next")
    {
        var relative = logicalSpecifier == packageName
            ? "es/style/index"
            : "es/" + logicalSpecifier[(packageName + "/").Length..][..logicalSpecifier[(packageName + "/").Length..].LastIndexOf('/')] + "/style/index";
        if (archive.Files.Contains(relative + ".css"))
            return [new StyleEdge(false, packageName + "/" + relative + ".css")];
        if (archive.Files.Contains(relative + ".mjs"))
            return [new StyleEdge(true, packageName + "/" + relative + ".mjs")];
        return [];
    }

    if (packageName == "element-plus")
    {
        if (logicalSpecifier == packageName)
            return [new StyleEdge(false, packageName + "/dist/index.css")];

        var module = logicalSpecifier[(packageName + "/").Length..];
        module = module[..module.LastIndexOf('/')];
        var styleSpecifier = packageName + "/es/components/" + module + "/style/css.mjs";
        return archive.Files.Contains("es/components/" + module + "/style/css.mjs")
            ? [new StyleEdge(true, styleSpecifier)]
            : [];
    }

    if (packageName == "vuetify")
    {
        if (logicalSpecifier == packageName)
            return [new StyleEdge(false, "vuetify/styles")];

        if (externalSpecifier.StartsWith("vuetify/components/", StringComparison.Ordinal))
        {
            var group = externalSpecifier["vuetify/components/".Length..];
            var path = "lib/components/" + group + "/" + group + ".css";
            return archive.Files.Contains(path)
                ? [new StyleEdge(false, "vuetify/" + path)]
                : [];
        }

        if (externalSpecifier.StartsWith("vuetify/labs/", StringComparison.Ordinal))
        {
            var group = externalSpecifier["vuetify/labs/".Length..];
            var path = "lib/labs/" + group + "/" + group + ".css";
            return archive.Files.Contains(path)
                ? [new StyleEdge(false, "vuetify/" + path)]
                : [];
        }

        if (externalSpecifier.StartsWith("vuetify/directives/", StringComparison.Ordinal))
        {
            var group = externalSpecifier["vuetify/directives/".Length..];
            var prefix = "lib/directives/" + group + "/";
            var css = archive.Files
                .Where(path => path.StartsWith(prefix, StringComparison.Ordinal) &&
                               path.EndsWith(".css", StringComparison.Ordinal))
                .OrderBy(static path => path, StringComparer.Ordinal)
                .FirstOrDefault();
            return css is null ? [] : [new StyleEdge(false, "vuetify/" + css)];
        }

        return [];
    }

    return [];
}

static void ValidateStyleEdges(
    string packageName,
    IReadOnlyList<StyleEdge> styles,
    NpmArchive archive)
{
    foreach (var style in styles)
    {
        ValidateSpecifier(style.Specifier);
        var remainder = style.Specifier[(packageName + "/").Length..];
        if (packageName == "vuetify" && style.Specifier == "vuetify/styles")
        {
            if (!HasPackageExport(archive.PackageJson, "./styles") ||
                !archive.Files.Contains("lib/styles/main.css"))
            {
                throw new InvalidOperationException("Vuetify does not publish its styles export.");
            }
        }
        else if (packageName == "vue-data-ui" && style.Specifier == "vue-data-ui/style.css")
        {
            if (!HasPackageExport(archive.PackageJson, "./style.css") ||
                !archive.Files.Contains("dist/style.css"))
            {
                throw new InvalidOperationException("vue-data-ui does not publish its style.css export.");
            }
        }
        else if (!archive.Files.Contains(remainder))
        {
            throw new InvalidOperationException(
                $"npm package '{packageName}' does not publish stylesheet/module '{style.Specifier}'.");
        }
    }
}

sealed class NpmArchive(
    JsonObject packageJson,
    HashSet<string> files,
    Dictionary<string, string> textFiles)
{
    public JsonObject PackageJson { get; } = packageJson;
    public HashSet<string> Files { get; } = files;
    public Dictionary<string, string> TextFiles { get; } = textFiles;
}

readonly record struct StyleEdge(bool Module, string Specifier);
