using System.Formats.Tar;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

const string packageName = "tdesign-vue-next";
var externalTypes = new[]
{
    new ExternalTypePackage("@types/sortablejs", "1.15.1", "sortablejs"),
    new ExternalTypePackage("@types/validator", "13.7.17", "validator"),
    new ExternalTypePackage("dayjs", "1.11.10", "dayjs"),
    new ExternalTypePackage("tdesign-icons-vue-next", "0.4.7", "tdesign-icons-vue-next")
};
var options = GeneratorOptions.Parse(args);
var repoRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
var snapshotRoot = options.SnapshotDirectory is null
    ? Path.Combine(repoRoot, "src", "ECMAScript.TDesign", "upstream", packageName, options.Version)
    : ResolvePath(options.SnapshotDirectory, repoRoot);
var cachePath = Path.Combine(repoRoot, ".tmp", "packages", $"{packageName}-{options.Version}.tgz");

if (options.Check)
{
    VerifySnapshot(snapshotRoot, options.Version, externalTypes);
    Console.WriteLine($"TDesign declaration snapshot is current: {snapshotRoot}");
    return;
}

Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
if (options.Refresh || !File.Exists(cachePath))
    await DownloadPackageAsync(packageName, options.Version, cachePath);

var declarations = ReadDeclarations(cachePath);
if (declarations.Count == 0)
    throw new InvalidOperationException($"{cachePath} does not contain TypeScript declarations.");

foreach (var externalType in externalTypes)
{
    var externalCachePath = Path.Combine(
        repoRoot,
        ".tmp",
        "packages",
        $"{externalType.PackageName.Replace('@', '_').Replace('/', '-')}-{externalType.Version}.tgz");
    if (options.Refresh || !File.Exists(externalCachePath))
        await DownloadPackageAsync(externalType.PackageName, externalType.Version, externalCachePath);

    foreach (var (path, content) in ReadExternalDeclarations(externalCachePath, externalType.ModuleName))
        declarations.Add(path, content);
}

WriteSnapshot(snapshotRoot, options.Version, declarations, externalTypes);
Console.WriteLine($"Wrote {declarations.Count} TypeScript declaration files to {snapshotRoot}");

static SortedDictionary<string, byte[]> ReadDeclarations(string archivePath)
{
    var declarations = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);
    using var archive = File.OpenRead(archivePath);
    using var gzip = new GZipStream(archive, CompressionMode.Decompress);
    using var reader = new TarReader(gzip, leaveOpen: false);
    TarEntry? entry;
    while ((entry = reader.GetNextEntry()) is not null)
    {
        if (entry.EntryType is not TarEntryType.RegularFile || entry.DataStream is null)
            continue;

        const string prefix = "package/";
        if (!entry.Name.StartsWith(prefix, StringComparison.Ordinal))
            continue;

        var relativePath = entry.Name[prefix.Length..];
        if (!IsSnapshotFile(relativePath))
            continue;

        using var content = new MemoryStream();
        entry.DataStream.CopyTo(content);
        declarations.Add(relativePath, content.ToArray());
    }

    return declarations;
}

static SortedDictionary<string, byte[]> ReadExternalDeclarations(string archivePath, string moduleName)
{
    var declarations = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);
    using var archive = File.OpenRead(archivePath);
    using var gzip = new GZipStream(archive, CompressionMode.Decompress);
    using var reader = new TarReader(gzip, leaveOpen: false);
    TarEntry? entry;
    while ((entry = reader.GetNextEntry()) is not null)
    {
        if (entry.EntryType is not TarEntryType.RegularFile || entry.DataStream is null)
            continue;

        var rootSeparator = entry.Name.IndexOf('/');
        if (rootSeparator < 0)
            continue;

        // npm's scoped @types archives use their package folder as the tar root,
        // whereas normal package archives use package/. Strip either safely.
        var relativePath = entry.Name[(rootSeparator + 1)..];
        if (!relativePath.EndsWith(".d.ts", StringComparison.Ordinal))
            continue;

        using var content = new MemoryStream();
        entry.DataStream.CopyTo(content);
        declarations.Add($"external/{moduleName}/{relativePath}", content.ToArray());
    }

    return declarations;
}

static bool IsSnapshotFile(string relativePath)
{
    if (relativePath is "package.json" or "helper/web-types.json" or "helper/attributes.json" or "helper/tags.json")
        return true;

    // Declarations below component subdirectories participate in exported aliases
    // (for example tree/utils/adapt.d.ts). Freezing only index/type/types files leaves
    // a snapshot that cannot reproduce the package's public contract.
    return relativePath.StartsWith("es/", StringComparison.Ordinal) &&
           relativePath.EndsWith(".d.ts", StringComparison.Ordinal);
}

