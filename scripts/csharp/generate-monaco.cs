#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

// Validate the locked monaco-editor package and regenerate package metadata and the upstream
// inventory. Runtime modules stay in the npm package so NetPack can resolve its exports and
// sideEffects metadata at the actual usage site.
//
// Usage:
//   dotnet run --file scripts/csharp/generate-monaco.cs -- --version 0.56.0
//   dotnet run --file scripts/csharp/generate-monaco.cs -- --version 0.56.0 --skip-install
var options = GeneratorOptions.Parse(args);
var root = Directory.GetCurrentDirectory();
if (!File.Exists(Path.Combine(root, "Jazor.slnx")))
    throw new InvalidOperationException("Run from the repository root.");

var projectRoot = Path.Combine(root, "src", "ECMAScript.Monaco");
var manifestPath = Path.Combine(projectRoot, "manifest.json");
var buildRoot = Path.Combine(root, "src", "ECMAScript.Vue.Generator", "monaco-runtime");

var version = options.Version;
if (version is null && File.Exists(manifestPath))
{
    version = JsonNode.Parse(File.ReadAllText(manifestPath))?["version"]?.GetValue<string>();
    if (version is not null)
        Console.WriteLine($"Using manifest version {version}.");
}

if (string.IsNullOrWhiteSpace(version))
    throw new ArgumentException("Provide --version on the first run; later runs reuse the manifest version.");

// The validation input is locked by package.json/package-lock.json; the requested version must
// match it before the package is inspected.
var buildPackage = JsonNode.Parse(File.ReadAllText(Path.Combine(buildRoot, "package.json")))!;
var lockedMonaco = buildPackage["dependencies"]!["monaco-editor"]!.GetValue<string>();
if (lockedMonaco != version)
    throw new InvalidOperationException(
        $"monaco-runtime/package.json pins monaco-editor {lockedMonaco}, but {version} was requested. " +
        "Update the build inputs (package.json + package-lock.json) first.");

if (!options.SkipInstall)
{
    await RunAsync("cmd.exe", ["/d", "/c", "npm.cmd", "ci", "--ignore-scripts", "--no-audit", "--no-fund"], buildRoot);
}

var monacoRoot = Path.Combine(buildRoot, "node_modules", "monaco-editor");
if (!Directory.Exists(monacoRoot))
    throw new DirectoryNotFoundException($"monaco-editor was not installed under {buildRoot}.");

var installedVersion = JsonNode.Parse(File.ReadAllText(Path.Combine(monacoRoot, "package.json")))!["version"]!.GetValue<string>();
if (installedVersion != version)
    throw new InvalidOperationException($"Installed monaco-editor version '{installedVersion}' does not match '{version}'.");

// Logical binding names map to package export subpaths. The package's `./*.js` export maps these
// paths to `esm/vs/*`; keeping the authored target preserves upstream module boundaries.
var entries = new (string Specifier, string Target, string Source)[]
{
    ("monaco-editor", "monaco-editor/editor/editor.api.js", "esm/vs/editor/editor.api.js"),
    ("monaco-editor/editor/editor.worker.start.js", "monaco-editor/editor/editor.worker.start.js", "esm/vs/editor/editor.worker.start.js"),
    ("monaco-editor/languages/features/json/json.worker.js", "monaco-editor/languages/features/json/json.worker.js", "esm/vs/languages/features/json/json.worker.js"),
    ("monaco-editor/languages/features/css/css.worker.js", "monaco-editor/languages/features/css/css.worker.js", "esm/vs/languages/features/css/css.worker.js"),
    ("monaco-editor/languages/features/html/html.worker.js", "monaco-editor/languages/features/html/html.worker.js", "esm/vs/languages/features/html/html.worker.js"),
    ("monaco-editor/languages/features/typescript/ts.worker.js", "monaco-editor/languages/features/typescript/ts.worker.js", "esm/vs/languages/features/typescript/ts.worker.js"),
};

foreach (var (specifier, _, source) in entries)
{
    var sourcePath = Path.Combine(monacoRoot, source.Replace('/', Path.DirectorySeparatorChar));
    if (!File.Exists(sourcePath))
        throw new FileNotFoundException($"monaco-editor does not expose the expected entry '{source}'.", sourcePath);

    ValidateEntry(sourcePath, specifier);
}

// Remove the historical binding-owned carrier when regeneration runs. The binding now emits
// metadata only; the restored npm package is the runtime source.
var distRoot = Path.Combine(projectRoot, "dist");
var licensesRoot = Path.Combine(projectRoot, "licenses");
if (Directory.Exists(distRoot))
    Directory.Delete(distRoot, recursive: true);
if (Directory.Exists(licensesRoot))
    Directory.Delete(licensesRoot, recursive: true);
Directory.CreateDirectory(licensesRoot);

