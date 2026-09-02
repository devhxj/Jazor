#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

var options = SmokeOptions.Parse(args);
var repoRoot = ScriptHelpers.FindRepositoryRoot(Directory.GetCurrentDirectory());
var sampleRoot = Path.Combine(repoRoot, "samples", "RazorVue.Authoring");
var buildScript = Path.Combine(sampleRoot, "build-local.cs");
var workRoot = ScriptHelpers.ResolvePath(repoRoot, options.WorkRoot ?? Path.Combine(".tmp", "sample-smoke", "RazorVue.Authoring", options.Configuration));
var packageOutput = ScriptHelpers.ResolvePath(repoRoot, options.PackageOutput ?? Path.Combine(".tmp", "nupkg-sample", "RazorVue.Authoring"));
var sourceJazorRoot = Path.Combine(workRoot, "source-jazor");
var packageJazorRoot = Path.Combine(workRoot, "package-jazor");
var releaseJazorRoot = Path.Combine(workRoot, "release-jazor");
var browserWorkRoot = Path.Combine(workRoot, "browser");

ScriptHelpers.EnsureInsideRepository(repoRoot, workRoot, packageOutput, sourceJazorRoot, packageJazorRoot, releaseJazorRoot, browserWorkRoot);

if (!options.SkipBuild)
{
    await ScriptHelpers.RunProcessAsync(
        "dotnet",
        [
            "run", "--no-launch-profile", "--file", buildScript, "--",
            "--configuration", options.Configuration,
            "--work-root", workRoot,
            "--package-output", packageOutput,
            "--source-jazor-dir", sourceJazorRoot,
            "--jazor-dir", packageJazorRoot,
            "--release-jazor-dir", releaseJazorRoot
        ],
        repoRoot);
}

AssertAuthoringSource(sampleRoot);
AssertDebugArtifacts(packageJazorRoot);
AssertReleaseArtifacts(releaseJazorRoot);
AssertLocalPackages(packageOutput);
AssertNoNodeModules(workRoot);

if (!options.SkipBrowser)
    await VerifyReleaseHostInBrowserAsync(repoRoot, workRoot, releaseJazorRoot, options.Configuration, options.BrowserPath);

Console.WriteLine("RazorVue.Authoring smoke verification passed.");
Console.WriteLine(options.SkipBrowser
    ? "Verified: source authoring boundary, isolated local package consumer, debug modules, manifests, source maps, and Release bundle closure. Browser mount was skipped."
    : "Verified: source authoring boundary, isolated local package consumer, debug modules, manifests, source maps, Release bundle closure, PathBase browser mount, and the interactive route/query/history/not-found journey.");

