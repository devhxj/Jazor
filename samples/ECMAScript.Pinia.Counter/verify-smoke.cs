using System.Diagnostics;
using System.Globalization;

var options = SmokeOptions.Parse(args);
var repoRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
var sampleRoot = Path.Combine(repoRoot, "samples", "ECMAScript.Pinia.Counter");
var consumerRoot = Path.Combine(sampleRoot, "pinia-consumer");
var hostRoot = Path.Combine(sampleRoot, "Pinia.Counter.Host");
var publishScriptPath = Path.Combine(repoRoot, "scripts", "csharp", "publish-nuget.cs");
var hostProject = Path.Combine(hostRoot, "Pinia.Counter.Host.csproj");
var packageOutput = Path.Combine(repoRoot, ".tmp", "nupkg-sample");
PackageInfo? resolvedPackageInfo = null;
string? restorePackagesPath = null;
var generatedOutputRoot = string.IsNullOrWhiteSpace(options.GeneratedOutputRoot)
    ? Path.Combine(repoRoot, ".tmp", "sample-smoke", "ECMAScript.Pinia.Counter", options.Configuration, "jazor")
    : ResolvePath(options.GeneratedOutputRoot, repoRoot);
var bundleOutputRoot = string.IsNullOrWhiteSpace(options.BundleOutputRoot)
    ? Path.Combine(repoRoot, ".tmp", "sample-smoke", "ECMAScript.Pinia.Counter", options.Configuration, "bundle")
    : ResolvePath(options.BundleOutputRoot, repoRoot);
var consumerDistRoot = Path.Combine(repoRoot, ".tmp", "sample-smoke", "ECMAScript.Pinia.Counter", options.Configuration, "consumer-dist");

SetCommonEnvironment(repoRoot);
var isolationArguments = GetIsolationArguments(options);

if (!options.FrontendOnly || options.BuildLocal)
{
    CleanDirectory(packageOutput, repoRoot);
    CleanDirectory(generatedOutputRoot, repoRoot);

    var packArguments = new List<string>
    {
        "run",
        "--file",
        publishScriptPath,
        "--",
        "--configuration", options.Configuration,
        "--output-directory", packageOutput,
        "--skip-push",
        "--package", "jazor",
        "--package", "pinia",
        "--package", "pinia-testing"
    };
    packArguments.AddRange(isolationArguments.PublishArguments);
    RunDotNet(repoRoot, packArguments);

    resolvedPackageInfo = ResolveLatestPackage(packageOutput);
    restorePackagesPath = Path.Combine(repoRoot, ".tmp", "nuget-sample-packages", $"{resolvedPackageInfo.Value.Version}-{resolvedPackageInfo.Value.Stamp}");
    var buildArguments = new List<string>
    {
        "build",
        hostProject,
        "-c", options.Configuration,
        "-t:Rebuild",
        "/m:1",
        "/p:BuildInParallel=false",
        $"-p:RestoreAdditionalProjectSources={packageOutput}",
        $"-p:RestorePackagesPath={restorePackagesPath}",
        "-p:RestoreForce=true",
        $"-p:JazorPackageVersion={resolvedPackageInfo.Value.Version}",
        $"-p:JazorDir={generatedOutputRoot}"
    };
    buildArguments.AddRange(isolationArguments.BuildArguments);
    buildArguments.AddRange(new[]
    {
        "/nr:false",
        "-p:UseSharedCompilation=false"
    });
    RunDotNet(repoRoot, buildArguments);

    var releaseBuildArguments = new List<string>
    {
        "build",
        hostProject,
        "-c", options.Configuration,
        "-t:Rebuild",
        "/m:1",
        "/p:BuildInParallel=false",
        $"-p:RestoreAdditionalProjectSources={packageOutput}",
        $"-p:RestorePackagesPath={restorePackagesPath}",
        "-p:RestoreForce=true",
        $"-p:JazorPackageVersion={resolvedPackageInfo.Value.Version}",
        "-p:JazorMode=release",
        $"-p:JazorDir={bundleOutputRoot}"
    };
    releaseBuildArguments.AddRange(isolationArguments.BuildArguments);
    releaseBuildArguments.AddRange(new[]
    {
        "/nr:false",
        "-p:UseSharedCompilation=false"
    });
    RunDotNet(repoRoot, releaseBuildArguments);
}

