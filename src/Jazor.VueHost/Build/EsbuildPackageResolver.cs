using System.Diagnostics;

namespace Jazor.VueHost.Build;

internal static class EsbuildPackageResolver
{
    public static string? ResolvePackageDirectory(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        rootDirectory = Path.GetFullPath(rootDirectory);

        var localPackageDirectory = Path.Combine(rootDirectory, "node_modules", "esbuild");
        if (ContainsPackage(localPackageDirectory))
        {
            return localPackageDirectory;
        }

        foreach (var path in ReadNodePathCandidates())
        {
            var packageDirectory = Path.Combine(path, "esbuild");
            if (ContainsPackage(packageDirectory))
            {
                return packageDirectory;
            }
        }

        var globalRoot = TryGetProcessOutput("npm", "root -g");
        if (!string.IsNullOrWhiteSpace(globalRoot))
        {
            var globalPackageDirectory = Path.Combine(globalRoot.Trim(), "esbuild");
            if (ContainsPackage(globalPackageDirectory))
            {
                return globalPackageDirectory;
            }
        }

        return null;
    }

    private static IEnumerable<string> ReadNodePathCandidates()
    {
        var nodePath = Environment.GetEnvironmentVariable("NODE_PATH");
        if (string.IsNullOrWhiteSpace(nodePath))
        {
            yield break;
        }

        foreach (var path in nodePath.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return path;
        }
    }

    private static bool ContainsPackage(string packageDirectory)
        => Directory.Exists(packageDirectory)
            && File.Exists(Path.Combine(packageDirectory, "package.json"));

    private static string? TryGetProcessOutput(string fileName, string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return process.ExitCode == 0 ? output : null;
        }
        catch
        {
            return null;
        }
    }
}
