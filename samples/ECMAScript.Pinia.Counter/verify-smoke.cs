using System.Diagnostics;
using System.Globalization;

var options = SmokeOptions.Parse(args);
var repoRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
var sampleRoot = Path.Combine(repoRoot, "samples", "ECMAScript.Pinia.Counter");
var consumerRoot = Path.Combine(sampleRoot, "pinia-consumer");
var hostRoot = Path.Combine(sampleRoot, "Pinia.Counter.Host");
var packageProject = Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj");
var hostProject = Path.Combine(hostRoot, "Pinia.Counter.Host.csproj");
var packageOutput = Path.Combine(repoRoot, ".tmp", "nupkg-sample");
var generatedOutputRoot = string.IsNullOrWhiteSpace(options.GeneratedOutputRoot)
    ? Path.Combine(repoRoot, ".tmp", "sample-smoke", "ECMAScript.Pinia.Counter", options.Configuration, "jazor")
    : ResolvePath(options.GeneratedOutputRoot, repoRoot);

SetCommonEnvironment(repoRoot);

if (!options.FrontendOnly || options.BuildLocal)
{
    CleanDirectory(packageOutput, repoRoot);
    CleanDirectory(generatedOutputRoot, repoRoot);

    var packArguments = new List<string>
    {
        "pack",
        packageProject,
        "-c", options.Configuration,
        "-o", packageOutput,
        "-v", "minimal"
    };
    packArguments.AddRange(GetIsolationArguments(options));
    RunDotNet(repoRoot, packArguments);

    var packageInfo = ResolveLatestPackage(packageOutput);
    var buildArguments = new List<string>
    {
        "build",
        hostProject,
        "-c", options.Configuration,
        "-t:Rebuild",
        "/m:1",
        "/p:BuildInParallel=false",
        $"-p:RestoreSources={packageOutput}",
        $"-p:RestorePackagesPath={Path.Combine(repoRoot, ".tmp", "nuget-sample-packages", $"{packageInfo.Version}-{packageInfo.Stamp}")}",
        "-p:RestoreForce=true",
        $"-p:JazorPackageVersion={packageInfo.Version}",
        $"-p:JazorOutDir={generatedOutputRoot}"
    };
    buildArguments.AddRange(GetIsolationArguments(options));
    buildArguments.AddRange(new[]
    {
        "/nr:false",
        "-p:UseSharedCompilation=false"
    });
    RunDotNet(repoRoot, buildArguments);
}

AssertPathExists(ResolveHostAssemblyPath(hostRoot, options), "sample host assembly for requested configuration");
AssertGeneratedHostArtifacts(generatedOutputRoot);

var denoExePath = ResolveDenoExecutable(repoRoot);
var denoEnvironment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["JAZOR_GENERATED_ROOT"] = generatedOutputRoot
};

RunDeno(denoExePath, consumerRoot, denoEnvironment, new[] { "task", "build" });
RunDeno(denoExePath, consumerRoot, denoEnvironment, new[] { "test", "-A", "--frozen", "--import-map", Path.Combine(consumerRoot, ".deno-build", "import-map.generated.json"), "src/pinia.generated.test.js" });
RunDeno(denoExePath, consumerRoot, denoEnvironment, new[] { "test", "-A", "--frozen", "--import-map", Path.Combine(consumerRoot, ".deno-build", "import-map.generated.json"), "src/pinia.runtime.test.js" });
RunDeno(denoExePath, consumerRoot, denoEnvironment, new[] { "test", "-A", "--frozen", "--import-map", Path.Combine(consumerRoot, ".deno-build", "import-map.generated.json"), "src/pinia.generated.dom.test.js" });

Console.WriteLine("ECMAScript.Pinia sample smoke verification passed.");
Console.WriteLine("Verified: local Jazor package pack, isolated generated Pinia/testing modules, Deno bundle build, and Deno runtime/DOM coverage.");

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

static string[] GetIsolationArguments(SmokeOptions options)
{
    var arguments = new List<string>();
    if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
    {
        arguments.Add($"-p:JazorIsolatedBaseOutputRoot={GetIsolatedBuildRoot(options.BaseOutputPath!, repoRoot: null)}");
    }

    if (!string.IsNullOrWhiteSpace(options.BaseIntermediateOutputPath))
    {
        arguments.Add($"-p:JazorIsolatedBaseIntermediateOutputRoot={GetIsolatedBuildRoot(options.BaseIntermediateOutputPath!, repoRoot: null)}");
    }

    return arguments.ToArray();
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

static string ResolveDenoExecutable(string repoRoot)
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

    var candidatePaths = new[]
    {
        Path.Combine(repoRoot, "src", "Jolt", "bin", "Debug", "net11.0", "runtimes", "win-x64", "native", "deno.exe"),
        Path.Combine(repoRoot, "src", "Jolt", "bin", "Release", "net11.0", "runtimes", "win-x64", "native", "deno.exe"),
        Path.Combine(repoRoot, "src", "Jazor.Emit", "bin", "Debug", "net11.0", "runtimes", "win-x64", "native", "deno.exe"),
        Path.Combine(repoRoot, "src", "Jazor.Emit", "bin", "Release", "net11.0", "runtimes", "win-x64", "native", "deno.exe"),
        Path.Combine(repoRoot, ".dotnet", ".nuget", "packages", "denohost.runtime.win-x64", "2.7.14", "runtimes", "win-x64", "native", "deno.exe")
    };

    var denoExePath = candidatePaths.FirstOrDefault(File.Exists);
    if (denoExePath is null)
    {
        throw new FileNotFoundException("Bundled Deno runtime was not found. Build Jolt or Jazor.Emit first so DenoHost runtime assets exist.");
    }

    return denoExePath;
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
        UseShellExecute = false
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

    using var process = new Process { StartInfo = startInfo };
    process.Start();
    process.WaitForExit();

    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException($"{Path.GetFileName(fileName)} {string.Join(" ", arguments)} failed with exit code {process.ExitCode}.");
    }
}

sealed record SmokeOptions(
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
            generatedOutputRoot);
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
