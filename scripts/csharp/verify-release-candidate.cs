#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

var options = CandidateOptions.Parse(args);
var repoRoot = RequireRepoRoot();
var version = NormalizeVersion(options.Tag);
var candidateRoot = ResolveInsideRepository(repoRoot, options.OutputDirectory ?? Path.Combine("artifacts", "release-candidate", SafeName(options.Tag)));
Directory.CreateDirectory(candidateRoot);
foreach (var directoryName in new[] { "logs", "packages", "coverage", "package-shape", "out", "obj" })
{
    var directory = Path.Combine(candidateRoot, directoryName);
    if (Directory.Exists(directory))
        Directory.Delete(directory, recursive: true);
}
foreach (var fileName in new[] { "public-api.md", "public-api-compatibility.md", "typed-bootstrap.md", "toolchain-matrix.md", "report.md" })
{
    var file = Path.Combine(candidateRoot, fileName);
    if (File.Exists(file))
        File.Delete(file);
}
var logRoot = Path.Combine(candidateRoot, "logs");
var packageRoot = Path.Combine(candidateRoot, "packages");
Directory.CreateDirectory(logRoot);
Directory.CreateDirectory(packageRoot);

var stages = new List<CandidateStage>();
var stageDefinitions = new (string Name, string[] Arguments)[]
{
    ("release-notes", ["run", "--file", "scripts/csharp/verify-release-notes.cs", "--", "--tag", options.Tag]),
    ("toolchain", ["run", "--file", "scripts/csharp/write-toolchain-matrix.cs", "--", Path.Combine(candidateRoot, "toolchain-matrix.md")]),
    ("build", ["build", "Jazor.slnx", "-c", "Release", "/m:1", "/nr:false", "-p:UseSharedCompilation=false"]),
    ("public-api", ["run", "--file", "scripts/csharp/inspect-public-api.cs", "--", "--configuration", "Release", "--output", Path.Combine(candidateRoot, "public-api.md")]),
    ("public-api-compatibility", ["run", "--file", "scripts/csharp/compare-public-api.cs", "--", "--current", Path.Combine(candidateRoot, "public-api.md"), "--baseline", "docs/03-guides/public-api-baseline.snapshot.md", "--output", Path.Combine(candidateRoot, "public-api-compatibility.md")]),
    ("compiler-coverage", ["run", "--file", "scripts/csharp/run-quality-gate.cs", "--", "compiler", "--output-directory", Path.Combine(candidateRoot, "coverage", "compiler")]),
    ("razorvue-coverage", ["run", "--file", "scripts/csharp/run-quality-gate.cs", "--", "razorvue", "--output-directory", Path.Combine(candidateRoot, "coverage", "razorvue")]),
    ("vue-binding-coverage", ["run", "--file", "scripts/csharp/run-quality-gate.cs", "--", "vue-bindings", "--output-directory", Path.Combine(candidateRoot, "coverage", "vue-bindings")]),
    ("binding-contracts", ["run", "--file", "scripts/csharp/verify-vue-binding-contracts.cs"]),
    ("typed-bootstrap", ["run", "--file", "scripts/csharp/verify-typed-bootstrap.cs", "--", "--report", Path.Combine(candidateRoot, "typed-bootstrap.md")]),
    ("mainline", ["run", "--file", "scripts/csharp/test-dotnet.cs", "--", "--configuration", "Release", "--base-output-path", Path.Combine(candidateRoot, "out"), "--base-intermediate-output-path", Path.Combine(candidateRoot, "obj")]),
    ("packages", ["run", "--file", "scripts/csharp/publish-nuget.cs", "--", "--configuration", "Release", "--output-directory", packageRoot, "--package-version", version, "--package", "jazor", "--package", "jazor-vue", "--package", "style", "--package", "admin", "--package", "devtools", "--package", "dataui", "--package", "vu-icons", "--package", "pinia", "--package", "pinia-testing", "--package", "vueroute", "--package", "vuetify", "--package", "elementplus", "--package", "tdesign", "--skip-push"]),
    ("package-shape", ["run", "--file", "scripts/csharp/verify-nuget-package.cs", "--", "--configuration", "Release", "--output-directory", Path.Combine(candidateRoot, "package-shape"), "--package-version", version, "--package", "Jazor", "--package", "Jazor.Vue"]),
    ("spa-consumer", ["run", "--file", "scripts/csharp/verify-windows-spa-release.cs", "--", "--package-source", packageRoot, "--skip-pack", "--path-base", "/docs", "--startup-timeout-seconds", "60", "--browser-startup-timeout-seconds", "20"]),
    ("ssr-consumer", ["run", "--file", "scripts/csharp/verify-windows-ssr-release.cs", "--", "--package-source", packageRoot, "--skip-pack", "--path-base", "/todo", "--startup-timeout-seconds", "90", "--browser-startup-timeout-seconds", "20"])
};
if (options.Only is not null)
{
    var knownStages = stageDefinitions.Select(static stage => stage.Name).ToHashSet(StringComparer.Ordinal);
    var unknownStages = options.Only.Where(stage => !knownStages.Contains(stage)).OrderBy(static stage => stage, StringComparer.Ordinal).ToArray();
    if (unknownStages.Length > 0)
        throw new InvalidOperationException("Unknown release-candidate stage(s): " + string.Join(", ", unknownStages));
}

