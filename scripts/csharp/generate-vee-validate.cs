#!/usr/bin/env dotnet run

using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

// Vendor the locked vee-validate ESM runtime into src/ECMAScript.VeeValidate and regenerate the
// package-local manifest and upstream inventory. vee-validate ships one self-contained bundle whose
// only bare import is the Vue peer supplied by the ECMAScript.Vue resource library.
// 上游以 npm registry integrity 锁定版本；vee-validate 只发布自包含 bundle，唯一裸导入是 vue peer。
//
// Usage:
//   dotnet run --file scripts/csharp/generate-vee-validate.cs -- --version 4.15.1
//   dotnet run --file scripts/csharp/generate-vee-validate.cs -- --source .tmp/p3b/node_modules --version 4.15.1
var options = GeneratorOptions.Parse(args);
var root = Directory.GetCurrentDirectory();
if (!File.Exists(Path.Combine(root, "Jazor.slnx")))
    throw new InvalidOperationException("Run from the repository root.");

const string libraryId = "vee-validate";
const string primaryPackage = "vee-validate";
const string documentation = "https://vee-validate.logaretm.com/v4/";
const string licenseId = "MIT";

var projectRoot = Path.Combine(root, "src", "ECMAScript.VeeValidate");
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

// 逻辑 import → 上游包、锁定版本与入口文件。入口闭包由相对导入静态推导，跨包边由裸 specifier 推导。
// 非作者入口包的版本是闭包锁定值；升级作者入口时在此同步评估。
var entries = new (string Specifier, string Package, string PackageVersion, string File)[]
{
    ("vee-validate", "vee-validate", version, "dist/vee-validate.mjs"),
};

// 由 ECMAScript.Vue 资源库提供的 peer specifier；不进入本包闭包，只写入 dependencies 与 requires。
var peerSpecifiers = new HashSet<string>(StringComparer.Ordinal) { "vue" };

// 绑定只支持 Vue 3；上游 package.json 仍列出 Vue 2 区间，因此在此显式声明 requires 范围。
var peerRequires = new SortedDictionary<string, string>(StringComparer.Ordinal) { ["vue3"] = "^3.4.26" };

var sources = await AcquireSourcesAsync(root, options.Source, entries.Select(static entry => (entry.Package, entry.PackageVersion)).Distinct().ToArray(), libraryId);

var packageVersions = new SortedDictionary<string, string>(StringComparer.Ordinal);
foreach (var package in sources.Keys.Order(StringComparer.Ordinal))
{
    var packageJson = JsonNode.Parse(File.ReadAllText(Path.Combine(sources[package], "package.json")))!;
    packageVersions[package] = packageJson["version"]!.GetValue<string>()!;
}

if (packageVersions[primaryPackage] != version)
    throw new InvalidOperationException($"{primaryPackage} extracted version '{packageVersions[primaryPackage]}' does not match requested '{version}'.");

var known = entries.Select(static entry => entry.Specifier).ToHashSet(StringComparer.Ordinal);
var closures = new Dictionary<string, SortedDictionary<string, string>>(StringComparer.Ordinal);
var packageEdges = new Dictionary<string, string[]>(StringComparer.Ordinal);
var unresolved = new SortedSet<string>(StringComparer.Ordinal);

foreach (var (specifier, package, _, file) in entries)
{
    var sourceRoot = sources[package];
    var closure = ComputeClosure(sourceRoot, file);
    closures[specifier] = closure;

    var bare = new SortedSet<string>(StringComparer.Ordinal);
    foreach (var relative in closure.Keys)
    {
        foreach (var spec in ReadBareSpecifiers(Path.Combine(sourceRoot, relative.Replace('/', Path.DirectorySeparatorChar))))
        {
            // peer specifier 同样要声明为 package 依赖：materializer 按闭包内 specifier 索引解析它们。
            if (known.Contains(spec) || peerSpecifiers.Contains(spec))
                bare.Add(spec);
            else
                unresolved.Add(specifier + " -> " + spec);
        }
    }

    packageEdges[specifier] = bare.ToArray();
}

// 稳定性断言：上游新增闭包外依赖时必须显式失败，而不是静默漏 vendoring。
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

var vendored = new SortedDictionary<string, string>(StringComparer.Ordinal); // dist-relative -> source path
foreach (var (specifier, package, _, _) in entries)
{
    foreach (var relative in closures[specifier].Keys)
    {
        // Keys are dist-relative logical paths and must stay forward-slashed across platforms.
        vendored[package + "/" + relative] = Path.Combine(sources[package], relative.Replace('/', Path.DirectorySeparatorChar));
    }
}

foreach (var (relative, source) in vendored)
{
    var destination = Path.Combine(distRoot, relative.Replace('/', Path.DirectorySeparatorChar));
    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
    File.Copy(source, destination, overwrite: false);
}

