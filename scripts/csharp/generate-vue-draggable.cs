#!/usr/bin/env dotnet run

using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

// Validate the locked vue-draggable-plus ESM entry and regenerate package metadata. Runtime
// bytes remain in the upstream npm package.
//
// Usage:
//   dotnet run --file scripts/csharp/generate-vue-draggable.cs -- --version 0.6.1
//   dotnet run --file scripts/csharp/generate-vue-draggable.cs -- --source <node_modules> --version 0.6.1
var options = GeneratorOptions.Parse(args);
var root = Directory.GetCurrentDirectory();
if (!File.Exists(Path.Combine(root, "Jazor.slnx")))
    throw new InvalidOperationException("Run from the repository root.");

var projectRoot = Path.Combine(root, "src", "ECMAScript.VueDraggable");
var manifestPath = Path.Combine(projectRoot, "manifest.json");

var version = options.Version;
if (version is null && File.Exists(manifestPath))
{
    version = JsonNode.Parse(File.ReadAllText(manifestPath))?["version"]?.GetValue<string>();
    if (version is not null)
        Console.WriteLine($"Using manifest version {version}.");
}

if (string.IsNullOrWhiteSpace(version))
    throw new ArgumentException("Provide --version on the first run; later runs reuse the manifest version.");

// 逻辑 import → 上游包与入口文件。闭包边由入口文件的裸 import 静态推导。
// vue-draggable-plus 的 ESM 入口自包含 sortablejs，只 peer 依赖 vue。
var entries = new (string Specifier, string Package, string File)[]
{
    ("vue-draggable-plus", "vue-draggable-plus", "dist/vue-draggable-plus.js"),
};

// 由 ECMAScript.Vue 资源库提供的 peer specifier；requires 约束版本，dependencies 记录真实解析边。
var peerSpecifiers = new HashSet<string>(StringComparer.Ordinal) { "vue" };
var peerRequires = new SortedDictionary<string, string>(StringComparer.Ordinal) { ["vue3"] = "^3.0.0" };

var sources = await AcquireSourcesAsync(root, options.Source, entries.Select(static entry => entry.Package).Distinct(StringComparer.Ordinal).ToArray(), version);

var primaryPackage = "vue-draggable-plus";
var packageVersions = new SortedDictionary<string, string>(StringComparer.Ordinal);
foreach (var package in sources.Keys.Order(StringComparer.Ordinal))
{
    var packageJson = JsonNode.Parse(File.ReadAllText(Path.Combine(sources[package], "package.json")))!;
    packageVersions[package] = packageJson["version"]!.GetValue<string>();
}
CopyExistingPackageVersions(manifestPath, packageVersions);

if (packageVersions[primaryPackage] != version)
    throw new InvalidOperationException($"{primaryPackage} extracted version '{packageVersions[primaryPackage]}' does not match requested '{version}'.");

var known = entries.Select(static entry => entry.Specifier).ToHashSet(StringComparer.Ordinal);
var dependencies = new Dictionary<string, string[]>(StringComparer.Ordinal);
var unresolved = new SortedSet<string>(StringComparer.Ordinal);
foreach (var (specifier, package, file) in entries)
{
    var source = Path.Combine(sources[package], file.Replace('/', Path.DirectorySeparatorChar));
    if (!File.Exists(source))
        throw new InvalidOperationException($"Upstream entry '{file}' is missing from {package}.");

    // 浏览器安全闸门：bundler 变体在模块顶层引用 process.env，浏览器导入即崩溃。
    // 任何 vendor 的模块都不得在裸浏览器环境引用 process.env（VueI18n 曾因此不可用）。
    AssertBrowserSafe(source, specifier);

    var bare = ReadBareSpecifiers(source);
    dependencies[specifier] = bare
        .Where(value => known.Contains(value) || peerSpecifiers.Contains(value))
        .Order(StringComparer.Ordinal)
        .ToArray();
    foreach (var value in bare)
        if (!known.Contains(value) && !peerSpecifiers.Contains(value))
            unresolved.Add(specifier + " -> " + value);
}

if (unresolved.Count > 0)
    throw new InvalidOperationException("Upstream adds references outside the declared closure: " + string.Join(", ", unresolved));

var licensesRoot = Path.Combine(projectRoot, "licenses");

if (Directory.Exists(licensesRoot))
    Directory.Delete(licensesRoot, recursive: true);
Directory.CreateDirectory(licensesRoot);

