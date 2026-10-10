#!/usr/bin/env dotnet run
#:property NoWarn=IL2026;IL3050

using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var options = BuildBenchmarkOptions.Parse(args);
var repoRoot = RequireRepositoryRoot();
var startedAt = DateTimeOffset.UtcNow;
var sdkVersion = await ReadCommandAsync("dotnet", ["--version"], repoRoot);
var gitCommit = await ReadCommandAsync("git", ["rev-parse", "HEAD"], repoRoot);
var browserObservations = options.BrowserObservations is null ? null
    : JsonSerializer.Deserialize<BrowserObservation[]>(await File.ReadAllTextAsync(options.BrowserObservations))!;
ConsumerProjectInfo? consumer = null;
if (options.ReleaseArtifacts is not null)
{
    // Read-only consumer mode must run before benchmark workspace cleanup/environment changes.
    // 只统计 dist；不扫描 node_modules 或作者源码，也不把静态闭包当成首屏实测。
    var distRoot = Path.GetFullPath(options.ReleaseArtifacts);
    var artifactReportPath = Path.GetFullPath(options.Output ?? Path.Combine(repoRoot, ".tmp", "razorvue-build-benchmark", "release-report.json"));
    if (artifactReportPath.StartsWith(distRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        throw new InvalidOperationException("Report output must be outside the read-only Release artifact directory.");
    var timer = Stopwatch.StartNew();
    var artifact = CaptureReleaseArtifact(distRoot);
    var artifactReport = CreateReport([new BuildMeasurement("artifact-scan", 1, timer.ElapsedMilliseconds, null, artifact)]);
    await WriteReportAsync(artifactReportPath, artifactReport);
    return;
}
// A real consumer must retain its restored candidate packages and cache configuration.
// Source-sample cache settings below would select a different NuGet package store.
if (options.ConsumerProject is null)
    ConfigureRepositoryEnvironment(repoRoot);
var workRoot = ResolveInsideRepository(repoRoot, options.WorkRoot ?? Path.Combine(".tmp", "razorvue-build-benchmark"));
var reportPath = ResolveInsideRepository(repoRoot, options.Output ?? Path.Combine(workRoot, "report.json"));
if (Directory.Exists(workRoot))
    Directory.Delete(workRoot, recursive: true);
Directory.CreateDirectory(workRoot);

if (options.ConsumerProject is not null)
{
    if (!options.SkipHmr)
        throw new ArgumentException("--consumer-project requires --skip-hmr; measure consumer HMR in its running browser.");
    var project = Path.GetFullPath(options.ConsumerProject);
    var projectDirectory = Path.GetDirectoryName(project)!;
    var intermediate = await ReadCommandAsync("dotnet", ["msbuild", project, "-nologo", "-p:Configuration=Debug", "-getProperty:IntermediateOutputPath"], projectDirectory);
    var manifest = Path.GetFullPath(Path.Combine(projectDirectory, intermediate, "jazor-manifest.json"));
    consumer = new ConsumerProjectInfo(project, await ReadCommandAsync("git", ["rev-parse", "HEAD"], projectDirectory), manifest,
        "Restored .NET packages and warm download caches; clean uses Rebuild and a fresh frontend directory; Release measures publish into a fresh directory.");
    var consumerMeasurements = new List<BuildMeasurement>();
    for (var sample = 1; sample <= options.Samples; sample++)
    {
        var sampleRoot = Path.Combine(workRoot, "consumer-" + sample);
        var frontend = Path.Combine(sampleRoot, "debug-project", "jazor");
        var output = EnsureTrailingSeparator(Path.Combine(sampleRoot, "debug-bin"));
        string[] BuildArguments(bool rebuild) => ["build", project, "-c", "Debug", "--no-restore", "/m:1", "/nr:false",
            "-p:UseSharedCompilation=false", "-p:OutDir=" + output, "-p:JazorDir=" + frontend, .. rebuild ? new[] { "-t:Rebuild" } : Array.Empty<string>()];
        var clean = await MeasureAsync("clean", sample, BuildArguments(rebuild: true), projectDirectory);
        clean = clean with { Artifact = CaptureArtifact(frontend, manifest, null) };
        consumerMeasurements.Add(clean);
        var incremental = await MeasureAsync("incremental", sample, BuildArguments(rebuild: false), projectDirectory);
        consumerMeasurements.Add(incremental with { Artifact = CaptureArtifact(frontend, manifest, clean.Artifact) });
        if (!options.SkipRelease)
        {
            var publish = Path.Combine(sampleRoot, "publish");
            var release = await MeasureAsync("release", sample, ["publish", project, "-c", "Release", "--no-restore", "/m:1", "/nr:false",
                "-p:UseSharedCompilation=false", "-p:OutDir=" + EnsureTrailingSeparator(Path.Combine(sampleRoot, "release-bin")),
                "-p:JazorDir=" + Path.Combine(sampleRoot, "release-project", "jazor"), "-o", publish], projectDirectory);
            consumerMeasurements.Add(release with { ReleaseArtifact = CaptureReleaseArtifact(Path.Combine(publish, "jazor", "dist")) });
        }
    }
    await WriteReportAsync(reportPath, CreateReport(consumerMeasurements));
    return;
}

var measurements = new List<BuildMeasurement>();
BuildArtifactSnapshot? previousArtifact = null;
var sourceRoot = Path.Combine(workRoot, "source");
var sourceOutput = Path.Combine(sourceRoot, "bin");
var sourceIntermediate = Path.Combine(sourceRoot, "obj");
var projectPath = Path.Combine(repoRoot, "samples", "RazorVue.Authoring", "RazorVue.Authoring.csproj");
var applicationManifestPath = Path.Combine(sourceIntermediate, "RazorVue.Authoring", "obj", "Debug", "net11.0", "jazor-manifest.json");

for (var sample = 1; sample <= options.Samples; sample++)
{
    var clean = await MeasureAsync("clean", sample, [
        "build", projectPath, "-c", "Debug", "-t:Rebuild", "/m:1", "/nr:false", "-p:UseSharedCompilation=false",
        "-p:AuthoringUsePackages=false", "-p:JazorDir=" + Path.Combine(sourceRoot, "jazor"),
        "-p:JazorIsolatedBaseOutputRoot=" + EnsureTrailingSeparator(sourceOutput),
        "-p:JazorIsolatedBaseIntermediateOutputRoot=" + EnsureTrailingSeparator(sourceIntermediate)
    ], repoRoot);
    clean = clean with { Artifact = CaptureArtifact(Path.Combine(sourceRoot, "jazor"), applicationManifestPath, previousArtifact) };
    previousArtifact = clean.Artifact;
    measurements.Add(clean);

    var incremental = await MeasureAsync("incremental", sample, [
        "build", projectPath, "-c", "Debug", "--no-restore", "/m:1", "/nr:false", "-p:UseSharedCompilation=false",
        "-p:AuthoringUsePackages=false", "-p:JazorDir=" + Path.Combine(sourceRoot, "jazor"),
        "-p:JazorIsolatedBaseOutputRoot=" + EnsureTrailingSeparator(sourceOutput),
        "-p:JazorIsolatedBaseIntermediateOutputRoot=" + EnsureTrailingSeparator(sourceIntermediate)
    ], repoRoot);
    incremental = incremental with { Artifact = CaptureArtifact(Path.Combine(sourceRoot, "jazor"), applicationManifestPath, previousArtifact) };
    previousArtifact = incremental.Artifact;
    measurements.Add(incremental);
}

if (!options.SkipHmr)
    for (var sample = 1; sample <= options.Samples; sample++)
        measurements.Add(await MeasureAsync("hmr", sample, ["run", "--file", Path.Combine(repoRoot, "scripts", "csharp", "verify-development-hmr.cs")], repoRoot));

if (!options.SkipRelease)
    for (var sample = 1; sample <= options.Samples; sample++)
    {
        var releaseRoot = Path.Combine(workRoot, "release-" + sample.ToString(CultureInfo.InvariantCulture));
        var measurement = await MeasureAsync("release", sample, [
            "run", "--file", Path.Combine(repoRoot, "samples", "RazorVue.Authoring", "build-local.cs"), "--",
            "--configuration", "Release", "--work-root", releaseRoot
        ], repoRoot);
        measurements.Add(measurement with { ReleaseArtifact = CaptureReleaseArtifact(Path.Combine(releaseRoot, "release-jazor", "dist")) });
    }

await WriteReportAsync(reportPath, CreateReport(measurements));
foreach (var measurement in measurements)
    Console.WriteLine($"  {measurement.Name}#{measurement.Sample}: {measurement.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)} ms" +
        (measurement.Artifact is null ? string.Empty : $", {measurement.Artifact.GeneratedModuleCount} generated modules, {measurement.Artifact.TotalBytes.ToString(CultureInfo.InvariantCulture)} bytes"));
foreach (var group in measurements.GroupBy(static measurement => measurement.Name, StringComparer.Ordinal))
    Console.WriteLine($"  {group.Key} median: {Median(group.Select(static measurement => measurement.ElapsedMilliseconds)):0} ms");

BuildBenchmarkReport CreateReport(IReadOnlyList<BuildMeasurement> values)
    => new("razorvue-build-v4", startedAt, Environment.Version.ToString(), sdkVersion, gitCommit,
        RuntimeInformation.OSDescription, RuntimeInformation.ProcessArchitecture.ToString(), options, values, consumer, browserObservations);

static async Task WriteReportAsync(string path, BuildBenchmarkReport report)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    await File.WriteAllTextAsync(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
    var markdownPath = Path.ChangeExtension(path, ".md");
    await File.WriteAllTextAsync(markdownPath, ToMarkdown(report));
    Console.WriteLine($"RazorVue benchmark report: {path}");
    Console.WriteLine($"RazorVue benchmark summary: {markdownPath}");
}

static async Task<string> ReadCommandAsync(string command, string[] arguments, string workdir)
{
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo(command)
        {
            WorkingDirectory = workdir, UseShellExecute = false,
            RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
        }
    };
    foreach (var argument in arguments)
        process.StartInfo.ArgumentList.Add(argument);
    process.Start();
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    var output = await stdout;
    var error = await stderr;
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"{command} exited with code {process.ExitCode}: {error}");
    return output.Trim();
}

