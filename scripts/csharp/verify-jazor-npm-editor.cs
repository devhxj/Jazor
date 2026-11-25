#!/usr/bin/env dotnet run
#:property PublishAot=false

using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

var repairManifest = args.Contains("--repair-manifest", StringComparer.Ordinal);
var firstManifest = repairManifest || args.Contains("--first-manifest", StringComparer.Ordinal);
var nestedManifest = args.Contains("--nested-manifest", StringComparer.Ordinal);
var pnpmWorkspace = args.Contains("--pnpm-workspace", StringComparer.Ordinal);
var sourceWorkspace = args.Contains("--source-workspace", StringComparer.Ordinal);
var sharedCache = args.Contains("--shared-cache", StringComparer.Ordinal);
string? editorRuntime = null;
var runtimeIndex = Array.IndexOf(args, "--editor-runtime");
if (runtimeIndex >= 0)
{
    if (runtimeIndex + 1 >= args.Length || !Directory.Exists(args[runtimeIndex + 1]))
        throw new ArgumentException("--editor-runtime requires the language server's .NET installation directory.");
    editorRuntime = Path.GetFullPath(args[runtimeIndex + 1]);
    args = [.. args[..runtimeIndex], .. args[(runtimeIndex + 2)..]];
}
args = args.Where(argument => argument is not ("--first-manifest" or "--nested-manifest" or "--repair-manifest" or
    "--pnpm-workspace" or "--source-workspace" or "--shared-cache")).ToArray();
var serverMode = args.LastOrDefault() is "Automatic" or "Balanced" ? args[^1] : "Automatic";
if (args.Length != 0 && args[^1] == serverMode)
    args = args[..^1];
if (args.Length is < 1 or > 2 || nestedManifest && (firstManifest || serverMode != "Automatic") ||
    pnpmWorkspace && (firstManifest || nestedManifest || serverMode != "Automatic") ||
    sourceWorkspace && (firstManifest || nestedManifest || pnpmWorkspace || serverMode != "Automatic") ||
    sharedCache && (firstManifest || nestedManifest || pnpmWorkspace || sourceWorkspace || serverMode != "Automatic"))
    throw new ArgumentException("Usage: verify-jazor-npm-editor.cs <Jazor.nupkg> [Microsoft.CodeAnalysis.LanguageServer apphost] [Automatic|Balanced] [--first-manifest|--repair-manifest|--nested-manifest|--pnpm-workspace|--source-workspace|--shared-cache (Automatic)] [--editor-runtime directory]");

var package = Path.GetFullPath(args[0]);
var server = args.Length == 2 ? Path.GetFullPath(args[1]) : FindInstalledServer();
var repo = Path.GetFullPath(Path.Combine(ScriptDirectory(), "..", ".."));
var store = Path.GetFullPath(Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
    ?? Path.Combine(repo, ".tmp", "nodeapi-store"));
var scratch = Directory.CreateDirectory(Path.Combine(repo, ".tmp", $"jazor-npm-editor-{Guid.NewGuid():N}")).FullName;
var workspace = Directory.CreateDirectory(Path.Combine(scratch, "workspace")).FullName;
var sourceDirectory = sourceWorkspace
    ? Directory.CreateDirectory(Path.Combine(workspace, "Frontend", "Features")).FullName
    : workspace;
var source = Path.Combine(sourceDirectory, "Calendar.cs");
const string disk = "public static class Calendar { }\n";

using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
var stop = timeout.Token;
try
{
    var feed = Directory.CreateDirectory(Path.Combine(scratch, "feed")).FullName;
    var version = ReadJazorVersion(package);
    File.Copy(package, Path.Combine(feed, $"Jazor.{version}.nupkg"));
    new XElement("configuration",
        new XElement("packageSources", new XElement("clear"),
            new XElement("add", new XAttribute("key", "candidate"), new XAttribute("value", feed)),
            new XElement("add", new XAttribute("key", "nuget.org"), new XAttribute("value", "https://api.nuget.org/v3/index.json"))),
        new XElement("packageSourceMapping",
            new XElement("packageSource", new XAttribute("key", "candidate"), new XElement("package", new XAttribute("pattern", "Jazor"))),
            new XElement("packageSource", new XAttribute("key", "nuget.org"), new XElement("package", new XAttribute("pattern", "*")))))
        .Save(Path.Combine(workspace, "NuGet.Config"));
    new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"),
        new XElement("PropertyGroup", new XElement("TargetFramework", "net11.0"), new XElement("LangVersion", "preview"),
            new XElement("JazorPnpmStoreDirectory", store)),
        new XElement("ItemGroup", new XElement("PackageReference", new XAttribute("Include", "Jazor"),
            new XAttribute("Version", "[" + version + "]"))))
        .Save(Path.Combine(workspace, "Calendar.csproj"));
    if (pnpmWorkspace)
    {
        await File.WriteAllTextAsync(Path.Combine(scratch, "package.json"), """{"name":"editor-fixture","private":true}""", stop);
        await File.WriteAllTextAsync(Path.Combine(scratch, "pnpm-workspace.yaml"), "packages:\n  - workspace\n  - hot-probe\n", stop);
        await File.WriteAllTextAsync(Path.Combine(scratch, "pnpm-lock.yaml"), """
            lockfileVersion: '9.0'
            settings:
              autoInstallPeers: true
              excludeLinksFromLockfile: false
            importers:
              .: {}
              hot-probe: {}
              workspace:
                dependencies:
                  hot-probe:
                    specifier: workspace:*
                    version: link:../hot-probe
            """, stop);
        await File.WriteAllTextAsync(Path.Combine(workspace, "package.json"),
            """{"name":"calendar","private":true,"dependencies":{"hot-probe":"workspace:*"}}""", stop);
    }
    else if (nestedManifest)
        await WriteEditorDependenciesAsync(scratch, includeDateFns: false, stop, hotProbe: "link:./hot-probe");
    else if (sharedCache)
        await WriteEditorDependenciesAsync(workspace, includeDateFns: true, stop, hotProbe: null);
    else if (sourceWorkspace)
    {
        await WriteEditorDependenciesAsync(Path.Combine(workspace, "Frontend"), includeDateFns: false, stop,
            hotProbe: "link:../../hot-probe");
        var sibling = Directory.CreateDirectory(Path.Combine(workspace, "Backoffice")).FullName;
        await File.WriteAllTextAsync(Path.Combine(sibling, "package.json"), """{"dependencies":{"wrong-package":"1.0.0"}}""", stop);
        await File.WriteAllTextAsync(Path.Combine(sibling, "Other.cs"), "public static class Other { }", stop);
    }
    else if (!firstManifest)
        await WriteEditorDependenciesAsync(workspace, includeDateFns: false, stop);
    else if (repairManifest)
        await File.WriteAllTextAsync(Path.Combine(workspace, "package.json"), "{", stop);
    var linked = Directory.CreateDirectory(Path.Combine(scratch, "hot-probe")).FullName;
    await File.WriteAllTextAsync(Path.Combine(linked, "package.json"),
        """{"name":"hot-probe","version":"1.0.0","type":"module","exports":{".":{"types":"./index.d.ts","import":"./index.mjs"}}}""", stop);
    const string reexport = "export { parseFlag as inspect, unprojectable } from './implementation.js';\n";
    await File.WriteAllTextAsync(Path.Combine(linked, "index.d.ts"), reexport, stop);
    await File.WriteAllTextAsync(Path.Combine(linked, "index.mjs"), reexport, stop);
    await File.WriteAllTextAsync(Path.Combine(linked, "implementation.js"), """
        export function parseFlag(value) { return value; }
        export function unprojectable() { return Symbol(); }
        """, stop);
    var declaration = Path.Combine(linked, "implementation.d.ts");
    await File.WriteAllTextAsync(declaration, """
        /** Parses a flag supplied as text. */
        export declare function parseFlag(value: string): string;
        export declare function unprojectable(): symbol;

        """, stop);
    await File.WriteAllTextAsync(Path.Combine(sourceDirectory, "NpmImports.cs"), "", stop);
    await File.WriteAllTextAsync(source, disk, stop);
    await RunDotnetAsync(workspace, stop, "restore", "Calendar.csproj", "--nologo");
    using (var restored = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(workspace, "obj", "project.assets.json"), stop)))
    using (var packaged = File.OpenRead(package))
    {
        var integrity = restored.RootElement.GetProperty("libraries").GetProperty("Jazor/" + version).GetProperty("sha512").GetString();
        if (Convert.ToBase64String(await SHA512.HashDataAsync(packaged, stop)) != integrity)
            throw new InvalidOperationException("NuGet restored a different Jazor package with the same version.");
    }
    if (pnpmWorkspace)
    {
        // Establish the installed workspace link before the IDE requests its design-time inputs.
        await File.WriteAllTextAsync(source,
            "using Hot = Npm.HotProbe.Exports; public static class Calendar { public static string Test() => Hot.Inspect(\"true\"); }", stop);
        await RunDotnetAsync(workspace, stop, "build", "Calendar.csproj", "--no-restore", "--nologo");
        if (!File.Exists(Path.Combine(scratch, "pnpm-lock.yaml")) || File.Exists(Path.Combine(workspace, "pnpm-lock.yaml")))
            throw new InvalidOperationException("Expected pnpm to use the shared workspace lockfile.");
        await File.WriteAllTextAsync(source, disk, stop);
    }
    if (sharedCache)
        await VerifySharedCacheAsync();
    else
    {
        await VerifyEditorAsync(server, serverMode, scratch, workspace, source, disk, declaration, firstManifest, nestedManifest, repairManifest,
            pnpmWorkspace, sourceWorkspace, editorRuntime, stop);
        if (!firstManifest && !nestedManifest && !pnpmWorkspace && !sourceWorkspace)
            await VerifyColdGenerationAsync(scratch, workspace, stop);
        Console.WriteLine(pnpmWorkspace
            ? "C# Roslyn LSP (Automatic) verified workspace:* declaration diagnostics and repair of the shared lockfile."
            : nestedManifest
            ? "C# Roslyn LSP (Automatic) verified shadowing and restoration of the parent npm workspace with the C# buffer unsaved."
            : sourceWorkspace
            ? "C# Roslyn LSP (Automatic) verified cold completion, global using, and linked declaration refresh in a source-owned npm workspace."
            : repairManifest
            ? $"C# Roslyn LSP ({serverMode}) verified unsaved completion and signature help after repairing the project npm manifest."
            : firstManifest
            ? $"C# Roslyn LSP ({serverMode}) verified the first npm manifest, unsaved completion, and typed signature help."
            : serverMode == "Balanced"
            ? "C# Roslyn LSP (Balanced) verified cold imports, export completion for a newly added dependency, and typed calls after saving."
            : "C# Roslyn LSP (Automatic) verified unsaved imports after adding a dependency, typed calls and navigation to updated linked declarations.");
    }
}
catch
{
    Console.Error.WriteLine($"Editor fixture and LSP log preserved at {scratch}");
    throw;
}
Directory.Delete(scratch, recursive: true);

