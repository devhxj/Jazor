#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Xml.Linq;

const string packageId = "Jazor.Jolt";
var editorFeatures = args.Contains("--editor-features", StringComparer.Ordinal);
var browserIndex = Array.IndexOf(args, "--browser");
var browserPath = browserIndex < 0 ? null :
    browserIndex + 1 < args.Length ? Path.GetFullPath(args[browserIndex + 1]) :
    throw new ArgumentException("--browser requires a browser executable path.");
args = args.Where((argument, index) => argument != "--editor-features" &&
    (browserIndex < 0 || index != browserIndex && index != browserIndex + 1)).ToArray();
if (args.Length != 0 && args is not ["--package-source", _])
    throw new ArgumentException("Usage: verify-jolt-package.cs [--package-source directory] [--editor-features] [--browser executable]");
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
    if (editorFeatures)
        await RunAsync("dotnet", root, "run", "--file", Path.Combine(sample, "verify-browser.cs"), "--", "--prepare", consumer);
    else
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
    // RID tools use an SDK-generated .cmd launcher on Windows.
    var commandPath = OperatingSystem.IsWindows() ? "cmd.exe" : Path.Combine(tools, "jolt");
    var output = Path.Combine(consumer, ".jolt");
    var npmStore = Path.Combine(scratch.FullName, "npm-store");
    await RunInstalledAsync("build");
    var app = await File.ReadAllTextAsync(Path.Combine(output, "App.mjs"));
    if (!app.Contains("reka-ui", StringComparison.Ordinal)
        || !File.Exists(Path.Combine(output, "App.mjs.map"))
        || !File.Exists(Path.Combine(output, "CounterButton.mjs")))
        throw new InvalidOperationException("Installed Jolt did not emit the npm import, author source map and local child module.");

    await RunInstalledAsync("bundle");
    if (!File.Exists(Path.Combine(consumer, "dist", "index.html")))
        throw new InvalidOperationException("Installed Jolt did not produce the Vite release bundle.");
    if (editorFeatures && (!File.Exists(Path.Combine(output, "App.jolt.css")) ||
        !Directory.EnumerateFiles(Path.Combine(consumer, "dist"), "*.css", SearchOption.AllDirectories)
            .Any(path => File.ReadAllText(path).Contains("data-v-", StringComparison.Ordinal))))
        throw new InvalidOperationException("Installed Jolt did not emit and bundle scoped component styles.");
    var appHost = Directory.EnumerateFiles(Path.Combine(tools, ".store"),
        OperatingSystem.IsWindows() ? "Jolt.Cli.exe" : "Jolt.Cli", SearchOption.AllDirectories).Single();
    if (editorFeatures)
        await RunProcessAsync("dotnet", root, npmStore,
            ["run", "--file", Path.Combine(root, "src", "Jolt.LanguageServer", "verify.cs"), "--", appHost]);
    if (browserPath is not null)
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        var start = new ProcessStartInfo(appHost) { WorkingDirectory = consumer, UseShellExecute = false };
        start.Environment["JazorPnpmStoreDirectory"] = npmStore;
        foreach (var argument in new[] { "dev", ".", "--port", port.ToString() })
            start.ArgumentList.Add(argument);
        using var server = Process.Start(start) ?? throw new InvalidOperationException("Could not start the packaged Jolt dev server.");
        try
        {
            var browserArguments = new List<string>
            {
                "run", "--file", Path.Combine(sample, "verify-browser.cs"), "--",
                browserPath, $"http://127.0.0.1:{port}/"
            };
            if (editorFeatures)
                browserArguments.Add("--features");
            await RunProcessAsync("dotnet", root, npmStore, browserArguments.ToArray());
        }
        finally
        {
            if (!server.HasExited)
                server.Kill(entireProcessTree: true);
            await server.WaitForExitAsync();
        }
    }
    if (editorFeatures)
    {
        var oldCss = Path.Combine(output, "CounterButton.jolt.css");
        var oldScope = (await File.ReadAllLinesAsync(oldCss))[0];
        File.Move(Path.Combine(consumer, "CounterButton.jazor"), Path.Combine(consumer, "StyledButton.jazor"));
        var appSource = Path.Combine(consumer, "App.jazor");
        await File.WriteAllTextAsync(appSource, (await File.ReadAllTextAsync(appSource))
            .Replace("CounterButton", "StyledButton", StringComparison.Ordinal));
        await RunInstalledAsync("build");
        var newCss = Path.Combine(output, "StyledButton.jolt.css");
        if (File.Exists(oldCss) || File.Exists(Path.Combine(output, "CounterButton.mjs")) ||
            !File.Exists(newCss) || (await File.ReadAllLinesAsync(newCss))[0] == oldScope)
            throw new InvalidOperationException("A renamed component did not replace its module and scoped stylesheet.");
    }

    Console.WriteLine($"Verified {packageId} {version} for {rid} and inspected the four RID packages.");

    Task RunInstalledAsync(string verb) => RunProcessAsync(commandPath, consumer, npmStore,
        OperatingSystem.IsWindows() ? ["/d", "/c", "jolt.cmd", verb, "."] : [verb, "."], tools);
}
finally
{
    scratch.Delete(recursive: true);
}

static Task RunAsync(string program, string workingDirectory, params string[] arguments)
    => RunProcessAsync(program, workingDirectory, null, arguments);

static async Task RunProcessAsync(string program, string workingDirectory, string? npmStore, string[] arguments, string? toolDirectory = null)
{
    var start = new ProcessStartInfo(program) { WorkingDirectory = workingDirectory, UseShellExecute = false };
    if (toolDirectory is not null)
        start.Environment["PATH"] = toolDirectory + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH");
    if (npmStore is not null)
        start.Environment["JazorPnpmStoreDirectory"] = npmStore;
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    using var process = Process.Start(start) ?? throw new InvalidOperationException($"Could not start {program}.");
    await process.WaitForExitAsync();
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"{program} {string.Join(' ', arguments)} returned exit code {process.ExitCode}.");
}