static async Task<BuildMeasurement> MeasureAsync(string name, int sample, IReadOnlyList<string> arguments, string workdir)
{
    Console.WriteLine($"[{name}#{sample}] started");
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
    var stdout = ReadStreamAsync(process.StandardOutput);
    var stderr = ReadStreamAsync(process.StandardError);
    await process.WaitForExitAsync();
    stopwatch.Stop();
    var output = await stdout;
    var error = await stderr;
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"{name} benchmark failed (exit {process.ExitCode}).{Environment.NewLine}{output}{Environment.NewLine}{error}");
    return new BuildMeasurement(name, sample, stopwatch.ElapsedMilliseconds, null);

    async Task<string> ReadStreamAsync(StreamReader reader)
    {
        var captured = new StringBuilder();
        while (await reader.ReadLineAsync() is { } line)
        {
            captured.AppendLine(line);
            Console.WriteLine($"[{name}#{sample}] {line}");
        }
        return captured.ToString();
    }
}

static ReleaseArtifactSnapshot CaptureReleaseArtifact(string distRoot)
{
    using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(distRoot, "manifest.json")));
    var manifest = document.RootElement;
    var entries = manifest.EnumerateObject()
        .Where(static item => item.Value.TryGetProperty("isEntry", out var flag) && flag.GetBoolean())
        .Select(static item => item.Name).ToArray();
    if (entries.Length == 0)
        throw new InvalidOperationException("Release manifest contains no entry.");

    var entryFiles = entries.Select(key => manifest.GetProperty(key).GetProperty("file").GetString()!).ToHashSet(StringComparer.Ordinal);
    var staticModules = CollectModules(includeDynamic: false);
    var staticFiles = CollectFiles(staticModules);
    var reachableFiles = CollectFiles(CollectModules(includeDynamic: true));
    var files = Directory.EnumerateFiles(distRoot, "*", SearchOption.AllDirectories)
        .Select(path =>
        {
            var relativePath = Path.GetRelativePath(distRoot, path).Replace('\\', '/');
            var group = entryFiles.Contains(relativePath) ? "entry"
                : staticFiles.Contains(relativePath) ? "static-dependency"
                : reachableFiles.Contains(relativePath) ? "lazy"
                : relativePath.EndsWith(".map", StringComparison.Ordinal) ? "source-map"
                : relativePath == "manifest.json" ? "metadata" : "other";
            return new ReleaseFileArtifact(relativePath, group, new FileInfo(path).Length, GzipLength(path));
        })
        .OrderBy(static file => file.Path, StringComparer.Ordinal).ToArray();

    // Manifest files must exist; an incomplete publish must fail instead of understating size.
    var existingFiles = files.Select(static file => file.Path).ToHashSet(StringComparer.Ordinal);
    foreach (var path in reachableFiles)
        if (!existingFiles.Contains(path))
            throw new FileNotFoundException($"Release manifest resource is missing: {path}", Path.Combine(distRoot, path));
    return new ReleaseArtifactSnapshot(distRoot, files);

    HashSet<string> CollectModules(bool includeDynamic)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in entries)
            Visit(entry);
        return visited;

        void Visit(string key)
        {
            // A shared/cyclic import is counted once; static reachability wins over lazy edges.
            if (!visited.Add(key))
                return;
            var module = manifest.GetProperty(key);
            foreach (var import in ReadPaths(module, "imports"))
                Visit(import);
            if (includeDynamic)
                foreach (var import in ReadPaths(module, "dynamicImports"))
                    Visit(import);
        }
    }

    HashSet<string> CollectFiles(HashSet<string> modules)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var key in modules)
        {
            var module = manifest.GetProperty(key);
            result.Add(module.GetProperty("file").GetString()!);
            result.UnionWith(ReadPaths(module, "css"));
            result.UnionWith(ReadPaths(module, "assets"));
        }
        return result;
    }

    static IEnumerable<string> ReadPaths(JsonElement module, string name)
        => module.TryGetProperty(name, out var paths)
            ? paths.EnumerateArray().Select(static path => path.GetString()!) : [];
}

