#!/usr/bin/env dotnet run

using System.Diagnostics;

var explicitDenoExe = Environment.GetEnvironmentVariable("JAZOR_DENO_EXE");
var denoExePath = !string.IsNullOrWhiteSpace(explicitDenoExe)
    ? ResolveExplicitDeno(explicitDenoExe)
    : ResolveBundledDeno(FindRepositoryRoot(Directory.GetCurrentDirectory()));

await RunProcessAsync(denoExePath, args, Directory.GetCurrentDirectory());

static string FindRepositoryRoot(string startDirectory)
{
    var current = new DirectoryInfo(Path.GetFullPath(startDirectory));
    while (current is not null)
    {
        if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
        {
            return current.FullName;
        }

        current = current.Parent;
    }

    throw new InvalidOperationException("Cannot locate repository root (Jazor.slnx).");
}

static string ResolveExplicitDeno(string path)
{
    var fullPath = Path.GetFullPath(path);
    if (!File.Exists(fullPath))
    {
        throw new FileNotFoundException("Explicit JAZOR_DENO_EXE path does not exist: " + fullPath, fullPath);
    }

    return fullPath;
}

static string ResolveBundledDeno(string repoRoot)
{
    var candidates = new[]
    {
        Path.Combine(repoRoot, "src", "Jolt", "bin", "Debug", "net11.0", "runtimes", "win-x64", "native", "deno.exe"),
        Path.Combine(repoRoot, "src", "Jolt", "bin", "Release", "net11.0", "runtimes", "win-x64", "native", "deno.exe"),
        Path.Combine(repoRoot, "src", "Jazor.Emit", "bin", "Debug", "net11.0", "runtimes", "win-x64", "native", "deno.exe"),
        Path.Combine(repoRoot, "src", "Jazor.Emit", "bin", "Release", "net11.0", "runtimes", "win-x64", "native", "deno.exe")
    };

    var resolved = candidates.FirstOrDefault(File.Exists);
    if (resolved is not null)
    {
        return resolved;
    }

    var denoHostPackageRoot = Path.Combine(repoRoot, ".dotnet", ".nuget", "packages", "denohost.runtime.win-x64");
    if (Directory.Exists(denoHostPackageRoot))
    {
        var cached = Directory
            .EnumerateFiles(denoHostPackageRoot, "deno.exe", SearchOption.AllDirectories)
            .OrderByDescending(static path => path, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        if (cached is not null)
        {
            return cached;
        }
    }

    throw new FileNotFoundException("Bundled Deno runtime was not found. Build Jolt or Jazor.Emit first so DenoHost runtime assets exist.");
}

static async Task RunProcessAsync(string fileName, IReadOnlyList<string> arguments, string workingDirectory, CancellationToken cancellationToken = default)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = fileName,
        WorkingDirectory = workingDirectory,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };

    foreach (var argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }

    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException("Failed to start process: " + fileName);

    var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
    var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
    await process.WaitForExitAsync(cancellationToken);
    Console.Write(await stdout);
    Console.Error.Write(await stderr);

    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException("Bundled deno command failed with exit code " + process.ExitCode + ".");
    }
}
