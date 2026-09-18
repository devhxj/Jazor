#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

// Materialize the locked monaco-editor runtime into src/ECMAScript.Monaco and regenerate the
// package-local manifest, inventory, and vendored resource closure.
//
// monaco-editor's published ESM tree is NOT directly loadable by a browser: 120 of its modules
// import plain `.css` files, which the native ESM loader rejects. The upstream `min/` build is AMD.
// Following the repository's TDesign precedent, this generator bundles both the editor entry and the
// language workers with esbuild so each artifact is a self-contained ESM module; esbuild extracts the
// stylesheet alongside the editor bundle.
//
// 上游 ESM 树不能被浏览器直接加载（120 个模块 import 纯 .css）；min/ 是 AMD。沿用仓库 TDesign 先例，
// 用 esbuild 把编辑器入口与各 worker 打成自包含 ESM，并把样式单独抽出交付。
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

// 构建输入由 ESLint 无关的 package.json/lockfile 锁定；版本必须与请求一致。
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

// 逻辑 import → 上游 ESM 入口 → 产物名。编辑器入口与四个语言 worker。
var entries = new (string Specifier, string Source, string Artifact)[]
{
    ("monaco-editor", "esm/vs/editor/editor.api.js", "editor.api.mjs"),
    ("monaco-editor/esm/vs/editor/editor.worker.start.js", "esm/vs/editor/editor.worker.start.js", "editor.worker.start.mjs"),
    ("monaco-editor/esm/vs/languages/features/json/json.worker.js", "esm/vs/languages/features/json/json.worker.js", "json.worker.mjs"),
    ("monaco-editor/esm/vs/languages/features/css/css.worker.js", "esm/vs/languages/features/css/css.worker.js", "css.worker.mjs"),
    ("monaco-editor/esm/vs/languages/features/html/html.worker.js", "esm/vs/languages/features/html/html.worker.js", "html.worker.mjs"),
    ("monaco-editor/esm/vs/languages/features/typescript/ts.worker.js", "esm/vs/languages/features/typescript/ts.worker.js", "ts.worker.mjs"),
};

