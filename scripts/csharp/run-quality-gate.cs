#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Text;

if (args.Length < 1 || args[0] is not ("compiler" or "razorvue" or "vue-bindings"))
{
    Console.Error.WriteLine("Usage: dotnet run --file scripts/csharp/run-quality-gate.cs -- <compiler|razorvue|vue-bindings> [--output-directory DIR]");
    return 1;
}

var gate = args[0];
var outputDirectory = ReadOption(args, "--output-directory");
var script = gate == "vue-bindings" ? "verify-vue-binding-coverage.cs" : $"verify-{gate}-coverage.cs";
// The working directory owns the tested sources, even when CI loads this tool from a different revision.
var repoRoot = Directory.GetCurrentDirectory();
ConfigureRepositoryEnvironment(repoRoot);
var resultRoot = outputDirectory is null
    ? Path.Combine(repoRoot, "artifacts", "quality", gate, Guid.NewGuid().ToString("N"))
    : Path.GetFullPath(Path.IsPathRooted(outputDirectory) ? outputDirectory : Path.Combine(repoRoot, outputDirectory));
Directory.CreateDirectory(resultRoot);
using var log = new StreamWriter(Path.Combine(resultRoot, "gate.log"), append: false, Encoding.UTF8) { AutoFlush = true };
var metrics = new List<string>();
var exitCode = 1;

try
{
    var startInfo = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = repoRoot,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    ConfigureProcessEnvironment(startInfo, repoRoot);
    // Debug is the existing coverage baseline; Release consumers have their own publish gates.
    foreach (var argument in new[]
    {
        "run", "--file", Path.Combine(repoRoot, "scripts", "csharp", script), "--",
        "--configuration", "Debug", "--results-directory", Path.Combine(resultRoot, "reports")
    })
        startInfo.ArgumentList.Add(argument);

    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException("Failed to start the coverage gate.");
    // Drain both pipes concurrently so a verbose build cannot block the test process.
    await Task.WhenAll(
        CopyOutputAsync(process.StandardOutput, Console.Out),
        CopyOutputAsync(process.StandardError, Console.Error),
        process.WaitForExitAsync());
    exitCode = process.ExitCode;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    log.WriteLine(exception.Message);
}

// Preserve the gate's exit code even though writing its evidence succeeds.
var summary = new StringBuilder()
    .AppendLine($"## Coverage: {gate}")
    .AppendLine()
    .AppendLine($"Result: {(exitCode == 0 ? "passed" : "failed")} (exit code {exitCode}). Configuration: Debug.")
    .AppendLine()
    .AppendLine("```text");
foreach (var metric in metrics)
    summary.AppendLine(metric);
if (metrics.Count == 0)
    summary.AppendLine("No metrics were produced. See gate.log and any available test reports.");
summary.AppendLine("```");
if (gate == "vue-bindings")
    summary.AppendLine().AppendLine("Binding coverage audits public contracts; it is not runtime line/branch coverage.");

await File.WriteAllTextAsync(Path.Combine(resultRoot, "summary.md"), summary.ToString());
var githubSummary = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
if (!string.IsNullOrEmpty(githubSummary))
    await File.AppendAllTextAsync(githubSummary, summary.ToString());
Console.WriteLine($"Evidence: {resultRoot}");
return exitCode;

static string? ReadOption(string[] arguments, string option)
{
    var index = Array.IndexOf(arguments, option);
    return index >= 0 && index + 1 < arguments.Length ? arguments[index + 1] : null;
}

static void ConfigureProcessEnvironment(ProcessStartInfo startInfo, string repoRoot)
{
    startInfo.Environment["TEMP"] = TempDirectory(repoRoot);
    startInfo.Environment["TMP"] = TempDirectory(repoRoot);
    startInfo.Environment["DOTNET_CLI_HOME"] = Path.Combine(repoRoot, ".dotnet");
    startInfo.Environment["NUGET_PACKAGES"] = NuGetPackages(repoRoot);
    startInfo.Environment["NUGET_HTTP_CACHE_PATH"] = NuGetHttpCache(repoRoot);
    startInfo.Environment["DENO_DIR"] = DenoCache(repoRoot);
    startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
    startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
    startInfo.Environment["UseSharedCompilation"] = "false";
}

static void ConfigureRepositoryEnvironment(string repoRoot)
{
    foreach (var directory in new[] { TempDirectory(repoRoot), NuGetPackages(repoRoot), NuGetHttpCache(repoRoot), DenoCache(repoRoot) })
        Directory.CreateDirectory(directory);
    Environment.SetEnvironmentVariable("TEMP", TempDirectory(repoRoot));
    Environment.SetEnvironmentVariable("TMP", TempDirectory(repoRoot));
    Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", Path.Combine(repoRoot, ".dotnet"));
    Environment.SetEnvironmentVariable("NUGET_PACKAGES", NuGetPackages(repoRoot));
    Environment.SetEnvironmentVariable("NUGET_HTTP_CACHE_PATH", NuGetHttpCache(repoRoot));
    Environment.SetEnvironmentVariable("DENO_DIR", DenoCache(repoRoot));
}

static string TempDirectory(string repoRoot) => Path.Combine(repoRoot, ".tmp", "agent-temp");
static string NuGetPackages(string repoRoot) => Path.Combine(repoRoot, ".dotnet", ".nuget", "packages") + Path.DirectorySeparatorChar;
static string NuGetHttpCache(string repoRoot) => Path.Combine(repoRoot, ".tmp", "nuget-http-cache");
static string DenoCache(string repoRoot) => Path.Combine(repoRoot, ".tmp", "deno-cache");

async Task CopyOutputAsync(StreamReader reader, TextWriter destination)
{
    while (await reader.ReadLineAsync() is { } line)
    {
        lock (log)
        {
            destination.WriteLine(line);
            log.WriteLine(line);
            if (line.StartsWith("Tests:", StringComparison.Ordinal)
                || line.StartsWith("Lines:", StringComparison.Ordinal)
                || line.StartsWith("Branches:", StringComparison.Ordinal)
                || line.Contains(" tests:", StringComparison.Ordinal)
                || line.Contains("binding contract units =", StringComparison.Ordinal))
                metrics.Add(line);
        }
    }
}