try
{
    foreach (var (name, arguments) in stageDefinitions)
    {
        if (options.Only is not null && !options.Only.Contains(name, StringComparer.Ordinal))
            continue;

        var logPath = Path.Combine(logRoot, name + ".log");
        Console.WriteLine($"[release-candidate] {name}");
        var result = await RunStageAsync(arguments, repoRoot, logPath);
        stages.Add(result);
        if (!result.Passed && !options.ContinueOnFailure)
            break;
    }
}
finally
{
    var reportPath = Path.Combine(candidateRoot, "report.md");
    await File.WriteAllTextAsync(reportPath, BuildReport(options.Tag, version, stages));
    Console.WriteLine($"Release candidate report: {reportPath}");
}

if (stages.Any(static stage => !stage.Passed))
    Environment.ExitCode = 1;

static async Task<CandidateStage> RunStageAsync(IReadOnlyList<string> arguments, string workdir, string logPath)
{
    var stopwatch = Stopwatch.StartNew();
    var output = new StringBuilder();
    var startInfo = new ProcessStartInfo
    {
        FileName = "dotnet",
        WorkingDirectory = workdir,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };
    foreach (var argument in arguments)
        startInfo.ArgumentList.Add(argument);
    startInfo.Environment["DOTNET_CLI_HOME"] = Path.Combine(workdir, ".dotnet");
    startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
    startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
    startInfo.Environment["UseSharedCompilation"] = "false";

    try
    {
        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Unable to start dotnet.");
        var stdout = DrainAsync(process.StandardOutput, output, Console.Out);
        var stderr = DrainAsync(process.StandardError, output, Console.Error);
        await Task.WhenAll(stdout, stderr, process.WaitForExitAsync());
        stopwatch.Stop();
        await File.WriteAllTextAsync(logPath, output.ToString());
        return new CandidateStage(Path.GetFileNameWithoutExtension(logPath), process.ExitCode == 0, process.ExitCode, stopwatch.Elapsed, logPath);
    }
    catch (Exception exception)
    {
        stopwatch.Stop();
        output.AppendLine(exception.ToString());
        await File.WriteAllTextAsync(logPath, output.ToString());
        return new CandidateStage(Path.GetFileNameWithoutExtension(logPath), false, -1, stopwatch.Elapsed, logPath);
    }
}