var staging = Path.Combine(root, ".tmp", "monaco-build", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(staging);

var artifacts = new SortedDictionary<string, string>(StringComparer.Ordinal); // artifact -> staged path
foreach (var (specifier, source, artifact) in entries)
{
    var sourcePath = Path.Combine(monacoRoot, source.Replace('/', Path.DirectorySeparatorChar));
    if (!File.Exists(sourcePath))
        throw new FileNotFoundException($"monaco-editor does not expose the expected entry '{source}'.", sourcePath);

    var outputPath = Path.Combine(staging, artifact);
    var (cssPath, code) = await BundleAsync(buildRoot, sourcePath, outputPath);
    if (code != 0)
        throw new InvalidOperationException($"esbuild failed for '{specifier}' with exit code {code}.");

    artifacts[artifact] = outputPath;
    AssertBrowserSafe(outputPath, specifier);

    // 只有编辑器入口会产生样式；worker 无样式。
    if (cssPath is not null)
    {
        if (!artifacts.TryAdd("editor.api.css", cssPath))
            throw new InvalidOperationException("Duplicate stylesheet artifact.");
    }
}

// 整体替换 dist 与 licenses，保证 manifest 与 vendored 文件始终一一对应。
var distRoot = Path.Combine(projectRoot, "dist", "monaco-editor");
var licensesRoot = Path.Combine(projectRoot, "licenses");
foreach (var directory in new[] { Path.Combine(projectRoot, "dist"), licensesRoot })
    if (Directory.Exists(directory))
        Directory.Delete(directory, recursive: true);
Directory.CreateDirectory(distRoot);
Directory.CreateDirectory(licensesRoot);

foreach (var (artifact, source) in artifacts)
    File.Copy(source, Path.Combine(distRoot, artifact), overwrite: false);

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

var manifest = BuildManifest(version, entries, artifacts, licenseFiles);
WriteLfText(manifestPath, manifest.ToJsonString(GeneratorJson.Manifest) + "\n");
WriteInventory(projectRoot, version, artifacts.Count);

Console.WriteLine($"Vendored {artifacts.Count} monaco-editor {version} artifact(s) (editor + stylesheet + {entries.Length - 1} workers).");
Console.WriteLine("Review contract drift for bound editor/model/language APIs before committing.");

static async Task<(string? CssPath, int ExitCode)> BundleAsync(string buildRoot, string sourcePath, string outputPath)
{
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    var esbuild = Path.Combine(buildRoot, "node_modules", "esbuild", "bin", "esbuild");
    var arguments = new List<string>
    {
        esbuild,
        sourcePath,
        "--bundle",
        "--format=esm",
        "--platform=browser",
        "--target=es2022",
        "--charset=utf8",
        "--outfile=" + outputPath,
    };
    var exitCode = await RunAsync("node", arguments, buildRoot, throwOnFailure: false);

    // esbuild 仅在入口引用样式时生成同名 .css。
    var cssPath = Path.ChangeExtension(outputPath, ".css");
    return (File.Exists(cssPath) ? cssPath : null, exitCode);
}

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

static void AssertBrowserSafe(string path, string specifier)
{
    // 浏览器安全闸门：vendor 的模块不得在裸浏览器环境引用 process.env。
    // Monaco 自带的 vs/base/common/process.js 有 typeof process 守卫，属正当用法；因此只在
    // 引用点没有同类守卫时失败。
    var source = File.ReadAllText(path);
    var index = source.IndexOf("process.env", StringComparison.Ordinal);
    while (index >= 0)
    {
        // 引用点前 400 字符串内出现 typeof process / typeof globalThis.process 视为已守卫。
        var window = source[Math.Max(0, index - 400)..index];
        var guarded = window.Contains("typeof process", StringComparison.Ordinal) ||
                      window.Contains("typeof globalThis.process", StringComparison.Ordinal) ||
                      window.Contains("typeof window", StringComparison.Ordinal);
        if (!guarded)
            throw new InvalidOperationException(
                $"Vendored artifact '{specifier}' references process.env without an environment guard; " +
                "it cannot load in a browser.");
        index = source.IndexOf("process.env", index + 1, StringComparison.Ordinal);
    }
}

static JsonNode BuildManifest(
    string version,
    (string Specifier, string Source, string Artifact)[] entries,
    SortedDictionary<string, string> artifacts,
    JsonArray licenseFiles)
{
    // 编辑器入口是唯一的 C# 作者入口；worker 以 manifest 模块依赖 + static 资源声明，
    // 让 Emit 把 worker 与样式一并物化，而不是留给应用手写 URL。
    var imports = new JsonObject();
    var workerArtifacts = entries
        .Where(static entry => entry.Artifact.EndsWith(".worker.mjs", StringComparison.Ordinal) ||
                               entry.Artifact == "editor.worker.start.mjs")
        .Select(static entry => entry.Artifact)
        .Order(StringComparer.Ordinal)
        .ToArray();

    var moduleDependencies = new JsonArray();
    var files = new JsonArray();
    foreach (var worker in workerArtifacts)
    {
        var distPath = "dist/monaco-editor/" + worker;
        moduleDependencies.Add((JsonNode)distPath);
        files.Add(new JsonObject
        {
            ["type"] = "module",
            ["path"] = distPath,
            ["hash"] = HashFile(artifacts[worker]),
            ["moduleId"] = distPath,
        });
    }

    var entryArtifact = "editor.api.mjs";
    imports["monaco-editor"] = new JsonObject
    {
        ["type"] = "module",
        ["development"] = "dist/monaco-editor/" + entryArtifact,
        ["production"] = "dist/monaco-editor/" + entryArtifact,
        ["developmentHash"] = HashFile(artifacts[entryArtifact]),
        ["productionHash"] = HashFile(artifacts[entryArtifact]),
        ["developmentDependencies"] = new JsonArray(),
        ["productionDependencies"] = new JsonArray(),
        ["developmentModuleDependencies"] = (JsonArray)moduleDependencies.DeepClone(),
        ["productionModuleDependencies"] = moduleDependencies,
        ["files"] = files,
    };

    var styles = new JsonArray
    {
        new JsonObject
        {
            ["type"] = "style",
            ["path"] = "dist/monaco-editor/editor.api.css",
            ["hash"] = HashFile(artifacts["editor.api.css"]),
        },
    };

    return new JsonObject
    {
        ["schemaVersion"] = 2,
        ["libraryId"] = "monaco",
        ["version"] = version,
        ["imports"] = imports,
        ["requires"] = new JsonObject(),
        ["styles"] = styles,
        ["files"] = licenseFiles,
    };
}

static void WriteInventory(string projectRoot, string version, int vendoredCount)
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
        ["vendoredModuleCount"] = vendoredCount,
        ["buildNote"] =
            "Bundled with esbuild from the upstream ESM tree: 120 of its modules import plain .css files, " +
            "which a browser ESM loader rejects, and the upstream min/ build is AMD.",
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
