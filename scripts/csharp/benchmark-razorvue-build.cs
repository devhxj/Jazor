#!/usr/bin/env dotnet run
#:property NoWarn=IL2026;IL3050

using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

var options = BuildBenchmarkOptions.Parse(args);
var repoRoot = RequireRepositoryRoot();
var workRoot = ResolveInsideRepository(repoRoot, options.WorkRoot ?? Path.Combine(".tmp", "razorvue-build-benchmark"));
var reportPath = ResolveInsideRepository(repoRoot, options.Output ?? Path.Combine(workRoot, "report.json"));
if (Directory.Exists(workRoot))
    Directory.Delete(workRoot, recursive: true);
Directory.CreateDirectory(workRoot);

var measurements = new List<BuildMeasurement>();
BuildArtifactSnapshot? previousArtifact = null;
var sourceRoot = Path.Combine(workRoot, "source");
var sourceOutput = Path.Combine(sourceRoot, "bin");
var sourceIntermediate = Path.Combine(sourceRoot, "obj");
var projectPath = Path.Combine(repoRoot, "samples", "RazorVue.Authoring", "RazorVue.Authoring.csproj");

for (var sample = 1; sample <= options.Samples; sample++)
{
    var clean = await MeasureAsync("clean", sample, [
        "build", projectPath, "-c", "Debug", "-t:Rebuild", "/m:1", "/nr:false", "-p:UseSharedCompilation=false",
        "-p:AuthoringUsePackages=false", "-p:JazorDir=" + Path.Combine(sourceRoot, "jazor"),
        "-p:JazorIsolatedBaseOutputRoot=" + EnsureTrailingSeparator(sourceOutput),
        "-p:JazorIsolatedBaseIntermediateOutputRoot=" + EnsureTrailingSeparator(sourceIntermediate)
    ], repoRoot);
    clean = clean with { Artifact = CaptureArtifact(Path.Combine(sourceRoot, "jazor"), previousArtifact) };
    previousArtifact = clean.Artifact;
    measurements.Add(clean);

    var incremental = await MeasureAsync("incremental", sample, [
        "build", projectPath, "-c", "Debug", "--no-restore", "/m:1", "/nr:false", "-p:UseSharedCompilation=false",
        "-p:AuthoringUsePackages=false", "-p:JazorDir=" + Path.Combine(sourceRoot, "jazor"),
        "-p:JazorIsolatedBaseOutputRoot=" + EnsureTrailingSeparator(sourceOutput),
        "-p:JazorIsolatedBaseIntermediateOutputRoot=" + EnsureTrailingSeparator(sourceIntermediate)
    ], repoRoot);
    incremental = incremental with { Artifact = CaptureArtifact(Path.Combine(sourceRoot, "jazor"), previousArtifact) };
    previousArtifact = incremental.Artifact;
    measurements.Add(incremental);
}

if (!options.SkipHmr)
    for (var sample = 1; sample <= options.Samples; sample++)
        measurements.Add(await MeasureAsync("hmr", sample, ["run", "--file", Path.Combine(repoRoot, "scripts", "csharp", "verify-development-hmr.cs")], repoRoot));

if (!options.SkipRelease)
    for (var sample = 1; sample <= options.Samples; sample++)
        measurements.Add(await MeasureAsync("release", sample, [
            "run", "--file", Path.Combine(repoRoot, "samples", "RazorVue.Authoring", "build-local.cs"), "--",
            "--configuration", "Release", "--work-root", Path.Combine(workRoot, "release-" + sample.ToString(CultureInfo.InvariantCulture))
        ], repoRoot));

var report = new BuildBenchmarkReport(
    "razorvue-build-v2",
    DateTimeOffset.UtcNow,
    Environment.Version.ToString(),
    measurements);
Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
Console.WriteLine($"RazorVue build benchmark passed: {reportPath}");
foreach (var measurement in measurements)
    Console.WriteLine($"  {measurement.Name}#{measurement.Sample}: {measurement.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)} ms" +
        (measurement.Artifact is null ? string.Empty : $", {measurement.Artifact.GeneratedModuleCount} generated modules, {measurement.Artifact.TotalBytes.ToString(CultureInfo.InvariantCulture)} bytes"));
foreach (var group in measurements.GroupBy(static measurement => measurement.Name, StringComparer.Ordinal))
    Console.WriteLine($"  {group.Key} median: {Median(group.Select(static measurement => measurement.ElapsedMilliseconds)):0} ms");

static async Task<BuildMeasurement> MeasureAsync(string name, int sample, IReadOnlyList<string> arguments, string workdir)
{
    var stopwatch = Stopwatch.StartNew();
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet", WorkingDirectory = workdir, UseShellExecute = false,
            RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
        }
    };
    foreach (var argument in arguments)
        process.StartInfo.ArgumentList.Add(argument);
    process.Start();
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    stopwatch.Stop();
    var output = await stdout;
    var error = await stderr;
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"{name} benchmark failed (exit {process.ExitCode}).{Environment.NewLine}{output}{Environment.NewLine}{error}");
    return new BuildMeasurement(name, sample, stopwatch.ElapsedMilliseconds, null);
}