static BuildArtifactSnapshot CaptureArtifact(string outputRoot, string manifestPath, BuildArtifactSnapshot? previous)
{
    if (!Directory.Exists(outputRoot))
        return new BuildArtifactSnapshot(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, null, null);

    // Restored node_modules is an input/cache, not generated output. MSBuild writes the
    // application manifest into obj; include it explicitly rather than silently counting zero.
    var files = EnumerateOutputFiles(outputRoot)
        .Select(path => new FileArtifact(Path.GetRelativePath(outputRoot, path).Replace('\\', '/'), new FileInfo(path).Length, path))
        .Append(new FileArtifact("jazor-manifest.json", new FileInfo(manifestPath).Length, manifestPath))
        .OrderBy(static file => file.Path, StringComparer.Ordinal)
        .ToArray();
    var generatedModules = ReadManifestModuleCount(manifestPath);
    var mjsFiles = files.Where(static file => file.Path.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase)).ToArray();
    var mapFiles = files.Where(static file => file.Path.EndsWith(".map", StringComparison.OrdinalIgnoreCase)).ToArray();
    var manifestFiles = files.Where(static file => string.Equals(Path.GetFileName(file.Path), "jazor-manifest.json", StringComparison.OrdinalIgnoreCase)).ToArray();
    var maps = files.Count(static file => file.Path.EndsWith(".map", StringComparison.OrdinalIgnoreCase));
    var moduleBytes = mjsFiles.Sum(static file => file.Bytes);
    var mapBytes = mapFiles.Sum(static file => file.Bytes);
    var manifestBytes = manifestFiles.Sum(static file => file.Bytes);
    var moduleGzipBytes = mjsFiles.Sum(file => GzipLength(file.FullPath));
    var mapGzipBytes = mapFiles.Sum(file => GzipLength(file.FullPath));
    var manifestGzipBytes = manifestFiles.Sum(file => GzipLength(file.FullPath));
    var totalGzipBytes = files.Sum(file => GzipLength(file.FullPath));
    var previousFiles = previous?.Files?.ToDictionary(static file => file.Path, StringComparer.Ordinal);
    int? changed = previousFiles is null ? null : files.Count(file => !previousFiles.TryGetValue(file.Path, out var old) || old.Bytes != file.Bytes);
    long? changedBytes = previousFiles is null ? null : files.Where(file => !previousFiles.TryGetValue(file.Path, out var old) || old.Bytes != file.Bytes).Sum(static file => file.Bytes);
    return new BuildArtifactSnapshot(generatedModules, mjsFiles.Length, maps, moduleBytes, mapBytes, manifestBytes,
        moduleGzipBytes, mapGzipBytes, manifestGzipBytes, totalGzipBytes, files.Sum(static file => file.Bytes), changed, changedBytes, files);
}

