#!/usr/bin/env dotnet run

using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

// Vendor the locked VueUse ESM runtime into src/ECMAScript.VueUse and regenerate the
// package-local manifest and upstream inventory. @vueuse/core is a single self-contained
// bundle whose only sibling bare import is @vueuse/shared; both peer-depend on vue.
// 上游以 npm registry integrity 锁定版本；core 是自包含 bundle，唯一同闭包裸依赖是 shared，两者 peer 依赖 vue。
//
// Usage:
//   dotnet run --file scripts/csharp/generate-vueuse.cs -- --version 15.0.0
//   dotnet run --file scripts/csharp/generate-vueuse.cs -- --source <node_modules> --version 15.0.0
var options = GeneratorOptions.Parse(args);
var root = Directory.GetCurrentDirectory();
if (!File.Exists(Path.Combine(root, "Jazor.slnx")))
    throw new InvalidOperationException("Run from the repository root.");

var projectRoot = Path.Combine(root, "src", "ECMAScript.VueUse");
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

// 逻辑 import → 上游包、入口文件与 vendored 目标路径。
var entries = new (string Specifier, string Package, string File, string Target)[]
{
    ("@vueuse/core", "@vueuse/core", "dist/index.js", "@vueuse/core/index.js"),
    ("@vueuse/shared", "@vueuse/shared", "dist/index.js", "@vueuse/shared/index.js"),
};

var packages = entries.Select(static entry => entry.Package).Distinct(StringComparer.Ordinal).ToArray();
var sources = await AcquireSourcesAsync(root, options.Source, packages, version);

var primaryPackage = "@vueuse/core";
var packageVersions = new SortedDictionary<string, string>(StringComparer.Ordinal);
foreach (var package in packages.Order(StringComparer.Ordinal))
{
    var packageJson = JsonNode.Parse(File.ReadAllText(Path.Combine(sources[package], "package.json")))!;
    packageVersions[package] = packageJson["version"]!.GetValue<string>();
}

if (packageVersions[primaryPackage] != version)
    throw new InvalidOperationException($"{primaryPackage} extracted version '{packageVersions[primaryPackage]}' does not match requested '{version}'.");