async Task VerifySharedCacheAsync()
{
    var second = Directory.CreateDirectory(Path.Combine(scratch, "second-workspace")).FullName;
    foreach (var file in new[] { "NuGet.Config", "Calendar.csproj", "Calendar.cs", "package.json", "pnpm-lock.yaml" })
        File.Copy(Path.Combine(workspace, file), Path.Combine(second, file));
    await RunDotnetAsync(second, stop, "restore", "Calendar.csproj", "--nologo");
    // Warm only the install inputs. No C# npm calls or TypeScript checker run before the first completion.
    await RunDotnetAsync(repo, stop, "run", "--file", Path.Combine(repo, "scripts", "csharp", "restore-node-project.cs"),
        "--", workspace, second);
    var shared = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jazor", "cache", "npm-metadata", "v1");
    var firstTime = await MeasureAsync(workspace, "first-project");
    using var manifest = File.OpenRead(Path.Combine(workspace, "node_modules", "date-fns", "package.json"));
    var packageFingerprint = Convert.ToHexString(SHA256.HashData(manifest));
    var snapshots = new Dictionary<string, DateTime>();
    var hasProjection = false;
    foreach (var file in CacheFiles(shared))
    {
        stop.ThrowIfCancellationRequested();
        try
        {
            using var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var json = JsonDocument.Parse(stream);
            if (!json.RootElement.TryGetProperty("inputs", out var inputs) || inputs.ValueKind != JsonValueKind.Object ||
                !inputs.TryGetProperty("\0project/node_modules/date-fns/package.json", out var packageInput) ||
                packageInput.GetString() != packageFingerprint ||
                !json.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object ||
                !data.TryGetProperty("modulePath", out var module) || module.GetString() != "\0project/node_modules/date-fns/index.js" ||
                !data.TryGetProperty("exports", out var exports) || exports.ValueKind != JsonValueKind.Array ||
                !exports.EnumerateArray().Any(value => value.GetString() == "addDays"))
                continue;
            var projected = data.TryGetProperty("values", out var values) && values.ValueKind == JsonValueKind.Array &&
                values.EnumerateArray().Any(value => value.TryGetProperty("exportName", out var name) && name.GetString() == "addDays" &&
                    value.TryGetProperty("version", out var version) && version.GetString() == "4.4.0");
            if (values.ValueKind == JsonValueKind.Array && !projected)
                continue;
            snapshots.Add(file, File.GetLastWriteTimeUtc(file));
            hasProjection |= projected;
        }
        catch (IOException) { }
        catch (JsonException) { }
    }
    if (!hasProjection)
        throw new InvalidOperationException("The first project did not leave a portable date-fns 4.4.0 snapshot containing the AddDays projection.");

    var secondTime = await MeasureAsync(second, "second-project");
    if (CacheFiles(Path.Combine(second, ".jazor", "cache", "npm-metadata")).Any() ||
        snapshots.Any(file => !File.Exists(file.Key) || File.GetLastWriteTimeUtc(file.Key) != file.Value))
        throw new InvalidOperationException("The second project wrote project-local metadata or changed the shared date-fns snapshots.");
    Console.WriteLine($"Time until the first Roslyn member completion offering AddDays was {firstTime.TotalMilliseconds:F0} ms in the first project and {secondTime.TotalMilliseconds:F0} ms in the second project. Both package installations were ready before timing.");
    Console.WriteLine($"Both registry declaration links resolved inside each project's own node_modules. {snapshots.Count} matching shared metadata snapshots remained unchanged.");

    Task<TimeSpan> MeasureAsync(string project, string name) => VerifyEditorAsync(server, "Automatic",
        Directory.CreateDirectory(Path.Combine(scratch, name)).FullName, project, Path.Combine(project, "Calendar.cs"), disk, "",
        firstManifest: false, nestedManifest: false, repairManifest: false, pnpmWorkspace: false, sourceWorkspace: false,
        editorRuntime, stop, measureRegistryCompletion: true);

    static IEnumerable<string> CacheFiles(string path) => Directory.Exists(path)
        ? Directory.EnumerateFiles(path, "*.json", SearchOption.AllDirectories)
        : [];
}

static async Task WriteEditorDependenciesAsync(string workspace, bool includeDateFns, CancellationToken stop, string? hotProbe = "link:../hot-probe")
{
    const string initialLock = """
        lockfileVersion: '9.0'
        settings:
          autoInstallPeers: true
          excludeLinksFromLockfile: false
        importers:
          .:
            dependencies:
              hot-probe:
                specifier: link:../hot-probe
                version: link:../hot-probe
        """;
    const string dateLock = """
              date-fns:
                specifier: 4.4.0
                version: 4.4.0
        packages:
          date-fns@4.4.0:
            resolution: {integrity: sha512-+1UMbeh68lH1SegH83CGWwpb6OHHbpSgr3+s5Eww5M4CAgswBpoWS0AjTOfEJ33HiYKz1hdj/KTFprzXHmq/6w==}
        snapshots:
          date-fns@4.4.0: {}
        """;
    var dependencies = new Dictionary<string, string>();
    var lockText = initialLock[..initialLock.IndexOf("      hot-probe:", StringComparison.Ordinal)];
    if (hotProbe is not null)
    {
        dependencies.Add("hot-probe", hotProbe);
        lockText = initialLock.Replace("link:../hot-probe", hotProbe, StringComparison.Ordinal);
    }
    if (includeDateFns)
    {
        dependencies.Add("date-fns", "4.4.0");
        lockText = lockText.TrimEnd() + "\n" + dateLock;
    }
    await File.WriteAllTextAsync(Path.Combine(workspace, "pnpm-lock.yaml"), lockText, stop);
    await File.WriteAllTextAsync(Path.Combine(workspace, "package.json"),
        JsonSerializer.Serialize(new { @private = true, type = "module", dependencies }), stop);
}

