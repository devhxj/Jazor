#!/usr/bin/env dotnet run

using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

// Validate the locked vue-filepond + filepond ESM entries and regenerate package metadata.
// Runtime modules and styles remain in the upstream npm packages.
//
// The Vue adapter is a default-export factory over the filepond core; the author entry is
// `vue-filepond` and `filepond` is its peer/dependency inside the same closure.
//
// Usage:
//   dotnet run --file scripts/csharp/generate-file-pond.cs -- --version 8.0.0
//   dotnet run --file scripts/csharp/generate-file-pond.cs -- --source <node_modules> --version 8.0.0
var options = GeneratorOptions.Parse(args);
var root = Directory.GetCurrentDirectory();
if (!File.Exists(Path.Combine(root, "Jazor.slnx")))
    throw new InvalidOperationException("Run from the repository root.");

var projectRoot = Path.Combine(root, "src", "ECMAScript.FilePond");
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

// 作者入口为 vue-filepond（Vue 适配器包）；filepond 核心是同一闭包内的依赖包。
// 两个入口都显式列入，使 C# 侧既能引用适配器工厂、也能引用核心的 create/setOptions。
// 核心版本按上游 peer 区间（>=4.7.4 <5.x）锁定为具体版本；升级时在此复核。
const string FilePondCoreVersion = "4.32.12";
var entries = new (string Specifier, string Package, string File, string RequestedVersion)[]
{
    ("vue-filepond", "vue-filepond", "dist/vue-filepond.esm.js", version),
    ("filepond", "filepond", "dist/filepond.esm.js", FilePondCoreVersion),
};

var peerSpecifiers = new HashSet<string>(StringComparer.Ordinal) { "vue" };
var peerRequires = new SortedDictionary<string, string>(StringComparer.Ordinal) { ["vue3"] = "^3.0.0" };

var sources = await AcquireSourcesAsync(
    root,
    options.Source,
    entries.Select(static entry => (entry.Package, entry.RequestedVersion)).Distinct().ToArray());

var primaryPackage = "vue-filepond";
var packageVersions = new SortedDictionary<string, string>(StringComparer.Ordinal);
foreach (var package in sources.Keys.Order(StringComparer.Ordinal))
{
    var packageJson = JsonNode.Parse(File.ReadAllText(Path.Combine(sources[package], "package.json")))!;
    packageVersions[package] = packageJson["version"]!.GetValue<string>();
}

if (packageVersions[primaryPackage] != version)
    throw new InvalidOperationException($"{primaryPackage} extracted version '{packageVersions[primaryPackage]}' does not match requested '{version}'.");

