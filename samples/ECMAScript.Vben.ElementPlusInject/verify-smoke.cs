using System.Diagnostics;
using System.Globalization;

var options = SmokeOptions.Parse(args);
var repoRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
var sampleRoot = Path.Combine(repoRoot, "samples", "ECMAScript.Vben.ElementPlusInject");
var consumerRoot = Path.Combine(sampleRoot, "Vben.ElementPlusInject.Host", "consumer");
var hostRoot = Path.Combine(sampleRoot, "Vben.ElementPlusInject.Host");
var packageOutput = Path.Combine(repoRoot, ".tmp", "nupkg-sample");
var buildScript = Path.Combine(sampleRoot, "build-local.cs");
var effectiveBaseOutputPath = options.BaseOutputPath ?? Path.Combine(repoRoot, ".tmp", "vben-sample-smoke-out");
var effectiveBaseIntermediateOutputPath = options.BaseIntermediateOutputPath ?? Path.Combine(repoRoot, ".tmp", "vben-sample-smoke-obj");
var generatedOutputRoot = string.IsNullOrWhiteSpace(options.GeneratedOutputRoot)
    ? Path.Combine(repoRoot, ".tmp", "sample-smoke", "ECMAScript.Vben.ElementPlusInject", options.Configuration, "jazor")
    : ResolvePath(options.GeneratedOutputRoot, repoRoot);
var hostBrowserOutputRoot = string.IsNullOrWhiteSpace(options.BrowserOutputRoot)
    ? Path.Combine(repoRoot, ".tmp", "sample-smoke", "ECMAScript.Vben.ElementPlusInject", options.Configuration, "wwwroot", "jazor")
    : ResolvePath(options.BrowserOutputRoot, repoRoot);

SetCommonEnvironment(repoRoot);

if (!options.FrontendOnly)
{
    CleanDirectory(packageOutput, repoRoot);
    CleanDirectory(generatedOutputRoot, repoRoot);
    CleanDirectory(hostBrowserOutputRoot, repoRoot);

    RunDotNet(repoRoot, [
        "run",
        "--file",
        buildScript,
        "--",
        "--configuration", options.Configuration,
        "--jazor-out-dir", generatedOutputRoot,
        "--base-output-path", effectiveBaseOutputPath,
        "--base-intermediate-output-path", effectiveBaseIntermediateOutputPath
    ]);
}

AssertPathExists(ResolveHostAssemblyPath(hostRoot, options.Configuration, effectiveBaseOutputPath), "sample host assembly for requested configuration");
AssertGeneratedHostArtifacts(generatedOutputRoot);

var hostRequirementsModulePath = Path.Combine(generatedOutputRoot, "__jazor", "razorvue-host.mjs");
AssertPathExists(hostRequirementsModulePath, "generated RazorVue host requirements module");
AssertContains(File.ReadAllText(hostRequirementsModulePath), "\"element-plus\"", "Element Plus plugin requirement in host requirements");
AssertContains(File.ReadAllText(hostRequirementsModulePath), "\"element-plus/dist/index.css\"", "Element Plus style requirement in host requirements");

var denoExePath = ResolveDenoExecutable(repoRoot, options);
var denoEnvironment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["RAZORVUE_HOST_JAZOR_ROOT"] = generatedOutputRoot,
    ["RAZORVUE_HOST_WWWROOT_ROOT"] = Path.GetDirectoryName(hostBrowserOutputRoot) ?? hostBrowserOutputRoot
};

RunDeno(denoExePath, consumerRoot, denoEnvironment, ["task", "smoke:ssr"]);
RunDeno(denoExePath, consumerRoot, denoEnvironment, ["task", "smoke:bundle-api"]);
RunDeno(denoExePath, consumerRoot, denoEnvironment, ["task", "build"]);
RunDeno(denoExePath, consumerRoot, denoEnvironment, ["task", "smoke:browser"]);

AssertPathExists(Path.Combine(hostBrowserOutputRoot, "client-entry.js"), "browser bundle entry copied to host browser root");
AssertPathExists(Path.Combine(hostBrowserOutputRoot, "client-entry.css"), "browser bundle css copied to host browser root");

Console.WriteLine("ECMAScript.Vben.ElementPlusInject sample smoke verification passed.");
Console.WriteLine("Verified: local Jazor/ECMAScript.Vben/ECMAScript.ElementPlus package pack, injected shell host rebuild, Deno SSR smoke, Deno bundle API smoke, browser build, and browser smoke on the supported Deno-only consumer path.");

static string FindRepositoryRoot(string startDirectory)
{
    var current = new DirectoryInfo(Path.GetFullPath(startDirectory));
    while (current is not null)
    {
        if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
        {
            return current.FullName;
        }

        current = current.Parent;
    }

    throw new InvalidOperationException("Cannot locate repository root (Jazor.slnx).");
}

static string ResolvePath(string path, string repoRoot)
{
    return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));
}

