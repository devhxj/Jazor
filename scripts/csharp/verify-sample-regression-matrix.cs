#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Text;
using System.Text.Json;

var repoRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
var options = Options.Parse(args, repoRoot);
Directory.CreateDirectory(Path.GetDirectoryName(options.ReportPath)!);

var cases = new[]
{
    new Case("RazorVue.Authoring", Path.Combine("samples", "RazorVue.Authoring", "verify-smoke.cs"),
        ["--work-root", Path.Combine(options.WorkRoot, "authoring"), "--package-output", Path.Combine(options.WorkRoot, "authoring-packages")]),
    new Case("JazorAdmin", Path.Combine("samples", "JazorAdmin", "verify-smoke.cs"),
        ["--base-output-path", Path.Combine(options.WorkRoot, "admin-out"), "--base-intermediate-output-path", Path.Combine(options.WorkRoot, "admin-obj")])
};

var results = new List<Result>();
foreach (var testCase in cases)
{
    var arguments = new List<string> { "run", "--no-launch-profile", "--file", Path.Combine(repoRoot, testCase.Script), "--", "--configuration", options.Configuration };
    arguments.AddRange(testCase.Arguments);
    if (options.SkipBrowser)
        arguments.Add("--skip-browser");

    var stopwatch = Stopwatch.StartNew();
    var (exitCode, stdout, stderr) = await RunAsync(arguments, repoRoot);
    stopwatch.Stop();
    var result = new Result(testCase.Name, exitCode == 0 ? "passed" : "failed", stopwatch.Elapsed, string.Join(" ", arguments),
        Trim(stdout), Trim(stderr));
    results.Add(result);
    Console.WriteLine($"[{result.Status}] {result.Name} ({result.Duration.TotalSeconds:F1}s)");
    if (exitCode != 0)
        Console.Error.WriteLine(result.StandardError);
}

var report = new Report(DateTimeOffset.UtcNow, RunGit(repoRoot, "rev-parse", "HEAD"), options.Configuration, options.SkipBrowser, results);
var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
await File.WriteAllTextAsync(options.ReportPath, json, new UTF8Encoding(false));
await File.WriteAllTextAsync(Path.ChangeExtension(options.ReportPath, ".md"), ToMarkdown(report), new UTF8Encoding(false));
if (results.Any(result => result.Status == "failed"))
    Environment.ExitCode = 1;

static async Task<(int ExitCode, string Stdout, string Stderr)> RunAsync(IReadOnlyList<string> arguments, string workingDirectory)
{
    var start = new ProcessStartInfo("dotnet") { WorkingDirectory = workingDirectory, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
    foreach (var argument in arguments) start.ArgumentList.Add(argument);
    start.Environment["DOTNET_CLI_HOME"] = Path.Combine(workingDirectory, ".dotnet");
    start.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
    start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
    using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start dotnet.");
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    var output = await stdout;
    var error = await stderr;
    Console.Write(output);
    Console.Error.Write(error);
    return (process.ExitCode, output, error);
}

static string ToMarkdown(Report report)
{
    var builder = new StringBuilder().AppendLine("# Sample Regression Matrix").AppendLine()
        .AppendLine($"- UTC: `{report.Timestamp:O}`").AppendLine($"- Commit: `{report.Commit}`")
        .AppendLine($"- Configuration: `{report.Configuration}`").AppendLine($"- Browser: `{(!report.SkipBrowser ? "enabled" : "skipped")}`").AppendLine()
        .AppendLine("| Scenario | Status | Duration | Command |").AppendLine("| --- | --- | ---: | --- |");
    foreach (var result in report.Results)
        builder.AppendLine($"| `{result.Name}` | **{result.Status}** | {result.Duration.TotalSeconds:F1}s | `{result.Command.Replace("|", "\\|")}` |");
    return builder.ToString();
}

static string Trim(string value) => value.Length <= 4000 ? value : value[^4000..];
static string FindRepositoryRoot(string start)
{
    var current = new DirectoryInfo(Path.GetFullPath(start));
    while (current is not null)
    {
        if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx"))) return current.FullName;
        current = current.Parent;
    }
    throw new InvalidOperationException("Cannot locate repository root.");
}
static string RunGit(string root, params string[] args)
{
    var start = new ProcessStartInfo("git") { WorkingDirectory = root, UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true };
    foreach (var arg in args) start.ArgumentList.Add(arg);
    using var process = Process.Start(start)!;
    var output = process.StandardOutput.ReadToEnd(); process.WaitForExit();
    return process.ExitCode == 0 ? output.Trim() : "unavailable";
}

record Case(string Name, string Script, string[] Arguments);
record Result(string Name, string Status, TimeSpan Duration, string Command, string StandardOutput, string StandardError);
record Report(DateTimeOffset Timestamp, string Commit, string Configuration, bool SkipBrowser, IReadOnlyList<Result> Results);

sealed class Options
{
    public string Configuration { get; private set; } = "Release";
    public string WorkRoot { get; private set; } = ".tmp/sample-regression-matrix";
    public string ReportPath { get; private set; } = "artifacts/quality/sample-regression/report.json";
    public bool SkipBrowser { get; private set; }
    public static Options Parse(string[] args, string root)
    {
        var options = new Options();
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--configuration": options.Configuration = args[++i]; break;
                case "--work-root": options.WorkRoot = args[++i]; break;
                case "--report": options.ReportPath = args[++i]; break;
                case "--skip-browser": options.SkipBrowser = true; break;
                case "--help": Console.WriteLine("--configuration <Debug|Release> --work-root <path> --report <json> --skip-browser"); Environment.Exit(0); break;
                default: throw new ArgumentException("Unknown option: " + args[i]);
            }
        }
        options.WorkRoot = Path.GetFullPath(Path.IsPathRooted(options.WorkRoot) ? options.WorkRoot : Path.Combine(root, options.WorkRoot));
        options.ReportPath = Path.GetFullPath(Path.IsPathRooted(options.ReportPath) ? options.ReportPath : Path.Combine(root, options.ReportPath));
        return options;
    }
}