static void AssertAuthoringSource(string sampleRoot)
{
    var forbidden = new[]
    {
        "BuildRenderTree", "RenderTreeBuilder", "AdminInput", "AdminForm", "VueProp", "IJSRuntime"
    };
    var violations = Directory.EnumerateFiles(sampleRoot, "*.*", SearchOption.AllDirectories)
        .Where(static path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
        .Where(static path => !IsGeneratedPath(path) && !IsVerificationScript(path))
        .SelectMany(path => File.ReadLines(path).Select((line, index) => (Path: path, Line: index + 1, Text: line)))
        .Where(item => forbidden.Any(token => item.Text.Contains(token, StringComparison.Ordinal)) ||
                       (!IsRouteHostFraming(item.Path) && item.Text.Contains("VueSlot", StringComparison.Ordinal)))
        .Select(item => Path.GetFileName(item.Path) + ":" + item.Line + ": " + item.Text.Trim())
        .ToArray();

    if (violations.Length > 0)
    {
        throw new InvalidOperationException(
            "The authoring sample must not depend on retired bridge/internal APIs:" + Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    RequireContains(File.ReadAllText(Path.Combine(sampleRoot, "TaskBoard.razor")), "<TForm FormData=\"TaskDraft\"", "typed TForm authoring");
    RequireContains(File.ReadAllText(Path.Combine(sampleRoot, "TaskBoard.razor")), "<TInput T=\"string\"", "typed TInput authoring");
    RequireContains(File.ReadAllText(Path.Combine(sampleRoot, "TaskBoard.razor")), "@bind-Value=\"Draft.Title\"", "Razor bind authoring");
    RequireContains(File.ReadAllText(Path.Combine(sampleRoot, "TaskTable.razor")), "<TPrimaryTable T=\"TaskRow\"", "typed primary-table authoring");
    RequireContains(File.ReadAllText(Path.Combine(sampleRoot, "TaskTable.razor")), "<CellEmptyContent Context=\"cell\">", "typed empty-cell slot authoring");
    RequireContains(File.ReadAllText(Path.Combine(sampleRoot, "TaskBoard.razor.cs")), "[Inject]", "writable injected NavigationManager");
    RequireContains(File.ReadAllText(Path.Combine(sampleRoot, "TaskTable.razor.cs")), "[CascadingParameter", "cascading parameter authoring");
    RequireContains(File.ReadAllText(Path.Combine(sampleRoot, "TaskBoard.razor")), "@layout AuthoringLayout", "standard Razor layout authoring");
    RequireContains(File.ReadAllText(Path.Combine(sampleRoot, "AuthoringLayout.razor")), "@Body", "LayoutComponentBase Body authoring");
    RequireContains(File.ReadAllText(Path.Combine(sampleRoot, "TaskDetails.razor")), "@page \"/tasks/{TaskId:int}\"", "typed route-parameter authoring");
    RequireContains(File.ReadAllText(Path.Combine(sampleRoot, "TaskDetails.razor.cs")), "[SupplyParameterFromQuery(Name = \"highlight\")]", "typed query-parameter authoring");
}

static void AssertDebugArtifacts(string root)
{
    RequireFile(root, "jazor-manifest.json", "debug Jazor manifest");
    RequireFile(root, "importmap.json", "debug import map");
    RequireFile(root, "manifest.json", "debug asset manifest");
    RequireFile(root, "app.mjs", "debug application entry");
    RequireFile(root, "app.mjs.map", "debug application source map");
    RequireFile(root, "components/task-board.mjs", "generated task-board module");
    RequireFile(root, "components/task-board.mjs.map", "task-board source map");
    RequireFile(root, "components/task-table.mjs", "generated task-table module");
    RequireFile(root, "components/task-table.mjs.map", "task-table source map");
    RequireFile(root, "components/authoring-layout.mjs", "generated authoring layout module");
    RequireFile(root, "components/authoring-layout.mjs.map", "authoring layout source map");
    RequireFile(root, "components/task-details.mjs", "generated task-details module");
    RequireFile(root, "components/task-details.mjs.map", "task-details source map");
    RequireFile(root, "@jazor/vue-runtime/routes.mjs", "generated route catalog");

    using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "jazor-manifest.json")));
    var modules = manifest.RootElement.GetProperty("modules").EnumerateArray().ToArray();
    RequireModule(modules, "RazorVue.Authoring.Bootstrap", "app.mjs");
    RequireModule(modules, "RazorVue.Authoring.AuthoringLayout", "components/authoring-layout.mjs");
    RequireModule(modules, "RazorVue.Authoring.TaskBoard", "components/task-board.mjs");
    RequireModule(modules, "RazorVue.Authoring.TaskDetails", "components/task-details.mjs");
    RequireModule(modules, "RazorVue.Authoring.TaskTable", "components/task-table.mjs");
    RequireModule(modules, "Jazor.Generated.RazorVue.RouteCatalog", "@jazor/vue-runtime/routes.mjs");

    var app = File.ReadAllText(Path.Combine(root, "app.mjs"));
    var board = File.ReadAllText(Path.Combine(root, "components", "task-board.mjs"));
    var table = File.ReadAllText(Path.Combine(root, "components", "task-table.mjs"));
    var layout = File.ReadAllText(Path.Combine(root, "components", "authoring-layout.mjs"));
    var details = File.ReadAllText(Path.Combine(root, "components", "task-details.mjs"));
    var routes = File.ReadAllText(Path.Combine(root, "@jazor", "vue-runtime", "routes.mjs"));
    RequireContains(app, "createNavigationHost", "route-host import in application entry");
    RequireContains(app, "h(layout, null, { Body:", "route layout Body-slot activation");
    RequireContains(app, "h(component, parameters)", "route component activation");
    RequireContains(board, "CascadingValue", "cascading provider lowering");
    RequireContains(board, "Button, Dialog, Form, FormItem, Input", "TDesign form/dialog imports");
    RequireContains(board, "onChange", "TInput bind lowering");
    RequireContains(table, "PrimaryTable", "typed table import");
    RequireContains(table, "cellEmptyContent", "typed table slot lowering");
    RequireContains(layout, "slots.Body()", "LayoutComponentBase Body slot lowering");
    RequireContains(details, "props.TaskId", "route parameter prop lowering");
    RequireContains(details, "props.Highlight", "query parameter prop lowering");
    RequireContains(routes, "template: \"/\"", "root route");
    RequireContains(routes, "template: \"/tasks\"", "tasks route");
    RequireContains(routes, "template: \"/tasks/{TaskId:int}\"", "detail route");
    RequireContains(routes, "{ name: \"TaskId\", prop: \"TaskId\", kind: \"number\" }", "typed route-parameter catalog metadata");
    RequireContains(routes, "{ name: \"highlight\", prop: \"Highlight\", kind: \"boolean\" }", "typed query-parameter catalog metadata");

    AssertSourceMap(Path.Combine(root, "app.mjs.map"), "Bootstrap.cs");
    AssertSourceMap(Path.Combine(root, "components", "authoring-layout.mjs.map"), "AuthoringLayout.razor");
    AssertSourceMap(Path.Combine(root, "components", "task-board.mjs.map"), "TaskBoard.razor");
    AssertSourceMap(Path.Combine(root, "components", "task-details.mjs.map"), "TaskDetails.razor");
    AssertSourceMap(Path.Combine(root, "components", "task-table.mjs.map"), "TaskTable.razor");
}