static async Task VerifyColdGenerationAsync(string scratch, string workspace, CancellationToken stop)
{
    var manifest = Path.Combine(workspace, "package.json");
    var package = System.Text.Json.Nodes.JsonNode.Parse(await File.ReadAllTextAsync(manifest, stop))!;
    foreach (var dependency in new[] { "cold-clash", "cold_clash", "cold-facade", "subpath-only" })
        package["dependencies"]![dependency] = "1.0.0";
    await File.WriteAllTextAsync(manifest, package.ToJsonString(), stop);
    var subpath = Directory.CreateDirectory(Path.Combine(workspace, "node_modules", "subpath-only")).FullName;
    await File.WriteAllTextAsync(Path.Combine(subpath, "package.json"),
        """{"name":"subpath-only","version":"1.0.0","exports":{"./feature":"./feature.mjs"}}""", stop);
    await File.WriteAllTextAsync(Path.Combine(subpath, "feature.mjs"), "export const value = 1;\n", stop);
    var source = Path.Combine(workspace, "ColdImports.cs");
    const string facade = "namespace Npm.ColdFacade { public static class Exports { public const int Marker = 42; } }";
    const string consumer = """
        using Dates = Npm.DateFns.Exports;
        using Facade = Npm.ColdFacade.Exports;
        namespace Consumer { internal class ColdImports { internal const int Marker = Facade.Marker; } }
        """;
    await File.WriteAllTextAsync(source, facade, stop);
    var output = Directory.CreateDirectory(Path.Combine(scratch, "g")).FullName;
    string[] build = ["build", "Calendar.csproj", "--no-restore", "--nologo",
        "-t:Rebuild", "-p:DesignTimeBuild=true", "-p:UseSharedCompilation=false",
        "-p:EmitCompilerGeneratedFiles=true", "-p:CompilerGeneratedFilesOutputPath=" + output];
    await RunDotnetAsync(workspace, stop, build);
    var snapshot = Directory.Exists(output)
        ? Directory.EnumerateFiles(output, "Jazor.Npm.Editor.g.cs", SearchOption.AllDirectories).SingleOrDefault() : null;
    if (snapshot is null)
        throw new InvalidOperationException("Design-time generation did not create package skeletons for the cold C# project.");
    // The generator escapes root identifiers to allow contextual C# keywords.
    var content = (await File.ReadAllTextAsync(snapshot, stop)).Replace("@", "", StringComparison.Ordinal);
    if (!content.Contains("namespace Npm.DateFns", StringComparison.Ordinal) ||
        !content.Contains("namespace Npm.HotProbe", StringComparison.Ordinal) ||
        !content.Contains("class Exports", StringComparison.Ordinal) ||
        content.Contains("namespace Npm.ColdClash", StringComparison.Ordinal) ||
        content.Contains("namespace Npm.ColdFacade", StringComparison.Ordinal) ||
        content.Contains("namespace Npm.SubpathOnly", StringComparison.Ordinal) ||
        content.Contains("AddDays", StringComparison.Ordinal) || content.Contains(".d.ts", StringComparison.Ordinal))
        throw new InvalidOperationException("The cold design-time C# project must contain only valid package roots without member projections.");

    await File.WriteAllTextAsync(source, consumer + "\n" + facade, stop);
    // Match a C# project with ordinary feature folders alongside its npm workspace.
    var unrelated = Directory.CreateDirectory(Path.Combine(workspace, "Unrelated")).FullName;
    await File.WriteAllTextAsync(Path.Combine(unrelated, "Other.cs"), "namespace Unrelated; internal class Other { }", stop);
    await RunDotnetAsync(workspace, stop, build);
    var editorInputs = Directory.EnumerateFiles(Path.Combine(workspace, "obj"), "Calendar.npm.editor.inputs", SearchOption.AllDirectories).Single();
    if ((await File.ReadAllLinesAsync(editorInputs, stop)).Any(path => Path.GetDirectoryName(path) == unrelated))
        throw new InvalidOperationException("A C# directory with no npm workspace added phantom npm inputs.");

    await File.WriteAllTextAsync(source, """
        using Clash = Npm.ColdClash.Exports;
        using Subpath = Npm.SubpathOnly.Exports;
        """ + "\n" + consumer + "\n" + facade, stop);
    var errorLog = Path.Combine(scratch, "cold-diagnostics.sarif");
    await RunDotnetCoreAsync(workspace, stop, expectedExitCode: 1, [.. build, "-p:ErrorLog=" + errorLog]);
    using var errors = JsonDocument.Parse(await File.ReadAllTextAsync(errorLog, stop));
    var diagnostics = errors.RootElement.GetProperty("runs")[0].GetProperty("results").EnumerateArray()
        .Where(diagnostic => diagnostic.GetProperty("level").GetString() == "error").ToArray();
    if (diagnostics.Length != 2 || !new[] { "ColdClash", "SubpathOnly" }.All(name => diagnostics.Any(diagnostic =>
        diagnostic.GetProperty("ruleId").GetString() is "CS0234" or "CS0246" &&
        diagnostic.GetProperty("message").GetString()!.Contains(name, StringComparison.Ordinal))))
        throw new InvalidOperationException("Cold design-time compilation must reject the ambiguous and subpath-only root imports.");
}

