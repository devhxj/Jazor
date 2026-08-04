#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;

const int minimumPassedTests = 10_000;
const double minimumLineRate = 0.98;
const double minimumBranchRate = 0.94;

try
{
    var options = CoverageGateOptions.Parse(args);
    var repoRoot = RequireRepoRoot();
    var resultBase = Path.GetFullPath(
        options.ResultsDirectory ?? Path.Combine(repoRoot, ".tmp", "compiler-coverage-gate"));
    var resultRoot = Path.Combine(resultBase, Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(resultRoot);

    var arguments = new List<string>
    {
        "test",
        Path.Combine(repoRoot, "src", "Jazor.CompilerTest", "Jazor.CompilerTest.csproj"),
        "--configuration",
        options.Configuration,
        "--settings",
        Path.Combine(repoRoot, "src", "Jazor.CompilerTest", "coverlet.runsettings"),
        "--collect:XPlat Code Coverage",
        "--logger",
        "trx;LogFileName=compiler.trx",
        "--results-directory",
        resultRoot,
        "--verbosity",
        "minimal"
    };
    if (options.NoBuild)
        arguments.Add("--no-build");
    if (options.NoRestore)
        arguments.Add("--no-restore");
    if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
        arguments.Add("-p:BaseOutputPath=" + Path.GetFullPath(options.BaseOutputPath));

    await RunDotNetAsync(arguments, repoRoot);

    var coveragePath = RequireCoverageFile(resultRoot);
    var trxPath = RequireSingleFile(resultRoot, "compiler.trx");
    var coverage = ReadCoverage(coveragePath);
    var tests = ReadTestCounters(trxPath);

    var failures = new List<string>();
    if (tests.Passed < minimumPassedTests)
        failures.Add($"passed tests {tests.Passed} are below {minimumPassedTests}");
    if (tests.Total != tests.Passed || tests.Failed != 0)
        failures.Add($"test counters are not clean: total={tests.Total}, passed={tests.Passed}, failed={tests.Failed}");
    if (coverage.LinesValid == 0 || coverage.BranchesValid == 0)
        failures.Add("coverage report contains no instrumented lines or branches");
    else if (coverage.LinesCovered < RequiredHits(coverage.LinesValid, minimumLineRate))
        failures.Add($"line coverage {FormatRate(coverage.LinesCovered, coverage.LinesValid)} is below {minimumLineRate:P0}");
    if (coverage.BranchesCovered < RequiredHits(coverage.BranchesValid, minimumBranchRate))
        failures.Add($"branch coverage {FormatRate(coverage.BranchesCovered, coverage.BranchesValid)} is below {minimumBranchRate:P0}");

    Console.WriteLine($"Tests:    {tests.Passed}/{tests.Total} passed (minimum {minimumPassedTests})");
    Console.WriteLine($"Lines:    {coverage.LinesCovered}/{coverage.LinesValid} = {FormatRate(coverage.LinesCovered, coverage.LinesValid)} (minimum {minimumLineRate:P0})");
    Console.WriteLine($"Branches: {coverage.BranchesCovered}/{coverage.BranchesValid} = {FormatRate(coverage.BranchesCovered, coverage.BranchesValid)} (minimum {minimumBranchRate:P0})");
    Console.WriteLine($"Report:   {coveragePath}");

    if (failures.Count > 0)
        throw new InvalidOperationException("Compiler coverage gate failed: " + string.Join("; ", failures));
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}

static int RequiredHits(int total, double minimumRate)
    => (int)Math.Ceiling(total * minimumRate);

static string FormatRate(int covered, int total)
    => total == 0
        ? "0.00%"
        : ((double)covered / total).ToString("P2", CultureInfo.InvariantCulture);

static CoverageMetrics ReadCoverage(string path)
{
    var root = XDocument.Load(path).Root
        ?? throw new InvalidOperationException($"Coverage report has no root element: {path}");
    return new CoverageMetrics(
        ReadIntAttribute(root, "lines-covered"),
        ReadIntAttribute(root, "lines-valid"),
        ReadIntAttribute(root, "branches-covered"),
        ReadIntAttribute(root, "branches-valid"));
}

static TestCounters ReadTestCounters(string path)
{
    var counters = XDocument.Load(path)
        .Descendants()
        .SingleOrDefault(static element => element.Name.LocalName == "Counters")
        ?? throw new InvalidOperationException($"TRX report has no Counters element: {path}");
    return new TestCounters(
        ReadIntAttribute(counters, "total"),
        ReadIntAttribute(counters, "passed"),
        ReadIntAttribute(counters, "failed"));
}

static int ReadIntAttribute(XElement element, string name)
{
    var value = element.Attribute(name)?.Value
        ?? throw new InvalidOperationException($"Element '{element.Name.LocalName}' is missing attribute '{name}'.");
    return int.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);
}

