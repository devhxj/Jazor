using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Razor.Language;

namespace Jazor.RazorVue.RazorIr.Test;

internal sealed record RazorSdkToolsetProbe(
    string RootPath,
    string SdkVersion,
    string SdkRootPath,
    string RazorSdkRootPath,
    string RazorSourceGeneratorPath,
    string RazorTasksPath,
    string RazorDesignTimeTargetsPath,
    string RazorComponentTargetsPath)
{
    public string Describe()
        => string.Join(
            Environment.NewLine,
            [
                "Razor SDK toolset probe: available",
                $"  root:                {RootPath}",
                $"  sdk version:         {SdkVersion}",
                $"  sdk root:            {SdkRootPath}",
                $"  razor sdk root:      {RazorSdkRootPath}",
                $"  source generator:    {RazorSourceGeneratorPath}",
                $"  tasks:               {RazorTasksPath}",
                $"  design-time targets: {RazorDesignTimeTargetsPath}",
                $"  component targets:   {RazorComponentTargetsPath}"
            ]);
}

internal static class RazorSdkToolsetProbeResolver
{
    private const string RazorSdkName = "Microsoft.NET.Sdk.Razor";

    public static RazorSdkToolsetProbe? Resolve()
    {
        foreach (var candidate in EnumerateDotNetRoots())
        {
            var toolset = TryResolveFromRoot(candidate.RootPath, candidate.PreferredVersion);
            if (toolset is not null)
            {
                return toolset;
            }
        }

        return null;
    }

    private static RazorSdkToolsetProbe? TryResolveFromRoot(string rootPath, string? preferredVersion)
    {
        var sdkRoot = Path.Combine(rootPath, "sdk");
        if (!Directory.Exists(sdkRoot))
        {
            return null;
        }

        var versions = Directory.GetDirectories(sdkRoot)
            .Select(Path.GetFileName)
            .Where(static version => !string.IsNullOrWhiteSpace(version))
            .Select(static version => version!)
            .OrderByDescending(static version => version, SdkVersionComparer.Instance)
            .ToArray();
        if (!string.IsNullOrWhiteSpace(preferredVersion))
        {
            versions = versions
                .OrderByDescending(version => string.Equals(version, preferredVersion, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(static version => version, SdkVersionComparer.Instance)
                .ToArray();
        }

        foreach (var version in versions)
        {
            var versionRoot = Path.Combine(sdkRoot, version);
            var razorSdkRoot = Path.Combine(versionRoot, "Sdks", RazorSdkName);
            var sourceGeneratorPath = Path.Combine(razorSdkRoot, "source-generators", "Microsoft.CodeAnalysis.Razor.Compiler.dll");
            var tasksPath = Path.Combine(razorSdkRoot, "tasks", "net10.0", "Microsoft.NET.Sdk.Razor.Tasks.dll");
            var designTimeTargetsPath = Path.Combine(razorSdkRoot, "targets", "Microsoft.NET.Sdk.Razor.DesignTime.targets");
            var componentTargetsPath = Path.Combine(razorSdkRoot, "targets", "Microsoft.NET.Sdk.Razor.Component.targets");

            if (!File.Exists(sourceGeneratorPath)
                || !File.Exists(tasksPath)
                || !File.Exists(designTimeTargetsPath)
                || !File.Exists(componentTargetsPath))
            {
                continue;
            }

            return new RazorSdkToolsetProbe(
                rootPath,
                version,
                versionRoot,
                razorSdkRoot,
                sourceGeneratorPath,
                tasksPath,
                designTimeTargetsPath,
                componentTargetsPath);
        }

        return null;
    }

    private static IEnumerable<(string RootPath, string? PreferredVersion)> EnumerateDotNetRoots()
    {
        var seen = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        var preferredVersion = TryGetPreferredSdkVersion();

        foreach (var environmentVariable in new[] { "DOTNET_ROOT", "DOTNET_ROOT_X64", "DOTNET_ROOT_ARM64", "DOTNET_ROOT(x86)" })
        {
            var rootPath = Environment.GetEnvironmentVariable(environmentVariable);
            if (TryAddRoot(rootPath, seen, out var resolved))
            {
                yield return (resolved, preferredVersion);
            }
        }

        if (OperatingSystem.IsWindows())
        {
            var programFilesRoot = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (TryAddRoot(Path.Combine(programFilesRoot, "dotnet"), seen, out var programFilesDotNet))
            {
                yield return (programFilesDotNet, preferredVersion);
            }

            var x86Root = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (TryAddRoot(Path.Combine(x86Root, "dotnet"), seen, out var programFilesX86DotNet))
            {
                yield return (programFilesX86DotNet, preferredVersion);
            }
        }

        var inferredRoot = TryGetDotNetRootFromInfo();
        if (TryAddRoot(inferredRoot, seen, out var inferred))
        {
            yield return (inferred, preferredVersion);
        }
    }

    private static string? TryGetPreferredSdkVersion()
        => TryReadSdkVersionFromGlobalJson()
            ?? TryGetCurrentSdkVersionFromDotNetInfo();

    private static string? TryGetDotNetRootFromInfo()
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--info",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (!process.Start())
            {
                return null;
            }

            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit((int)TimeSpan.FromSeconds(3).TotalMilliseconds))
            {
                TryTerminate(process);
                return null;
            }

            _ = standardError.GetAwaiter().GetResult();
            return TryParseDotNetRootFromInfoOutput(standardOutput.GetAwaiter().GetResult());
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    private static string? TryGetCurrentSdkVersionFromDotNetInfo()
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--info",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (!process.Start())
            {
                return null;
            }

            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit((int)TimeSpan.FromSeconds(3).TotalMilliseconds))
            {
                TryTerminate(process);
                return null;
            }