static void AssertReleaseArtifacts(string root)
{
    RequireFile(root, "bundle.js", "Release browser bundle");
    RequireFile(root, "bundle.js.map", "Release browser source map");
    RequireFile(root, "bundle.css", "Release TDesign stylesheet bundle");
    RequireDirectory(root, "vendor", "Release vendor closure");
    if (File.Exists(Path.Combine(root, "jazor-manifest.json")))
        throw new InvalidOperationException("Release artifacts must not retain the debug Jazor manifest.");
    if (File.Exists(Path.Combine(root, "app.mjs")))
        throw new InvalidOperationException("Release artifacts must not retain the debug application entry.");

    var bundle = File.ReadAllText(Path.Combine(root, "bundle.js"));
    RequireContains(bundle, "createNavigationHost", "route host in Release bundle");
    RequireContains(bundle, "RazorVueAuthoringRoot", "sample root in Release bundle");
    RequireContains(bundle, "cellEmptyContent", "typed table slot in Release bundle");
    RequireContains(bundle, "Task created from the typed form.", "async form result in Release bundle");
    RequireContains(bundle, "Route catalog layout", "layout content in Release bundle");
    RequireContains(bundle, "data-route-highlight", "query-parameter rendering in Release bundle");
    RequireDoesNotContain(bundle, "@jazor/vue-runtime/routes.mjs", "bare generated route-catalog import in Release bundle");
    RequireDoesNotContain(bundle, "BuildRenderTree", "render-tree implementation in Release bundle");
    RequireDoesNotContain(bundle, "AdminInput", "retired bridge in Release bundle");
    RequireDoesNotContain(bundle, "node_modules", "node_modules path in Release bundle");

    RequireAnyFile(root, "tdesign.mjs", "TDesign runtime module");
    RequireAnyFile(root, "blazor-routing.mjs", "routing runtime module");
    RequireAnyFile(root, "vue.runtime.esm-browser.prod.js", "production Vue runtime module");
    RequireAnyFile(root, "NavigationManagerModule.js", "NavigationManager runtime module");

    using var sourceMap = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "bundle.js.map")));
    var mappedSources = sourceMap.RootElement.GetProperty("sources")
        .EnumerateArray()
        .Select(static value => value.GetString() ?? string.Empty)
        .ToArray();
    foreach (var expected in new[] { "app.mjs", "components/authoring-layout.mjs", "components/task-board.mjs", "components/task-details.mjs", "components/task-table.mjs", "@jazor/vue-runtime/routes.mjs", "__jazor_runtime/blazor-routing.mjs" })
    {
        if (!mappedSources.Contains(expected, StringComparer.Ordinal))
            throw new InvalidOperationException("Release source map is missing " + expected + ".");
    }
}