static BuildArtifactSnapshot CaptureArtifact(string outputRoot, BuildArtifactSnapshot? previous)
{
    if (!Directory.Exists(outputRoot))
        return new BuildArtifactSnapshot(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, null, null);

    var files = Directory.EnumerateFiles(outputRoot, "*", SearchOption.AllDirectories)
        .Select(path => new FileArtifact(Path.GetRelativePath(outputRoot, path).Replace('\\', '/'), new FileInfo(path).Length))
        .OrderBy(static file => file.Path, StringComparer.Ordinal)
        .ToArray();
    var generatedModules = ReadManifestModuleCount(outputRoot);
    var mjsFiles = files.Where(static file => file.Path.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase)).ToArray();
    var mapFiles = files.Where(static file => file.Path.EndsWith(".map", StringComparison.OrdinalIgnoreCase)).ToArray();
    var manifestFiles = files.Where(static file => string.Equals(Path.GetFileName(file.Path), "jazor-manifest.json", StringComparison.OrdinalIgnoreCase)).ToArray();
    var maps = files.Count(static file => file.Path.EndsWith(".map", StringComparison.OrdinalIgnoreCase));
    var moduleBytes = mjsFiles.Sum(static file => file.Bytes);
    var mapBytes = mapFiles.Sum(static file => file.Bytes);
    var manifestBytes = manifestFiles.Sum(static file => file.Bytes);
    var moduleGzipBytes = mjsFiles.Sum(file => GzipLength(Path.Combine(outputRoot, file.Path)));
    var mapGzipBytes = mapFiles.Sum(file => GzipLength(Path.Combine(outputRoot, file.Path)));
    var manifestGzipBytes = manifestFiles.Sum(file => GzipLength(Path.Combine(outputRoot, file.Path)));
    var totalGzipBytes = files.Sum(file => GzipLength(Path.Combine(outputRoot, file.Path)));
    var previousFiles = previous?.Files?.ToDictionary(static file => file.Path, StringComparer.Ordinal);
    int? changed = previousFiles is null ? null : files.Count(file => !previousFiles.TryGetValue(file.Path, out var old) || old.Bytes != file.Bytes);
    long? changedBytes = previousFiles is null ? null : files.Where(file => !previousFiles.TryGetValue(file.Path, out var old) || old.Bytes != file.Bytes).Sum(static file => file.Bytes);
    return new BuildArtifactSnapshot(generatedModules, mjsFiles.Length, maps, moduleBytes, mapBytes, manifestBytes,
        moduleGzipBytes, mapGzipBytes, manifestGzipBytes, totalGzipBytes, files.Sum(static file => file.Bytes), changed, changedBytes, files);
}

static int ReadManifestModuleCount(string outputRoot)
{
    var path = Path.Combine(outputRoot, "jazor-manifest.json");
    if (!File.Exists(path))
        return 0;
    using var document = JsonDocument.Parse(File.ReadAllText(path));
    return document.RootElement.TryGetProperty("modules", out var modules) && modules.ValueKind == JsonValueKind.Array
        ? modules.GetArrayLength()
        : 0;
}

static long GzipLength(string path)
{
    using var input = File.OpenRead(path);
    using var output = new MemoryStream();
    using (var gzip = new GZipStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
        input.CopyTo(gzip);
    return output.Length;
}

static double Median(IEnumerable<long> values)
{
    var ordered = values.OrderBy(static value => value).ToArray();
    return ordered.Length % 2 == 1
        ? ordered[ordered.Length / 2]
        : (ordered[ordered.Length / 2 - 1] + ordered[ordered.Length / 2]) / 2d;
}

static string RequireRepositoryRoot()
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
        throw new InvalidOperationException("Benchmark path must stay inside the repository: " + full);
    return full;
}

static string EnsureTrailingSeparator(string path)
    => path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;

sealed record FileArtifact(string Path, long Bytes);
sealed record BuildArtifactSnapshot(int GeneratedModuleCount, int MjsFileCount, int SourceMapCount, long MjsBytes, long SourceMapBytes, long ManifestBytes, long MjsGzipBytes, long SourceMapGzipBytes, long ManifestGzipBytes, long TotalGzipBytes, long TotalBytes, int? ChangedFileCount, long? ChangedBytes, [property: JsonIgnore] IReadOnlyList<FileArtifact>? Files = null);
sealed record BuildMeasurement(string Name, int Sample, long ElapsedMilliseconds, BuildArtifactSnapshot? Artifact);
sealed record BuildBenchmarkReport(string SchemaVersion, DateTimeOffset StartedAt, string DotnetVersion, IReadOnlyList<BuildMeasurement> Measurements);

sealed record BuildBenchmarkOptions(string? WorkRoot, string? Output, int Samples, bool SkipHmr, bool SkipRelease)
{
    public static BuildBenchmarkOptions Parse(string[] args)
    {
        string? workRoot = null;
        string? output = null;
        var samples = 3;
        var skipHmr = false;
        var skipRelease = false;
        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--work-root": workRoot = Next(args, ref index); break;
                case "--out": output = Next(args, ref index); break;
                case "--samples": samples = ParsePositiveInt(Next(args, ref index), "--samples"); break;
                case "--skip-hmr": skipHmr = true; break;
                case "--skip-release": skipRelease = true; break;
                case "--help":
                    Console.WriteLine("Usage: dotnet run --file scripts/csharp/benchmark-razorvue-build.cs -- [--work-root DIR] [--out FILE] [--samples N] [--skip-hmr] [--skip-release]");
                    Environment.Exit(0);
                    break;
                default: throw new InvalidOperationException("Unknown argument: " + args[index]);
            }
        }
        return new BuildBenchmarkOptions(workRoot, output, samples, skipHmr, skipRelease);
    }

    private static string Next(string[] args, ref int index)
        => ++index < args.Length ? args[index] : throw new InvalidOperationException("Missing option value.");

    private static int ParsePositiveInt(string value, string option)
        => int.TryParse(value, out var result) && result > 0 ? result : throw new InvalidOperationException(option + " must be a positive integer.");
}