            _ = standardError.GetAwaiter().GetResult();
            var output = standardOutput.GetAwaiter().GetResult();
            foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("Version:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var version = trimmed["Version:".Length..].Trim().Trim('"');
                if (!string.IsNullOrWhiteSpace(version))
                {
                    return version;
                }
            }

            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    private static string? TryReadSdkVersionFromGlobalJson()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (currentDirectory is not null)
        {
            var globalJsonPath = Path.Combine(currentDirectory.FullName, "global.json");
            if (File.Exists(globalJsonPath))
            {
                try
                {
                    using var stream = File.OpenRead(globalJsonPath);
                    using var document = JsonDocument.Parse(stream);
                    if (document.RootElement.TryGetProperty("sdk", out var sdkElement)
                        && sdkElement.TryGetProperty("version", out var versionElement))
                    {
                        var version = versionElement.GetString();
                        if (!string.IsNullOrWhiteSpace(version))
                        {
                            return version;
                        }
                    }
                }
                catch (IOException)
                {
                    return null;
                }
                catch (UnauthorizedAccessException)
                {
                    return null;
                }
                catch (JsonException)
                {
                    return null;
                }
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }

    internal static string? TryParseDotNetRootFromInfoOutput(string? infoOutput)
    {
        if (string.IsNullOrWhiteSpace(infoOutput))
        {
            return null;
        }

        foreach (var line in infoOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("Base Path:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var basePath = trimmed["Base Path:".Length..].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(basePath))
            {
                continue;
            }

            var normalizedBasePath = basePath.Replace('\\', '/').TrimEnd('/');
            var sdkSegmentIndex = normalizedBasePath.LastIndexOf("/sdk/", StringComparison.OrdinalIgnoreCase);
            if (sdkSegmentIndex <= 0)
            {
                continue;
            }

            var rootPath = normalizedBasePath[..sdkSegmentIndex];
            return basePath.Contains('\\', StringComparison.Ordinal)
                ? rootPath.Replace('/', '\\')
                : rootPath;
        }

        return null;
    }

    private static bool TryAddRoot(string? rootPath, ISet<string> seen, out string resolvedPath)
    {
        resolvedPath = string.Empty;
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            return false;
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(rootPath);
        }
        catch (Exception exception) when (
            exception is ArgumentException
            or IOException
            or NotSupportedException)
        {
            return false;
        }

        if (!Directory.Exists(fullPath) || !seen.Add(fullPath))
        {
            return false;
        }

        resolvedPath = fullPath;
        return true;
    }

    private static void TryTerminate(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
    }

    private sealed class SdkVersionComparer : IComparer<string>
    {
        public static SdkVersionComparer Instance { get; } = new();

        public int Compare(string? left, string? right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left is null)
            {
                return -1;
            }

            if (right is null)
            {
                return 1;
            }

            var leftParsed = TryParse(left, out var leftVersion, out var leftPrerelease);
            var rightParsed = TryParse(right, out var rightVersion, out var rightPrerelease);
            if (leftParsed && rightParsed)
            {
                var versionComparison = leftVersion!.CompareTo(rightVersion);
                if (versionComparison != 0)
                {
                    return versionComparison;
                }

                if (leftPrerelease.Length == 0 && rightPrerelease.Length > 0)
                {
                    return 1;
                }

                if (leftPrerelease.Length > 0 && rightPrerelease.Length == 0)
                {
                    return -1;
                }

                var prereleaseComparison = string.Compare(leftPrerelease, rightPrerelease, StringComparison.OrdinalIgnoreCase);
                if (prereleaseComparison != 0)
                {
                    return prereleaseComparison;
                }
            }
            else if (leftParsed != rightParsed)
            {
                return leftParsed ? 1 : -1;
            }

            return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParse(string versionText, out Version? version, out string prereleaseLabel)
        {
            version = null;
            prereleaseLabel = string.Empty;

            if (string.IsNullOrWhiteSpace(versionText))
            {
                return false;
            }

            var separatorIndex = versionText.IndexOf('-', StringComparison.Ordinal);
            var numericPart = separatorIndex >= 0
                ? versionText[..separatorIndex]
                : versionText;
            prereleaseLabel = separatorIndex >= 0
                ? versionText[(separatorIndex + 1)..]
                : string.Empty;
            return Version.TryParse(numericPart, out version);
        }
    }
}