AssertPathExists(ResolveHostAssemblyPath(hostRoot, options), "sample host assembly for requested configuration");
AssertGeneratedHostArtifacts(generatedOutputRoot);
AssertNetpackBundleArtifacts(bundleOutputRoot);

var denoExePath = ResolveDenoHostRuntime(repoRoot, restorePackagesPath, resolvedPackageInfo);
var denoEnvironment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["JAZOR_GENERATED_ROOT"] = generatedOutputRoot,
    ["JAZOR_BUNDLE_ROOT"] = bundleOutputRoot,
    // Deno release assets 保持隔离，避免 smoke 覆盖跟踪的 sample fixture。
    ["PINIA_DENO_DIST_ROOT"] = consumerDistRoot
};

RunDeno(denoExePath, consumerRoot, denoEnvironment, new[] { "task", "build" });
RunDeno(denoExePath, consumerRoot, denoEnvironment, new[] { "test", "-A", "--frozen", "--import-map", Path.Combine(consumerRoot, ".deno-build", "import-map.generated.json"), "src/pinia.generated.test.js" });
RunDeno(denoExePath, consumerRoot, denoEnvironment, new[] { "test", "-A", "--frozen", "--import-map", Path.Combine(consumerRoot, ".deno-build", "import-map.generated.json"), "src/pinia.runtime.test.js" });
RunDeno(denoExePath, consumerRoot, denoEnvironment, new[] { "test", "-A", "--frozen", "--import-map", Path.Combine(consumerRoot, ".deno-build", "import-map.generated.json"), "src/pinia.generated.dom.test.js" });

Console.WriteLine("ECMAScript.Pinia sample smoke verification passed.");
Console.WriteLine("Verified: local Jazor package pack, isolated generated Pinia/testing modules, Netpack release bundle, and DenoHost runtime/DOM coverage.");

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
    Environment.SetEnvironmentVariable("MSBUILDDISABLENODEREUSE", "1");
    Environment.SetEnvironmentVariable("UseSharedCompilation", "false");
}

