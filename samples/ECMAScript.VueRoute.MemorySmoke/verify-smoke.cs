#!/usr/bin/env dotnet run

using System.Diagnostics;

var options = SmokeOptions.Parse(args);
var repoRoot = ScriptHelpers.FindRepositoryRoot(Directory.GetCurrentDirectory());
var sampleRoot = Path.Combine(repoRoot, "samples", "ECMAScript.VueRoute.MemorySmoke");
var consumerRoot = Path.Combine(sampleRoot, "vueroute-consumer");
var hostRoot = Path.Combine(sampleRoot, "VueRoute.MemorySmoke.Host");
var generatedOutputRoot = string.IsNullOrWhiteSpace(options.GeneratedOutputRoot)
    ? Path.Combine(repoRoot, ".tmp", "sample-smoke", "ECMAScript.VueRoute.MemorySmoke", options.Configuration, "jazor")
    : ScriptHelpers.ResolvePath(repoRoot, options.GeneratedOutputRoot);

if (!options.FrontendOnly || options.BuildLocal)
{
    var buildArguments = new List<string>
    {
        "run",
        "--file",
        Path.Combine("samples", "ECMAScript.VueRoute.MemorySmoke", "build-local.cs"),
        "--",
        "--configuration",
        options.Configuration,
        "--jazor-out-dir",
        generatedOutputRoot
    };

    if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
    {
        buildArguments.Add("--base-output-path");
        buildArguments.Add(options.BaseOutputPath);
    }

    if (!string.IsNullOrWhiteSpace(options.BaseIntermediateOutputPath))
    {
        buildArguments.Add("--base-intermediate-output-path");
        buildArguments.Add(options.BaseIntermediateOutputPath);
    }

    await ScriptHelpers.RunDotNetAsync(buildArguments, repoRoot);
}

ScriptHelpers.AssertPathExists(ScriptHelpers.ResolveHostAssemblyPath(hostRoot, options), "sample host assembly for requested configuration");
ScriptHelpers.AssertGeneratedHostArtifacts(generatedOutputRoot);

var npmEnvironment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["JAZOR_GENERATED_ROOT"] = generatedOutputRoot
};

await ScriptHelpers.RunProcessAsync("npm", ["ci"], consumerRoot, npmEnvironment);
await ScriptHelpers.RunProcessAsync("npm", ["run", "build"], consumerRoot, npmEnvironment);
await ScriptHelpers.RunProcessAsync("npm", ["run", "test", "--", "--run"], consumerRoot, npmEnvironment);

Console.WriteLine("ECMAScript.VueRoute sample smoke verification passed.");
Console.WriteLine("Verified: local Jazor package pack, isolated generated Vue Router modules, Vite build, and Vitest runtime/DOM coverage.");

