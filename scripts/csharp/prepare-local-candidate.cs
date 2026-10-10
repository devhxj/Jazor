#!/usr/bin/env dotnet run
#:property PublishAot=false

// Local candidate preparation owns a fresh feed and records the working source, including
// uncommitted/untracked inputs. Package selection and packing stay in publish-nuget.cs.
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

if (args.Contains("--help") || args.Contains("-h"))
{
    Console.WriteLine("Usage: dotnet run --file scripts/csharp/prepare-local-candidate.cs -- --package-version <version> --output-directory <fresh-directory> [options]");
    Console.WriteLine("  --package <selector>       Repeatable publish-nuget.cs package selector.");
    Console.WriteLine("                            Default: jazor, jazor-vue, elementplus, lucide, monaco, vueroute.");
    Console.WriteLine("  --consumer-project <path> Restore the existing consumer with the generated nuget.config.");
    Console.WriteLine("  --configuration <name>    Default: Release.");
    Console.WriteLine("Outputs: packages/, isolated build/bin and build/obj, nuget.config, candidate-identity.json.");
    Console.WriteLine("The output directory must be empty. Consumer package references are selected by the consumer project.");
    return 0;
}

try
{
    var options = CandidateOptions.Parse(args);
    var repoRoot = RequireRepoRoot();
    var outputRoot = ResolvePath(repoRoot, options.OutputDirectory);
    if (File.Exists(outputRoot) || Directory.Exists(outputRoot) && Directory.EnumerateFileSystemEntries(outputRoot).Any())
        throw new InvalidOperationException($"Candidate output already exists and is not empty: {outputRoot}. Use a fresh directory and a unique candidate version.");
    var consumerProject = options.ConsumerProject is null ? null : ResolvePath(repoRoot, options.ConsumerProject);
    if (consumerProject is not null && !File.Exists(consumerProject))
        throw new FileNotFoundException("Consumer project does not exist.", consumerProject);

    var source = await CaptureSourceAsync(repoRoot, outputRoot);
    Console.WriteLine($"Source: {source.Commit} (dirty={source.Dirty}, diffSha256={source.DiffSha256}, inputsSha256={source.InputsSha256}, inputs={source.Inputs.Count})");
    Directory.CreateDirectory(outputRoot);
    var packageDirectory = Path.Combine(outputRoot, "packages");
    var selectors = options.Packages.Count == 0
        ? new[] { "jazor", "jazor-vue", "elementplus", "lucide", "monaco", "vueroute" }
        : options.Packages;
    var packArguments = new List<string>
    {
        "run", "--file", Path.Combine(repoRoot, "scripts", "csharp", "publish-nuget.cs"), "--",
        "--skip-push", "--configuration", options.Configuration,
        "--package-version", options.PackageVersion,
        "--output-directory", packageDirectory,
        "--base-output-path", Path.Combine(outputRoot, "build", "bin"),
        "--base-intermediate-output-path", Path.Combine(outputRoot, "build", "obj")
    };
    foreach (var selector in selectors)
        packArguments.AddRange(["--package", selector]);
    await RunAsync("dotnet", packArguments, repoRoot);

    var packages = Directory.EnumerateFiles(packageDirectory, "*.nupkg")
        .Select(path => ReadPackage(path, outputRoot))
        .OrderBy(static package => package.Id, StringComparer.Ordinal)
        .ToArray();
    if (packages.Length == 0)
        throw new InvalidOperationException("Packing produced no packages.");
    foreach (var package in packages)
    {
        if (!string.Equals(package.Version, options.PackageVersion, StringComparison.Ordinal))
            throw new InvalidOperationException($"Actual package version differs from the requested candidate: {package.Id}@{package.Version}, expected {options.PackageVersion}.");
        Console.WriteLine($"Package: {package.Id}@{package.Version} sha256={package.Sha256} ({package.Path})");
    }
    if (packages.Select(static package => package.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != packages.Length)
        throw new InvalidOperationException("Candidate feed contains duplicate package identities.");

    // A HEAD alone cannot identify dirty builds. Recheck the exact input snapshot so edits
    // during packing cannot be attributed to a package built from a different source state.
    var afterPack = await CaptureSourceAsync(repoRoot, outputRoot);
    if (source.Commit != afterPack.Commit || source.DiffSha256 != afterPack.DiffSha256 || source.InputsSha256 != afterPack.InputsSha256)
        throw new InvalidOperationException("Source inputs changed during packing. The candidate is incomplete; prepare a fresh candidate after the edits finish.");

    var configPath = Path.Combine(outputRoot, "nuget.config");
    new XDocument(new XElement("configuration", new XElement("packageSources",
        new XElement("clear"),
        new XElement("add", new XAttribute("key", "jazor-local-candidate"), new XAttribute("value", "./packages")),
        new XElement("add", new XAttribute("key", "nuget.org"), new XAttribute("value", "https://api.nuget.org/v3/index.json")))))
        .Save(configPath);
    var identityPath = Path.Combine(outputRoot, "candidate-identity.json");
    using (var identityFile = new FileStream(identityPath, FileMode.CreateNew, FileAccess.Write))
    {
        await JsonSerializer.SerializeAsync(identityFile, new
        {
            SchemaVersion = 1,
            PackageVersion = options.PackageVersion,
            Configuration = options.Configuration,
            PackageSelectors = selectors,
            Source = source,
            Packages = packages,
            NuGetConfig = "nuget.config",
            ConsumerProject = consumerProject
        }, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
    Console.WriteLine($"Candidate identity: {identityPath}");
    Console.WriteLine($"Local restore config: {configPath}");
    if (consumerProject is not null)
    {
        await RunAsync("dotnet", ["restore", consumerProject, "--configfile", configPath], Path.GetDirectoryName(consumerProject)!);
        Console.WriteLine($"Consumer restore completed: {consumerProject}");
    }
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

static string RequireRepoRoot()
{
    for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
    throw new InvalidOperationException("Run from the repository containing Jazor.slnx.");
}

static string ResolvePath(string repoRoot, string path)
    => Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));

static async Task<SourceIdentity> CaptureSourceAsync(string repoRoot, string outputRoot)
{
    var commit = (await CaptureAsync("git", ["rev-parse", "HEAD"], repoRoot)).Trim();
    var status = await CaptureAsync("git", ["status", "--porcelain=v1", "-z"], repoRoot);
    var diff = await CaptureAsync("git", ["diff", "--no-ext-diff", "--binary", "HEAD"], repoRoot);
    var tracked = await CaptureAsync("git", ["ls-files", "--cached", "-z"], repoRoot);
    var untracked = (await CaptureAsync("git", ["ls-files", "--others", "--exclude-standard", "-z"], repoRoot))
        .Split('\0', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);
    var outputPrefix = Path.TrimEndingDirectorySeparator(outputRoot) + Path.DirectorySeparatorChar;
    var inputs = new List<SourceInput>();
    foreach (var path in tracked.Split('\0', StringSplitOptions.RemoveEmptyEntries).Concat(untracked)
                 .Distinct(StringComparer.Ordinal).OrderBy(static path => path, StringComparer.Ordinal))
    {
        var fullPath = Path.GetFullPath(Path.Combine(repoRoot, path));
        // The feed/build output is produced by this script even when the caller chooses an
        // unignored directory; it is not a source input to its own candidate identity.
        if (fullPath.StartsWith(outputPrefix, StringComparison.OrdinalIgnoreCase))
            continue;
        if (!File.Exists(fullPath))
        {
            inputs.Add(new SourceInput(path, null, null, untracked.Contains(path)));
            continue;
        }
        using var stream = File.OpenRead(fullPath);
        inputs.Add(new SourceInput(path, Hex(SHA256.HashData(stream)), stream.Length, untracked.Contains(path)));
    }
    // Hash the compact camelCase input array so the stored records are enough to reproduce
    // the fingerprint without a checkout of the original working tree.
    var inputBytes = JsonSerializer.SerializeToUtf8Bytes(inputs,
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    return new SourceIdentity(commit, status.Length != 0, Sha256(diff), Hex(SHA256.HashData(inputBytes)), inputs);
}

static CandidatePackage ReadPackage(string path, string outputRoot)
{
    using var archive = ZipFile.OpenRead(path);
    using var nuspecStream = archive.Entries.Single(entry => entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase)).Open();
    var document = XDocument.Load(nuspecStream);
    var ns = document.Root!.Name.Namespace;
    var metadata = document.Root.Element(ns + "metadata")!;
    var id = metadata.Element(ns + "id")?.Value ?? throw new InvalidOperationException($"Missing nuspec package id: {path}");
    var version = metadata.Element(ns + "version")?.Value ?? throw new InvalidOperationException($"Missing nuspec package version: {path}");
    using var packageFile = File.OpenRead(path);
    return new CandidatePackage(id, version, Path.GetRelativePath(outputRoot, path).Replace('\\', '/'),
        Hex(SHA256.HashData(packageFile)), packageFile.Length, metadata.Element(ns + "repository")?.Attribute("commit")?.Value);
}

static string Sha256(string value) => Hex(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
static string Hex(byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();

static async Task RunAsync(string fileName, IReadOnlyList<string> arguments, string workdir)
{
    using var process = Process.Start(CreateStartInfo(fileName, arguments, workdir))
        ?? throw new InvalidOperationException($"Could not start {fileName}.");
    await process.WaitForExitAsync();
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"{fileName} exited with code {process.ExitCode}.");
}

static async Task<string> CaptureAsync(string fileName, IReadOnlyList<string> arguments, string workdir)
{
    var startInfo = CreateStartInfo(fileName, arguments, workdir);
    startInfo.RedirectStandardOutput = true;
    startInfo.RedirectStandardError = true;
    startInfo.StandardOutputEncoding = Encoding.UTF8;
    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Could not start {fileName}.");
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    var output = await stdout;
    var error = await stderr;
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"{fileName} exited with code {process.ExitCode}: {error}");
    return output;
}

static ProcessStartInfo CreateStartInfo(string fileName, IReadOnlyList<string> arguments, string workdir)
{
    var startInfo = new ProcessStartInfo(fileName) { WorkingDirectory = workdir, UseShellExecute = false, CreateNoWindow = true };
    foreach (var argument in arguments)
        startInfo.ArgumentList.Add(argument);
    return startInfo;
}

sealed record SourceInput(string Path, string? Sha256, long? Bytes, bool Untracked);
sealed record SourceIdentity(string Commit, bool Dirty, string DiffSha256, string InputsSha256, IReadOnlyList<SourceInput> Inputs);
sealed record CandidatePackage(string Id, string Version, string Path, string Sha256, long Bytes, string? RepositoryCommit);

sealed record CandidateOptions(string PackageVersion, string OutputDirectory, string Configuration, IReadOnlyList<string> Packages, string? ConsumerProject)
{
    public static CandidateOptions Parse(IReadOnlyList<string> arguments)
    {
        string? version = null;
        string? output = null;
        string? consumer = null;
        var configuration = "Release";
        var packages = new List<string>();
        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            if (++index >= arguments.Count || string.IsNullOrWhiteSpace(arguments[index]) || arguments[index].StartsWith("--", StringComparison.Ordinal))
                throw new InvalidOperationException($"Missing value for {argument}.");
            var value = arguments[index];
            switch (argument)
            {
                case "--package-version": version = value; break;
                case "--output-directory": output = value; break;
                case "--package": packages.Add(value); break;
                case "--configuration": configuration = value; break;
                case "--consumer-project": consumer = value; break;
                default: throw new InvalidOperationException($"Unknown argument: {argument}.");
            }
        }
        return new CandidateOptions(version ?? throw new InvalidOperationException("--package-version is required."),
            output ?? throw new InvalidOperationException("--output-directory is required."), configuration, packages, consumer);
    }
}