static string RequireSingleFile(string root, string fileName)
{
    var matches = Directory.GetFiles(root, fileName, SearchOption.AllDirectories);
    return matches.Length switch
    {
        1 => matches[0],
        0 => throw new InvalidOperationException($"'{fileName}' was not produced under '{root}'."),
        _ => throw new InvalidOperationException($"Expected one '{fileName}' under '{root}', found {matches.Length}.")
    };
}

static string RequireCoverageFile(string root)
{
    var matches = Directory
        .GetFiles(root, "coverage.cobertura.xml", SearchOption.AllDirectories)
        .Where(static path => Guid.TryParse(Path.GetFileName(Path.GetDirectoryName(path)), out _))
        .ToArray();
    return matches.Length switch
    {
        1 => matches[0],
        0 => throw new InvalidOperationException($"A coverlet-owned 'coverage.cobertura.xml' was not produced under '{root}'."),
        _ => throw new InvalidOperationException($"Expected one coverlet-owned report under '{root}', found {matches.Length}.")
    };
}

static async Task RunDotNetAsync(IReadOnlyList<string> arguments, string workingDirectory)
{
    var startInfo = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = workingDirectory,
        UseShellExecute = false
    };
    startInfo.Environment["DOTNET_CLI_HOME"] = Path.Combine(workingDirectory, ".dotnet");
    foreach (var argument in arguments)
        startInfo.ArgumentList.Add(argument);

    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException("Failed to start dotnet.");
    await process.WaitForExitAsync();
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"dotnet {string.Join(' ', arguments)} failed with exit code {process.ExitCode}.");
}

static string RequireRepoRoot()
{
    for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
    }

    throw new InvalidOperationException("Repository root containing Jazor.slnx was not found.");
}

internal sealed record CoverageMetrics(
    int LinesCovered,
    int LinesValid,
    int BranchesCovered,
    int BranchesValid);

internal sealed record TestCounters(int Total, int Passed, int Failed);

internal sealed record CoverageGateOptions
{
    public string Configuration { get; init; } = "Debug";

    public bool NoBuild { get; init; }

    public bool NoRestore { get; init; }

    public string? ResultsDirectory { get; init; }

    public string? BaseOutputPath { get; init; }

    public static CoverageGateOptions Parse(string[] args)
    {
        var result = new CoverageGateOptions();
        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--configuration":
                case "-c":
                    result = result with { Configuration = ReadValue(args, ref index) };
                    break;
                case "--no-build":
                    result = result with { NoBuild = true };
                    break;
                case "--no-restore":
                    result = result with { NoRestore = true };
                    break;
                case "--results-directory":
                    result = result with { ResultsDirectory = ReadValue(args, ref index) };
                    break;
                case "--base-output-path":
                    result = result with { BaseOutputPath = ReadValue(args, ref index) };
                    break;
                default:
                    throw new InvalidOperationException("Unknown argument: " + args[index]);
            }
        }

        if (result.Configuration is not "Debug" and not "Release")
            throw new InvalidOperationException("--configuration must be Debug or Release.");
        if (result.NoBuild && !result.NoRestore)
            result = result with { NoRestore = true };
        return result;
    }

    private static string ReadValue(string[] args, ref int index)
    {
        if (++index >= args.Length)
            throw new InvalidOperationException("Missing value for " + args[index - 1]);
        return args[index];
    }
}
