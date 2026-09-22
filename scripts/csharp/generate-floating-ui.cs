#!/usr/bin/env dotnet run

using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

// Validate the locked Floating UI ESM entry graph and regenerate package metadata. Runtime bytes
// remain in the upstream npm packages; this generator does not recreate a binding-owned dist.
//
// Usage:
//   dotnet run --file scripts/csharp/generate-floating-ui.cs -- --version 2.0.1
//   dotnet run --file scripts/csharp/generate-floating-ui.cs -- --source <dir> --version 2.0.1
//
// --source expects a directory containing <package>@<version> extracted trees in
// <source>/<package-without-scope-and-@>/<package>/ layout, or set VUE_ECOSYSTEM_NODE_MODULES.
var options = GeneratorOptions.Parse(args);
var root = Directory.GetCurrentDirectory();
if (!File.Exists(Path.Combine(root, "Jazor.slnx")))
    throw new InvalidOperationException("Run from the repository root.");

var projectRoot = Path.Combine(root, "src", "ECMAScript.FloatingUi");
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

// 逻辑 import → 上游包与入口文件。闭包边由入口文件的裸 import 静态推导，不手工维护。
var entries = new (string Specifier, string Package, string File)[]
{
    ("@floating-ui/vue", "@floating-ui/vue", "dist/floating-ui.vue.mjs"),
    ("@floating-ui/dom", "@floating-ui/dom", "dist/floating-ui.dom.mjs"),
    ("@floating-ui/core", "@floating-ui/core", "dist/floating-ui.core.mjs"),
    ("@floating-ui/utils", "@floating-ui/utils", "dist/floating-ui.utils.mjs"),
    ("@floating-ui/utils/dom", "@floating-ui/utils", "dist/floating-ui.utils.dom.mjs"),
};

var sources = await AcquireSourcesAsync(root, options.Source, entries.Select(static entry => entry.Package).Distinct(StringComparer.Ordinal), version);

// 作者入口包（@floating-ui/vue）的版本是 manifest version；闭包内其他包各自锁定版本并记入 inventory。
// 校验作者入口版本，并采集全部包版本。
var primaryPackage = "@floating-ui/vue";
var packageVersions = new SortedDictionary<string, string>(StringComparer.Ordinal);
foreach (var package in sources.Keys.Order(StringComparer.Ordinal))
{
    var packageJson = JsonNode.Parse(File.ReadAllText(Path.Combine(sources[package], "package.json")))!;
    var packageVersion = packageJson["version"]!.GetValue<string>();
    packageVersions[package] = packageVersion;
}

if (packageVersions[primaryPackage] != version)
    throw new InvalidOperationException($"{primaryPackage} extracted version '{packageVersions[primaryPackage]}' does not match requested '{version}'.");

var known = entries.Select(static entry => entry.Specifier).ToHashSet(StringComparer.Ordinal);
var dependencies = new Dictionary<string, string[]>(StringComparer.Ordinal);
foreach (var (specifier, package, file) in entries)
{
    var source = Path.Combine(sources[package], file.Replace('/', Path.DirectorySeparatorChar));
    if (!File.Exists(source))
        throw new InvalidOperationException($"Upstream entry '{file}' is missing from {package}.");
    var bare = ReadBareSpecifiers(source);
    // peer 依赖（vue）不是本闭包条目，改为 requires 声明。
    dependencies[specifier] = bare.Where(known.Contains).Order(StringComparer.Ordinal).ToArray();
}

// 稳定性断言：若上游新增闭包内依赖而本表未收录，必须显式失败而不是静默漏 vendoring。
var unresolved = new SortedSet<string>(StringComparer.Ordinal);
foreach (var (specifier, package, file) in entries)
{
    foreach (var bare in ReadBareSpecifiers(Path.Combine(sources[package], file.Replace('/', Path.DirectorySeparatorChar))))
        if (!known.Contains(bare) && bare != "vue")
            unresolved.Add(specifier + " -> " + bare);
}

