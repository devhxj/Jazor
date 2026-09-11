#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Text;
using System.Text.Json;

var repoRoot = Directory.GetCurrentDirectory();
var outputPath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.Combine(repoRoot, "artifacts", "quality", "toolchain-matrix.md");

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

var dotnet = await ReadVersionAsync("dotnet", "--version");
var node = await ReadVersionAsync("node", "--version");
var chrome = ResolveChromeVersion();
var sdk = ReadGlobalJson(repoRoot);

var content = new StringBuilder()
    .AppendLine("## Toolchain Matrix")
    .AppendLine()
    .AppendLine($"- Commit: `{RunGit(repoRoot, "rev-parse", "HEAD")}`")
    .AppendLine($"- .NET SDK requested by `global.json`: `{sdk}`")
    .AppendLine($"- .NET CLI: `{dotnet}`")
    .AppendLine($"- Node.js: `{node}`")
    .AppendLine($"- Chrome: `{chrome}`")
    .AppendLine($"- OS: `{Environment.OSVersion}`")
    .ToString();

await File.WriteAllTextAsync(outputPath, content, new UTF8Encoding(false));
Console.WriteLine(content);

static async Task<string> ReadVersionAsync(string fileName, string argument)
{
    try
    {
        using var process = Process.Start(new ProcessStartInfo(fileName, argument)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        });
        if (process is null)
            return "unavailable";
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return process.ExitCode == 0 ? output.Trim() : "unavailable";
    }
    catch
    {
        return "unavailable";
    }
}

static string ResolveChromeVersion()
{
    var candidates = new[]
    {
        @"C:\Program Files\Google\Chrome\Application\chrome.exe",
        @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
    };
    var path = candidates.FirstOrDefault(File.Exists);
    if (path is null)
        return "unavailable";

    return FileVersionInfo.GetVersionInfo(path).FileVersion ?? "unavailable";
}

static string ReadGlobalJson(string repoRoot)
{
    var path = Path.Combine(repoRoot, "global.json");
    if (!File.Exists(path))
        return "unavailable";
    using var document = JsonDocument.Parse(File.ReadAllText(path));
    return document.RootElement
        .GetProperty("sdk")
        .GetProperty("version")
        .GetString() ?? "unavailable";
}

static string RunGit(string workingDirectory, params string[] arguments)
{
    var startInfo = new ProcessStartInfo("git")
    {
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    foreach (var argument in arguments)
        startInfo.ArgumentList.Add(argument);
    using var process = Process.Start(startInfo);
    if (process is null)
        return "unavailable";
    var output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();
    return process.ExitCode == 0 ? output.Trim() : "unavailable";
}
