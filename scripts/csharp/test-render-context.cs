#!/usr/bin/env dotnet run

using System.Diagnostics;

var repoRoot = RequireRepoRoot();
var testPath = Path.Combine(repoRoot, "src", "Jazor.RazorVue", "Runtime.Tests", "render-context.test.mjs");
var startInfo = new ProcessStartInfo
{
    FileName = "node",
    WorkingDirectory = repoRoot,
    UseShellExecute = false,
    CreateNoWindow = true
};
startInfo.ArgumentList.Add("--test");
startInfo.ArgumentList.Add(testPath);

using var process = Process.Start(startInfo)
    ?? throw new InvalidOperationException("Failed to start Node.js for render-context tests.");
await process.WaitForExitAsync();
if (process.ExitCode != 0)
{
    throw new InvalidOperationException($"Render-context tests failed with exit code {process.ExitCode}.");
}

static string RequireRepoRoot()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new InvalidOperationException("Repository root containing Jazor.slnx was not found from the current directory upward.");
}