var packages = entries.Select(static entry => entry.Package).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
var licenseFiles = new JsonArray();
foreach (var package in packages)
{
    var licenseSource = Path.Combine(sources[package], "LICENSE");
    if (!File.Exists(licenseSource))
        throw new InvalidOperationException($"{package} does not ship a LICENSE file.");
    var licenseName = ToLicenseFileName(package);
    File.Copy(licenseSource, Path.Combine(licensesRoot, licenseName), overwrite: true);
    licenseFiles.Add(new JsonObject
    {
        ["type"] = "license",
        ["path"] = "licenses/" + licenseName,
        ["hash"] = HashFile(Path.Combine(licensesRoot, licenseName)),
    });
}

var manifest = BuildManifest(libraryId, version, entries, closures, packageEdges, vendored, peerRequires, licenseFiles);
WriteLfText(manifestPath, manifest.ToJsonString(GeneratorJson.Manifest) + "\n");

WriteInventory(projectRoot, libraryId, version, licenseId, documentation, vendored.Count, entries.Select(static entry => entry.Specifier).ToArray(), packageVersions);

Console.WriteLine($"Vendored {vendored.Count} {libraryId} {version} module(s); license: {licenseId}.");
Console.WriteLine("Review contract drift for bound exports before committing.");

static SortedDictionary<string, string> ComputeClosure(string sourceRoot, string entry)
{
    // Static relative-import closure. Comments are stripped so JSDoc examples never count as edges.
    // 静态相对导入闭包；剥离注释避免把 JSDoc 示例当成依赖。
    var importSpecifier = ImportSpecifierRegex();
    var closure = new SortedDictionary<string, string>(StringComparer.Ordinal);
    var queue = new Queue<string>();
    queue.Enqueue(entry);
    while (queue.Count > 0)
    {
        var current = queue.Dequeue();
        if (closure.ContainsKey(current))
            continue;
        var fullPath = Path.Combine(sourceRoot, current.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            throw new InvalidOperationException($"Closure references missing module '{current}'.");
        closure.Add(current, fullPath);

        foreach (Match match in importSpecifier.Matches(StripComments(fullPath)))
        {
            var specifier = match.Groups[1].Value is { Length: > 0 } first ? first : match.Groups[2].Value;
            if (!specifier.StartsWith(".", StringComparison.Ordinal))
                continue;
            var resolved = NormalizeModulePath(Path.Join(Path.GetDirectoryName(current) ?? string.Empty, specifier));
            if (!closure.ContainsKey(resolved))
                queue.Enqueue(resolved);
        }
    }

    return closure;
}

static HashSet<string> ReadBareSpecifiers(string path)
{
    var result = new HashSet<string>(StringComparer.Ordinal);
    foreach (Match match in ImportSpecifierRegex().Matches(StripComments(path)))
    {
        var specifier = match.Groups[1].Value is { Length: > 0 } first ? first : match.Groups[2].Value;
        if (!specifier.StartsWith(".", StringComparison.Ordinal) && !specifier.StartsWith("/", StringComparison.Ordinal))
            result.Add(specifier);
    }

    return result;
}

static string StripComments(string path)
{
    var blockComment = new Regex(@"/\*[\s\S]*?\*/", RegexOptions.CultureInvariant);
    var lineComment = new Regex(@"(?m)^\s*//.*$", RegexOptions.CultureInvariant);
    return lineComment.Replace(blockComment.Replace(File.ReadAllText(path), string.Empty), string.Empty)
        .ReplaceLineEndings("\n");
}

static Regex ImportSpecifierRegex()
    => new(@"(?:import|export)\s[^;]*?from\s*[""']([^""']+)[""']|import\s*[""']([^""']+)[""']", RegexOptions.CultureInvariant);

static string NormalizeModulePath(string path)
{
    // 词法解析 `.`/`..`，保证同一模块只有一个闭包键，避免重复拷贝与 manifest 双条目。
    var segments = new Stack<string>();
    foreach (var segment in path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries))
    {
        if (segment == ".")
            continue;
        if (segment == "..")
        {
            if (segments.Count > 0)
                segments.Pop();
            continue;
        }

        segments.Push(segment);
    }

    return string.Join('/', segments.Reverse());
}

static string ToLicenseFileName(string package)
    => package.Replace("@", string.Empty, StringComparison.Ordinal).Replace('/', '-') + "-LICENSE";