static void SetCommonEnvironment(string repoRoot)
{
    Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", Path.Combine(repoRoot, ".dotnet"));
    Environment.SetEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1");
}

static string ResolveHostAssemblyPath(string hostRoot, string configuration, string baseOutputPath)
{
    var isolatedOutputRoot = GetIsolatedBuildRoot(baseOutputPath);
    return Path.Combine(isolatedOutputRoot, "Vben.ElementPlusInject.Host", "bin", configuration, "net11.0", "Vben.ElementPlusInject.Host.dll");
}

static string ResolveDenoExecutable(string repoRoot, SmokeOptions options)
{
    var explicitDenoExePath = Environment.GetEnvironmentVariable("JAZOR_DENO_EXE");
    if (!string.IsNullOrWhiteSpace(explicitDenoExePath))
    {
        var fullPath = Path.GetFullPath(explicitDenoExePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Explicit JAZOR_DENO_EXE path does not exist: {fullPath}");
        }

        return fullPath;
    }

    var candidatePaths = new List<string>();
    if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
    {
        var isolatedOutputRoot = GetIsolatedBuildRoot(options.BaseOutputPath!);
        AddDenoRuntimeCandidates(candidatePaths, Path.Combine(isolatedOutputRoot, "Jazor.Emit", "bin", options.Configuration, "net11.0"));
    }

    AddDenoRuntimeCandidates(candidatePaths, Path.Combine(repoRoot, "src", "Jolt", "bin", "Debug", "net11.0"));
    AddDenoRuntimeCandidates(candidatePaths, Path.Combine(repoRoot, "src", "Jolt", "bin", "Release", "net11.0"));
    AddDenoRuntimeCandidates(candidatePaths, Path.Combine(repoRoot, "src", "Jazor.Emit", "bin", "Debug", "net11.0"));
    AddDenoRuntimeCandidates(candidatePaths, Path.Combine(repoRoot, "src", "Jazor.Emit", "bin", "Release", "net11.0"));

    var denoHostPackageRoot = Path.Combine(repoRoot, ".dotnet", ".nuget", "packages", "denohost.runtime.win-x64");
    if (Directory.Exists(denoHostPackageRoot))
    {
        var cachedDenoRuntime = Directory
            .EnumerateFiles(denoHostPackageRoot, "deno.exe", SearchOption.AllDirectories)
            .OrderByDescending(static path => path, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        if (cachedDenoRuntime is not null)
        {
            candidatePaths.Add(cachedDenoRuntime);
        }
    }

    var denoExePath = candidatePaths.FirstOrDefault(File.Exists);
    if (denoExePath is null)
    {
        throw new FileNotFoundException("Bundled Deno runtime was not found. Build Jolt or Jazor.Emit first so DenoHost runtime assets exist.");
    }

    return denoExePath;
}

static void AddDenoRuntimeCandidates(ICollection<string> candidates, string baseDirectory)
{
    candidates.Add(Path.Combine(baseDirectory, "runtimes", "win-x64", "native", "deno.exe"));
    candidates.Add(Path.Combine(baseDirectory, "publish", "runtimes", "win-x64", "native", "deno.exe"));
}

static void AssertGeneratedHostArtifacts(string generatedOutputRoot)
{
    var rootComponentPath = Path.Combine(generatedOutputRoot, "components", "vben-dashboard-app.vue");
    var adminLayoutPath = Path.Combine(generatedOutputRoot, "components", "element-admin-layout.vue");
    var sidebarMenuPath = Path.Combine(generatedOutputRoot, "components", "element-sidebar-menu.vue");
    var headerBarPath = Path.Combine(generatedOutputRoot, "components", "element-header-bar.vue");
    var pageContainerPath = Path.Combine(generatedOutputRoot, "components", "element-page-container.vue");
    var manifestPath = Path.Combine(generatedOutputRoot, "jazor-manifest.json");

    AssertPathExists(rootComponentPath, "generated root dashboard component");
    AssertPathExists(adminLayoutPath, "generated injected admin layout component");
    AssertPathExists(sidebarMenuPath, "generated injected sidebar component");
    AssertPathExists(headerBarPath, "generated injected header component");
    AssertPathExists(pageContainerPath, "generated injected page container component");
    AssertPathExists(manifestPath, "generated manifest");

    var rootComponent = File.ReadAllText(rootComponentPath);
    var adminLayoutComponent = File.ReadAllText(adminLayoutPath);
    var manifest = File.ReadAllText(manifestPath);

    AssertContains(rootComponent, "<VbenAdminLayout", "root Vben shell authoring in dashboard SFC");
    AssertContains(adminLayoutComponent, "vben-ep-shell", "Element Plus injected layout markup");
    AssertContains(manifest, "\"Vben.ElementPlusInject.Library.VbenDashboardApp\"", "dashboard component manifest id");
    AssertContains(manifest, "\"element-plus/dist/index.css\"", "Element Plus style manifest requirement");
    AssertContains(manifest, "\"element-plus\"", "Element Plus plugin manifest requirement");
}

static void AssertPathExists(string path, string description)
{
    if (!File.Exists(path) && !Directory.Exists(path))
    {
        throw new FileNotFoundException($"Missing {description}: {path}");
    }
}

static void AssertContains(string text, string snippet, string description)
{
    if (!text.Contains(snippet, StringComparison.Ordinal))
    {
        throw new InvalidOperationException($"Missing {description}: expected to find '{snippet}'.");
    }
}

static void CleanDirectory(string path, string repoRoot)
{
    var fullPath = Path.GetFullPath(path);
    var fullRoot = Path.GetFullPath(repoRoot);
    if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Refusing to delete a path outside the repository root: {fullPath}");
    }

    if (Directory.Exists(fullPath))
    {
        Directory.Delete(fullPath, recursive: true);
    }
}