static void AssertLocalPackages(string packageOutput)
{
    var required = new[] { "Jazor", "Jazor.Vue", "ECMAScript.TDesign", "ECMAScript.Style" };
    foreach (var packageId in required)
    {
        var package = new DirectoryInfo(packageOutput)
            .EnumerateFiles("*.nupkg", SearchOption.TopDirectoryOnly)
            .Where(static file => !file.Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
            .Where(file => string.Equals(ReadPackageId(file.FullName), packageId, StringComparison.Ordinal))
            .OrderByDescending(static file => file.LastWriteTimeUtc)
            .FirstOrDefault()
            ?? throw new FileNotFoundException("Local package was not produced: " + packageId);
        using var archive = ZipFile.OpenRead(package.FullName);
        if (archive.Entries.Count == 0)
            throw new InvalidOperationException("Local package is empty: " + package.FullName);
    }
}

static string? ReadPackageId(string packagePath)
{
    using var archive = ZipFile.OpenRead(packagePath);
    var nuspec = archive.Entries.FirstOrDefault(static entry =>
        entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
    if (nuspec is null)
        return null;

    try
    {
        using var stream = nuspec.Open();
        var document = XDocument.Load(stream);
        var metadata = document.Root?.Elements()
            .FirstOrDefault(static element => string.Equals(element.Name.LocalName, "metadata", StringComparison.Ordinal));
        return metadata?.Elements()
            .FirstOrDefault(static element => string.Equals(element.Name.LocalName, "id", StringComparison.Ordinal))
            ?.Value.Trim();
    }
    catch (XmlException)
    {
        return null;
    }
}

static void AssertNoNodeModules(string root)
{
    var nodeModules = Directory.Exists(root)
        ? Directory.EnumerateDirectories(root, "node_modules", SearchOption.AllDirectories).ToArray()
        : [];
    if (nodeModules.Length > 0)
        throw new InvalidOperationException("Sample verification must not materialize node_modules:" + Environment.NewLine + string.Join(Environment.NewLine, nodeModules));
}

static async Task VerifyReleaseHostInBrowserAsync(
    string repoRoot,
    string workRoot,
    string releaseJazorRoot,
    string configuration,
    string? requestedBrowserPath)
{
    var browserPath = ScriptHelpers.ResolveBrowserExecutable(requestedBrowserPath)
        ?? throw new FileNotFoundException("Microsoft Edge, Google Chrome, or Chromium is required for RazorVue.Authoring browser smoke.");
    var hostAssembly = Path.Combine(workRoot, "consumer-build-out", "RazorVue.Authoring", "bin", configuration, "net11.0", "RazorVue.Authoring.dll");
    RequireFile(Path.GetDirectoryName(hostAssembly)!, Path.GetFileName(hostAssembly), "isolated package-consumer host assembly");

    var browserRoot = Path.Combine(workRoot, "browser");
    ScriptHelpers.CleanDirectoryWithinRepo(browserRoot, repoRoot);
    Directory.CreateDirectory(browserRoot);
    var port = ScriptHelpers.GetAvailableLoopbackPort();
    var baseUri = new Uri($"http://127.0.0.1:{port}/authoring/");
    var hostLog = Path.Combine(browserRoot, "host.log");
    using var host = ScriptHelpers.StartHost(hostAssembly, releaseJazorRoot, port, hostLog);

    try
    {
        await ScriptHelpers.WaitForPageAsync(baseUri, TimeSpan.FromSeconds(45));
        var html = await new HttpClient().GetStringAsync(baseUri);
        RequireContains(html, "bundle.js", "Release entry selected by host shell");
        RequireContains(html, "/authoring/jazor/bundle.css", "PathBase-aware Release stylesheet");

        // A dump-dom invocation starts a new document every time, which cannot prove that
        // NavigationManager updates the live route host or preserves browser history. Keep one
        // CDP session for the complete SPA journey. / 单一 session 才能覆盖 query 刷新和 history。
        await using var browser = await ScriptHelpers.StartBrowserSessionAsync(browserPath, browserRoot, TimeSpan.FromSeconds(45));
        await browser.NavigateAsync(baseUri, TimeSpan.FromSeconds(45));
        await browser.WaitUntilAsync(
            "location.pathname === '/authoring/' && document.querySelector('[data-authoring-page=\"task-board\"]') !== null",
            "mounted task board",
            TimeSpan.FromSeconds(20));

        var board = await browser.ReadDocumentHtmlAsync();
        RequireContains(board, "data-authoring-layout=\"task-shell\"", "mounted authoring layout");
        RequireContains(board, "data-authoring-page=\"task-board\"", "mounted authoring page");
        RequireContains(board, "Task board", "mounted task-board heading");
        RequireContains(board, "Review the generated module", "mounted typed table row");
        RequireContains(board, "RazorVue workspace", "mounted cascading value");

        await browser.ClickAsync("[data-route-action=\"open-task-details\"]");
        await browser.WaitUntilAsync(
            "location.pathname === '/tasks/2' && location.search === '?highlight=true' && document.querySelector('[data-route-highlight=\"highlighted\"]') !== null",
            "highlighted detail route after the board click",
            TimeSpan.FromSeconds(20));

        var details = await browser.ReadDocumentHtmlAsync();
        RequireContains(details, "data-authoring-layout=\"task-shell\"", "detail route layout");
        RequireContains(details, "data-authoring-page=\"task-details\"", "detail route page");
        RequireContains(details, "Task 2", "typed route parameter rendered by detail route");
        RequireContains(details, "Query highlight: highlighted", "typed query parameter rendered by detail route");
        RequireContains(details, "data-route-parameter=\"2\"", "numeric route parameter attribute");

        // This is the proxy-identity regression path: a nested page receives a reactive
        // NavigationManager, then its imported CLR call must still refresh the route host.
        await browser.ClickAsync("[data-route-action=\"clear-highlight\"]");
        await browser.WaitUntilAsync(
            "location.pathname === '/tasks/2' && location.search === '?highlight=false' && document.querySelector('[data-route-highlight=\"standard\"]') !== null",
            "query refresh after Clear highlight",
            TimeSpan.FromSeconds(20));
        var clearedDetail = await browser.ReadDocumentHtmlAsync();
        RequireContains(clearedDetail, "Query highlight: standard", "cleared query state");

        await browser.ClickAsync("[data-route-action=\"back-to-board\"]");
        await browser.WaitUntilAsync(
            "location.pathname === '/tasks' && document.querySelector('[data-authoring-page=\"task-board\"]') !== null",
            "board route after Back to task board",
            TimeSpan.FromSeconds(20));

        await browser.GoBackAsync();
        await browser.WaitUntilAsync(
            "location.pathname === '/tasks/2' && location.search === '?highlight=false' && document.querySelector('[data-route-highlight=\"standard\"]') !== null",
            "standard detail state after browser back",
            TimeSpan.FromSeconds(20));

        await browser.GoBackAsync();
        await browser.WaitUntilAsync(
            "location.pathname === '/tasks/2' && location.search === '?highlight=true' && document.querySelector('[data-route-highlight=\"highlighted\"]') !== null",
            "highlighted detail state after browser back",
            TimeSpan.FromSeconds(20));

        await browser.GoBackAsync();
        await browser.WaitUntilAsync(
            "location.pathname === '/authoring/' && document.querySelector('[data-authoring-page=\"task-board\"]') !== null",
            "root board state after browser back",
            TimeSpan.FromSeconds(20));

        await browser.NavigateAsync(new Uri(baseUri, "missing-route"), TimeSpan.FromSeconds(20));
        await browser.WaitUntilAsync(
            "location.pathname === '/authoring/missing-route' && document.querySelector('.authoring-not-found') !== null",
            "not-found route",
            TimeSpan.FromSeconds(20));
        var missing = await browser.ReadDocumentHtmlAsync();
        RequireContains(missing, "authoring-not-found", "application-owned not-found branch");
        RequireContains(missing, "Page not found", "not-found message");
        await browser.AssertNoErrorsAsync();
    }
    finally
    {
        await ScriptHelpers.StopProcessAsync(host);
    }
}

static void AssertSourceMap(string path, string expectedSource)
{
    using var sourceMap = JsonDocument.Parse(File.ReadAllText(path));
    var sources = sourceMap.RootElement.GetProperty("sources")
        .EnumerateArray()
        .Select(static source => source.GetString() ?? string.Empty)
        .ToArray();
    if (!sources.Any(source => source.Contains(expectedSource, StringComparison.OrdinalIgnoreCase)))
        throw new InvalidOperationException("Source map is missing " + expectedSource + ": " + path);
}

static void RequireModule(JsonElement[] modules, string id, string path)
{
    if (!modules.Any(module =>
            string.Equals(module.GetProperty("id").GetString(), id, StringComparison.Ordinal) &&
            string.Equals(module.GetProperty("path").GetString(), path, StringComparison.Ordinal)))
        throw new InvalidOperationException("Debug manifest is missing module '" + id + "' at '" + path + "'.");
}

static void RequireFile(string root, string relativePath, string description)
{
    var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
    if (!File.Exists(path))
        throw new FileNotFoundException("Missing " + description + ": " + path);
}

static void RequireDirectory(string root, string relativePath, string description)
{
    var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
    if (!Directory.Exists(path))
        throw new DirectoryNotFoundException("Missing " + description + ": " + path);
}

static void RequireAnyFile(string root, string fileName, string description)
{
    if (!Directory.EnumerateFiles(root, fileName, SearchOption.AllDirectories).Any())
        throw new FileNotFoundException("Missing " + description + " beneath " + root);
}

static void RequireContains(string text, string expected, string description)
{
    if (!text.Contains(expected, StringComparison.Ordinal))
        throw new InvalidOperationException("Missing " + description + ": expected '" + expected + "'.");
}

static void RequireDoesNotContain(string text, string unexpected, string description)
{
    if (text.Contains(unexpected, StringComparison.Ordinal))
        throw new InvalidOperationException("Unexpected " + description + ": '" + unexpected + "'.");
}

static bool IsVerificationScript(string path)
    => string.Equals(Path.GetFileName(path), "build-local.cs", StringComparison.OrdinalIgnoreCase) ||
       string.Equals(Path.GetFileName(path), "verify-smoke.cs", StringComparison.OrdinalIgnoreCase);

static bool IsRouteHostFraming(string path)
    => string.Equals(Path.GetFileName(path), "Bootstrap.cs", StringComparison.OrdinalIgnoreCase);

static bool IsGeneratedPath(string path)
    => path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
       path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
       path.Contains(Path.DirectorySeparatorChar + "jazor" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

internal sealed record SmokeOptions(
    string Configuration,
    bool SkipBuild,
    bool SkipBrowser,
    string? WorkRoot,
    string? PackageOutput,
    string? BrowserPath)
{
    public static SmokeOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Release";
        var skipBuild = false;
        var skipBrowser = false;
        string? workRoot = null;
        string? packageOutput = null;
        string? browserPath = null;

        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            switch (argument)
            {
                case "--configuration":
                case "-Configuration":
                case "-c":
                    configuration = RequireValue(arguments, ref index, argument);
                    break;
                case "--skip-build":
                    skipBuild = true;
                    break;
                case "--skip-browser":
                    skipBrowser = true;
                    break;
                case "--work-root":
                    workRoot = RequireValue(arguments, ref index, argument);
                    break;
                case "--package-output":
                    packageOutput = RequireValue(arguments, ref index, argument);
                    break;
                case "--browser-path":
                    browserPath = RequireValue(arguments, ref index, argument);
                    break;
                case "--help":
                case "-h":
                    WriteUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported argument: " + argument);
            }
        }

        return new SmokeOptions(configuration, skipBuild, skipBrowser, workRoot, packageOutput, browserPath);
    }

    private static string RequireValue(IReadOnlyList<string> arguments, ref int index, string option)
    {
        var next = index + 1;
        if (next >= arguments.Count)
            throw new InvalidOperationException("Missing value for " + option + ".");
        index = next;
        return arguments[index];
    }

    private static void WriteUsage()
    {
        Console.WriteLine("Usage: dotnet run --file samples/RazorVue.Authoring/verify-smoke.cs -- [options]");
        Console.WriteLine("  -c|--configuration <Debug|Release> (default Release)");
        Console.WriteLine("  --skip-build       inspect an existing isolated build root");
        Console.WriteLine("  --skip-browser     skip the actual browser mount lane");
        Console.WriteLine("  --work-root <path>");
        Console.WriteLine("  --package-output <path>");
        Console.WriteLine("  --browser-path <path>");
    }
}

internal static class ScriptHelpers
{
    public static string FindRepositoryRoot(string startDirectory)
    {
        var current = new DirectoryInfo(Path.GetFullPath(startDirectory));
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
                return current.FullName;
            current = current.Parent;
        }

        throw new InvalidOperationException("Cannot locate repository root (Jazor.slnx).");
    }

    public static string ResolvePath(string repoRoot, string path)
        => Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));

    public static void EnsureInsideRepository(string repoRoot, params string[] paths)
    {
        var fullRoot = Path.GetFullPath(repoRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var rootPrefix = fullRoot + Path.DirectorySeparatorChar;
        foreach (var path in paths)
        {
            var full = Path.GetFullPath(path);
            if (!full.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), fullRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("A smoke path must stay inside the repository: " + full);
        }
    }

    public static void CleanDirectoryWithinRepo(string path, string repoRoot)
    {
        EnsureInsideRepository(repoRoot, path);
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    public static async Task RunProcessAsync(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);
        startInfo.Environment["DOTNET_CLI_HOME"] = Path.Combine(FindRepositoryRoot(workingDirectory), ".dotnet");
        startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        startInfo.Environment["UseSharedCompilation"] = "false";

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start " + fileName + ".");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var output = await stdout;
        var error = await stderr;
        if (!string.IsNullOrWhiteSpace(output))
            Console.Write(output);
        if (!string.IsNullOrWhiteSpace(error))
            Console.Error.Write(error);
        if (process.ExitCode != 0)
            throw new InvalidOperationException(fileName + " exited with code " + process.ExitCode + ".");
    }

    public static Process StartHost(string hostAssembly, string artifactRoot, int port, string logPath)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = Path.GetDirectoryName(hostAssembly)!,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var argument in new[]
        {
            hostAssembly,
            "--urls", "http://127.0.0.1:" + port,
            "--Authoring:JazorRoot=" + artifactRoot,
            "--Authoring:PathBase=/authoring"
        })
            startInfo.ArgumentList.Add(argument);

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start the isolated package-consumer host.");
        _ = CopyToLogAsync(process.StandardOutput, logPath + ".stdout");
        _ = CopyToLogAsync(process.StandardError, logPath + ".stderr");
        return process;
    }

    public static async Task WaitForPageAsync(Uri pageUri, TimeSpan timeout)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        var deadline = DateTimeOffset.UtcNow + timeout;
        Exception? lastFailure = null;
        while (DateTimeOffset.UtcNow < deadline)
        {
            try
            {
                using var response = await client.GetAsync(pageUri);
                if (response.StatusCode == HttpStatusCode.OK)
                    return;
            }
            catch (Exception error)
            {
                lastFailure = error;
            }

            await Task.Delay(150);
        }

        throw new TimeoutException("Timed out waiting for the sample host at " + pageUri + ". " + lastFailure?.Message);
    }

    public static async Task StopProcessAsync(Process process)
    {
        if (process.HasExited)
            return;
        try
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
        }
        catch (InvalidOperationException)
        {
        }
    }

    public static int GetAvailableLoopbackPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    public static string? ResolveBrowserExecutable(string? requestedPath)
    {
        if (!string.IsNullOrWhiteSpace(requestedPath))
            return File.Exists(requestedPath) ? requestedPath : null;

        var environmentPath = Environment.GetEnvironmentVariable("RAZORVUE_BROWSER_EXE")?.Trim();
        if (string.IsNullOrWhiteSpace(environmentPath))
            environmentPath = Environment.GetEnvironmentVariable("RAZORVUE_BROWSER_PATH")?.Trim();
        if (!string.IsNullOrWhiteSpace(environmentPath))
            return File.Exists(environmentPath) ? environmentPath : null;

        var candidates = OperatingSystem.IsWindows()
            ? new[]
            {
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                "msedge.exe",
                "chrome.exe"
            }
            : new[] { "microsoft-edge", "google-chrome", "chromium", "chromium-browser" };
        return candidates.Select(ResolveExecutable).FirstOrDefault(static path => path is not null);
    }

    public static async Task<BrowserSession> StartBrowserSessionAsync(string browserPath, string root, TimeSpan timeout)
    {
        var profileRoot = Path.Combine(root, "profile-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(profileRoot);
        Process? process = null;
        try
        {
            var cdpPort = GetAvailableLoopbackPort();
            var startInfo = new ProcessStartInfo(browserPath)
            {
                WorkingDirectory = root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            foreach (var argument in new[]
            {
                "--headless=new",
                "--disable-gpu",
                "--disable-dev-shm-usage",
                "--no-first-run",
                "--no-default-browser-check",
                "--no-sandbox",
                "--run-all-compositor-stages-before-draw",
                "--remote-debugging-port=" + cdpPort,
                "--user-data-dir=" + profileRoot,
                "about:blank"
            })
                startInfo.ArgumentList.Add(argument);

            process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Could not start browser smoke process.");
            _ = CopyToLogAsync(process.StandardOutput, Path.Combine(root, "browser.stdout"));
            _ = CopyToLogAsync(process.StandardError, Path.Combine(root, "browser.stderr"));
            var endpoint = await WaitForCdpEndpointAsync(cdpPort, process, timeout);
            return await BrowserSession.ConnectAsync(process, profileRoot, endpoint, timeout);
        }
        catch
        {
            if (process is not null)
                await StopProcessAsync(process);
            DeleteDirectoryBestEffort(profileRoot);
            throw;
        }
    }

    private static async Task<Uri> WaitForCdpEndpointAsync(int port, Process process, TimeSpan timeout)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTimeOffset.UtcNow + timeout;
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (process.HasExited)
                throw new InvalidOperationException("Browser exited before its CDP endpoint became ready.");

            try
            {
                using var response = await client.GetAsync("http://127.0.0.1:" + port + "/json/list");
                if (response.IsSuccessStatusCode)
                {
                    using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    foreach (var target in document.RootElement.EnumerateArray())
                    {
                        if (!target.TryGetProperty("type", out var type) ||
                            !string.Equals(type.GetString(), "page", StringComparison.Ordinal) ||
                            !target.TryGetProperty("webSocketDebuggerUrl", out var endpoint))
                            continue;

                        var endpointText = endpoint.GetString();
                        if (!string.IsNullOrWhiteSpace(endpointText))
                            return new Uri(endpointText);
                    }
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(150);
        }

        throw new TimeoutException("Timed out waiting for browser CDP endpoint on port " + port + ".");
    }

    internal static void DeleteDirectoryBestEffort(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static string? ResolveExecutable(string candidate)
    {
        if (candidate.Contains(':', StringComparison.Ordinal) || candidate.Contains(Path.DirectorySeparatorChar, StringComparison.Ordinal))
            return File.Exists(candidate) ? candidate : null;

        var extensions = OperatingSystem.IsWindows()
            ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT").Split(';', StringSplitOptions.RemoveEmptyEntries)
            : [string.Empty];
        foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(directory))
                continue;
            foreach (var extension in extensions)
            {
                var path = Path.Combine(directory, candidate.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? candidate : candidate + extension);
                if (File.Exists(path))
                    return path;
            }
        }

        return null;
    }

    private static async Task CopyToLogAsync(StreamReader reader, string path)
    {
        await using var writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read));
        while (await reader.ReadLineAsync() is { } line)
            await writer.WriteLineAsync(line);
    }
}