static async Task<Dictionary<string, string>> AcquireSourcesAsync(
    string root,
    string? localSource,
    (string Package, string Version)[] packages,
    string libraryId)
{
    var result = new Dictionary<string, string>(StringComparer.Ordinal);
    foreach (var (package, packageVersion) in packages)
    {
        if (!string.IsNullOrWhiteSpace(localSource))
        {
            result[package] = FindLocalPackage(localSource, package, packageVersion);
            continue;
        }

        var cache = Path.Combine(root, ".tmp", "packages", libraryId);
        var extraction = Path.Combine(cache, "extracted");
        var archive = Path.Combine(cache, package.Replace("@", string.Empty, StringComparison.Ordinal).Replace('/', '-') + ".tgz");
        Directory.CreateDirectory(cache);
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        using var metadata = JsonDocument.Parse(await client.GetStringAsync($"https://registry.npmjs.org/{package}/{packageVersion}"));
        var distribution = metadata.RootElement.GetProperty("dist");
        var integrity = distribution.GetProperty("integrity").GetString()!;
        if (!integrity.StartsWith("sha512-", StringComparison.Ordinal))
            throw new InvalidOperationException("Expected npm SHA-512 integrity metadata.");
        var expectedHash = integrity[7..];
        if (!File.Exists(archive) || HashArchive(archive) != expectedHash)
        {
            Console.WriteLine($"Downloading {package}@{packageVersion}");
            await using var input = await client.GetStreamAsync(distribution.GetProperty("tarball").GetString()!);
            await using var output = File.Create(archive);
            await input.CopyToAsync(output);
        }
        if (HashArchive(archive) != expectedHash)
            throw new InvalidOperationException($"Downloaded archive for {package} does not match npm integrity metadata.");

        var packageRoot = Path.Combine(extraction, package.Replace("@", string.Empty, StringComparison.Ordinal).Replace('/', '-'));
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

static string FindLocalPackage(string localSource, string package, string version)
{
    // Local source is a node_modules-style tree; transitive packages may be nested, so match
    // package.json identity instead of assuming a flat layout.
    // 本地源是 node_modules 形态；传递依赖可能嵌套，因此按 package.json 身份匹配而非假设扁平布局。
    var direct = Path.Combine(localSource, package.Replace('/', Path.DirectorySeparatorChar));
    if (Matches(direct))
        return direct;
    var nested = Path.Combine(localSource, package.Split('/')[^1], "package");
    if (Matches(nested))
        return nested;

    foreach (var candidate in Directory.EnumerateDirectories(localSource, package.Split('/')[^1], SearchOption.AllDirectories))
    {
        if (Matches(candidate))
            return candidate;
    }

    throw new DirectoryNotFoundException($"Local source for {package}@{version} not found under {localSource}.");

    bool Matches(string candidate)
    {
        var manifest = Path.Combine(candidate, "package.json");
        if (!File.Exists(manifest))
            return false;
        var json = JsonNode.Parse(File.ReadAllText(manifest));
        return json?["name"]?.GetValue<string>() == package && json["version"]?.GetValue<string>() == version;
    }
}

static JsonNode BuildManifest(
    string libraryId,
    string version,
    (string Specifier, string Package, string PackageVersion, string File)[] entries,
    Dictionary<string, SortedDictionary<string, string>> closures,
    Dictionary<string, string[]> packageEdges,
    SortedDictionary<string, string> vendored,
    SortedDictionary<string, string> peerRequires,
    JsonArray licenseFiles)
{
    var imports = new JsonObject();
    foreach (var (specifier, package, _, file) in entries.OrderBy(static entry => entry.Specifier, StringComparer.Ordinal))
    {
        var closure = closures[specifier];
        var moduleDependencies = new JsonArray();
        var files = new JsonArray();
        foreach (var relative in closure.Keys.Where(key => !string.Equals(key, file, StringComparison.Ordinal)))
        {
            var distPath = "dist/" + package + "/" + relative;
            moduleDependencies.Add((JsonNode)distPath);
            files.Add(new JsonObject
            {
                ["type"] = "module",
                ["path"] = distPath,
                ["hash"] = HashFile(vendored[package + "/" + relative]),
                ["moduleId"] = distPath,
            });
        }

        var entryDependencies = new JsonArray();
        foreach (var dependency in packageEdges[specifier])
            entryDependencies.Add((JsonNode)dependency);

        var entryPath = "dist/" + package + "/" + file;
        var entryHash = HashFile(vendored[package + "/" + file]);
        imports[specifier] = new JsonObject
        {
            ["type"] = "module",
            ["development"] = entryPath,
            ["production"] = entryPath,
            ["developmentHash"] = entryHash,
            ["productionHash"] = entryHash,
            // 兄弟条目通过 package 通道解析；materializer 会用 import 索引递归闭包。
            ["developmentDependencies"] = (JsonArray)entryDependencies.DeepClone(),
            ["productionDependencies"] = entryDependencies,
            ["developmentModuleDependencies"] = (JsonArray)moduleDependencies.DeepClone(),
            ["productionModuleDependencies"] = moduleDependencies,
            ["files"] = files,
        };
    }

    var requiresNode = new JsonObject();
    foreach (var (name, range) in peerRequires)
        requiresNode[name] = range;

    return new JsonObject
    {
        ["schemaVersion"] = 2,
        ["libraryId"] = libraryId,
        ["version"] = version,
        ["imports"] = imports,
        ["requires"] = requiresNode,
        ["styles"] = new JsonArray(),
        ["files"] = licenseFiles,
    };
}

static void WriteInventory(
    string projectRoot,
    string libraryId,
    string version,
    string licenseId,
    string documentation,
    int vendoredCount,
    string[] specifiers,
    SortedDictionary<string, string> packageVersions)
{
    var packages = new JsonObject();
    foreach (var (package, packageVersion) in packageVersions)
        packages[package] = packageVersion;

    var inventory = new JsonObject
    {
        ["upstream"] = libraryId,
        ["version"] = version,
        ["license"] = licenseId,
        ["packages"] = packages,
        ["source"] = $"https://registry.npmjs.org/{libraryId}/{version}",
        ["documentation"] = documentation,
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