static async Task<TimeSpan> VerifyEditorAsync(string server, string mode, string scratch, string workspace, string source, string disk,
    string declaration, bool firstManifest, bool nestedManifest, bool repairManifest, bool pnpmWorkspace, bool sourceWorkspace,
    string? editorRuntime, CancellationToken stop, bool measureRegistryCompletion = false)
{
    var app = new ProcessStartInfo(server)
    {
        WorkingDirectory = workspace,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
    };
    if (editorRuntime is not null)
    {
        // Reproduce an IDE that ships its own runtime, while the project selects the SDK for Jazor.
        app.Environment["DOTNET_ROOT"] = editorRuntime;
        var architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
        app.Environment["DOTNET_ROOT_" + architecture.ToString().ToUpperInvariant()] = editorRuntime;
    }
    var designTimeTargets = Path.Combine(Path.GetDirectoryName(server)!, "Targets", "Microsoft.CSharpExtension.DesignTime.targets");
    foreach (var argument in new[] { "--stdio", "--autoLoadProjects", "--logLevel", "Information", "--telemetryLevel", "off",
                 "--sourceGeneratorExecutionPreference", mode,
                 "--csharpDesignTimePath", designTimeTargets, "--clientProcessId", Environment.ProcessId.ToString() })
        app.ArgumentList.Add(argument);

    using var process = Process.Start(app)!;
    var stderr = process.StandardError.ReadToEndAsync();
    using var transcript = new StreamWriter(Path.Combine(scratch, "lsp.jsonl")) { AutoFlush = true };
    using var input = new BufferedStream(process.StandardOutput.BaseStream);
    var lsp = new LspClient(process.StandardInput.BaseStream, input, transcript, stop);
    var root = new Uri(workspace + Path.DirectorySeparatorChar).AbsoluteUri;
    var uri = new Uri(source).AbsoluteUri;
    var document = new { uri };
    var version = 1;
    var savedSource = disk;
    const string import = "using Dates = Npm.DateFns.Exports;";
    const string module = import + "\n[ECMAScript.ECMAScriptModule(\"calendar.mjs\")]\npublic static class Calendar";
    const string firstCompletion = import + "\nstatic class Calendar { static object Test() => Dates.$$; }";
    const string packageList = "using Dates = Npm.$$;\nstatic class Calendar { }";
    const string exportType = "using Dates = Npm.DateFns.$$;\nstatic class Calendar { }";
    const string unresolvedImport = "using Dates = Npm.$$DateFns.Exports;\nstatic class Calendar { }";
    try
    {
        await lsp.RequestAsync("initialize", new
        {
            processId = Environment.ProcessId,
            workspaceFolders = new[] { new { uri = root, name = "Calendar" } },
            rootUri = root,
            capabilities = new
            {
                workspace = firstManifest || nestedManifest
                    ? (object)new { configuration = true, workspaceFolders = true,
                        didChangeWatchedFiles = new { dynamicRegistration = true } }
                    : new { configuration = true, workspaceFolders = true },
                textDocument = new { diagnostic = new { }, hover = new { contentFormat = new[] { "markdown" } } },
            },
        });
        await lsp.NotifyAsync("initialized", new { });
        await lsp.WaitForNotificationAsync("workspace/projectInitializationComplete");
        await lsp.NotifyAsync("textDocument/didOpen", new { textDocument = new { uri, languageId = "csharp", version, text = disk } });
        // Initialize the host's generator state from the empty disk source before the first npm edit.
        await ExpectEventuallyAsync("hover on the original class before any npm import",
            () => HoverAsync(new Position(0, disk.IndexOf("Calendar", StringComparison.Ordinal) + 1)),
            result => Markdown(result)?.Contains("class Calendar", StringComparison.Ordinal) == true, stop);
        await ExpectEventuallyAsync("no compiler errors before any npm import", DiagnosticsAsync, NoErrors, stop);
        if (measureRegistryCompletion)
        {
            var timing = await VerifyRegistryCompletionAsync();
            await StopServerAsync();
            return timing;
        }
        if (pnpmWorkspace)
        {
            const string call = "using Hot = Npm.HotProbe.Exports;\nstatic class Calendar { static object Test() => Hot.Inspect($$\"true\"); }";
            var argument = await ChangeAsync(call);
            await ExpectEventuallyAsync("the workspace import binds with its initial string parameter", DiagnosticsAsync, NoErrors, stop);
            var initial = await File.ReadAllTextAsync(declaration, stop);
            await File.WriteAllTextAsync(declaration, initial.Replace("value: string): string", "value: boolean): boolean", StringComparison.Ordinal), stop);
            await ExpectEventuallyAsync("CS1503 after changing the workspace parameter to bool", DiagnosticsAsync,
                result => HasDiagnostic(result, "CS1503", argument, "\"true\"".Length), stop);
            await File.WriteAllTextAsync(declaration, initial, stop);
            await ExpectEventuallyAsync("no errors after restoring the workspace declaration", DiagnosticsAsync, NoErrors, stop);
            var lockPath = Path.Combine(scratch, "pnpm-lock.yaml");
            var initialLock = await File.ReadAllTextAsync(lockPath, stop);
            var importer = new System.Text.RegularExpressions.Regex(
                @"(?m)^  workspace:\r?\n(?:^[ ]{4,}[^\r\n]*(?:\r?\n|$))*");
            if (!importer.IsMatch(initialLock))
                throw new InvalidOperationException("Expected the shared lockfile to describe the C# project's importer.");
            await File.WriteAllTextAsync(lockPath, importer.Replace(initialLock, "", 1), stop);
            var member = argument with { Character = argument.Character - "Inspect(".Length + 1 };
            await ExpectEventuallyAsync("pnpm repairs the shared lockfile and hover shows the workspace string signature", async () =>
                {
                    var hover = await HoverAsync(member);
                    var repaired = await File.ReadAllTextAsync(lockPath, stop);
                    return importer.Match(repaired).Value.Contains("link:../hot-probe", StringComparison.Ordinal) ? hover : default;
                },
                result => result.ValueKind == JsonValueKind.Object &&
                    Markdown(result)?.Contains("Inspect(string value)", StringComparison.Ordinal) == true, stop);
            await ExpectEventuallyAsync("no errors after pnpm repairs the shared lockfile", DiagnosticsAsync, NoErrors, stop);
            if (await File.ReadAllTextAsync(source, stop) != disk)
                throw new InvalidOperationException("The workspace declaration refresh saved the editor fixture's C# source.");
            await StopServerAsync();
            return TimeSpan.Zero;
        }
        if (nestedManifest)
        {
            await VerifyNestedManifestAsync();
            await StopServerAsync();
            return TimeSpan.Zero;
        }
        if (sourceWorkspace)
        {
            await VerifySourceWorkspaceAsync();
            await StopServerAsync();
            return TimeSpan.Zero;
        }

        if (!firstManifest)
        {
            var initialAlias = await ChangeAsync("using Hot = Npm.HotProbe.Ex$$ports;\nstatic class Calendar { }");
            await ExpectEventuallyAsync("a cold alias for the dependency present at project load",
                () => HoverAsync(initialAlias), result => Markdown(result)?.Contains("class Npm.HotProbe.Exports", StringComparison.Ordinal) == true, stop);
            var initialPackages = await ChangeAsync(packageList);
            await ExpectEventuallyAsync("HotProbe present and DateFns absent before adding the dependency",
                () => CompleteAsync(initialPackages),
                result => HasCompletion(result, "HotProbe") && !HasCompletion(result, "DateFns"), stop);
            var missingPackage = await ChangeAsync(unresolvedImport);
            await ExpectEventuallyAsync("missing DateFns before adding the dependency", DiagnosticsAsync,
                result => HasDiagnostic(result, "CS0234", missingPackage, "DateFns".Length), stop);
        }
        else
        {
            await ChangeAsync(unresolvedImport);
            if (!repairManifest)
                await ExpectEventuallyAsync("missing Npm before creating the first manifest", DiagnosticsAsync,
                    result => !NoErrors(result), stop);
        }
        // Change only the npm files while the unresolved alias stays open and unsaved.
        await WriteEditorDependenciesAsync(workspace, includeDateFns: true, stop);
        if (firstManifest)
            await lsp.NotifyAsync("workspace/didChangeWatchedFiles", new
            {
                changes = new[] { "package.json", "pnpm-lock.yaml" }.Select(file =>
                    new { uri = new Uri(Path.Combine(workspace, file)).AbsoluteUri,
                        type = repairManifest && file == "package.json" ? 2 : 1 }).ToArray(),
            });
        if (mode == "Automatic")
            await ExpectEventuallyAsync("the existing unsaved alias binding after only the npm files change",
                DiagnosticsAsync, NoErrors, stop);
        var addedPackages = await ChangeAsync(packageList);
        await ExpectEventuallyAsync("DateFns and HotProbe in package completion before saving",
            () => CompleteAsync(addedPackages),
            result => HasCompletion(result, "DateFns") && HasCompletion(result, "HotProbe"), stop);
        var addedExportType = await ChangeAsync(exportType);
        await ExpectEventuallyAsync("Exports in the newly added namespace before saving",
            () => CompleteAsync(addedExportType), result => HasCompletion(result, "Exports"), stop);
        var addedMember = await ChangeAsync(firstCompletion);
        await ExpectEventuallyAsync("exports of the newly added package before saving",
            () => CompleteAsync(addedMember), result => HasCompletion(result, "AddDays"), stop);
        if (await File.ReadAllTextAsync(source, stop) != disk)
            throw new InvalidOperationException("Completion for the new dependency changed the C# source on disk.");
        if (firstManifest)
        {
            const string useDate = module +
                " { public static double Test() => Dates.AddDays($$new ECMAScript.Date(2024, 0, 31), 2).GetDate(); }";
            var arguments = await ChangeAsync(useDate);
            if (mode == "Balanced")
                await SaveAsync(useDate);
            await ExpectSignatureAsync(arguments, "AddDays");
            await ExpectEventuallyAsync("no compiler errors on the first npm call", DiagnosticsAsync, NoErrors, stop);
            if (mode == "Automatic" && await File.ReadAllTextAsync(source, stop) != disk)
                throw new InvalidOperationException("Automatic mode required a source save.");
            await StopServerAsync();
            return TimeSpan.Zero;
        }
        if (mode == "Balanced")
        {
            // Leave AddDays unused so the later check still exercises first use of a member.
            const string useNewDependency = module +
                " { public static double Test() => Dates.AddWeeks($$new ECMAScript.Date(2024, 0, 31), 2).GetDate(); }";
            var arguments = await ChangeAsync(useNewDependency);
            await SaveAsync(useNewDependency);
            await ExpectSignatureAsync(arguments, "AddWeeks");
        }
        else
            await ChangeAsync(unresolvedImport);
        await ExpectEventuallyAsync("the open file binding the newly added date-fns dependency", DiagnosticsAsync, NoErrors, stop);
        var aliasPosition = await ChangeAsync("using Dates = Npm.DateFns.Ex$$ports;\nstatic class Calendar { }");
        await ExpectEventuallyAsync("hover resolving the unsaved npm alias without a member use",
            () => HoverAsync(aliasPosition),
            result => Markdown(result)?.Contains("class Npm.DateFns.Exports", StringComparison.Ordinal) == true, stop);
        await ExpectEventuallyAsync("no compiler errors on the bare npm import", DiagnosticsAsync, NoErrors, stop);

        await ChangeAsync("using Missing = Npm.$$MissingProbe.Exports;\nstatic class Calendar { }");
        await ExpectEventuallyAsync("a compiler error for the undeclared npm package", DiagnosticsAsync,
            result => result.ValueKind == JsonValueKind.Object && result.TryGetProperty("items", out var diagnostics) &&
                      diagnostics.EnumerateArray().Any(diagnostic =>
                          diagnostic.TryGetProperty("code", out var code) && code.ToString() is "CS0234" or "CS0246" &&
                          diagnostic.GetProperty("message").GetString()!.Contains("MissingProbe", StringComparison.Ordinal)), stop);

        // The VS Code Balanced mode defers new projected members until a generator run.
        if (mode == "Balanced")
        {
            if (await File.ReadAllTextAsync(source, stop) != savedSource ||
                File.Exists(Path.Combine(workspace, "bin", "Debug", "net11.0", "Calendar.dll")))
                throw new InvalidOperationException("The Balanced editor fixture unexpectedly changed or built the C# project.");
            await VerifyBalancedSaveAsync();
            await VerifyProjectionFailureAsync();
            await VerifyGlobalImportAsync();
            await VerifyDependencyRemovalAsync();
            await StopServerAsync();
            // The separate cold generation check needs the original source without imports.
            await File.WriteAllTextAsync(source, disk, stop);
            await WriteEditorDependenciesAsync(workspace, includeDateFns: true, stop);
            return TimeSpan.Zero;
        }

        var signaturePosition = await ChangeAsync(module +
            " { public static double Test() => Dates.AddDays($$).GetDate(); }");
        await ExpectSignatureAsync(signaturePosition, "AddDays");

        var invalidArgument = await ChangeAsync(module +
            " { public static double Test() => Dates.AddDays(new ECMAScript.Date(2024, 0, 31), $$true).GetDate(); }");
        await ExpectEventuallyAsync("CS1503 on the unsaved bool argument", DiagnosticsAsync,
            result => HasDiagnostic(result, "CS1503", invalidArgument, "true".Length), stop);

        await ChangeAsync(module +
            " { public static double Test() => Dates.AddDays(new ECMAScript.Date(2024, 0, 31), $$2).GetDate(); }");
        await ExpectEventuallyAsync("no errors after correcting the argument", DiagnosticsAsync, NoErrors, stop);

        await ChangeAsync("""
            [ECMAScript.ECMAScriptModule("calendar.mjs")]
            public static class Calendar
            {
                public static int Test(int value)
                {
                    if (value > 0) goto Found;
                    return 0;
                    Found: return $$1;
                }
            }
            """);
        await ExpectEventuallyAsync("JAZORG001 for a bound but unsupported goto statement", DiagnosticsAsync,
            result => result.ValueKind == JsonValueKind.Object && result.TryGetProperty("items", out var diagnostics) &&
                      diagnostics.EnumerateArray().Any(diagnostic =>
                          diagnostic.TryGetProperty("code", out var code) && code.ToString() == "JAZORG001" &&
                          diagnostic.GetProperty("message").GetString()!.Contains("Goto statements", StringComparison.Ordinal)), stop);

        var shadowedPosition = await ChangeAsync(import +
            "\nstatic class Calendar { static object Test() { var Dates = new Local(); return Dates.$$; } }" +
            "\nsealed class Local { public int LocalOnly => 1; }");
        await ExpectEventuallyAsync("members of the local value shadowing the npm alias",
            () => CompleteAsync(shadowedPosition),
            result => HasCompletion(result, "LocalOnly") && !HasCompletion(result, "AddDays"), stop);

        var removedPosition = await ChangeAsync("static class Calendar { static object Test() => Dates.$$; }");
        if (HasCompletion(await CompleteAsync(removedPosition), "AddDays"))
            throw new InvalidOperationException("The deleted npm alias still offers AddDays.");
        var assembly = Path.Combine(workspace, "bin", "Debug", "net11.0", "Calendar.dll");
        if (await File.ReadAllTextAsync(source, stop) != disk || File.Exists(assembly))
            throw new InvalidOperationException("The editor fixture saved or built the C# project.");

        await RunDotnetAsync(workspace, stop, "build", "Calendar.csproj", "--no-restore", "--nologo");
        if (!File.Exists(assembly))
            throw new InvalidOperationException("The terminal build did not produce the project assembly.");
        var afterBuild = await ChangeAsync(firstCompletion);
        await ExpectEventuallyAsync("member completion after a terminal build", () => CompleteAsync(afterBuild),
            result => HasCompletion(result, "AddWeeks"), stop);
        var afterBuildSignature = await ChangeAsync(module +
            " { public static double Test() => Dates.AddWeeks($$).GetDate(); }");
        await ExpectSignatureAsync(afterBuildSignature, "AddWeeks");
        if (await File.ReadAllTextAsync(source, stop) != disk)
            throw new InvalidOperationException("The terminal build changed the editor fixture's C# source.");

        await VerifyProjectionFailureAsync();
        await VerifyLinkedDeclarationAsync();
        await VerifyGlobalImportAsync();
        await VerifyDependencyRemovalAsync();

        await StopServerAsync();
        await WriteEditorDependenciesAsync(workspace, includeDateFns: true, stop);
        return TimeSpan.Zero;
    }
    finally
    {
        if (!process.HasExited)
            process.Kill(entireProcessTree: true);
        await File.WriteAllTextAsync(Path.Combine(scratch, "lsp.stderr.log"), await stderr);
    }

    async Task VerifyNestedManifestAsync()
    {
        const string parentCall = "using Hot = Npm.HotProbe.Exports;\nstatic class ParentCall { static string Run() => Hot.Inspect($$\"ok\"); }";
        string parentDiskCall = parentCall.Replace("$$", "", StringComparison.Ordinal);
        var signature = await ChangeAsync(parentCall);
        await ExpectParentSignatureAsync(signature);
        await ExpectEventuallyAsync("the inherited parent alias binds", DiagnosticsAsync, NoErrors, stop);
        await VerifyBuildAsync(parentDiskCall, 0);

        await WriteEditorDependenciesAsync(workspace, includeDateFns: true, stop, hotProbe: null);
        await NotifyManifestAsync(1);
        const string childCall = module + " { public static double Test() => Dates.AddDays($$new ECMAScript.Date(2024, 0, 31), 2).GetDate(); }";
        await VerifyBuildAsync(childCall.Replace("$$", "", StringComparison.Ordinal).Replace("class Calendar", "class ChildCall", StringComparison.Ordinal), 0);
        await VerifyBuildAsync(parentDiskCall, 1);
        var parentAlias = new Position(0, "using Hot = Npm.".Length);
        var npmRoot = new Position(0, "using Hot = ".Length);
        await ExpectEventuallyAsync("the child manifest hides the active parent alias",
            DiagnosticsAsync, result => HasDiagnostic(result, "CS0234", parentAlias, "HotProbe".Length) ||
                HasDiagnostic(result, "CS0246", npmRoot, "Npm".Length), stop);
        await ExpectPackagesAsync("DateFns", "HotProbe");
        await ExpectSignatureAsync(await ChangeAsync(childCall), "AddDays");
        await ExpectEventuallyAsync("the child alias binds", DiagnosticsAsync, NoErrors, stop);

        signature = await ChangeAsync(parentCall);
        File.Delete(Path.Combine(workspace, "package.json"));
        File.Delete(Path.Combine(workspace, "pnpm-lock.yaml"));
        await NotifyManifestAsync(3);
        await ExpectEventuallyAsync("restoring the parent manifest rebinds the active parent call", DiagnosticsAsync, NoErrors, stop);
        await ExpectParentSignatureAsync(signature);
        await ExpectPackagesAsync("HotProbe", "DateFns");
        await VerifyBuildAsync(parentDiskCall, 0);
        if (await File.ReadAllTextAsync(source, stop) != disk)
            throw new InvalidOperationException("The nested manifest fixture saved the C# buffer.");

        Task NotifyManifestAsync(int type) => lsp.NotifyAsync("workspace/didChangeWatchedFiles", new
        {
            changes = new[] { "package.json", "pnpm-lock.yaml" }.Select(file =>
                new { uri = new Uri(Path.Combine(workspace, file)).AbsoluteUri, type }).ToArray(),
        });

        Task ExpectParentSignatureAsync(Position position) => ExpectEventuallyAsync("the typed inherited Inspect signature",
            () => lsp.RequestAsync("textDocument/signatureHelp", new { textDocument = document, position }),
            result => result.ValueKind == JsonValueKind.Object && result.TryGetProperty("signatures", out var signatures) &&
                signatures.EnumerateArray().Any(item =>
                    item.GetProperty("label").GetString() == "string Hot.Inspect(string value)"), stop);

        async Task ExpectPackagesAsync(string present, string absent)
        {
            var position = await ChangeAsync(packageList);
            await ExpectEventuallyAsync($"{present} present and {absent} absent from package completion",
                () => CompleteAsync(position),
                result => HasCompletion(result, present) && !HasCompletion(result, absent), stop);
        }

        async Task VerifyBuildAsync(string text, int expected)
        {
            var buildSource = Path.Combine(workspace, "BuildManifestProbe.cs");
            await File.WriteAllTextAsync(buildSource, text, stop);
            try
            {
                await RunDotnetCoreAsync(workspace, stop, expected, ["build", "Calendar.csproj", "--no-restore", "--nologo"]);
            }
            finally
            {
                File.Delete(buildSource);
            }
        }
    }

    async Task StopServerAsync()
    {
        await lsp.RequestAsync("shutdown", null);
        await lsp.NotifyAsync("exit", null);
        await process.WaitForExitAsync(stop);
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"The C# language server exited with code {process.ExitCode}.");
    }

    async Task<TimeSpan> VerifyRegistryCompletionAsync()
    {
        var alias = await ChangeAsync(import + "\nstatic class Calendar { static System.Type Test() => typeof(Da$$tes); }");
        await ExpectEventuallyAsync("a cold date-fns facade before requesting any members", () => HoverAsync(alias),
            result => Markdown(result)?.Contains("Npm.DateFns.Exports", StringComparison.Ordinal) == true, stop);
        var completion = await ChangeAsync(firstCompletion);
        var elapsed = Stopwatch.StartNew();
        await ExpectEventuallyAsync("the first date-fns member completion offering AddDays", () => CompleteAsync(completion),
            result => HasCompletion(result, "AddDays"), stop);
        elapsed.Stop();

        var call = await ChangeAsync(import + "\nstatic class Calendar { static ECMAScript.Date Test() => Dates.$$AddDays(new ECMAScript.Date(2024, 0, 31), 2); }");
        var root = new Uri(Path.Combine(workspace, "node_modules") + Path.DirectorySeparatorChar).AbsoluteUri;
        const string link = "[Open TypeScript declaration](";
        await ExpectEventuallyAsync("date-fns documentation and the project's own TypeScript declaration link", () => HoverAsync(call),
            result => Markdown(result) is { } hover && IsLocalDeclaration(hover) &&
                hover.Contains("Add the specified number of days to the given date", StringComparison.Ordinal), stop);
        return elapsed.Elapsed;

        bool IsLocalDeclaration(string? hover)
        {
            var linkStart = hover?.IndexOf(link + root, StringComparison.OrdinalIgnoreCase) ?? -1;
            if (linkStart < 0)
                return false;
            var targetStart = linkStart + link.Length;
            var end = hover!.IndexOf(')', targetStart);
            return end > targetStart && Uri.TryCreate(hover[targetStart..end], UriKind.Absolute, out var declaration) &&
                declaration.IsFile && declaration.LocalPath.EndsWith(".d.ts", StringComparison.Ordinal) && File.Exists(declaration.LocalPath);
        }
    }

    async Task VerifySourceWorkspaceAsync()
    {
        const string packages = "using Hot = Npm.$$;\nstatic class Calendar { }";
        var packagePosition = await ChangeAsync(packages);
        await ExpectEventuallyAsync("cold package completion at an incomplete source-local import", () => CompleteAsync(packagePosition),
            result => HasCompletion(result, "HotProbe") && !HasCompletion(result, "WrongPackage"), stop);

        var importsPath = Path.Combine(Path.GetDirectoryName(source)!, "NpmImports.cs");
        var importsUri = new Uri(importsPath).AbsoluteUri;
        const string imports = "global using Probe = Npm.HotProbe.Exports;\nglobal using static Npm.HotProbe.Exports;\n";
        await lsp.NotifyAsync("textDocument/didOpen", new
        {
            textDocument = new { uri = importsUri, languageId = "csharp", version = 1, text = imports },
        });
        // Wait for Roslyn to bind the separate unsaved document, without requesting any
        // export member that could pre-populate the generated facade or completion index.
        var alias = await ChangeAsync("static class Calendar { static System.Type Test() => typeof(Pro$$be); }");
        await ExpectEventuallyAsync("the cross-file global alias resolves to the cold facade", () => HoverAsync(alias),
            result => Markdown(result)?.Contains("Npm.HotProbe.Exports", StringComparison.Ordinal) == true, stop);
        var globalAlias = await ChangeAsync("static class Calendar { static object Test() => Probe.$$; }");
        await ExpectEventuallyAsync("cold member completion through the cross-file global alias", () => CompleteAsync(globalAlias),
            result => HasCompletion(result, "Inspect"), stop);
        const string call = "static class Calendar { static string Test() => $$Inspect(\"true\"); }";
        var callPosition = await ChangeAsync(call);
        await ExpectEventuallyAsync("typed cross-file global using from the source manifest", () => HoverAsync(callPosition),
            result => Markdown(result)?.Contains("Inspect(string value)", StringComparison.Ordinal) == true, stop);
        await ExpectEventuallyAsync("no errors on the global using call", DiagnosticsAsync, NoErrors, stop);

        var original = await File.ReadAllTextAsync(declaration, stop);
        await File.WriteAllTextAsync(declaration, original.Replace("value: string): string", "value: boolean): boolean", StringComparison.Ordinal), stop);
        var argument = callPosition with { Character = callPosition.Character + "Inspect(".Length };
        await ExpectEventuallyAsync("CS1503 after a linked declaration changes under source ownership", DiagnosticsAsync,
            result => HasDiagnostic(result, "CS1503", argument, "\"true\"".Length), stop);
        await File.WriteAllTextAsync(declaration, original, stop);
        await ExpectEventuallyAsync("no errors after restoring the source-owned linked declaration", DiagnosticsAsync, NoErrors, stop);
        if (await File.ReadAllTextAsync(source, stop) != disk || await File.ReadAllTextAsync(importsPath, stop) != "")
            throw new InvalidOperationException("Source-owned projection saved an unsaved C# file.");
        await lsp.NotifyAsync("textDocument/didClose", new { textDocument = new { uri = importsUri } });
    }

    async Task<Position> ChangeAsync(string markedText)
    {
        int offset = markedText.IndexOf("$$", StringComparison.Ordinal);
        var line = markedText[..offset].Count(character => character == '\n');
        var column = offset - markedText.LastIndexOf('\n', offset) - 1;
        var text = markedText.Remove(offset, 2);
        await lsp.NotifyAsync("textDocument/didChange",
            new { textDocument = new { uri, version = ++version }, contentChanges = new[] { new { text } } });
        return new Position(line, column);
    }

    Task<JsonElement> CompleteAsync(Position position) =>
        lsp.RequestAsync("textDocument/completion", new
        {
            textDocument = document, position, context = new { triggerKind = 2, triggerCharacter = "." },
        });

    Task<JsonElement> DiagnosticsAsync() => lsp.RequestAsync("textDocument/diagnostic", new { textDocument = document });
    Task<JsonElement> HoverAsync(Position position) => lsp.RequestAsync("textDocument/hover", new { textDocument = document, position });

    Task ExpectSignatureAsync(Position position, string member) => ExpectEventuallyAsync($"typed {member} signature",
        () => lsp.RequestAsync("textDocument/signatureHelp", new { textDocument = document, position }),
        result => result.ValueKind == JsonValueKind.Object && result.TryGetProperty("signatures", out var signatures) &&
                  signatures.EnumerateArray().Any(item => item.GetProperty("label").GetString() is { } label &&
                      label.Contains($"Dates.{member}(ECMAScript.Date date, double amount)", StringComparison.Ordinal)),
        stop);

    async Task VerifyBalancedSaveAsync()
    {
        const string call = " { public static double Test() => Dates.$$AddDays(new ECMAScript.Date(2024, 0, 31), true).GetDate(); }";
        var invalidSource = module + call;
        var member = await ChangeAsync(invalidSource);
        var argument = member with { Character = member.Character + "AddDays(new ECMAScript.Date(2024, 0, 31), ".Length };
        var signature = member with { Character = member.Character + "AddDays(".Length };
        await ExpectEventuallyAsync("CS0117 on the newly selected member before saving in Balanced mode",
            DiagnosticsAsync, result => HasDiagnostic(result, "CS0117", member, "AddDays".Length), stop);
        if (await File.ReadAllTextAsync(source, stop) != savedSource)
            throw new InvalidOperationException("The selected npm member reached the C# file before the save.");

        await SaveAsync(invalidSource);
        await ExpectSignatureAsync(signature, "AddDays");
        await ExpectEventuallyAsync("CS1503 on the saved bool argument in Balanced mode",
            DiagnosticsAsync, result => HasDiagnostic(result, "CS1503", argument, "true".Length), stop);

        var corrected = invalidSource.Replace(", true)", ", 2)", StringComparison.Ordinal);
        await ChangeAsync(corrected);
        await SaveAsync(corrected);
        await ExpectEventuallyAsync("no errors after saving the corrected argument in Balanced mode",
            DiagnosticsAsync, NoErrors, stop);
    }

    async Task SaveAsync(string markedText)
    {
        savedSource = markedText.Replace("$$", "", StringComparison.Ordinal);
        await File.WriteAllTextAsync(source, savedSource, stop);
        await lsp.NotifyAsync("textDocument/didSave", new { textDocument = document });
    }

    async Task VerifyGlobalImportAsync()
    {
        var importsPath = Path.Combine(workspace, "NpmImports.cs");
        var importsUri = new Uri(importsPath).AbsoluteUri;
        var importsDocument = new { uri = importsUri };
        int importsVersion = 1;
        await lsp.NotifyAsync("textDocument/didOpen", new
        {
            textDocument = new { uri = importsUri, languageId = "csharp", version = importsVersion, text = "" },
        });
        const string use = "static class Calendar { static double Run() => $$AddDays(new ECMAScript.Date(2024, 0, 31), 2).GetDate(); }";
        var position = await ChangeAsync(use);
        await ExpectMissingAsync();
        const string globalImport = "global using static Npm.DateFns.Exports;\n";
        await ChangeImportsAsync(globalImport);
        if (await File.ReadAllTextAsync(importsPath, stop) != "")
            throw new InvalidOperationException("The cross-file import was saved before the unsaved-buffer check.");
        if (mode == "Balanced")
            await SaveImportsAsync(globalImport);
        await ExpectEventuallyAsync("an npm call resolved by another document's global using",
            DiagnosticsAsync, NoErrors, stop);
        await ExpectEventuallyAsync("typed AddDays hover through a cross-file global using", () => HoverAsync(position),
            result => Markdown(result)?.Contains("AddDays", StringComparison.Ordinal) == true, stop);
        await ChangeImportsAsync("");
        await ExpectMissingAsync();
        if (mode == "Balanced")
            await SaveImportsAsync("");
        await lsp.NotifyAsync("textDocument/didClose", new { textDocument = importsDocument });
        Console.WriteLine($"Verified cross-file global using addition and removal in {mode} mode.");

        Task ExpectMissingAsync() => ExpectEventuallyAsync("CS0103 after removing the cross-file global using",
            DiagnosticsAsync, result => HasDiagnostic(result, "CS0103", position, "AddDays".Length), stop);

        Task ChangeImportsAsync(string text) => lsp.NotifyAsync("textDocument/didChange", new
        {
            textDocument = new { uri = importsUri, version = ++importsVersion }, contentChanges = new[] { new { text } },
        });

        async Task SaveImportsAsync(string text)
        {
            await File.WriteAllTextAsync(importsPath, text, stop);
            await lsp.NotifyAsync("textDocument/didSave", new { textDocument = importsDocument });
        }
    }

    async Task VerifyProjectionFailureAsync()
    {
        const string good = "using Hot = Npm.HotProbe.Exports;\npublic static class Calendar\n{\n" +
            "    public static string Good() => Hot.Inspect(\"ok\");\n";
        const string bad = "    public static object Bad() => Hot.$$Unprojectable();\n}\n";
        var originalDisk = await File.ReadAllTextAsync(source, stop);
        var before = await EditAsync(good + bad);
        await ExpectProjectionErrorAsync(before);
        if (mode == "Balanced")
        {
            // The saved editor request and the SDK build must diagnose the same caller and declaration.
            var log = Path.Combine(scratch, "npm-projection-build.log");
            await RunDotnetCoreAsync(workspace, stop, expectedExitCode: 1,
                ["build", "Calendar.csproj", "--no-restore", "--nologo", "-flp:LogFile=" + log + ";Verbosity=minimal"]);
            var caller = $"Calendar.cs({before.Line + 1},{before.Character + 1}): error JAZOR:";
            if (!File.ReadLines(log).Any(line => line.Contains(caller, StringComparison.Ordinal) &&
                IsProjectionFailure(line)))
                throw new InvalidOperationException($"Build and editor disagreed on the unsupported npm projection. See {log}");
        }
        var moved = await EditAsync("// A source-only edit moves the call.\n" + good + bad);
        await ExpectProjectionErrorAsync(moved);
        var goodMember = await ChangeAsync(("// A source-only edit moves the call.\n" + good +
            bad.Replace("$$", "", StringComparison.Ordinal)).Replace("Hot.Inspect", "Hot.$$Inspect", StringComparison.Ordinal));
        await ExpectEventuallyAsync("typed supported sibling while the other projection fails",
            () => HoverAsync(goodMember), result => Markdown(result)?.Contains("string Hot.Inspect(string value)", StringComparison.Ordinal) == true, stop);
        await EditAsync(good + "}\n// $$");
        await ExpectEventuallyAsync("projection error clears after removing the unsupported call", DiagnosticsAsync, NoErrors, stop);
        if (mode == "Balanced")
            await SaveAsync(originalDisk);
        else if (await File.ReadAllTextAsync(source, stop) != originalDisk)
            throw new InvalidOperationException("The projection failure check wrote the editor's unsaved buffer to disk.");

        async Task<Position> EditAsync(string markedText)
        {
            var position = await ChangeAsync(markedText);
            if (mode == "Balanced")
                await SaveAsync(markedText);
            return position;
        }

        Task ExpectProjectionErrorAsync(Position position) => ExpectEventuallyAsync("JAZOR at the current unsupported member",
            DiagnosticsAsync, result => HasDiagnostic(result, "JAZOR", position, "Unprojectable".Length) &&
                result.GetProperty("items").EnumerateArray().Any(item =>
                    item.GetProperty("code").ToString() == "JAZOR" &&
                    item.GetProperty("message").GetString() is { } message &&
                    IsProjectionFailure(message)), stop);

        static bool IsProjectionFailure(string message) =>
            message.Contains("unprojectable", StringComparison.Ordinal) &&
            message.Contains("symbol", StringComparison.Ordinal) &&
            message.Contains("return type", StringComparison.Ordinal) &&
            message.Contains("implementation.d.ts(3,42)", StringComparison.Ordinal);
    }

    async Task VerifyDependencyRemovalAsync()
    {
        const string useDateFns = import +
            "\nstatic class Calendar { static object Test() => Dates.AddDays(new ECMAScript.Date(2024, 0, 31), 2); }";
        var missing = await ChangeAsync(useDateFns.Replace("DateFns", "$$DateFns", StringComparison.Ordinal));
        if (mode == "Balanced")
            await SaveAsync(useDateFns);
        await ExpectEventuallyAsync("the installed date-fns import and member binding before removal",
            DiagnosticsAsync, NoErrors, stop);
        await WriteEditorDependenciesAsync(workspace, includeDateFns: false, stop);
        if (mode == "Balanced")
            await SaveAsync(useDateFns);
        await ExpectEventuallyAsync("a compiler diagnostic for the removed DateFns dependency", DiagnosticsAsync,
            result => HasDiagnostic(result, "CS0234", missing, "DateFns".Length) ||
                HasDiagnostic(result, "CS0246", missing, "DateFns".Length), stop);
        var packages = await ChangeAsync(packageList);
        await ExpectEventuallyAsync("HotProbe remains and DateFns disappears from package completion",
            () => CompleteAsync(packages), result => HasCompletion(result, "HotProbe") && !HasCompletion(result, "DateFns"), stop);
        var members = await ChangeAsync(firstCompletion);
        await ExpectEventuallyAsync("the removed alias no longer offers date-fns members",
            () => CompleteAsync(members), result => !HasCompletion(result, "AddDays") && !HasCompletion(result, "AddWeeks"), stop);
        if (mode == "Automatic" && await File.ReadAllTextAsync(source, stop) != disk)
            throw new InvalidOperationException("Removing the dependency saved the editor fixture's C# source.");
    }

    async Task VerifyLinkedDeclarationAsync()
    {
        const string call = "using Hot = Npm.HotProbe.Exports;\nstatic class Calendar { static object Test() => Hot.In$$spect(default); }";
        var memberPosition = await ChangeAsync(call);
        var argumentPosition = memberPosition with { Character = memberPosition.Character + "spect(".Length };
        await ExpectLinkedSignatureAsync("string");
        await ExpectDeclarationLinkAsync("Parses a flag supplied as text");

        const string updatedSummary = "Reads the flag from the provided text.";
        var originalDeclaration = await File.ReadAllTextAsync(declaration, stop);
        await File.WriteAllTextAsync(declaration,
            originalDeclaration.Replace("Parses a flag supplied as text.", updatedSummary, StringComparison.Ordinal), stop);
        await ExpectDeclarationLinkAsync("Reads the flag from the provided text", staleSummary: "Parses a flag supplied as text");
        await ExpectLinkedSignatureAsync("string");

        // Only this declaration changes between requests. Drive the real host's own file watcher.
        await File.WriteAllTextAsync(declaration,
            "// linked package declaration\n\nexport declare function parseFlag(value: boolean): boolean;\n", stop);
        await ExpectLinkedSignatureAsync("bool");
        await ExpectDeclarationLinkAsync();

        const string exportList = "using Hot = Npm.HotProbe.Exports;\nstatic class Calendar { static object Test() => Hot.$$; }";
        var exportPosition = await ChangeAsync(exportList);
        await ExpectEventuallyAsync("Inspect in the linked npm export index", () => CompleteAsync(exportPosition),
            result => HasCompletion(result, "Inspect") && !HasCompletion(result, "Measure"), stop);
        var index = Path.Combine(Path.GetDirectoryName(declaration)!, "index.d.ts");
        var previousTimestamp = File.GetLastWriteTimeUtc(index);
        var originalIndex = await File.ReadAllTextAsync(index, stop);
        await File.WriteAllTextAsync(index, originalIndex.Replace("as inspect", "as measure", StringComparison.Ordinal), stop);
        File.SetLastWriteTimeUtc(index, previousTimestamp);
        await ExpectEventuallyAsync("Measure offered without changing the linked declaration timestamp",
            () => CompleteAsync(exportPosition),
            result => HasCompletion(result, "Measure"), stop);

        // Roslyn's frozen completion snapshot may still offer its previous typed member.
        // Check semantic binding on a real invocation, then its next completion snapshot.
        var removedExport = await ChangeAsync(call.Replace("In$$spect", "$$Inspect", StringComparison.Ordinal));
        await ExpectEventuallyAsync("CS0117 for the former linked npm export", DiagnosticsAsync,
            result => HasDiagnostic(result, "CS0117", removedExport, "Inspect".Length), stop);
        var refreshedExportPosition = await ChangeAsync(exportList);
        await ExpectEventuallyAsync("Measure replacing Inspect on a fresh completion snapshot", () => CompleteAsync(refreshedExportPosition),
            result => HasCompletion(result, "Measure") && !HasCompletion(result, "Inspect"), stop);

        if (await File.ReadAllTextAsync(source, stop) != disk)
            throw new InvalidOperationException("The linked declaration refresh saved the editor fixture's C# source.");

        Task ExpectLinkedSignatureAsync(string type) => ExpectEventuallyAsync($"linked Inspect signature with {type} parameter",
            () => lsp.RequestAsync("textDocument/signatureHelp", new { textDocument = document, position = argumentPosition }),
            result => result.ValueKind == JsonValueKind.Object && result.TryGetProperty("signatures", out var signatures) &&
                      signatures.EnumerateArray().Any(item => item.GetProperty("label").GetString() is { } label &&
                          label.Contains($"Hot.Inspect({type} value)", StringComparison.Ordinal)),
            stop);

        async Task ExpectDeclarationLinkAsync(string? summary = null, string? staleSummary = null)
        {
            var lines = await File.ReadAllLinesAsync(declaration, stop);
            int line = Array.FindIndex(lines, text => text.Contains("parseFlag", StringComparison.Ordinal));
            int column = lines[line].IndexOf("parseFlag", StringComparison.Ordinal);
            var target = new Uri(declaration).AbsoluteUri + $"#L{line + 1},{column + 1}";
            var link = $"[Open TypeScript declaration]({target})";
            await ExpectEventuallyAsync($"hover link to {target}",
                () => HoverAsync(memberPosition),
                result => Markdown(result) is { } hover && hover.Contains(link, StringComparison.Ordinal) &&
                    (summary is null || hover.Contains(summary, StringComparison.Ordinal)) &&
                    (staleSummary is null || !hover.Contains(staleSummary, StringComparison.Ordinal)),
                stop);
            await ExpectEventuallyAsync("stock C# definition returns the generated declaration",
                () => lsp.RequestAsync("textDocument/definition", new { textDocument = document, position = memberPosition }),
                result => result.ValueKind == JsonValueKind.Array && result.EnumerateArray().Any(location =>
                    location.TryGetProperty("uri", out var uri) &&
                    uri.GetString()?.StartsWith("roslyn-source-generated:", StringComparison.Ordinal) == true),
                stop);
            Console.WriteLine($"C# definition returned a generated declaration. Its call-site hover linked to {target}.");
        }
    }
}