var known = entries.Select(static entry => entry.Specifier).ToHashSet(StringComparer.Ordinal);
var dependencies = new Dictionary<string, string[]>(StringComparer.Ordinal);
var unresolved = new SortedSet<string>(StringComparer.Ordinal);
foreach (var (specifier, package, file, _) in entries)
{
    var source = Path.Combine(sources[package], file.Replace('/', Path.DirectorySeparatorChar));
    if (!File.Exists(source))
        throw new InvalidOperationException($"Upstream entry '{file}' is missing from {package}.");

    // 浏览器安全闸门：bundler 变体在模块顶层引用 process.env，浏览器导入即崩溃。
    AssertBrowserSafe(source, specifier);

    var bare = ReadBareSpecifiers(source);
    dependencies[specifier] = bare.Where(known.Contains).Order(StringComparer.Ordinal).ToArray();
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

// Validate the upstream stylesheet and record it as an entry-level package edge.
var filePondDist = Path.Combine(sources["filepond"], "dist");
if (!File.Exists(Path.Combine(filePondDist, "filepond.css")))
    throw new InvalidOperationException("filepond does not ship 'filepond.css'.");

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

WriteInventory(projectRoot, version, entries.Length + 1, entries.Select(static entry => entry.Specifier).ToArray(), packageVersions);

Console.WriteLine($"Validated {entries.Length} FilePond {version} npm entries and one stylesheet; license: MIT.");
Console.WriteLine("Review contract drift for bound options and callbacks before committing.");

static void AssertBrowserSafe(string path, string specifier)
{
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

static string? ResolveLocalPackage(string localSource, string package, string simpleName)
{
    // 探测 node_modules（<root>/<pkg>/…）、解压 tarball（<root>/<dir>/package/…）以及带版本
    // 后缀的解压目录（<root>/<name>-<version>/package/…）。以 package.json 的 name 字段确认匹配。
    var candidates = new List<string>
    {
        Path.Combine(localSource, package.Replace('/', Path.DirectorySeparatorChar)),
        Path.Combine(localSource, simpleName, "package"),
        Path.Combine(localSource, simpleName),
    };
    if (Directory.Exists(localSource))
    {
        foreach (var directory in Directory.EnumerateDirectories(localSource, simpleName + "*"))
            candidates.Add(Path.Combine(directory, "package"));
        foreach (var directory in Directory.EnumerateDirectories(localSource, simpleName + "*"))
            candidates.Add(directory);
    }

    foreach (var candidate in candidates)
    {
        var packageJson = Path.Combine(candidate, "package.json");
        if (!File.Exists(packageJson))
            continue;
        var name = JsonNode.Parse(File.ReadAllText(packageJson))?["name"]?.GetValue<string>();
        if (string.Equals(name, package, StringComparison.Ordinal))
            return candidate;
    }

    return null;
}

static async Task<Dictionary<string, string>> AcquireSourcesAsync(string root, string? localSource, (string Package, string Version)[] packages)
{
    var result = new Dictionary<string, string>(StringComparer.Ordinal);
    foreach (var (package, requestedVersion) in packages)
    {
        var simpleName = package.Replace("@", string.Empty, StringComparison.Ordinal).Replace('/', '-');
        if (!string.IsNullOrWhiteSpace(localSource))
        {
            // 探测两种受支持布局：node_modules（<root>/<pkg>/…）与解压 tarball
            // （<root>/<name>/package/… 或 <root>/<name>/…）。找到含匹配 package.json 的目录即用。
            var candidate = ResolveLocalPackage(localSource, package, simpleName);
            if (candidate is null)
                throw new DirectoryNotFoundException($"Local source for {package} not found under {localSource}.");
            result[package] = candidate;
            continue;
        }

        var cache = Path.Combine(root, ".tmp", "packages", "filepond");
        var extraction = Path.Combine(cache, "extracted");
        var archive = Path.Combine(cache, simpleName + ".tgz");
        Directory.CreateDirectory(cache);
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        using var metadata = JsonDocument.Parse(await client.GetStringAsync($"https://registry.npmjs.org/{package}/{requestedVersion}"));
        var distribution = metadata.RootElement.GetProperty("dist");
        var integrity = distribution.GetProperty("integrity").GetString()!;
        if (!integrity.StartsWith("sha512-", StringComparison.Ordinal))
            throw new InvalidOperationException("Expected npm SHA-512 integrity metadata.");
        var expectedHash = integrity[7..];
        if (!File.Exists(archive) || HashArchive(archive) != expectedHash)
        {
            Console.WriteLine($"Downloading {package}@{requestedVersion}");
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
    (string Specifier, string Package, string File, string RequestedVersion)[] entries,
    Dictionary<string, string[]> dependencies,
    SortedDictionary<string, string> packageVersions,
    SortedDictionary<string, string> peerRequires,
    JsonArray licenseFiles)
{
    var imports = new JsonObject();
    foreach (var (specifier, _, _, _) in entries.OrderBy(static entry => entry.Specifier, StringComparer.Ordinal))
    {
        var dependenciesNode = new JsonArray();
        foreach (var dependency in dependencies[specifier])
            dependenciesNode.Add((JsonNode)dependency);

        imports[specifier] = new JsonObject
        {
            ["type"] = "module",
            ["development"] = specifier,
            ["production"] = specifier,
            ["developmentDependencies"] = (JsonArray)dependenciesNode.DeepClone(),
            ["productionDependencies"] = dependenciesNode,
        };
        if (string.Equals(specifier, "filepond", StringComparison.Ordinal))
        {
            imports[specifier]!["developmentStylesheetImports"] = new JsonArray { "filepond/dist/filepond.css" };
            imports[specifier]!["productionStylesheetImports"] = new JsonArray { "filepond/dist/filepond.css" };
        }
    }

    var requiresNode = new JsonObject();
    foreach (var (name, range) in peerRequires)
        requiresNode[name] = range;

    return new JsonObject
    {
        ["schemaVersion"] = 2,
        ["libraryId"] = "file-pond",
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
    SortedDictionary<string, string> packageVersions)
{
    var packages = new JsonObject();
    foreach (var (package, packageVersion) in packageVersions)
        packages[package] = packageVersion;

    var inventory = new JsonObject
    {
        ["upstream"] = "file-pond",
        ["version"] = version,
        ["license"] = "MIT",
        ["packages"] = packages,
        ["source"] = $"https://registry.npmjs.org/vue-filepond/{version}",
        ["documentation"] = "https://pqina.nl/filepond/docs/",
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