var licenseFiles = new JsonArray();
foreach (var package in entries.Select(static entry => entry.Package).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
{
    var licenseSource = Path.Combine(sources[package], "LICENSE");
    if (!File.Exists(licenseSource))
        throw new InvalidOperationException($"{package} does not ship a LICENSE file.");
    var licenseName = package.Replace("@", string.Empty, StringComparison.Ordinal).Replace('/', '-') + "-LICENSE";
    File.Copy(licenseSource, Path.Combine(licensesRoot, licenseName), overwrite: true);
    licenseFiles.Add(new JsonObject
    {
        ["type"] = "license",
        ["path"] = "licenses/" + licenseName,
        ["hash"] = HashFile(Path.Combine(licensesRoot, licenseName)),
    });
}

var manifest = BuildManifest(version, entries, dependencies, packageVersions, peerRequires, licenseFiles);
WriteLfText(manifestPath, manifest.ToJsonString(GeneratorJson.Manifest) + "\n");

WriteInventory(projectRoot, version, entries.Length, entries.Select(static entry => entry.Specifier).ToArray(), packageVersions, "vue-draggable-plus", "https://vue-draggable-plus.pages.dev/");

Console.WriteLine($"Validated {entries.Length} vue-draggable-plus {version} npm entries; license: MIT.");
Console.WriteLine("Review contract drift for bound exports before committing.");

static void CopyExistingPackageVersions(string manifestPath, IDictionary<string, string> packageVersions)
{
    if (!File.Exists(manifestPath))
        return;
    var packages = JsonNode.Parse(File.ReadAllText(manifestPath))?["packages"] as JsonObject;
    if (packages is null)
        return;
    foreach (var property in packages)
    {
        if (!packageVersions.ContainsKey(property.Key) && property.Value?["version"]?.GetValue<string>() is { } version)
            packageVersions[property.Key] = version;
    }
}

static void AssertBrowserSafe(string path, string specifier)
{
    // 顶层 process.env 的 bundler 变体在浏览器不可用；CLR 无法在 vendor 时替换它。
    var source = File.ReadAllText(path);
    var depth = 0;
    var line = 0;
    foreach (var text in source.Split('\n'))
    {
        line++;
        if (text.Contains("process.env", StringComparison.Ordinal) && depth == 0)
            throw new InvalidOperationException(
                $"Vendored entry '{specifier}' references process.env at module top level (line {line}); " +
                "it cannot load in a browser. Vendor the upstream browser/ESM build instead.");
        depth += text.Count(static c => c == '{') - text.Count(static c => c == '}');
    }
}

static HashSet<string> ReadBareSpecifiers(string path)
{
    var blockComment = new Regex(@"/\*[\s\S]*?\*/", RegexOptions.CultureInvariant);
    var lineComment = new Regex(@"(?m)^\s*//.*$", RegexOptions.CultureInvariant);
    var importSpecifier = new Regex(
        @"(?:import|export)\s[^;]*?from\s*[""']([^""']+)[""']|import\s*[""']([^""']+)[""']",
        RegexOptions.CultureInvariant);
    var source = lineComment.Replace(blockComment.Replace(File.ReadAllText(path), string.Empty), string.Empty);
    var result = new HashSet<string>(StringComparer.Ordinal);
    foreach (Match match in importSpecifier.Matches(source))
    {
        var specifier = match.Groups[1].Value is { Length: > 0 } first ? first : match.Groups[2].Value;
        if (!specifier.StartsWith(".", StringComparison.Ordinal) && !specifier.StartsWith("/", StringComparison.Ordinal))
            result.Add(specifier);
    }

    return result;
}

static async Task<Dictionary<string, string>> AcquireSourcesAsync(string root, string? localSource, string[] packages, string version)
{
    var result = new Dictionary<string, string>(StringComparer.Ordinal);
    foreach (var package in packages)
    {
        var simpleName = package.Replace("@", string.Empty, StringComparison.Ordinal).Replace('/', '-');
        if (!string.IsNullOrWhiteSpace(localSource))
        {
            // Local source may be a node_modules-style root or an extracted tarball directory.
            var candidate = Path.Combine(localSource, package.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(candidate))
                candidate = Path.Combine(localSource, simpleName, "package");
            if (!Directory.Exists(candidate))
                candidate = Path.Combine(localSource, "package");
            if (!Directory.Exists(candidate))
                throw new DirectoryNotFoundException($"Local source for {package} not found under {localSource}.");
            result[package] = candidate;
            continue;
        }

        var cache = Path.Combine(root, ".tmp", "packages", "vue-draggable");
        var extraction = Path.Combine(cache, "extracted");
        var archive = Path.Combine(cache, simpleName + ".tgz");
        Directory.CreateDirectory(cache);
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        using var metadata = JsonDocument.Parse(await client.GetStringAsync($"https://registry.npmjs.org/{package}/{version}"));
        var distribution = metadata.RootElement.GetProperty("dist");
        var resolvedVersion = metadata.RootElement.GetProperty("version").GetString()!;
        if (resolvedVersion != version)
            throw new InvalidOperationException($"Registry returned {resolvedVersion} for {package}, expected {version}.");
        var integrity = distribution.GetProperty("integrity").GetString()!;
        if (!integrity.StartsWith("sha512-", StringComparison.Ordinal))
            throw new InvalidOperationException("Expected npm SHA-512 integrity metadata.");
        var expectedHash = integrity[7..];
        if (!File.Exists(archive) || HashArchive(archive) != expectedHash)
        {
            Console.WriteLine($"Downloading {package}@{version}");
            await using var input = await client.GetStreamAsync(distribution.GetProperty("tarball").GetString()!);
            await using var output = File.Create(archive);
            await input.CopyToAsync(output);
        }
        if (HashArchive(archive) != expectedHash)
            throw new InvalidOperationException($"Downloaded archive for {package} does not match npm integrity metadata.");

        var packageRoot = Path.Combine(extraction, simpleName);
        if (Directory.Exists(packageRoot))
            Directory.Delete(packageRoot, recursive: true);
        Directory.CreateDirectory(packageRoot);
        using var archiveStream = File.OpenRead(archive);
        using var gzip = new GZipStream(archiveStream, CompressionMode.Decompress);
        using var reader = new TarReader(gzip);
        while (reader.GetNextEntry() is { } entry)
        {
            if (!entry.Name.StartsWith("package/", StringComparison.Ordinal) || entry.DataStream is null ||
                entry.Name.EndsWith('/', StringComparison.Ordinal))
                continue;
            var destination = Path.Combine(packageRoot, entry.Name[8..].Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            using var output = File.Create(destination);
            entry.DataStream.CopyTo(output);
        }

        result[package] = packageRoot;
    }

    return result;
}

static JsonNode BuildManifest(
    string version,
    (string Specifier, string Package, string File)[] entries,
    Dictionary<string, string[]> dependencies,
    SortedDictionary<string, string> packageVersions,
    SortedDictionary<string, string> peerRequires,
    JsonArray licenseFiles)
{
    var imports = new JsonObject();
    foreach (var (specifier, _, _) in entries.OrderBy(static entry => entry.Specifier, StringComparer.Ordinal))
    {
        var dependenciesNode = new JsonArray();
        foreach (var dependency in dependencies[specifier])
            dependenciesNode.Add((JsonNode)dependency);

        imports[specifier] = new JsonObject
        {
            ["type"] = "module",
            ["path"] = specifier,
            ["dependencies"] = dependenciesNode,
        };
    }

    var requiresNode = new JsonObject();
    foreach (var (name, range) in peerRequires)
        requiresNode[name] = range;

    return new JsonObject
    {
        ["schemaVersion"] = 2,
        ["libraryId"] = "vue-draggable",
        ["version"] = version,
        ["source"] = "npm",
        ["packages"] = BuildPackages(packageVersions),
        ["imports"] = imports,
        ["requires"] = requiresNode,
        ["styles"] = new JsonArray(),
        ["files"] = licenseFiles,
    };
}

static JsonObject BuildPackages(SortedDictionary<string, string> packageVersions)
{
    var packages = new JsonObject();
    foreach (var (name, packageVersion) in packageVersions.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        packages[name] = new JsonObject { ["source"] = "npm", ["version"] = packageVersion };
    return packages;
}

static void WriteInventory(
    string projectRoot,
    string version,
    int validatedEntryCount,
    string[] specifiers,
    SortedDictionary<string, string> packageVersions,
    string libraryId,
    string documentation)
{
    var packages = new JsonObject();
    foreach (var (package, packageVersion) in packageVersions)
        packages[package] = packageVersion;

    var inventory = new JsonObject
    {
        ["upstream"] = libraryId,
        ["version"] = version,
        ["license"] = "MIT",
        ["packages"] = packages,
        ["source"] = $"https://registry.npmjs.org/{libraryId}/{version}",
        ["documentation"] = documentation,
        ["entryImports"] = new JsonArray(specifiers.Order(StringComparer.Ordinal).Select(static value => (JsonNode)value).ToArray()),
        ["validatedEntryCount"] = validatedEntryCount,
    };
    var payload = inventory.ToJsonString(GeneratorJson.Manifest);
    inventory["fingerprint"] = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    WriteLfText(Path.Combine(projectRoot, "inventory.json"), inventory.ToJsonString(GeneratorJson.Manifest) + "\n");
}

static string HashFile(string path)
    => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

static string HashArchive(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToBase64String(SHA512.HashData(stream));
}

// 生成的文本必须纯 LF；manifest 哈希逐字节校验，CRLF 在 fresh clone 后会校验失败。
static void WriteLfText(string path, string content)
    => File.WriteAllText(path, content.Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));

internal sealed class GeneratorOptions
{
    public string? Version { get; private set; }

    public string? Source { get; private set; }

    public static GeneratorOptions Parse(string[] args)
    {
        var options = new GeneratorOptions();
        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--version":
                    options.Version = args[++index];
                    break;
                case "--source":
                    options.Source = Path.GetFullPath(args[++index]);
                    break;
                default:
                    throw new ArgumentException("Unknown argument: " + args[index]);
            }
        }

        return options;
    }
}

internal static class GeneratorJson
{
    public static readonly JsonSerializerOptions Manifest = new()
    {
        // JsonObject keys are authored verbatim; indentation only mirrors the other manifests.
        WriteIndented = true,
    };
}