static async Task ExpectEventuallyAsync(string outcome, Func<Task<JsonElement>> request, Func<JsonElement, bool> matches,
    CancellationToken stop)
{
    var wait = Stopwatch.StartNew();
    JsonElement result;
    do
    {
        result = await request();
        if (matches(result))
            return;
        await Task.Delay(300, stop);
    }
    while (wait.Elapsed < TimeSpan.FromSeconds(20));
    throw new InvalidOperationException($"Expected {outcome}. Last LSP result: {result}");
}

static bool HasCompletion(JsonElement result, string expected)
{
    if (result.ValueKind == JsonValueKind.Object && result.TryGetProperty("items", out var items))
        result = items;
    return result.ValueKind == JsonValueKind.Array &&
        result.EnumerateArray().Any(item => item.GetProperty("label").GetString() == expected);
}

static string? Markdown(JsonElement result) =>
    result.ValueKind == JsonValueKind.Object && result.TryGetProperty("contents", out var contents)
    && contents.ValueKind == JsonValueKind.Object && contents.TryGetProperty("value", out var markdown)
        ? markdown.GetString()
        : null;

static bool NoErrors(JsonElement result) =>
    result.ValueKind == JsonValueKind.Object && result.TryGetProperty("items", out var diagnostics) &&
    !diagnostics.EnumerateArray().Any(diagnostic =>
        diagnostic.TryGetProperty("severity", out var severity) && severity.GetInt32() == 1 ||
        diagnostic.TryGetProperty("code", out var code) && code.ToString().StartsWith("JAZOR", StringComparison.Ordinal));

