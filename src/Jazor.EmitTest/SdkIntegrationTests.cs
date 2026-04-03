using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace Jazor.EmitTest;

[TestClass]
public sealed class SdkIntegrationTests
{
    private static readonly Lazy<Task<LocalPackageFixture>> LocalPackage = new(CreateLocalPackageAsync);

    [TestMethod]
    public async Task Build_LocalJazorPackage_MultiProjectSample_EmitsModulesAndBundle()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var sourceSampleRoot = Path.Combine(package.RepoRoot, "samples", "Jazor.MultiProject");
        CopyDirectory(sourceSampleRoot, workspace.SampleRoot);

        var hostProjectPath = Path.Combine(workspace.SampleRoot, "Sample.Host", "Sample.Host.csproj");
        var build = await RunDotNetAsync(
            package.RepoRoot,
            [
                "build",
                hostProjectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                $"-p:JazorPackageVersion={package.PackageVersion}",
                "-p:JazorBundle=true"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var hostRoot = Path.Combine(workspace.SampleRoot, "Sample.Host");
        var manifestPath = Path.Combine(hostRoot, "wwwroot", "jazor", "jazor-manifest.json");
        var bundlePath = Path.Combine(hostRoot, "wwwroot", "app.bundle.js");
        var sharedModulePath = Path.Combine(hostRoot, "wwwroot", "jazor", "shared", "greetings.mjs");
        var featureModulePath = Path.Combine(hostRoot, "wwwroot", "jazor", "features", "greeter.mjs");
        var hostModulePath = Path.Combine(hostRoot, "wwwroot", "jazor", "host", "app.mjs");

        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(bundlePath), $"Bundle was not generated: {bundlePath}");
        Assert.IsTrue(File.Exists(sharedModulePath), $"Shared module was not generated: {sharedModulePath}");
        Assert.IsTrue(File.Exists(featureModulePath), $"Feature module was not generated: {featureModulePath}");
        Assert.IsTrue(File.Exists(hostModulePath), $"Host module was not generated: {hostModulePath}");

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
        var modulePaths = manifest.RootElement
            .GetProperty("Modules")
            .EnumerateArray()
            .Select(static module => module.GetProperty("RelativePath").GetString())
            .OfType<string>()
            .ToArray();

        CollectionAssert.AreEquivalent(
            new[]
            {
                "shared/greetings.mjs",
                "features/greeter.mjs",
                "host/app.mjs"
            },
            modulePaths);

        var sharedModule = await File.ReadAllTextAsync(sharedModulePath);
        var featureModule = await File.ReadAllTextAsync(featureModulePath);
        var hostModule = await File.ReadAllTextAsync(hostModulePath);
        var bundle = await File.ReadAllTextAsync(bundlePath);

        StringAssert.Contains(sharedModule, "export function Prefix()");
        StringAssert.Contains(sharedModule, "export function Compose(name)");
        StringAssert.Contains(featureModule, "import { Compose } from \"shared/greetings.mjs\";");
        StringAssert.Contains(featureModule, "export function Greet(name)");
        StringAssert.Contains(hostModule, "import { Greet } from \"features/greeter.mjs\";");
        StringAssert.Contains(hostModule, "export function Boot()");
        StringAssert.Contains(bundle, "function Prefix()");
        StringAssert.Contains(bundle, "function Greet(name)");
        StringAssert.Contains(bundle, "function Boot()");
        StringAssert.Contains(bundle, "export {");
        StringAssert.Contains(bundle, "Boot");
    }