// 许可证与第三方声明必须随包交付。
var licenseFiles = new JsonArray();
foreach (var (source, name) in new[]
         {
             (Path.Combine(monacoRoot, "LICENSE"), "MONACO-LICENSE"),
             (Path.Combine(monacoRoot, "ThirdPartyNotices.txt"), "THIRD-PARTY-NOTICES.txt"),
         })
{
    if (!File.Exists(source))
        throw new FileNotFoundException($"monaco-editor does not ship '{Path.GetFileName(source)}'.", source);
    File.Copy(source, Path.Combine(licensesRoot, name), overwrite: true);
    licenseFiles.Add(new JsonObject
    {
        ["type"] = "license",
        ["path"] = "licenses/" + name,
        ["hash"] = HashFile(Path.Combine(licensesRoot, name)),
    });
}

var manifest = BuildManifest(version, entries, licenseFiles);
WriteLfText(manifestPath, manifest.ToJsonString(GeneratorJson.Manifest) + "\n");
WriteInventory(projectRoot, version, entries.Length);

Console.WriteLine($"Validated {entries.Length} monaco-editor {version} export entries.");
Console.WriteLine("Review contract drift for bound editor/model/language APIs before committing.");

static async Task<int> RunAsync(string fileName, IEnumerable<string> arguments, string workingDirectory, bool throwOnFailure = true)
{
    var startInfo = new ProcessStartInfo(fileName)
    {
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };
    foreach (var argument in arguments)
        startInfo.ArgumentList.Add(argument);

    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException($"Failed to start '{fileName}'.");
    // 并发读取两个管道，避免先读完 stdout 时子进程写满 stderr 造成死锁。
    var stdoutTask = process.StandardOutput.ReadToEndAsync();
    var stderrTask = process.StandardError.ReadToEndAsync();
    await Task.WhenAll(stdoutTask, stderrTask);
    await process.WaitForExitAsync();

    if (process.ExitCode != 0 && throwOnFailure)
        throw new InvalidOperationException($"{fileName} failed ({process.ExitCode}).\n{stdoutTask.Result}\n{stderrTask.Result}");

    return process.ExitCode;
}

static void ValidateEntry(string path, string specifier)
{
    // Validate the authored entry without rewriting it. Runtime import edges and CSS are resolved
    // by the package-aware bundler from the upstream package graph.
    var source = File.ReadAllText(path);
    if (source.Contains("require(", StringComparison.Ordinal))
        throw new InvalidOperationException($"monaco-editor entry '{specifier}' contains CommonJS require().");
}

static JsonNode BuildManifest(
    string version,
    (string Specifier, string Target, string Source)[] entries,
    JsonArray licenseFiles)
{
    var imports = new JsonObject();
    foreach (var (specifier, target, _) in entries.OrderBy(static entry => entry.Specifier, StringComparer.Ordinal))
    {
        imports[specifier] = new JsonObject
        {
            ["type"] = "module",
            ["development"] = target,
            ["production"] = target,
            ["developmentDependencies"] = new JsonArray(),
            ["productionDependencies"] = new JsonArray(),
        };
    }

    return new JsonObject
    {
        ["schemaVersion"] = 2,
        ["libraryId"] = "monaco",
        ["version"] = version,
        ["source"] = "npm",
        ["packages"] = new JsonObject
        {
            ["monaco-editor"] = new JsonObject { ["source"] = "npm", ["version"] = version }
        },
        ["imports"] = imports,
        ["requires"] = new JsonObject(),
        ["styles"] = new JsonArray(),
        ["files"] = licenseFiles,
    };
}

static void WriteInventory(string projectRoot, string version, int validatedEntryCount)
{
    var inventory = new JsonObject
    {
        ["upstream"] = "monaco-editor",
        ["version"] = version,
        ["license"] = "MIT",
        ["packages"] = new JsonObject { ["monaco-editor"] = version },
        ["source"] = $"https://registry.npmjs.org/monaco-editor/{version}",
        ["documentation"] = "https://microsoft.github.io/monaco-editor/",
        ["entryImports"] = new JsonArray { "monaco-editor" },
        ["validatedEntryCount"] = validatedEntryCount,
        ["runtimeNote"] = "NetPack resolves the upstream ESM exports, CSS edges, and worker modules from node_modules.",
    };
    var payload = inventory.ToJsonString(GeneratorJson.Manifest);
    inventory["fingerprint"] = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    WriteLfText(Path.Combine(projectRoot, "inventory.json"), inventory.ToJsonString(GeneratorJson.Manifest) + "\n");
}

static string HashFile(string path)
    => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

// 生成的文本必须纯 LF；manifest 哈希逐字节校验，CRLF 在 fresh clone 后会校验失败。
static void WriteLfText(string path, string content)
    => File.WriteAllText(path, content.Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));

internal sealed class GeneratorOptions
{
    public string? Version { get; private set; }

    public bool SkipInstall { get; private set; }

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
                case "--skip-install":
                    options.SkipInstall = true;
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