static bool HasDiagnostic(JsonElement result, string expectedCode, Position expected, int length)
{
    return result.ValueKind == JsonValueKind.Object && result.TryGetProperty("items", out var diagnostics) &&
        diagnostics.EnumerateArray().Any(diagnostic =>
        {
            if (!diagnostic.TryGetProperty("code", out var code) || code.ToString() != expectedCode)
                return false;
            var range = diagnostic.GetProperty("range");
            return ReadPosition(range.GetProperty("start")) == expected &&
                   ReadPosition(range.GetProperty("end")) == expected with { Character = expected.Character + length };
        });
}

static Position ReadPosition(JsonElement point) =>
    new(point.GetProperty("line").GetInt32(), point.GetProperty("character").GetInt32());

static Task RunDotnetAsync(string workspace, CancellationToken stop, params string[] arguments)
    => RunDotnetCoreAsync(workspace, stop, 0, arguments);

static async Task RunDotnetCoreAsync(string workspace, CancellationToken stop, int expectedExitCode, string[] arguments)
{
    var start = new ProcessStartInfo("dotnet") { WorkingDirectory = workspace, UseShellExecute = false };
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    using var process = Process.Start(start)!;
    try
    {
        await process.WaitForExitAsync(stop);
    }
    finally
    {
        if (!process.HasExited)
            process.Kill(entireProcessTree: true);
    }
    if (process.ExitCode != expectedExitCode)
        throw new InvalidOperationException($"dotnet {arguments[0]} exited with {process.ExitCode}, expected {expectedExitCode} in the Jazor editor fixture.");
}

