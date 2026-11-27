#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
var scratch = Directory.CreateDirectory(Path.Combine(root, "test", "jolt", "package", Guid.NewGuid().ToString("N")));
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
            "Jazor.Npm.dll",
            "Jolt.Vite.dll",
            "ECMAScript.Vue.dll",
            "ECMAScript.Pinia.dll",
            "ECMAScript.Pinia.Testing.dll",
            "ECMAScript.Vue.Devtools.dll",
            "ECMAScript.VueRoute.dll",
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
    // Source candidates may share the version of a locally cached official tool.
    await RunProcessAsync("dotnet", consumer, null,
        ["tool", "install", packageId, "--version", version, "--tool-path", tools, "--configfile", config, "--no-cache"],
        packageCache: Path.Combine(scratch.FullName, "nuget"));
    // Windows SDK versions can produce either an executable or a command shim.
    var useCommandShim = OperatingSystem.IsWindows() && !File.Exists(Path.Combine(tools, "jolt.exe"));
    var commandPath = useCommandShim ? "cmd.exe" : Path.Combine(tools, OperatingSystem.IsWindows() ? "jolt.exe" : "jolt");
    var output = Path.Combine(consumer, ".jazor");
    var npmStore = Path.GetFullPath(Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
        ?? Path.Combine(scratch.FullName, "npm-store"));
    var help = await RunInstalledAsync("--help", captureOutput: true);
    if (!help.Contains("Usage: jolt build", StringComparison.Ordinal) ||
        !help.Contains("--stop-on-stdin-close", StringComparison.Ordinal) ||
        help.Contains("bindings", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException($"Unexpected help from installed Jolt.\n{help}");
    await RunInstalledAsync("bindings", expectedError: "Usage: jolt build");
    await RunInstalledAsync("build", expectedError: "Usage: jolt build", extraArgument: "--stop-on-stdin-close");
    await RunInstalledAsync("build");
    var app = await File.ReadAllTextAsync(Path.Combine(output, "App.mjs"));
    if (!app.Contains("reka-ui", StringComparison.Ordinal)
        || !File.Exists(Path.Combine(output, "App.mjs.map"))
        || !File.Exists(Path.Combine(output, "CounterButton.mjs")))
        throw new InvalidOperationException("Installed Jolt did not emit the npm import, author source map and local child module.");
    if (editorFeatures)
    {
        if (!File.Exists(Path.Combine(output, "TypedTable.mjs")) || File.Exists(Path.Combine(output, "_Imports.mjs")))
            throw new InvalidOperationException("Installed Jolt must emit TypedTable.mjs and omit _Imports.mjs.");
        var nativeForm = Path.Combine(output, "NativeForm.mjs");
        var stringModule = Regex.Match(await File.ReadAllTextAsync(nativeForm),
            """
            \bfrom\s+"(?<path>\.{1,2}/[^"]*clr/System/StringModule\.js)"
            """).Groups["path"].Value;
        if (stringModule.Length == 0 || !File.Exists(Path.Combine(Path.GetDirectoryName(nativeForm)!, stringModule)))
            throw new InvalidOperationException("Installed Jolt did not emit and materialize the NativeForm StringModule import.");
    }
    if (editorFeatures)
    {
        var counter = await File.ReadAllTextAsync(Path.Combine(output, "NpmCounter.mjs"));
        foreach (var specifier in new[] { "es-toolkit/string", "@vueuse/core" })
            if (!counter.Contains($"from \"{specifier}\"", StringComparison.Ordinal))
                throw new InvalidOperationException($"Installed Jolt did not preserve the public ESM specifier '{specifier}' in NpmCounter.mjs.");
    }

    await RunInstalledAsync("bundle");
    if (!File.Exists(Path.Combine(consumer, ".jazor", "dist", "index.html")))
        throw new InvalidOperationException("Installed Jolt did not produce the Vite release bundle.");
    if (editorFeatures && (!File.Exists(Path.Combine(output, "App.jolt.css")) ||
        !Directory.EnumerateFiles(Path.Combine(consumer, ".jazor", "dist"), "*.css", SearchOption.AllDirectories)
            .Any(path => File.ReadAllText(path).Contains("data-v-", StringComparison.Ordinal))))
        throw new InvalidOperationException("Installed Jolt did not emit and bundle scoped component styles.");
    var appHost = Directory.EnumerateFiles(Path.Combine(tools, ".store"),
        OperatingSystem.IsWindows() ? "Jolt.Cli.exe" : "Jolt.Cli", SearchOption.AllDirectories).Single();
    await VerifyHandwrittenBindingsAsync(appHost, root, scratch.FullName, npmStore);
    if (editorFeatures)
    {
        await RunProcessAsync("dotnet", root, npmStore,
            ["run", "--file", Path.Combine(root, "src", "Jolt.LanguageServer", "verify.cs"), "--", appHost]);
        await RunProcessAsync("dotnet", root, npmStore,
            ["run", "--file", Path.Combine(root, "src", "Jolt.LanguageServer", "verify.cs"), "--", "--local-vue", appHost]);
        await RunProcessAsync("dotnet", root, npmStore,
            ["run", "--file", Path.Combine(root, "scripts", "csharp", "verify-jolt-projection-cache.cs"), "--", appHost, "--external-link"]);
        await RunProcessAsync("dotnet", root, npmStore,
            ["run", "--file", Path.Combine(root, "src", "Jolt.LanguageServer", "verify.cs"), "--", "--router", appHost]);
    }
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
        if (editorFeatures)
        {
            foreach (var check in new[] { "verify-jolt-library.cs", "verify-jolt-mixed.cs", "verify-jolt-ssr.cs" })
                await RunProcessAsync("dotnet", root, npmStore,
                    ["run", "--file", Path.Combine(root, "scripts", "csharp", check), "--", appHost, browserPath]);
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

    Task<string> RunInstalledAsync(string verb, bool captureOutput = false, string? expectedError = null, string? extraArgument = null)
    {
        string[] arguments = verb == "--help" ? [verb] : [verb, "."];
        if (extraArgument is not null)
            arguments = [.. arguments, extraArgument];
        return RunProcessAsync(commandPath, consumer, npmStore,
            useCommandShim ? ["/d", "/c", "%JOLT_PACKAGE_SHIM%", .. arguments] : arguments,
            tools, expectedError: expectedError, captureOutput: captureOutput);
    }
}
finally
{
    scratch.Delete(recursive: true);
}

static Task RunAsync(string program, string workingDirectory, params string[] arguments)
    => RunProcessAsync(program, workingDirectory, null, arguments);

static async Task VerifyHandwrittenBindingsAsync(string appHost, string root, string scratch, string npmStore)
{
    var fixture = Path.Combine(root, "scripts", "csharp", "fixtures", "jolt-handwritten-bindings");
    var project = Directory.CreateDirectory(Path.Combine(scratch, "handwritten-bindings")).FullName;
    foreach (var file in new[] { "App.jazor", "package.json", "pnpm-lock.yaml" })
        File.Copy(Path.Combine(fixture, file), Path.Combine(project, file));

    await RunProcessAsync(appHost, project, npmStore, ["build"]);
    var module = await File.ReadAllTextAsync(Path.Combine(project, ".jazor", "App.mjs"));
    foreach (var statement in new[]
    {
        """import { createTestingPinia } from "@pinia/testing";""",
        """import { setActivePinia } from "pinia";""",
        """import { removeCustomCommand } from "@vue/devtools-api";""",
        """import { createMemoryHistory, createRouter } from "vue-router";""",
        "setActivePinia(createTestingPinia());",
        """removeCustomCommand("probe");""",
        "history: createMemoryHistory()"
    })
    {
        if (!module.Contains(statement, StringComparison.Ordinal))
            throw new InvalidOperationException($"Missing {statement} in the generated module.\n{module}");
    }

    var source = Path.Combine(project, "App.jazor");
    await File.WriteAllTextAsync(source, (await File.ReadAllTextAsync(source)).Replace(
        """RemoveCustomCommand("probe")""", "RemoveCustomCommand(true)", StringComparison.Ordinal));
    await RunProcessAsync(appHost, project, npmStore, ["build"], expectedError: "error CS1503");
}

static async Task<string> RunProcessAsync(string program, string workingDirectory, string? npmStore, string[] arguments,
    string? toolDirectory = null, string? packageCache = null, string? expectedError = null, bool captureOutput = false)
{
    var start = new ProcessStartInfo(program) { WorkingDirectory = workingDirectory, UseShellExecute = false };
    if (toolDirectory is not null)
        start.Environment["JOLT_PACKAGE_SHIM"] = '"' + Path.Combine(toolDirectory, "jolt.cmd") + '"';
    if (npmStore is not null)
        start.Environment["JazorPnpmStoreDirectory"] = npmStore;
    if (packageCache is not null)
        start.Environment["NUGET_PACKAGES"] = packageCache;
    if (captureOutput || expectedError is not null)
    {
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
    }
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    using var process = Process.Start(start) ?? throw new InvalidOperationException($"Could not start {program}.");
    var output = start.RedirectStandardOutput ? process.StandardOutput.ReadToEndAsync() : Task.FromResult("");
    var errors = start.RedirectStandardError ? process.StandardError.ReadToEndAsync() : Task.FromResult("");
    await process.WaitForExitAsync();
    var diagnostics = await output + await errors;
    if (expectedError is not null)
    {
        if (process.ExitCode == 0 || !diagnostics.Contains(expectedError, StringComparison.Ordinal))
            throw new InvalidOperationException($"Expected {expectedError} from {program}.\n{diagnostics}");
    }
    else if (process.ExitCode != 0)
        throw new InvalidOperationException($"{program} {string.Join(' ', arguments)} returned exit code {process.ExitCode}.\n{diagnostics}");
    return diagnostics;
}