internal sealed class BrowserSession : IAsyncDisposable
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(15);

    private readonly Process process;
    private readonly string profileRoot;
    private readonly ClientWebSocket socket;
    private readonly List<string> errors = [];
    private int nextCommandId = 1;
    private bool disposed;

    private BrowserSession(Process process, string profileRoot, ClientWebSocket socket)
    {
        this.process = process;
        this.profileRoot = profileRoot;
        this.socket = socket;
    }

    public static async Task<BrowserSession> ConnectAsync(Process process, string profileRoot, Uri endpoint, TimeSpan timeout)
    {
        var socket = new ClientWebSocket();
        try
        {
            using var timeoutSource = new CancellationTokenSource(timeout);
            await socket.ConnectAsync(endpoint, timeoutSource.Token);

            var session = new BrowserSession(process, profileRoot, socket);
            await session.SendAsync("Runtime.enable");
            await session.SendAsync("Page.enable");
            await session.SendAsync("Log.enable");
            await session.SendAsync("Network.enable");
            return session;
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    public async Task NavigateAsync(Uri pageUri, TimeSpan timeout)
    {
        await SendAsync("Page.navigate", "{\"url\":" + JsonString(pageUri.AbsoluteUri) + "}");
        await WaitUntilAsync("document.readyState === 'complete'", "completed browser document", timeout);
    }

    public async Task ClickAsync(string selector)
    {
        var selectorLiteral = JsonString(selector);
        var clicked = await EvaluateAsync(
            "(() => { const element = document.querySelector(" + selectorLiteral + "); if (!element) return false; element.click(); return true; })()");
        if (clicked is not { ValueKind: JsonValueKind.True })
            throw new InvalidOperationException("Browser could not click selector: " + selector);
    }

    public async Task GoBackAsync()
        => await EvaluateAsync("history.back(); true");

    public async Task WaitUntilAsync(string expression, string description, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        Exception? lastFailure = null;
        while (DateTimeOffset.UtcNow < deadline)
        {
            try
            {
                var value = await EvaluateAsync(expression);
                if (value is { ValueKind: JsonValueKind.True })
                    return;
            }
            catch (Exception error) when (error is InvalidOperationException or OperationCanceledException or WebSocketException)
            {
                // A history traversal can briefly destroy the old execution context. Retry until
                // the destination route has mounted, while retaining the last failure for triage.
                lastFailure = error;
            }

            await Task.Delay(100);
        }

        var location = await TryReadLocationAsync();
        throw new TimeoutException(
            "Timed out waiting for " + description + " at " + location + "." +
            (lastFailure is null ? string.Empty : " Last CDP failure: " + lastFailure.Message));
    }

    public async Task<string> ReadDocumentHtmlAsync()
    {
        var value = await EvaluateAsync("document.documentElement.outerHTML");
        if (value is not { ValueKind: JsonValueKind.String })
            throw new InvalidOperationException("Browser did not return the document HTML.");
        return value.Value.GetString() ?? string.Empty;
    }

    public async Task AssertNoErrorsAsync()
    {
        // The command response is ordered after preceding CDP events, so this drains console and
        // exception notifications before asserting the journey result.
        await EvaluateAsync("0");
        if (errors.Count == 0)
            return;

        throw new InvalidOperationException("Browser runtime diagnostics:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
            return;
        disposed = true;

        socket.Abort();
        socket.Dispose();
        await ScriptHelpers.StopProcessAsync(process);
        ScriptHelpers.DeleteDirectoryBestEffort(profileRoot);
    }

    private async Task<string> TryReadLocationAsync()
    {
        try
        {
            var value = await EvaluateAsync("location.href");
            return value is { ValueKind: JsonValueKind.String }
                ? value.Value.GetString() ?? "unknown location"
                : "unknown location";
        }
        catch
        {
            return "unknown location";
        }
    }

    private async Task<JsonElement?> EvaluateAsync(string expression)
    {
        var response = await SendAsync(
            "Runtime.evaluate",
            "{\"expression\":" + JsonString(expression) + ",\"returnByValue\":true,\"awaitPromise\":true}");
        if (response.TryGetProperty("exceptionDetails", out var exceptionDetails))
            throw new InvalidOperationException("Runtime.evaluate failed: " + DescribeException(exceptionDetails));

        if (!response.TryGetProperty("result", out var result) || !result.TryGetProperty("value", out var value))
            return null;
        return value.Clone();
    }

    private async Task<JsonElement> SendAsync(string method, string? parameters = null, CancellationToken cancellationToken = default)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(CommandTimeout);

        var id = nextCommandId++;
        var command = "{\"id\":" + id.ToString(CultureInfo.InvariantCulture) +
            ",\"method\":" + JsonString(method) +
            ",\"params\":" + (parameters ?? "{}") + "}";
        var payload = Encoding.UTF8.GetBytes(command);
        await socket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, timeoutSource.Token);

        while (true)
        {
            using var document = JsonDocument.Parse(await ReceiveMessageAsync(timeoutSource.Token));
            var message = document.RootElement;
            if (!message.TryGetProperty("id", out var responseId))
            {
                RecordEvent(message);
                continue;
            }

            if (responseId.GetInt32() != id)
                throw new InvalidOperationException("Received an unexpected CDP response while waiting for " + method + ".");
            if (message.TryGetProperty("error", out var error))
            {
                var description = error.TryGetProperty("message", out var errorMessage)
                    ? errorMessage.GetString()
                    : error.GetRawText();
                throw new InvalidOperationException("CDP " + method + " failed: " + description);
            }

            return message.TryGetProperty("result", out var result) ? result.Clone() : default;
        }
    }

    private async Task<string> ReceiveMessageAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        await using var message = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
                throw new WebSocketException("CDP websocket closed before the response arrived.");
            await message.WriteAsync(buffer.AsMemory(0, result.Count), cancellationToken);
        }
        while (!result.EndOfMessage);

        return Encoding.UTF8.GetString(message.GetBuffer(), 0, checked((int)message.Length));
    }

    private void RecordEvent(JsonElement message)
    {
        if (!message.TryGetProperty("method", out var methodProperty) ||
            !message.TryGetProperty("params", out var parameters))
            return;

        switch (methodProperty.GetString())
        {
            case "Runtime.consoleAPICalled":
            {
                var type = parameters.TryGetProperty("type", out var typeProperty) ? typeProperty.GetString() : null;
                if (!string.Equals(type, "error", StringComparison.Ordinal) &&
                    !string.Equals(type, "assert", StringComparison.Ordinal))
                    return;

                var arguments = parameters.TryGetProperty("args", out var values)
                    ? string.Join(" ", values.EnumerateArray().Select(DescribeRemoteValue))
                    : string.Empty;
                errors.Add("console." + type + ": " + arguments);
                return;
            }
            case "Runtime.exceptionThrown":
                if (parameters.TryGetProperty("exceptionDetails", out var exceptionDetails))
                    errors.Add("runtime exception: " + DescribeException(exceptionDetails));
                return;
            case "Log.entryAdded":
                if (!parameters.TryGetProperty("entry", out var entry) ||
                    !entry.TryGetProperty("level", out var level) ||
                    !string.Equals(level.GetString(), "error", StringComparison.Ordinal))
                    return;

                var url = entry.TryGetProperty("url", out var urlProperty) ? urlProperty.GetString() ?? string.Empty : string.Empty;
                var text = entry.TryGetProperty("text", out var textProperty) ? textProperty.GetString() ?? string.Empty : string.Empty;
                // The host does not ship a favicon; Chromium's automatic probe is harness noise,
                // not a bundle/runtime failure. All other Log-level errors remain blocking.
                if (!url.EndsWith("/favicon.ico", StringComparison.OrdinalIgnoreCase) &&
                    !text.Contains("favicon.ico", StringComparison.OrdinalIgnoreCase))
                    errors.Add("browser log: " + text + " " + url);
                return;
            case "Network.loadingFailed":
                if (!parameters.TryGetProperty("canceled", out var canceled) || canceled.ValueKind != JsonValueKind.True)
                {
                    var errorText = parameters.TryGetProperty("errorText", out var error) ? error.GetString() ?? string.Empty : string.Empty;
                    errors.Add("network failed: " + errorText);
                }
                return;
        }
    }

    private static string DescribeException(JsonElement details)
    {
        if (details.TryGetProperty("exception", out var exception) &&
            exception.TryGetProperty("description", out var description))
            return description.GetString() ?? "Unknown exception";
        return details.TryGetProperty("text", out var text)
            ? text.GetString() ?? "Unknown exception"
            : details.GetRawText();
    }

    private static string DescribeRemoteValue(JsonElement value)
    {
        if (value.TryGetProperty("value", out var literal))
            return literal.ValueKind == JsonValueKind.String ? literal.GetString() ?? string.Empty : literal.GetRawText();
        if (value.TryGetProperty("description", out var description))
            return description.GetString() ?? string.Empty;
        return value.TryGetProperty("type", out var type) ? type.GetString() ?? string.Empty : string.Empty;
    }

    private static string JsonString(string value)
        => "\"" + JsonEncodedText.Encode(value).ToString() + "\"";
}