static string ReadJazorVersion(string package)
{
    using var archive = ZipFile.OpenRead(package);
    using var stream = (archive.GetEntry("Jazor.nuspec") ?? throw new InvalidDataException("Expected a Jazor NuGet package.")).Open();
    var document = XDocument.Load(stream);
    var ns = document.Root!.Name.Namespace;
    return document.Root.Element(ns + "metadata")!.Element(ns + "version")!.Value;
}

static string FindInstalledServer()
{
    var name = OperatingSystem.IsWindows() ? "Microsoft.CodeAnalysis.LanguageServer.exe" : "Microsoft.CodeAnalysis.LanguageServer";
    var extensions = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".vscode", "extensions");
    if (Directory.Exists(extensions))
    {
        var candidate = Directory.EnumerateDirectories(extensions, "ms-dotnettools.csharp-*")
            .Select(directory => Path.Combine(directory, ".roslyn", name))
            .Where(File.Exists)
            .MaxBy(File.GetLastWriteTimeUtc);
        if (candidate is not null)
            return candidate;
    }
    throw new FileNotFoundException("Pass the C# extension's Microsoft.CodeAnalysis.LanguageServer apphost.");
}

static string ScriptDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;

internal readonly record struct Position(int Line, int Character);

internal sealed class LspClient(Stream input, Stream output, TextWriter transcript, CancellationToken stop)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HashSet<string> _notifications = new(StringComparer.Ordinal);
    private int _sequence;

    public async Task<JsonElement> RequestAsync(string method, object? parameters)
    {
        int id = ++_sequence;
        await SendAsync(new { jsonrpc = "2.0", id, method, @params = parameters });
        while (true)
        {
            var message = await ReceiveAsync();
            if (message.ValueKind == JsonValueKind.Undefined || !message.TryGetProperty("id", out var receivedId) || receivedId.GetInt32() != id)
                continue;
            if (message.TryGetProperty("error", out var error))
                throw new InvalidOperationException($"{method}: {error}");
            return message.GetProperty("result");
        }
    }

    public Task NotifyAsync(string method, object? parameters) => SendAsync(new { jsonrpc = "2.0", method, @params = parameters });

    public async Task WaitForNotificationAsync(string method)
    {
        while (!_notifications.Contains(method))
            await ReceiveAsync();
    }

    private async Task<JsonElement> ReceiveAsync()
    {
        var header = new List<byte>();
        var singleByte = new byte[1];
        do
        {
            await output.ReadExactlyAsync(singleByte, stop);
            header.Add(singleByte[0]);
        }
        while (header.Count < 4 || header[^4] != '\r' || header[^3] != '\n' || header[^2] != '\r' || header[^1] != '\n');

        var length = int.Parse(Encoding.ASCII.GetString(header.ToArray()).Split("\r\n")
            .Single(line => line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)).Split(':', 2)[1]);
        var body = new byte[length];
        await output.ReadExactlyAsync(body, stop);
        await transcript.WriteLineAsync("< " + Encoding.UTF8.GetString(body));
        using var json = JsonDocument.Parse(body);
        var message = json.RootElement.Clone();
        if (!message.TryGetProperty("method", out var method))
            return message;
        if (message.TryGetProperty("id", out var id))
        {
            object? result = method.GetString() == "workspace/configuration"
                ? new object?[message.GetProperty("params").GetProperty("items").GetArrayLength()]
                : null;
            await SendAsync(new { jsonrpc = "2.0", id, result });
        }
        else
            _notifications.Add(method.GetString()!);
        return default;
    }

    private async Task SendAsync(object payload)
    {
        byte[] body = JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions);
        await transcript.WriteLineAsync("> " + Encoding.UTF8.GetString(body));
        await input.WriteAsync(Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n"), stop);
        await input.WriteAsync(body, stop);
        await input.FlushAsync(stop);
    }
}
