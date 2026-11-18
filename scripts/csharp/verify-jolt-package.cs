#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Xml.Linq;

const string packageId = "Jazor.Jolt";
if (args.Length != 0 && args is not ["--package-source", _])
    throw new ArgumentException("Usage: verify-jolt-package.cs [--package-source directory]");
var repository = new DirectoryInfo(Directory.GetCurrentDirectory());
while (repository is not null && !File.Exists(Path.Combine(repository.FullName, "Jazor.slnx")))
    repository = repository.Parent;
var root = repository?.FullName ?? throw new InvalidOperationException("Cannot locate Jazor.slnx.");
var rid = RuntimeInformation.ProcessArchitecture switch
{
    Architecture.X64 when OperatingSystem.IsLinux() => "linux-x64",
    Architecture.Arm64 when OperatingSystem.IsLinux() => "linux-arm64",
    Architecture.X64 when OperatingSystem.IsWindows() => "win-x64",
    Architecture.Arm64 when OperatingSystem.IsWindows() => "win-arm64",
    _ => throw new PlatformNotSupportedException("Jolt packages target glibc Linux and Windows x64/ARM64."),
};
var scratch = Directory.CreateTempSubdirectory("jolt-package-");
try
{
    var feed = args.Length == 2 ? Path.GetFullPath(args[1]) : Path.Combine(scratch.FullName, "feed");
    var tools = Path.Combine(scratch.FullName, "tools");
    var consumer = Path.Combine(scratch.FullName, "consumer");
    Directory.CreateDirectory(consumer);
    var sample = Path.Combine(root, "samples", "Jolt.Counter");
    foreach (var name in new[] { "App.jazor", "CounterButton.jazor", "package.json", "pnpm-lock.yaml", "vite.config.js", "index.html", "main.js" })
        File.Copy(Path.Combine(sample, name), Path.Combine(consumer, name));

    if (args.Length == 0)
        await RunAsync("dotnet", root, "pack", "src/Jolt.Cli/Jolt.Cli.csproj", "-c", "Release", "-o", feed);
    var mainPackage = Directory.EnumerateFiles(feed, $"{packageId}.*.nupkg")
        .Single(path => !Path.GetFileName(path).StartsWith($"{packageId}.linux-", StringComparison.OrdinalIgnoreCase)
                     && !Path.GetFileName(path).StartsWith($"{packageId}.win-", StringComparison.OrdinalIgnoreCase));
    using var mainArchive = ZipFile.OpenRead(mainPackage);
    using var manifest = mainArchive.Entries.Single(entry => entry.Name.EndsWith(".nuspec", StringComparison.Ordinal)).Open();
    var version = XDocument.Load(manifest).Descendants().Single(node => node.Name.LocalName == "version").Value;
    foreach (var packagedRid in new[] { "linux-x64", "linux-arm64", "win-x64", "win-arm64" })
    {
        var ridPackage = Directory.EnumerateFiles(feed, $"{packageId}.{packagedRid}.*.nupkg").Single();
        using var archive = ZipFile.OpenRead(ridPackage);
        foreach (var name in new[]
        {
            "DotnetToolSettings.xml",
            "Microsoft.CodeAnalysis.Razor.Compiler.dll",
            "Microsoft.AspNetCore.Razor.Utilities.Shared.dll",
            "Microsoft.JavaScript.NodeApi.dll",
            packagedRid.StartsWith("win-", StringComparison.Ordinal) ? "libnode.dll" : "libnode.so",
            "licenses/dotnet-sdk/LICENSE.txt",
            "licenses/dotnet-sdk/ThirdPartyNotices.txt",
            "licenses/libnode/NOTICE.txt",
        })
            if (!archive.Entries.Any(entry => entry.FullName == name || entry.FullName.EndsWith('/' + name, StringComparison.Ordinal)))
                throw new InvalidOperationException($"{ridPackage} is missing {name}.");
        using var settingsStream = archive.Entries.Single(entry => entry.Name == "DotnetToolSettings.xml").Open();
        var command = XDocument.Load(settingsStream).Descendants("Command").Single();
        if ((string?)command.Attribute("Runner") != "executable")
            throw new InvalidOperationException($"{ridPackage} does not use the packaged apphost.");
    }

    var config = Path.Combine(scratch.FullName, "NuGet.config");
    new XDocument(new XElement("configuration",
        new XElement("packageSources", new XElement("clear"), new XElement("add", new XAttribute("key", "jolt"), new XAttribute("value", feed)))))
        .Save(config);
    await RunAsync("dotnet", consumer, "tool", "install", packageId, "--version", version, "--tool-path", tools, "--configfile", config, "--no-cache");
    var commandPath = Path.Combine(tools, OperatingSystem.IsWindows() ? "jolt.exe" : "jolt");
    var output = Path.Combine(consumer, ".jolt");
    var npmStore = Path.Combine(scratch.FullName, "npm-store");
    await RunProcessAsync(commandPath, consumer, npmStore, ["build", ".", "--output", output]);
    var app = await File.ReadAllTextAsync(Path.Combine(output, "App.mjs"));
    if (!app.Contains("reka-ui", StringComparison.Ordinal)
        || !File.Exists(Path.Combine(output, "App.mjs.map"))
        || !File.Exists(Path.Combine(output, "CounterButton.mjs")))
        throw new InvalidOperationException("Installed Jolt did not emit the npm import, author source map and local child module.");

    await RunProcessAsync(commandPath, consumer, npmStore, ["bundle", "."]);
    if (!File.Exists(Path.Combine(consumer, "dist", "index.html")))
        throw new InvalidOperationException("Installed Jolt did not produce the Vite release bundle.");

    Console.WriteLine($"Verified {packageId} {version} for {rid} and inspected the four RID packages.");
}
finally
{
    scratch.Delete(recursive: true);
}

static Task RunAsync(string program, string workingDirectory, params string[] arguments)
    => RunProcessAsync(program, workingDirectory, null, arguments);

static async Task RunProcessAsync(string program, string workingDirectory, string? npmStore, string[] arguments)
{
    var start = new ProcessStartInfo(program) { WorkingDirectory = workingDirectory, UseShellExecute = false };
    if (npmStore is not null)
        start.Environment["JazorPnpmStoreDirectory"] = npmStore;
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    using var process = Process.Start(start) ?? throw new InvalidOperationException($"Could not start {program}.");
    await process.WaitForExitAsync();
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"{program} {string.Join(' ', arguments)} returned exit code {process.ExitCode}.");
}