    private static async Task<LocalPackageFixture> CreateLocalPackageAsync()
    {
        var repoRoot = FindRepoRoot();
        var packageVersion = ReadPackageVersion(Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj"));
        var packageOutputDirectory = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "nupkg");

        if (Directory.Exists(packageOutputDirectory))
            Directory.Delete(packageOutputDirectory, recursive: true);

        Directory.CreateDirectory(packageOutputDirectory);

        await RunDotNetAndAssertAsync(
            repoRoot,
            [
                "build",
                Path.Combine(repoRoot, "src", "ECMAScript", "ECMAScript.csproj"),
                "-c",
                "Debug",
                "/m:1",
                "/p:BuildInParallel=false"
            ]);
        await RunDotNetAndAssertAsync(
            repoRoot,
            [
                "build",
                Path.Combine(repoRoot, "src", "Jazor.Analyzer", "Jazor.Analyzer.csproj"),
                "-c",
                "Debug",
                "/m:1",
                "/p:BuildInParallel=false"
            ]);
        await RunDotNetAndAssertAsync(
            repoRoot,
            [
                "publish",
                Path.Combine(repoRoot, "src", "Jazor.Emit", "Jazor.Emit.csproj"),
                "-c",
                "Debug",
                "-o",
                Path.Combine(repoRoot, "src", "Jazor.Emit", "bin", "Debug", "net10.0", "publish"),
                "/m:1",
                "/p:BuildInParallel=false"
            ]);
        await RunDotNetAndAssertAsync(
            repoRoot,
            [
                "pack",
                Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj"),
                "-c",
                "Debug",
                "--no-build",
                "-o",
                packageOutputDirectory
            ]);

        return new LocalPackageFixture(repoRoot, packageVersion, packageOutputDirectory);
    }

    private static async Task RunDotNetAndAssertAsync(string workingDirectory, IReadOnlyList<string> arguments)
    {
        var result = await RunDotNetAsync(workingDirectory, arguments);
        Assert.AreEqual(0, result.ExitCode, result.ToString());
    }

    private static async Task<ProcessResult> RunDotNetAsync(string workingDirectory, IReadOnlyList<string> arguments)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        startInfo.Environment["DOTNET_CLI_HOME"] = Path.Combine(FindRepoRoot(), ".dotnet");
        startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new ProcessResult(
            process.ExitCode,
            await standardOutput,
            await standardError);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private static string ReadPackageVersion(string projectPath)
    {
        var document = XDocument.Load(projectPath);
        var version = document.Root?
            .Elements("PropertyGroup")
            .Elements("Version")
            .Select(static element => element.Value.Trim())
            .FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value));

        if (string.IsNullOrWhiteSpace(version))
            throw new InvalidOperationException($"Could not read package version from '{projectPath}'.");

        return version;
    }

    private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, directory);
            if (ShouldSkip(relativePath))
                continue;

            Directory.CreateDirectory(Path.Combine(destinationDirectory, relativePath));
        }

        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, file);
            if (ShouldSkip(relativePath))
                continue;

            var destinationPath = Path.Combine(destinationDirectory, relativePath);
            var destinationParent = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationParent))
                Directory.CreateDirectory(destinationParent);

            File.Copy(file, destinationPath, overwrite: true);
        }
    }

    private static bool ShouldSkip(string relativePath)
    {
        var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return segments.Any(static segment =>
            segment.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("TestResults", StringComparison.OrdinalIgnoreCase));
    }

    private sealed record LocalPackageFixture(
        string RepoRoot,
        string PackageVersion,
        string PackageOutputDirectory);

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace(string repoRoot)
        {
            RootPath = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
            SampleRoot = Path.Combine(RootPath, "Jazor.MultiProject");
            Directory.CreateDirectory(SampleRoot);
        }

        public string RootPath { get; }

        public string SampleRoot { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(RootPath))
                    Directory.Delete(RootPath, recursive: true);
            }
            catch
            {
            }
        }
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"ExitCode: {ExitCode}");

            if (!string.IsNullOrWhiteSpace(StandardOutput))
            {
                builder.AppendLine("STDOUT:");
                builder.AppendLine(StandardOutput);
            }

            if (!string.IsNullOrWhiteSpace(StandardError))
            {
                builder.AppendLine("STDERR:");
                builder.AppendLine(StandardError);
            }

            return builder.ToString();
        }
    }
}