static async Task DrainAsync(StreamReader reader, StringBuilder output, TextWriter console)
{
    while (await reader.ReadLineAsync() is { } line)
    {
        lock (output)
            output.AppendLine(line);
        console.WriteLine(line);
    }
}

static string BuildReport(string tag, string version, IReadOnlyList<CandidateStage> stages)
{
    var lines = new List<string>
    {
        "# Release Candidate Verification",
        "",
        $"- Tag: `{tag}`",
        $"- Package version: `{version}`",
        $"- Completed (UTC): `{DateTimeOffset.UtcNow:O}`",
        "",
        "| Stage | Result | Exit code | Elapsed | Log |",
        "| --- | --- | ---: | ---: | --- |"
    };
    foreach (var stage in stages)
    {
        var result = stage.Passed ? "passed" : "failed";
        lines.Add($"| `{stage.Name}` | {result} | {stage.ExitCode} | {stage.Elapsed.TotalSeconds:0.0}s | `{stage.LogPath.Replace('\\', '/')}` |");
    }
    lines.Add("");
    lines.Add(stages.Count == 0
        ? "No stages were selected."
        : stages.All(static stage => stage.Passed)
            ? "All selected release-candidate stages passed."
            : "One or more release-candidate stages failed; inspect the stage log before publishing.");
    return string.Join(Environment.NewLine, lines) + Environment.NewLine;
}

static string NormalizeVersion(string tag)
{
    var version = tag.StartsWith('v', StringComparison.OrdinalIgnoreCase) ? tag[1..] : tag;
    if (!Regex.IsMatch(version, "^[0-9]+\\.[0-9]+\\.[0-9]+(?:-[0-9A-Za-z.-]+)?(?:\\+[0-9A-Za-z.-]+)?$", RegexOptions.CultureInvariant))
        throw new InvalidOperationException("Tag must be a semantic version such as v1.0.0-rc.1.");
    return version;
}

static string SafeName(string value)
    => Regex.Replace(value, "[^A-Za-z0-9._-]", "-");

static string RequireRepoRoot()
{
    for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
    throw new InvalidOperationException("Unable to locate Jazor.slnx.");
}

static string ResolveInsideRepository(string repoRoot, string path)
{
    var full = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));
    var root = Path.GetFullPath(repoRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
    if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Release candidate output must stay inside the repository: " + full);
    return full;
}

sealed record CandidateStage(string Name, bool Passed, int ExitCode, TimeSpan Elapsed, string LogPath);

sealed record CandidateOptions(string Tag, string? OutputDirectory, IReadOnlySet<string>? Only, bool ContinueOnFailure)
{
    public static CandidateOptions Parse(string[] arguments)
    {
        string? tag = null;
        string? output = null;
        HashSet<string>? only = null;
        var continueOnFailure = false;
        for (var index = 0; index < arguments.Length; index++)
        {
            switch (arguments[index])
            {
                case "--tag": tag = Next(arguments, ref index, "--tag"); break;
                case "--output-directory": output = Next(arguments, ref index, "--output-directory"); break;
                case "--only":
                    only = new HashSet<string>(Next(arguments, ref index, "--only").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), StringComparer.Ordinal);
                    break;
                case "--continue-on-failure": continueOnFailure = true; break;
                case "--help":
                    Console.WriteLine("Usage: dotnet run --file scripts/csharp/verify-release-candidate.cs -- --tag v1.0.0-rc.1 [--output-directory DIR] [--only stage1,stage2] [--continue-on-failure]");
                    Environment.Exit(0);
                    break;
                default: throw new InvalidOperationException("Unknown argument: " + arguments[index]);
            }
        }
        return new CandidateOptions(tag ?? throw new InvalidOperationException("Missing --tag."), output, only, continueOnFailure);
    }

    private static string Next(string[] arguments, ref int index, string option)
        => ++index < arguments.Length ? arguments[index] : throw new InvalidOperationException("Missing value for " + option + ".");
}