static IEnumerable<string> EnumerateOutputFiles(string directory)
{
    foreach (var path in Directory.EnumerateFiles(directory))
        yield return path;
    foreach (var child in Directory.EnumerateDirectories(directory))
        if (Path.GetFileName(child) != "node_modules")
            foreach (var path in EnumerateOutputFiles(child))
                yield return path;
}

static int ReadManifestModuleCount(string path)
{
    using var document = JsonDocument.Parse(File.ReadAllText(path));
    return document.RootElement.GetProperty("modules").GetArrayLength();
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

static void ConfigureRepositoryEnvironment(string repoRoot)
{
    var temp = Path.Combine(repoRoot, ".tmp", "agent-temp");
    var dotnet = Path.Combine(repoRoot, ".dotnet");
    var nuget = Path.Combine(dotnet, ".nuget", "packages") + Path.DirectorySeparatorChar;
    var nugetHttp = Path.Combine(repoRoot, ".tmp", "nuget-http-cache");
    var deno = Path.Combine(repoRoot, ".tmp", "deno-cache");
    var npm = Path.Combine(repoRoot, ".tmp", "npm-cache");
    foreach (var directory in new[] { temp, dotnet, nuget, nugetHttp, deno, npm })
        Directory.CreateDirectory(directory);

    Environment.SetEnvironmentVariable("TEMP", temp);
    Environment.SetEnvironmentVariable("TMP", temp);
    Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", dotnet);
    Environment.SetEnvironmentVariable("NUGET_PACKAGES", nuget);
    Environment.SetEnvironmentVariable("NUGET_HTTP_CACHE_PATH", nugetHttp);
    Environment.SetEnvironmentVariable("DENO_DIR", deno);
    Environment.SetEnvironmentVariable("NPM_CONFIG_CACHE", npm);
    Environment.SetEnvironmentVariable("npm_config_cache", npm);
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

static string ToMarkdown(BuildBenchmarkReport report)
{
    var lines = new List<string>
    {
        "# RazorVue Build Benchmark",
        "",
        $"- Schema: `{report.SchemaVersion}`",
        $"- Started (UTC): `{report.StartedAt:O}`",
        $"- .NET: `{report.DotnetVersion}`",
        $"- SDK: `{report.SdkVersion}`",
        $"- Commit: `{report.GitCommit}`",
        $"- Platform: `{report.OperatingSystem}` / `{report.Architecture}`",
        "",
        "| Scenario | Sample | Elapsed (ms) | Modules | Source maps | Total bytes | Total gzip bytes | Changed files | Changed bytes |",
        "| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |"
    };
    foreach (var measurement in report.Measurements)
    {
        var artifact = measurement.Artifact;
        lines.Add($"| {measurement.Name} | {measurement.Sample} | {measurement.ElapsedMilliseconds} | {artifact?.GeneratedModuleCount.ToString() ?? ""} | {artifact?.SourceMapCount.ToString() ?? ""} | {artifact?.TotalBytes.ToString() ?? ""} | {artifact?.TotalGzipBytes.ToString() ?? ""} | {artifact?.ChangedFileCount?.ToString() ?? ""} | {artifact?.ChangedBytes?.ToString() ?? ""} |");
    }
    lines.Add("");
    lines.Add("Median elapsed time by scenario:");
    foreach (var group in report.Measurements.GroupBy(static measurement => measurement.Name, StringComparer.Ordinal))
        lines.Add($"- `{group.Key}`: {Median(group.Select(static measurement => measurement.ElapsedMilliseconds)):0} ms");
    if (report.Consumer is { } consumer)
        lines.AddRange(["", $"Consumer: `{consumer.Project}`", $"Consumer commit: `{consumer.GitCommit}`", $"Cache policy: {consumer.CachePolicy}"]);
    if (report.BrowserObservations is { } observations)
    {
        lines.AddRange(["", "## Browser observations", "",
            "Ready time uses the collector's visible-page/update condition. Resource sizes are actual Resource Timing values; zero can mean a cached response.", "",
            "| Scenario | Sample | Browser | Ready (ms) | Requests | Decoded body bytes | Encoded body bytes | Transfer bytes |",
            "| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |"]);
        foreach (var observation in observations)
            lines.Add($"| {observation.Scenario} | {observation.Sample} | {observation.Browser} | {observation.ReadyMilliseconds:0.0} | {observation.Resources.Count} | {observation.Resources.Sum(file => file.DecodedBodySize)} | {observation.Resources.Sum(file => file.EncodedBodySize)} | {observation.Resources.Sum(file => file.TransferSize)} |");
        lines.Add("");
        foreach (var observation in observations)
        {
            lines.Add($"- `{observation.Scenario}#{observation.Sample}`: `{observation.Url}`, cache disabled: `{observation.CacheDisabled}`; requested paths: {string.Join(", ", observation.Resources.Select(file => "`" + file.Path + "`"))}");
        }
    }
    foreach (var measurement in report.Measurements.Where(static measurement => measurement.ReleaseArtifact is not null))
    {
        var artifact = measurement.ReleaseArtifact!;
        lines.AddRange(["", $"## Release assets: {measurement.Name}#{measurement.Sample}", "",
            $"Directory: `{artifact.Root}`", "",
            "Gzip is estimated per file with .NET SmallestSize; it does not assert HTTP compression or first-screen requests.", "",
            "| Group | Files | Raw bytes | Estimated gzip bytes |", "| --- | ---: | ---: | ---: |"]);
        foreach (var group in artifact.Files.GroupBy(static file => file.Group, StringComparer.Ordinal).OrderBy(static group => group.Key, StringComparer.Ordinal))
            lines.Add($"| {group.Key} | {group.Count()} | {group.Sum(static file => file.Bytes)} | {group.Sum(static file => file.GzipBytes)} |");
        lines.Add($"| total | {artifact.Files.Count} | {artifact.Files.Sum(static file => file.Bytes)} | {artifact.Files.Sum(static file => file.GzipBytes)} |");
        lines.AddRange(["", "| File | Group | Raw bytes | Estimated gzip bytes |", "| --- | --- | ---: | ---: |"]);
        foreach (var file in artifact.Files)
            lines.Add($"| {file.Path} | {file.Group} | {file.Bytes} | {file.GzipBytes} |");
    }
    return string.Join(Environment.NewLine, lines) + Environment.NewLine;
}

sealed record FileArtifact(string Path, long Bytes, string FullPath);
sealed record BuildArtifactSnapshot(int GeneratedModuleCount, int MjsFileCount, int SourceMapCount, long MjsBytes, long SourceMapBytes, long ManifestBytes, long MjsGzipBytes, long SourceMapGzipBytes, long ManifestGzipBytes, long TotalGzipBytes, long TotalBytes, int? ChangedFileCount, long? ChangedBytes, [property: JsonIgnore] IReadOnlyList<FileArtifact>? Files = null);
sealed record ReleaseFileArtifact(string Path, string Group, long Bytes, long GzipBytes);
sealed record ReleaseArtifactSnapshot(string Root, IReadOnlyList<ReleaseFileArtifact> Files);
sealed record BuildMeasurement(string Name, int Sample, long ElapsedMilliseconds, BuildArtifactSnapshot? Artifact, ReleaseArtifactSnapshot? ReleaseArtifact = null);
sealed record ConsumerProjectInfo(string Project, string GitCommit, string Manifest, string CachePolicy);
sealed record BrowserResource(string Path, double DurationMilliseconds, long DecodedBodySize, long EncodedBodySize, long TransferSize);
sealed record BrowserObservation(string Scenario, int Sample, string Browser, string Url, bool CacheDisabled, double ReadyMilliseconds, IReadOnlyList<BrowserResource> Resources);
sealed record BuildBenchmarkReport(string SchemaVersion, DateTimeOffset StartedAt, string DotnetVersion, string SdkVersion, string GitCommit,
    string OperatingSystem, string Architecture, BuildBenchmarkOptions Options, IReadOnlyList<BuildMeasurement> Measurements,
    ConsumerProjectInfo? Consumer, IReadOnlyList<BrowserObservation>? BrowserObservations);

sealed record BuildBenchmarkOptions(string? WorkRoot, string? Output, int Samples, bool SkipHmr, bool SkipRelease, string? ReleaseArtifacts, string? ConsumerProject, string? BrowserObservations)
{
    public static BuildBenchmarkOptions Parse(string[] args)
    {
        string? workRoot = null;
        string? output = null;
        string? releaseArtifacts = null;
        string? consumerProject = null;
        string? browserObservations = null;
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
                case "--release-artifacts": releaseArtifacts = Next(args, ref index); break;
                case "--consumer-project": consumerProject = Next(args, ref index); break;
                case "--browser-observations": browserObservations = Next(args, ref index); break;
                case "--help":
                    Console.WriteLine("Usage: dotnet run --file scripts/csharp/benchmark-razorvue-build.cs -- [--work-root DIR] [--out FILE] [--samples N] [--skip-hmr] [--skip-release] [--release-artifacts DIST_DIR] [--consumer-project CSPROJ --skip-hmr] [--browser-observations JSON]");
                    Environment.Exit(0);
                    break;
                default: throw new InvalidOperationException("Unknown argument: " + args[index]);
            }
        }
        return new BuildBenchmarkOptions(workRoot, output, samples, skipHmr, skipRelease, releaseArtifacts, consumerProject, browserObservations);
    }

    private static string Next(string[] args, ref int index)
        => ++index < args.Length ? args[index] : throw new InvalidOperationException("Missing option value.");

    private static int ParsePositiveInt(string value, string option)
        => int.TryParse(value, out var result) && result > 0 ? result : throw new InvalidOperationException(option + " must be a positive integer.");
}
