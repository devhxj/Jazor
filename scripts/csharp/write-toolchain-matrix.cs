#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Text;
using System.Text.Json;

var repoRoot = RequireRepositoryRoot();
var outputPath = args.Length > 0
    ? ResolveInsideRepository(repoRoot, args[0])
    : Path.Combine(repoRoot, "artifacts", "quality", "toolchain-matrix.md");

ConfigureRepositoryEnvironment(repoRoot);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

var dotnet = await ReadVersionAsync("dotnet", "--version");
var node = await ReadVersionAsync("node", "--version");
var chrome = ResolveChromeVersion();
var sdk = ReadGlobalJson(repoRoot);
var sdkStatus = CompareSdk(sdk, dotnet);

var content = new StringBuilder()
    .AppendLine("## Toolchain Matrix")
    .AppendLine()
    .AppendLine($"- Commit: `{RunGit(repoRoot, "rev-parse", "HEAD")}`")
    .AppendLine($"- .NET SDK requested by `global.json`: `{sdk}`")
    .AppendLine($"- .NET CLI: `{dotnet}`")
    .AppendLine($"- .NET SDK check: **{sdkStatus}**")
    .AppendLine($"- Node.js: `{node}`")
    .AppendLine($"- Chrome: `{chrome}`")
    .AppendLine($"- OS: `{Environment.OSVersion}`")
    .ToString();

await File.WriteAllTextAsync(outputPath, content, new UTF8Encoding(false));
Console.WriteLine(content);
if (sdkStatus == "FAIL")
    Environment.ExitCode = 1;

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

static string CompareSdk(string requested, string actual)
{
    if (requested == "unavailable" || actual == "unavailable")
        return "FAIL";
    var requestedBase = requested.Split('-', 2)[0];
    var actualBase = actual.Split('-', 2)[0];
    return string.Equals(requestedBase, actualBase, StringComparison.OrdinalIgnoreCase)
        ? "PASS"
        : "FAIL";
}

static string ResolveInsideRepository(string repoRoot, string path)
{
    var fullPath = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));
    var root = Path.GetFullPath(repoRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        + Path.DirectorySeparatorChar;
    if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Toolchain matrix output must stay inside the repository: " + fullPath);
    return fullPath;
}

static string RequireRepositoryRoot()
{
    for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
    }

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