if (unresolved.Count > 0)
    throw new InvalidOperationException("Upstream adds references outside the declared closure: " + string.Join(", ", unresolved));

var licensesRoot = Path.Combine(projectRoot, "licenses");
Directory.CreateDirectory(licensesRoot);

var licenses = entries.Select(static entry => entry.Package).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
var licenseFiles = new JsonArray();
foreach (var package in licenses)
{
    var licenseSource = Path.Combine(sources[package], "LICENSE");
    if (!File.Exists(licenseSource))
        throw new InvalidOperationException($"{package} does not ship a LICENSE file.");
    var licenseName = package.Replace("@floating-ui/", string.Empty, StringComparison.Ordinal) + "-LICENSE";
    File.Copy(licenseSource, Path.Combine(licensesRoot, licenseName), overwrite: true);
    licenseFiles.Add(new JsonObject
    {
        ["type"] = "license",
        ["path"] = "licenses/" + licenseName,
        ["hash"] = HashFile(Path.Combine(licensesRoot, licenseName)),
    });
}

var manifest = BuildManifest(version, entries, dependencies, packageVersions, licenseFiles);
WriteLfText(manifestPath, manifest.ToJsonString(GeneratorJson.Manifest) + "\n");

WriteInventory(projectRoot, version, entries.Length, entries.Select(static entry => entry.Specifier).ToArray(), packageVersions);

Console.WriteLine($"Validated {entries.Length} Floating UI {version} npm entries; license: MIT.");
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

static async Task<Dictionary<string, string>> AcquireSourcesAsync(
    string root,
    string? localSource,
    IEnumerable<string> packages,
    string version)
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

        // 缓存目录按 registry slug 命名，多包各自 lock 版本互不干扰。
        var cache = Path.Combine(root, ".tmp", "packages", "floating-ui");
        var extraction = Path.Combine(root, ".tmp", "packages", "floating-ui", "extracted");
        var archive = Path.Combine(cache, (package.Replace("@", string.Empty, StringComparison.Ordinal).Replace('/', '-')) + ".tgz");
        Directory.CreateDirectory(cache);
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        using var metadata = JsonDocument.Parse(await client.GetStringAsync($"https://registry.npmjs.org/{package}/{version}"));
        var distribution = metadata.RootElement.GetProperty("dist");
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
    JsonArray licenseFiles)
{
    var imports = new JsonObject();
    foreach (var (specifier, _, _) in entries.OrderBy(static entry => entry.Specifier, StringComparer.Ordinal))
    {
        var packageDependencies = new JsonArray();
        foreach (var dependency in dependencies[specifier])
            packageDependencies.Add(dependency);

        imports[specifier] = new JsonObject
        {
            ["type"] = "module",
            ["path"] = specifier,
            ["dependencies"] = packageDependencies,
        };
    }

    return new JsonObject
    {
        ["schemaVersion"] = 2,
        ["libraryId"] = "floating-ui",
        ["version"] = version,
        ["source"] = "npm",
        ["packages"] = BuildPackages(packageVersions),
        ["imports"] = imports,
        // @floating-ui/vue 的 peer 依赖；core/dom/utils 不需要 vue，但 manifest 级 requires 是统一闭包。
        ["requires"] = new JsonObject { ["vue3"] = "^3.5.0" },
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
    SortedDictionary<string, string> packageVersions)
{
    var packages = new JsonObject();
    foreach (var (package, packageVersion) in packageVersions)
        packages[package] = packageVersion;

    var inventory = new JsonObject
    {
        ["upstream"] = "floating-ui",
        ["version"] = version,
        ["license"] = "MIT",
        ["packages"] = packages,
        ["source"] = $"https://registry.npmjs.org/@floating-ui/vue/{version}",
        ["documentation"] = "https://floating-ui.com/docs/vue",
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