static async Task DownloadPackageAsync(string packageName, string version, string cachePath)
{
    var archiveName = packageName[(packageName.LastIndexOf('/') + 1)..];
    var packageUrl = $"https://registry.npmjs.org/{packageName}/-/{archiveName}-{version}.tgz";
    var temporaryPath = cachePath + ".download";
    File.Delete(temporaryPath);

    using var handler = new SocketsHttpHandler();
    using var client = new HttpClient(handler)
    {
        DefaultRequestVersion = HttpVersion.Version11,
        DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
    };
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Jazor-TDesign-Generator/1.0");

    for (var attempt = 1; attempt <= 3; attempt++)
    {
        try
        {
            Console.WriteLine($"Downloading {packageUrl} (attempt {attempt}/3)");
            using var response = await client.GetAsync(packageUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            await using var input = await response.Content.ReadAsStreamAsync();
            await using (var output = File.Create(temporaryPath))
                await input.CopyToAsync(output);
            File.Move(temporaryPath, cachePath, overwrite: true);
            return;
        }
        catch (HttpRequestException) when (attempt < 3)
        {
            File.Delete(temporaryPath);
            await Task.Delay(TimeSpan.FromSeconds(attempt));
        }
        catch (IOException) when (attempt < 3)
        {
            File.Delete(temporaryPath);
            await Task.Delay(TimeSpan.FromSeconds(attempt));
        }
    }

    throw new InvalidOperationException($"Unable to download {packageName}@{version} after three attempts.");
}

static void WriteSnapshot(
    string snapshotRoot,
    string version,
    IReadOnlyDictionary<string, byte[]> declarations,
    IReadOnlyList<ExternalTypePackage> externalTypes)
{
    if (Directory.Exists(snapshotRoot))
        Directory.Delete(snapshotRoot, recursive: true);

    foreach (var (relativePath, content) in declarations)
    {
        var destination = Path.GetFullPath(Path.Combine(snapshotRoot, relativePath));
        var root = Path.GetFullPath(snapshotRoot) + Path.DirectorySeparatorChar;
        if (!destination.StartsWith(root, StringComparison.Ordinal))
            throw new InvalidOperationException($"Invalid package declaration path: {relativePath}");

        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.WriteAllBytes(destination, content);
    }

    using (var stream = new MemoryStream())
    {
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        writer.WriteStartObject();
        writer.WriteString("package", "tdesign-vue-next");
        writer.WriteString("version", version);
        writer.WriteStartObject("externalTypes");
        foreach (var externalType in externalTypes.OrderBy(static item => item.PackageName, StringComparer.Ordinal))
            writer.WriteString(externalType.PackageName, externalType.Version);
        writer.WriteEndObject();
        writer.WriteEndObject();
        writer.Flush();
        File.WriteAllBytes(
            Path.Combine(snapshotRoot, "snapshot.json"),
            [.. stream.ToArray(), (byte)'\n']);
    }

    WriteComponentInventory(snapshotRoot, version, declarations);
}

static void WriteComponentInventory(
    string snapshotRoot,
    string version,
    IReadOnlyDictionary<string, byte[]> declarations)
{
    const string webTypesPath = "helper/web-types.json";
    if (!declarations.TryGetValue(webTypesPath, out var webTypes))
        throw new InvalidOperationException($"The TDesign package does not contain {webTypesPath}.");

    using var document = JsonDocument.Parse(webTypes);
    var runtimeExports = declarations
        .Where(static pair => pair.Key.StartsWith("es/", StringComparison.Ordinal) &&
                              pair.Key.EndsWith("/index.d.ts", StringComparison.Ordinal))
        .SelectMany(static pair => ReadRuntimeExports(Encoding.UTF8.GetString(pair.Value)))
        .ToHashSet(StringComparer.Ordinal);

    var components = document.RootElement
        .GetProperty("contributions")
        .GetProperty("html")
        .GetProperty("vue-components")
        .EnumerateArray()
        .Select(component => new
        {
            Element = component,
            Tag = component.GetProperty("name").GetString()!,
            SourceExport = component.TryGetProperty("source", out var source) &&
                           source.TryGetProperty("symbol", out var symbol)
                ? symbol.GetString()!
                : throw new InvalidOperationException("TDesign component metadata is missing its source export."),
        })
        .Select(component => new
        {
            component.Element,
            component.Tag,
            component.SourceExport,
            RuntimeExport = ResolveRuntimeExport(component.Tag, component.SourceExport)
        })
        .Where(component => runtimeExports.Contains(component.RuntimeExport))
        .OrderBy(static component => component.Tag, StringComparer.Ordinal)
        .ToArray();

    var inventoryPath = Path.Combine(snapshotRoot, "components.json");
    using var stream = File.Create(inventoryPath);
    using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
    writer.WriteStartObject();
    writer.WriteString("package", packageName);
    writer.WriteString("version", version);
    writer.WriteNumber("componentCount", components.Length);
    writer.WriteStartArray("components");
    foreach (var component in components)
    {
        writer.WriteStartObject();
        writer.WriteString("tag", component.Tag);
        writer.WriteString("sourceExport", component.SourceExport);
        writer.WriteString("export", component.RuntimeExport);
        WriteComponentMembers(writer, "props", component.Element, "props", includeType: true);
        WriteComponentMembers(writer, "slots", component.Element, "slots", includeType: false);
        if (component.Element.TryGetProperty("js", out var javascript) && javascript.TryGetProperty("events", out var events))
            WriteMemberArray(writer, "events", events, includeType: false);
        else
        {
            writer.WriteStartArray("events");
            writer.WriteEndArray();
        }
        writer.WriteEndObject();
    }

    writer.WriteEndArray();
    writer.WriteEndObject();
}

static IEnumerable<string> ReadRuntimeExports(string declaration)
{
    foreach (Match match in Regex.Matches(
        declaration,
        @"(?m)^export\s+declare\s+const\s+(?<name>[A-Za-z_$][A-Za-z0-9_$]*)",
        RegexOptions.CultureInvariant))
    {
        yield return match.Groups["name"].Value;
    }
}

static string ResolveRuntimeExport(string tag, string sourceExport)
    => tag switch
    {
        // The two documented icon variants are realized by the single public Icon export.
        "icon" or "t-icon" or "icon-font" => "Icon",
        // web-types currently points both tags at their sibling component. The ESM
        // declaration and bundled runtime expose these as distinct public exports.
        "t-radio-button" => "RadioButton",
        "t-swiper-item" => "SwiperItem",
        "t-table" => "Table",
        _ => sourceExport
    };

static void WriteComponentMembers(
    Utf8JsonWriter writer,
    string propertyName,
    JsonElement parent,
    string sourcePropertyName,
    bool includeType)
{
    if (parent.TryGetProperty(sourcePropertyName, out var members))
        WriteMemberArray(writer, propertyName, members, includeType);
    else
    {
        writer.WriteStartArray(propertyName);
        writer.WriteEndArray();
    }
}

static void WriteMemberArray(Utf8JsonWriter writer, string propertyName, JsonElement members, bool includeType)
{
    writer.WriteStartArray(propertyName);
    foreach (var member in members.EnumerateArray())
    {
        writer.WriteStartObject();
        writer.WriteString("name", member.GetProperty("name").GetString());
        if (includeType)
            writer.WriteString("type", member.TryGetProperty("type", out var type) ? type.GetString() : null);
        writer.WriteEndObject();
    }

    writer.WriteEndArray();
}

static void VerifySnapshot(
    string snapshotRoot,
    string version,
    IReadOnlyList<ExternalTypePackage> externalTypes)
{
    var metadataPath = Path.Combine(snapshotRoot, "snapshot.json");
    if (!File.Exists(metadataPath))
        throw new InvalidOperationException($"TDesign declaration snapshot is missing: {metadataPath}");

    var metadata = File.ReadAllText(metadataPath);
    if (!metadata.Contains($"\"version\": \"{version}\"", StringComparison.Ordinal) ||
        !File.Exists(Path.Combine(snapshotRoot, "es", "index.d.ts")) ||
        !File.Exists(Path.Combine(snapshotRoot, "components.json")) ||
        externalTypes.Any(externalType =>
            !metadata.Contains($"\"{externalType.PackageName}\": \"{externalType.Version}\"", StringComparison.Ordinal) ||
            !Directory.Exists(Path.Combine(snapshotRoot, "external", externalType.ModuleName))))
    {
        throw new InvalidOperationException(
            $"TDesign declaration snapshot does not match {packageName}@{version}. Run scripts/csharp/generate-tdesign.cs.");
    }
}

static string FindRepositoryRoot(string startDirectory)
{
    for (var directory = new DirectoryInfo(Path.GetFullPath(startDirectory)); directory is not null; directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
    }

    throw new InvalidOperationException("Unable to locate Jazor.slnx.");
}

static string ResolvePath(string path, string repoRoot)
    => Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));

file sealed record ExternalTypePackage(string PackageName, string Version, string ModuleName);

file sealed record GeneratorOptions(string Version, string? SnapshotDirectory, bool Check, bool Refresh)
{
    public static GeneratorOptions Parse(string[] args)
    {
        const string defaultVersion = "1.20.5";
        var version = defaultVersion;
        string? snapshotDirectory = null;
        var check = false;
        var refresh = false;

        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--version" when index + 1 < args.Length:
                    version = args[++index];
                    break;
                case "--snapshot" when index + 1 < args.Length:
                    snapshotDirectory = args[++index];
                    break;
                case "--check":
                    check = true;
                    break;
                case "--refresh":
                    refresh = true;
                    break;
                default:
                    throw new ArgumentException($"Unknown generator argument: {args[index]}");
            }
        }

        return new GeneratorOptions(version, snapshotDirectory, check, refresh);
    }
}
