#!/usr/bin/env dotnet run
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj
using Jazor.NodeHost;
using System.Text.Json;
using System.Diagnostics;
using System.Globalization;

var options = SmokeOptions.Parse(args);
var repoRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
Environment.SetEnvironmentVariable("JazorPnpmStoreDirectory", Path.Combine(repoRoot, ".tmp", "nodeapi-store"));
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
        "--package", "jazor-vue",
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
AssertStandardReleaseArtifacts(bundleOutputRoot);

await NodeToolchain.RestoreAsync(consumerRoot, Path.Combine(repoRoot, ".tmp", "nodeapi-store"),
    CancellationToken.None, Console.Out);
var distRoot = Path.Combine(repoRoot, ".tmp", "sample-smoke", "ECMAScript.Pinia.Counter", options.Configuration, "consumer-dist");
using (var environment = new NodeEnvironment(consumerRoot))
{
    var result = await environment.InvokeAsync(Path.Combine(consumerRoot, "scripts", "build.cjs"), "build",
        JsonSerializer.Serialize(new { consumerRoot, generatedRoot = generatedOutputRoot, bundleRoot = bundleOutputRoot, distRoot }));
    Console.WriteLine(result);
}
var imports = new Dictionary<string, string>(StringComparer.Ordinal);
foreach (var folder in "components,host,stores,tests,clr,runtime,System".Split(','))
    imports[folder + "/"] = new Uri(Path.Combine(generatedOutputRoot, folder) + Path.DirectorySeparatorChar).AbsoluteUri;
foreach (var package in "vue,pinia,@pinia/testing,jsdom".Split(','))
    imports[package] = package;
foreach (var testFile in new[] { "src/pinia.generated.test.js", "src/pinia.runtime.test.js", "src/pinia.generated.dom.test.js" })
    Console.Write(await NodeModuleRunner.RunAsync(consumerRoot, Path.Combine(consumerRoot, testFile), imports: imports));
Console.WriteLine("ECMAScript.Pinia sample smoke verification passed.");
Console.WriteLine("Verified: local Jazor package pack, isolated generated Pinia/testing modules, standard JavaScript release project, and in-process NodeApi runtime/DOM coverage.");

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
        // Jazor.Vue shares the Jazor.* prefix; only a numeric suffix is the core package version.
        .Where(file => file.Name.Length > "Jazor.".Length && char.IsDigit(file.Name["Jazor.".Length]))
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

static void AssertStandardReleaseArtifacts(string bundleOutputRoot)
{
    AssertPathExists(Path.Combine(bundleOutputRoot, "entry.js"), "standard JavaScript release entry");
    AssertPathExists(Path.Combine(bundleOutputRoot, "package.json"), "standard JavaScript release package metadata");
}

static void AssertGeneratedHostArtifacts(string generatedOutputRoot)
{
    var counterStoreModulePath = Path.Combine(generatedOutputRoot, "stores", "counter-store.mjs");
    var testingModulePath = Path.Combine(generatedOutputRoot, "tests", "counter-testing.mjs");
    var hostAppModulePath = Path.Combine(generatedOutputRoot, "host", "app.mjs");
    var packagePath = Path.Combine(generatedOutputRoot, "package.json");

    AssertPathExists(counterStoreModulePath, "generated counter store module");
    AssertPathExists(testingModulePath, "generated testing module");
    AssertPathExists(hostAppModulePath, "generated host app module");
    AssertPathExists(packagePath, "generated standard JavaScript package metadata");

    var counterStoreModule = File.ReadAllText(counterStoreModulePath);
    var testingModule = File.ReadAllText(testingModulePath);
    var hostAppModule = File.ReadAllText(hostAppModulePath);
    var package = File.ReadAllText(packagePath);

    AssertContains(counterStoreModule, "from \"pinia\"", "pinia runtime import in counter store module");
    AssertContains(counterStoreModule, "defineStore(", "defineStore lowering in counter store module");
    AssertContains(counterStoreModule, "storeToRefs(", "storeToRefs lowering in counter store module");

    AssertContains(testingModule, "from \"@pinia/testing\"", "@pinia/testing runtime import in testing module");
    AssertContains(testingModule, "createTestingPinia({", "createTestingPinia lowering in testing module");
    AssertContains(testingModule, "stubActions", "testing stubActions contract in testing module");

    AssertContains(hostAppModule, "disposePinia(", "disposePinia teardown in host app module");
    AssertContains(hostAppModule, "createPinia()", "createPinia root creation in host app module");

    AssertContains(package, "\"type\": \"module\"", "standard JavaScript package metadata");
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