var known = entries.Select(static entry => entry.Specifier).ToHashSet(StringComparer.Ordinal);
var peerSpecifiers = new HashSet<string>(StringComparer.Ordinal) { "vue" };
var dependencies = new Dictionary<string, string[]>(StringComparer.Ordinal);
var unresolved = new SortedSet<string>(StringComparer.Ordinal);
foreach (var (specifier, package, file, _) in entries)
{
    var source = Path.Combine(sources[package], file.Replace('/', Path.DirectorySeparatorChar));
    if (!File.Exists(source))
        throw new InvalidOperationException($"Upstream entry '{file}' is missing from {package}.");
    var bare = ReadBareSpecifiers(source);
    // requires 约束 provider 版本；dependencies 才是 NetPack 解析真实 bare import 的图边。
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

var distRoot = Path.Combine(projectRoot, "dist");
var licensesRoot = Path.Combine(projectRoot, "licenses");
Directory.CreateDirectory(distRoot);
Directory.CreateDirectory(licensesRoot);

// 整体替换 dist，保证 manifest 与 vendored 文件始终一一对应。
if (Directory.Exists(distRoot))
    Directory.Delete(distRoot, recursive: true);
Directory.CreateDirectory(distRoot);

var vendored = new SortedDictionary<string, string>(StringComparer.Ordinal); // target relative -> source path
foreach (var (_, package, file, target) in entries)
    vendored[target] = Path.Combine(sources[package], file.Replace('/', Path.DirectorySeparatorChar));

foreach (var (relative, source) in vendored)
{
    var destination = Path.Combine(distRoot, relative.Replace('/', Path.DirectorySeparatorChar));
    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
    File.Copy(source, destination, overwrite: false);
}

var licenseFiles = new JsonArray();
foreach (var package in packages.Order(StringComparer.Ordinal))
{
    var licenseSource = Path.Combine(sources[package], "LICENSE");
    if (!File.Exists(licenseSource))
        throw new InvalidOperationException($"{package} does not ship a LICENSE file.");
    var licenseName = package.Replace("@vueuse/", string.Empty, StringComparison.Ordinal) + "-LICENSE";
    File.Copy(licenseSource, Path.Combine(licensesRoot, licenseName), overwrite: true);
    licenseFiles.Add(new JsonObject
    {
        ["type"] = "license",
        ["path"] = "licenses/" + licenseName,
        ["hash"] = HashFile(Path.Combine(licensesRoot, licenseName)),
    });
}

var manifest = BuildManifest(version, entries, dependencies, vendored, licenseFiles);
WriteLfText(manifestPath, manifest.ToJsonString(GeneratorJson.Manifest) + "\n");

WriteInventory(projectRoot, version, vendored.Count, entries.Select(static entry => entry.Specifier).ToArray(), packageVersions);

Console.WriteLine($"Vendored {vendored.Count} VueUse {version} bundle(s); license: MIT.");
Console.WriteLine("Review contract drift for bound exports before committing.");

static HashSet<string> ReadBareSpecifiers(string path)
{
    // Bundled entries only reference other packages; strip comments so JSDoc examples do not count.
    // 自包含 bundle 只引用其他包；剥离注释避免把 JSDoc 示例当成依赖。
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
        var simpleName = package.Split('/')[^1];
        if (!string.IsNullOrWhiteSpace(localSource))
        {
            // Local source follows a node_modules-style layout: <source>/<@scope/name>/<package files>.
            var candidate = Path.Combine(localSource, package.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(candidate))
                candidate = Path.Combine(localSource, simpleName, "package");
            if (!Directory.Exists(candidate))
                throw new DirectoryNotFoundException($"Local source for {package} not found under {localSource}.");
            result[package] = candidate;
            continue;
        }

        var cache = Path.Combine(root, ".tmp", "packages", "vueuse");
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
            Console.WriteLine($"Downloading {package}@{resolvedVersion}");
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
    (string Specifier, string Package, string File, string Target)[] entries,
    Dictionary<string, string[]> dependencies,
    SortedDictionary<string, string> vendored,
    JsonArray licenseFiles)
{
    var imports = new JsonObject();
    foreach (var (specifier, _, _, target) in entries.OrderBy(static entry => entry.Specifier, StringComparer.Ordinal))
    {
        var hash = HashFile(vendored[target]);
        var packageDependencies = new JsonArray();
        foreach (var dependency in dependencies[specifier])
            packageDependencies.Add(dependency);

        imports[specifier] = new JsonObject
        {
            ["type"] = "module",
            ["development"] = "dist/" + target,
            ["production"] = "dist/" + target,
            ["developmentHash"] = hash,
            ["productionHash"] = hash,
            // 兄弟条目和 peer 都通过 package 通道解析；requires 只负责 provider 版本约束。
            ["developmentDependencies"] = (JsonArray)packageDependencies.DeepClone(),
            ["productionDependencies"] = packageDependencies,
            ["developmentModuleDependencies"] = new JsonArray(),
            ["productionModuleDependencies"] = new JsonArray(),
            ["files"] = new JsonArray(),
        };
    }

    return new JsonObject
    {
        ["schemaVersion"] = 2,
        ["libraryId"] = "vueuse",
        ["version"] = version,
        ["imports"] = imports,
        // core 与 shared 都 peer 依赖 vue，由 Vue 资源库提供；具体入口依赖边记录在 imports 中。
        ["requires"] = new JsonObject { ["vue3"] = "^3.5.0" },
        ["styles"] = new JsonArray(),
        ["files"] = licenseFiles,
    };
}

static void WriteInventory(
    string projectRoot,
    string version,
    int vendoredCount,
    string[] specifiers,
    SortedDictionary<string, string> packageVersions)
{
    var packages = new JsonObject();
    foreach (var (package, packageVersion) in packageVersions)
        packages[package] = packageVersion;

    var inventory = new JsonObject
    {
        ["upstream"] = "vueuse",
        ["version"] = version,
        ["license"] = "MIT",
        ["packages"] = packages,
        ["source"] = $"https://registry.npmjs.org/@vueuse/core/{version}",
        ["documentation"] = "https://vueuse.org/functions.html",
        ["entryImports"] = new JsonArray(specifiers.Order(StringComparer.Ordinal).Select(static value => (JsonNode)value).ToArray()),
        ["vendoredModuleCount"] = vendoredCount,
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

// Generated text must be LF-only: .gitattributes normalizes CRLF to LF on commit, and manifest
// hashes are byte-exact, so CRLF would fail verification after a fresh clone.
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