static IsolationArguments GetIsolationArguments(SmokeOptions options)
{
    var publishArguments = new List<string>();
    var buildArguments = new List<string>();

    if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
    {
        var isolatedOutputRoot = GetIsolatedBuildRoot(options.BaseOutputPath!, repoRoot: null);
        publishArguments.Add("--base-output-path");
        publishArguments.Add(isolatedOutputRoot);
        buildArguments.Add($"-p:JazorIsolatedBaseOutputRoot={isolatedOutputRoot}");
    }

    if (!string.IsNullOrWhiteSpace(options.BaseIntermediateOutputPath))
    {
        var isolatedIntermediateOutputRoot = GetIsolatedBuildRoot(options.BaseIntermediateOutputPath!, repoRoot: null);
        publishArguments.Add("--base-intermediate-output-path");
        publishArguments.Add(isolatedIntermediateOutputRoot);
        buildArguments.Add($"-p:JazorIsolatedBaseIntermediateOutputRoot={isolatedIntermediateOutputRoot}");
    }

    return new IsolationArguments(publishArguments, buildArguments);
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

static PackageInfo ResolveLatestPackage(string packageOutput)
{
    var packageFile = new DirectoryInfo(packageOutput)
        .EnumerateFiles("Jazor.*.nupkg", SearchOption.TopDirectoryOnly)
        .Where(file => !file.Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(file => file.LastWriteTimeUtc)
        .FirstOrDefault();

    if (packageFile is null)
    {
        throw new InvalidOperationException($"Packed Jazor package not found under '{packageOutput}'.");
    }

    var packageVersion = Path.GetFileNameWithoutExtension(packageFile.Name).Replace("Jazor.", "", StringComparison.Ordinal);
    var packageStamp = packageFile.LastWriteTimeUtc.ToString("yyyyMMddHHmmssffff", CultureInfo.InvariantCulture);
    return new PackageInfo(packageVersion, packageStamp);
}

static string ResolveHostAssemblyPath(string hostRoot, SmokeOptions options)
{
    if (string.IsNullOrWhiteSpace(options.BaseOutputPath))
    {
        return Path.Combine(hostRoot, "bin", options.Configuration, "net11.0", "Pinia.Counter.Host.dll");
    }

    var isolatedOutputRoot = GetIsolatedBuildRoot(options.BaseOutputPath!, repoRoot: null);
    return Path.Combine(isolatedOutputRoot, "Pinia.Counter.Host", "bin", options.Configuration, "net11.0", "Pinia.Counter.Host.dll");
}

static string ResolveDenoHostRuntime(string repoRoot, string? restorePackagesPath, PackageInfo? packageInfo)
{
    var candidatePaths = new List<string>();

    if (!string.IsNullOrWhiteSpace(restorePackagesPath) && Directory.Exists(restorePackagesPath))
    {
        AddDenoRuntimeCandidates(candidatePaths, Path.Combine(restorePackagesPath, "denohost.runtime.win-x64"));
    }

    var denoHostPackageRoot = Path.Combine(repoRoot, ".dotnet", ".nuget", "packages", "denohost.runtime.win-x64");
    AddDenoRuntimeCandidates(candidatePaths, denoHostPackageRoot);

    var denoExePath = candidatePaths.FirstOrDefault(File.Exists);
    if (denoExePath is null)
    {
        var packageLabel = packageInfo is { } package
            ? $"local Jazor {package.Version} package restore"
            : "local Jazor package restore";
        throw new FileNotFoundException($"DenoHost runtime was not found after the {packageLabel}.");
    }

    return denoExePath;
}

static void AddDenoRuntimeCandidates(ICollection<string> candidates, string baseDirectory)
{
    if (!Directory.Exists(baseDirectory))
    {
        return;
    }

    foreach (var candidate in Directory
        .EnumerateFiles(baseDirectory, "deno.exe", SearchOption.AllDirectories)
        .OrderByDescending(static path => path, StringComparer.OrdinalIgnoreCase))
    {
        candidates.Add(candidate);
    }
}

static void AssertNetpackBundleArtifacts(string bundleOutputRoot)
{
    AssertPathExists(Path.Combine(bundleOutputRoot, "bundle.js"), "Netpack browser bundle");
    AssertPathExists(Path.Combine(bundleOutputRoot, "bundle.js.map"), "Netpack browser bundle source map");
}

static void AssertGeneratedHostArtifacts(string generatedOutputRoot)
{
    var counterStoreModulePath = Path.Combine(generatedOutputRoot, "stores", "counter-store.mjs");
    var testingModulePath = Path.Combine(generatedOutputRoot, "tests", "counter-testing.mjs");
    var hostAppModulePath = Path.Combine(generatedOutputRoot, "host", "app.mjs");
    var manifestPath = Path.Combine(generatedOutputRoot, "jazor-manifest.json");

    AssertPathExists(counterStoreModulePath, "generated counter store module");
    AssertPathExists(testingModulePath, "generated testing module");
    AssertPathExists(hostAppModulePath, "generated host app module");
    AssertPathExists(manifestPath, "generated manifest");

    var counterStoreModule = File.ReadAllText(counterStoreModulePath);
    var testingModule = File.ReadAllText(testingModulePath);
    var hostAppModule = File.ReadAllText(hostAppModulePath);
    var manifest = File.ReadAllText(manifestPath);

    AssertContains(counterStoreModule, "from \"pinia\"", "pinia runtime import in counter store module");
    AssertContains(counterStoreModule, "defineStore(", "defineStore lowering in counter store module");
    AssertContains(counterStoreModule, "storeToRefs(", "storeToRefs lowering in counter store module");

    AssertContains(testingModule, "from \"@pinia/testing\"", "@pinia/testing runtime import in testing module");
    AssertContains(testingModule, "createTestingPinia({", "createTestingPinia lowering in testing module");
    AssertContains(testingModule, "stubActions", "testing stubActions contract in testing module");

    AssertContains(hostAppModule, "disposePinia(", "disposePinia teardown in host app module");
    AssertContains(hostAppModule, "createPinia()", "createPinia root creation in host app module");

    AssertContains(manifest, "\"host/app.mjs\"", "host app entry in manifest");
    AssertContains(manifest, "\"stores/counter-store.mjs\"", "counter store entry in manifest");
    AssertContains(manifest, "\"tests/counter-testing.mjs\"", "testing entry in manifest");
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

static string GetIsolatedBuildRoot(string path, string? repoRoot)
{
    var resolvedPath = path;
    if (!Path.IsPathRooted(resolvedPath))
    {
        var root = repoRoot ?? Directory.GetCurrentDirectory();
        resolvedPath = Path.Combine(root, resolvedPath);
    }

    resolvedPath = Path.GetFullPath(resolvedPath);
    if (!resolvedPath.EndsWith(Path.DirectorySeparatorChar))
    {
        resolvedPath += Path.DirectorySeparatorChar;
    }

    return resolvedPath;
}

static void RunDotNet(string repoRoot, IEnumerable<string> arguments)
{
    RunProcess("dotnet", arguments, repoRoot);
}

static void RunDeno(string denoExePath, string workingDirectory, IReadOnlyDictionary<string, string> environment, IReadOnlyList<string> arguments)
{
    RunProcess(denoExePath, arguments, workingDirectory, environment);
}

static void RunProcess(string fileName, IEnumerable<string> arguments, string workingDirectory, IReadOnlyDictionary<string, string>? environment = null)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = fileName,
        WorkingDirectory = workingDirectory,
        UseShellExecute = false,
        RedirectStandardError = true,
        RedirectStandardOutput = true
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

    var outputLines = new List<string>();
    var outputLock = new object();

    using var process = new Process { StartInfo = startInfo };
    process.OutputDataReceived += (_, eventArgs) =>
    {
        if (eventArgs.Data is null)
        {
            return;
        }

        Console.WriteLine(eventArgs.Data);
        lock (outputLock)
        {
            AppendOutputLine(outputLines, eventArgs.Data);
        }
    };
    process.ErrorDataReceived += (_, eventArgs) =>
    {
        if (eventArgs.Data is null)
        {
            return;
        }

        Console.Error.WriteLine(eventArgs.Data);
        lock (outputLock)
        {
            AppendOutputLine(outputLines, eventArgs.Data);
        }
    };

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    process.WaitForExit();

    if (process.ExitCode != 0)
    {
        string[] failureTail;
        lock (outputLock)
        {
            failureTail = outputLines.TakeLast(40).ToArray();
        }

        var tailText = failureTail.Length == 0
            ? string.Empty
            : $"{Environment.NewLine}Last output:{Environment.NewLine}{string.Join(Environment.NewLine, failureTail)}";
        throw new InvalidOperationException($"{Path.GetFileName(fileName)} {string.Join(" ", arguments)} failed with exit code {process.ExitCode}.{tailText}");
    }
}

static void AppendOutputLine(List<string> outputLines, string line)
{
    outputLines.Add(line);
    if (outputLines.Count > 200)
    {
        outputLines.RemoveAt(0);
    }
}

sealed record SmokeOptions(
    string Configuration,
    bool BuildLocal,
    bool FrontendOnly,
    string? BaseOutputPath,
    string? BaseIntermediateOutputPath,
    string? GeneratedOutputRoot,
    string? BundleOutputRoot)
{
    public static SmokeOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Debug";
        var buildLocal = false;
        var frontendOnly = false;
        string? baseOutputPath = null;
        string? baseIntermediateOutputPath = null;
        string? generatedOutputRoot = null;
        string? bundleOutputRoot = null;

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
                case "--bundle-output-root":
                    bundleOutputRoot = RequireValue(arguments, ref index, argument);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported argument: {argument}");
            }
        }

        return new SmokeOptions(
            configuration,
            buildLocal,
            frontendOnly,
            baseOutputPath,
            baseIntermediateOutputPath,
            generatedOutputRoot,
            bundleOutputRoot);
    }

    static string RequireValue(IReadOnlyList<string> arguments, ref int index, string option)
    {
        var nextIndex = index + 1;
        if (nextIndex >= arguments.Count)
        {
            throw new InvalidOperationException($"Missing value for {option}.");
        }

        index = nextIndex;
        return arguments[index];
    }
}

readonly record struct PackageInfo(string Version, string Stamp);
readonly record struct IsolationArguments(IReadOnlyList<string> PublishArguments, IReadOnlyList<string> BuildArguments);