static string GetIsolatedBuildRoot(string path)
{
    var fullPath = Path.GetFullPath(path);
    return fullPath.EndsWith(Path.DirectorySeparatorChar)
        ? fullPath
        : fullPath + Path.DirectorySeparatorChar;
}

static void RunDotNet(string workdir, IReadOnlyList<string> arguments)
{
    var environment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1",
        ["MSBUILDDISABLENODEREUSE"] = "1",
        ["UseSharedCompilation"] = "false"
    };

    var dotnetCliHome = Environment.GetEnvironmentVariable("DOTNET_CLI_HOME");
    if (!string.IsNullOrWhiteSpace(dotnetCliHome))
    {
        environment["DOTNET_CLI_HOME"] = dotnetCliHome;
    }

    using var process = StartProcess(
        "dotnet",
        arguments,
        workdir,
        environment);

    process.WaitForExit();
    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException($"Process failed with exit code {process.ExitCode}: dotnet {string.Join(' ', arguments)}");
    }
}

static void RunDeno(string denoExePath, string workdir, IReadOnlyDictionary<string, string> environment, IReadOnlyList<string> arguments)
{
    using var process = StartProcess(denoExePath, arguments, workdir, environment);
    process.WaitForExit();
    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException($"Process failed with exit code {process.ExitCode}: {denoExePath} {string.Join(' ', arguments)}");
    }
}

static Process StartProcess(
    string fileName,
    IReadOnlyList<string> arguments,
    string workdir,
    IReadOnlyDictionary<string, string>? environment = null)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = fileName,
        WorkingDirectory = workdir,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    foreach (var argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }

    if (environment is not null)
    {
        foreach (var (key, value) in environment)
        {
            startInfo.Environment[key] = value;
        }
    }

    var process = new Process
    {
        StartInfo = startInfo
    };
    process.Start();
    return process;
}

internal sealed record SmokeOptions(
    string Configuration,
    string? BaseOutputPath,
    string? BaseIntermediateOutputPath,
    string? GeneratedOutputRoot,
    string? BrowserOutputRoot,
    bool FrontendOnly)
{
    public static SmokeOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Release";
        string? baseOutputPath = null;
        string? baseIntermediateOutputPath = null;
        string? generatedOutputRoot = null;
        string? browserOutputRoot = null;
        var frontendOnly = false;

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
                case "--base-output-path":
                case "-BaseOutputPath":
                    baseOutputPath = RequireValue(arguments, ref index, argument);
                    break;
                case "--base-intermediate-output-path":
                case "-BaseIntermediateOutputPath":
                    baseIntermediateOutputPath = RequireValue(arguments, ref index, argument);
                    break;
                case "--generated-output-root":
                    generatedOutputRoot = RequireValue(arguments, ref index, argument);
                    break;
                case "--browser-output-root":
                    browserOutputRoot = RequireValue(arguments, ref index, argument);
                    break;
                case "--frontend-only":
                    frontendOnly = true;
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

        return new SmokeOptions(
            configuration,
            baseOutputPath,
            baseIntermediateOutputPath,
            generatedOutputRoot,
            browserOutputRoot,
            frontendOnly);
    }

    static string RequireValue(IReadOnlyList<string> arguments, ref int index, string option)
    {
        var nextIndex = index + 1;
        if (nextIndex >= arguments.Count)
        {
            throw new InvalidOperationException("Missing value for " + option + ".");
        }

        index = nextIndex;
        return arguments[index];
    }

    static void WriteUsage()
    {
        Console.WriteLine("Usage: dotnet run --file samples/ECMAScript.Vben.ElementPlusInject/verify-smoke.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --base-output-path <path>");
        Console.WriteLine("  --base-intermediate-output-path <path>");
        Console.WriteLine("  --generated-output-root <path>");
        Console.WriteLine("  --browser-output-root <path>");
        Console.WriteLine("  --frontend-only");
    }
}