internal sealed record SmokeOptions(
    string Configuration,
    bool BuildLocal,
    bool FrontendOnly,
    string? BaseOutputPath,
    string? BaseIntermediateOutputPath,
    string? GeneratedOutputRoot)
{
    public static SmokeOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Debug";
        var buildLocal = false;
        var frontendOnly = false;
        string? baseOutputPath = null;
        string? baseIntermediateOutputPath = null;
        string? generatedOutputRoot = null;

        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            switch (argument)
            {
                case "--configuration":
                case "-Configuration":
                    configuration = RequireValue(arguments, ref index, argument);
                    break;
                case "--build-local":
                case "-BuildLocal":
                    buildLocal = true;
                    break;
                case "--frontend-only":
                case "-FrontendOnly":
                    frontendOnly = true;
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
                case "-GeneratedOutputRoot":
                    generatedOutputRoot = RequireValue(arguments, ref index, argument);
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
            buildLocal,
            frontendOnly,
            baseOutputPath,
            baseIntermediateOutputPath,
            generatedOutputRoot);
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
        Console.WriteLine("Usage: dotnet run --file samples/ECMAScript.VueRoute.MemorySmoke/verify-smoke.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --build-local");
        Console.WriteLine("  --frontend-only");
        Console.WriteLine("  --base-output-path <path>");
        Console.WriteLine("  --base-intermediate-output-path <path>");
        Console.WriteLine("  --generated-output-root <path>");
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
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Cannot locate repository root (Jazor.slnx).");
    }

    public static string ResolvePath(string repoRoot, string path)
    {
        return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));
    }

    public static string ResolveHostAssemblyPath(string hostRoot, SmokeOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BaseOutputPath))
        {
            return Path.Combine(hostRoot, "bin", options.Configuration, "net11.0", "VueRoute.MemorySmoke.Host.dll");
        }

        var isolatedOutputRoot = ResolveBuildRoot(options.BaseOutputPath);
        return Path.Combine(isolatedOutputRoot, "VueRoute.MemorySmoke.Host", "bin", options.Configuration, "net11.0", "VueRoute.MemorySmoke.Host.dll");
    }

    public static string ResolveBuildRoot(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return fullPath.EndsWith(Path.DirectorySeparatorChar)
            ? fullPath
            : fullPath + Path.DirectorySeparatorChar;
    }

    public static void AssertGeneratedHostArtifacts(string generatedOutputRoot)
    {
        var routerModulePath = Path.Combine(generatedOutputRoot, "router", "memory-router.mjs");
        var componentModulePath = Path.Combine(generatedOutputRoot, "components", "route-shell.mjs");
        var testingModulePath = Path.Combine(generatedOutputRoot, "tests", "router-testing.mjs");
        var hostAppModulePath = Path.Combine(generatedOutputRoot, "host", "app.mjs");
        var manifestPath = Path.Combine(generatedOutputRoot, "jazor-manifest.json");

        AssertPathExists(routerModulePath, "generated router module");
        AssertPathExists(componentModulePath, "generated route-shell component module");
        AssertPathExists(testingModulePath, "generated router testing module");
        AssertPathExists(hostAppModulePath, "generated host app module");
        AssertPathExists(manifestPath, "generated manifest");

        var routerModule = File.ReadAllText(routerModulePath);
        var componentModule = File.ReadAllText(componentModulePath);
        var testingModule = File.ReadAllText(testingModulePath);
        var hostAppModule = File.ReadAllText(hostAppModulePath);
        var manifest = File.ReadAllText(manifestPath);

        AssertContains(routerModule, "from \"npm:vue-router@4\"", "vue-router runtime import in router module");
        AssertContains(routerModule, "createMemoryHistory(", "createMemoryHistory lowering in router module");
        AssertContains(routerModule, "createRouter({", "createRouter lowering in router module");
        AssertContains(routerModule, "beforeEach(", "beforeEach lowering in router module");
        AssertContains(routerModule, "afterEach(", "afterEach lowering in router module");

        AssertContains(componentModule, "useRouter()", "useRouter lowering in component module");
        AssertContains(componentModule, "useRoute()", "useRoute lowering in component module");
        AssertContains(componentModule, "useLink({", "useLink lowering in component module");
        AssertContains(componentModule, "onBeforeRouteLeave(", "component leave guard usage in component module");
        AssertContains(componentModule, "inject(routerViewLocationKey)", "typed router-view injection usage in component module");

        AssertContains(testingModule, "loadRouteLocation(", "loadRouteLocation lowering in testing module");
        AssertContains(testingModule, "router.push(", "router push lowering in testing module");
        AssertContains(testingModule, "navigateScenario(", "navigateScenario usage in testing module");

        AssertContains(hostAppModule, "app.use(router);", "router installation in host app module");
        AssertContains(hostAppModule, "router.isReady()", "router readiness flow in host app module");
        AssertContains(hostAppModule, "RouterLink", "RouterLink usage in host app module");
        AssertContains(hostAppModule, "RouterView", "RouterView usage in host app module");

        AssertContains(manifest, "\"host/app.mjs\"", "host app entry in manifest");
        AssertContains(manifest, "\"router/memory-router.mjs\"", "router module entry in manifest");
        AssertContains(manifest, "\"tests/router-testing.mjs\"", "testing module entry in manifest");
    }

    public static void AssertPathExists(string path, string description)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            throw new FileNotFoundException("Missing " + description + ": " + path);
        }
    }

    static void AssertContains(string text, string snippet, string description)
    {
        if (!text.Contains(snippet, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Missing {description}: expected to find '{snippet}'.");
        }
    }

    public static async Task RunDotNetAsync(IReadOnlyList<string> arguments, string workdir, CancellationToken cancellationToken = default)
    {
        await RunProcessAsync(
            "dotnet",
            arguments,
            workdir,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["DOTNET_CLI_HOME"] = Path.Combine(FindRepositoryRoot(workdir), ".dotnet"),
                ["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1",
                ["MSBUILDDISABLENODEREUSE"] = "1",
                ["UseSharedCompilation"] = "false"
            },
            cancellationToken);
    }

    public static async Task RunProcessAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string workingDirectory,
        IReadOnlyDictionary<string, string>? environment = null,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
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

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start process: " + fileName);

        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var output = await stdout;
        var error = await stderr;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"{fileName} {string.Join(" ", arguments)} failed with exit code {process.ExitCode}.{Environment.NewLine}{output}{error}".TrimEnd());
        }
    }
}
