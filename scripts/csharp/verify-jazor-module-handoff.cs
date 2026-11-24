#!/usr/bin/env dotnet run
#:sdk Microsoft.NET.Sdk.Web
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj
#:project ../../src/Jazor.Common/Jazor.Common.csproj

using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Jazor.NodeHost;
using Jazor.Common.SourceMaps;

if (args is not [var packagePath, var joltPath, var chromiumPath])
    throw new ArgumentException("Usage: verify-jazor-module-handoff.cs <Jazor.nupkg> <Jolt CLI apphost> <Chromium executable>");

var repo = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ScriptPath())!, "..", ".."));
var package = Path.GetFullPath(packagePath, repo);
var jolt = Path.GetFullPath(joltPath, repo);
var chromium = Path.GetFullPath(chromiumPath, repo);
var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(repo, ".tmp", "nodeapi-store");
var scratch = Directory.CreateDirectory(Path.Combine(repo, ".tmp", $"handoff-{Guid.NewGuid():N}"));
using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(5));
var stop = deadline.Token;
var libraryId = $"Jazor.Handoff.Calendar.{Guid.NewGuid():N}";
const string firstVersion = "1.0.0-fixture";
const string secondVersion = "1.0.1-fixture";
const string oldModule = "calendar.mjs";
const string newModule = "calendar-current.mjs";
const string removedModule = "retired.mjs";
const string producerNpmVersion = "4.4.0";
const string frontendNpmVersion = "4.1.0";
var feed = scratch.CreateSubdirectory("feed").FullName;
var producer = scratch.CreateSubdirectory("producer").FullName;
var standalone = scratch.CreateSubdirectory("standalone").FullName;
var integration = scratch.CreateSubdirectory("integration").FullName;
var front = Directory.CreateDirectory(Path.Combine(integration, "front")).FullName;
var standaloneProject = Path.Combine(standalone, "Host.csproj");
var integrationProject = Path.Combine(integration, "Host.csproj");
var frontOutput = Path.Combine(front, "projected");
var standaloneOutput = Path.Combine(standalone, "custom", "modules");
var version = SdkVersion();
Console.WriteLine($"Module handoff fixture: {scratch}");
try
{
    File.Copy(package, Path.Combine(feed, $"Jazor.{version}.nupkg"));
    new XElement("configuration",
        new XElement("packageSources", new XElement("clear"),
            new XElement("add", new XAttribute("key", "candidate"), new XAttribute("value", feed)),
            new XElement("add", new XAttribute("key", "nuget.org"), new XAttribute("value", "https://api.nuget.org/v3/index.json"))),
        new XElement("packageSourceMapping",
            new XElement("packageSource", new XAttribute("key", "candidate"),
                new XElement("package", new XAttribute("pattern", "Jazor")),
                new XElement("package", new XAttribute("pattern", libraryId))),
            new XElement("packageSource", new XAttribute("key", "nuget.org"),
                new XElement("package", new XAttribute("pattern", "*")))))
        .Save(Path.Combine(scratch.FullName, "NuGet.Config"));
    await WriteAsync(producer, "package.json", """{"private":true,"type":"module","dependencies":{"date-fns":"4.4.0"}}""");
    await WriteAsync(producer, "pnpm-lock.yaml", """
        lockfileVersion: '9.0'
        settings:
          autoInstallPeers: true
          excludeLinksFromLockfile: false
        importers:
          .:
            dependencies:
              date-fns:
                specifier: 4.4.0
                version: 4.4.0
        packages:
          date-fns@4.4.0:
            resolution: {integrity: sha512-+1UMbeh68lH1SegH83CGWwpb6OHHbpSgr3+s5Eww5M4CAgswBpoWS0AjTOfEJ33HiYKz1hdj/KTFprzXHmq/6w==}
        snapshots:
          date-fns@4.4.0: {}
        """);

    await PackProducerAsync(firstVersion, oldModule, 2, includeRetired: true);
    WriteHost(standaloneProject, "custom/modules", firstVersion);
    WriteHost(integrationProject, "front/projected", firstVersion);
    await WriteAsync(standalone, "Program.cs", "public static class Program { public static void Main() { } }");
    File.Copy(Path.Combine(standalone, "Program.cs"), Path.Combine(integration, "Program.cs"));
    await WriteFrontendAsync();
    var authored = ReadFrontendFiles();

    await BuildHostAsync(standaloneProject);
    await BuildHostAsync(standaloneProject, "none", "--no-restore");
    Require(!Directory.Exists(standaloneOutput), "Default and none modes must leave the raw module output untouched.");
    await BuildHostAsync(standaloneProject, "debug", "--no-restore");
    await BuildHostAsync(integrationProject, "debug");
    await VerifyRestoreAsync(standalone, firstVersion);
    await VerifyRestoreAsync(integration, firstVersion);
    await VerifyModulesAsync(standaloneOutput, oldModule, 2);
    await VerifyModulesAsync(frontOutput, oldModule, 2);
    Require(File.ReadAllBytes(Path.Combine(standaloneOutput, oldModule)).SequenceEqual(File.ReadAllBytes(Path.Combine(frontOutput, oldModule))),
        "Both independent SDK hosts should materialize the same packed module bytes.");
    await BuildAndRunSpaAsync("day:2", authored);

    await WriteAsync(standaloneOutput, "keep.txt", "host-owned");
    await WriteAsync(frontOutput, "keep.txt", "frontend-owned");
    var unchanged = File.GetLastWriteTimeUtc(Path.Combine(standaloneOutput, oldModule));
    await BuildHostAsync(standaloneProject, "debug", "--no-restore");
    Require(File.GetLastWriteTimeUtc(Path.Combine(standaloneOutput, oldModule)) == unchanged,
        "An unchanged host build rewrote its producer module.");

    await PackProducerAsync(secondVersion, newModule, 3, includeRetired: false);
    WriteHost(standaloneProject, "custom/modules", secondVersion);
    WriteHost(integrationProject, "front/projected", secondVersion);
    // A directory at the destination fails the actual module write on all supported hosts.
    var blocked = Directory.CreateDirectory(Path.Combine(standaloneOutput, newModule));
    File.WriteAllText(Path.Combine(blocked.FullName, "keep"), "blocking directory");
    var failure = await RunAsync(standalone, "dotnet", ["build", standaloneProject, "--nologo", "-p:JazorMode=debug"]);
    Require(failure.Code != 0 && failure.Output.Contains("Output target is a directory", StringComparison.Ordinal) &&
            failure.Output.Contains(newModule, StringComparison.Ordinal),
        "Expected module write failure at the intentionally blocked destination.");
    Require(File.ReadAllText(Path.Combine(blocked.FullName, "keep")) == "blocking directory", "Emit edited the blocking directory.");
    blocked.Delete(recursive: true);
    await BuildHostAsync(standaloneProject, "debug", "--no-restore");
    await BuildHostAsync(integrationProject, "debug");
    await VerifyRestoreAsync(standalone, secondVersion);
    await VerifyRestoreAsync(integration, secondVersion);
    foreach (var output in new[] { standaloneOutput, frontOutput })
    {
        Require(!File.Exists(Path.Combine(output, oldModule)) && !File.Exists(Path.Combine(output, oldModule + ".map")) &&
                !File.Exists(Path.Combine(output, removedModule)) && !File.Exists(Path.Combine(output, removedModule + ".map")),
            $"Stale modules and maps remain in {output} after upgrade.");
        await VerifyModulesAsync(output, newModule, 3);
    }
    Require(File.ReadAllText(Path.Combine(standaloneOutput, "keep.txt")) == "host-owned" &&
            File.ReadAllText(Path.Combine(frontOutput, "keep.txt")) == "frontend-owned", "Rebuild removed a neighboring author file.");
    await ReplaceFrontendModuleAsync();
    authored["main.ts"] = File.ReadAllBytes(Path.Combine(front, "main.ts"));
    await BuildAndRunSpaAsync("day:3", authored);
    await VerifyDevelopmentSpaAsync("day:3");

    var publish = Path.Combine(scratch.FullName, "publish");
    await BuildHostAsync(standaloneProject, "release", "--no-restore");
    await RequireSuccessAsync(standalone, "dotnet", ["publish", standaloneProject, "--no-restore", "--nologo",
        "-p:JazorMode=release", "-p:JazorPublishSourceMaps=true", "-c", "Debug", "-o", publish]);
    var publishedDist = Path.Combine(publish, "jazor", "dist");
    Require(File.Exists(Path.Combine(publishedDist, "bundle.js")) &&
            File.Exists(Path.Combine(publishedDist, "bundle.js.map")), "Folder Publish omitted the standalone release bundle or source map.");
    await WriteAsync(publishedDist, "verify.mjs", "import { Read } from './bundle.js'; if (Read() !== 'day:3') throw new Error(Read());");
    await NodeModuleRunner.RunAsync(publishedDist, Path.Combine(publishedDist, "verify.mjs"), cancellationToken: stop);
    Console.WriteLine("Independent NuGet consumers executed the same npm/CLR library. The Jolt SPA shared its locked npm export with the packed module while the standalone host used the producer's version. Upgrades pruned stale modules and maps and recovered from a write failure.");
}
catch
{
    Console.Error.WriteLine($"Module handoff fixture preserved at {scratch.FullName}");
    throw;
}
scratch.Delete(recursive: true);

async Task PackProducerAsync(string revision, string module, int days, bool includeRetired)
{
    var project = Path.Combine(producer, "Calendar.csproj");
    new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"),
        Properties(new XElement("PackageId", libraryId), new XElement("Version", revision)),
        new XElement("ItemGroup", Reference("Jazor", version))).Save(project);
    await WriteAsync(producer, "Calendar.cs", $$"""
        using ECMAScript;
        using Dates = Npm.DateFns.Exports;

        [ECMAScriptModule("{{module}}")]
        public static class Calendar
        {
            public static string Read() =>
                string.Intern("day:" + Dates.AddDays(new Date(2024, 0, 31), {{days}}).GetDate());

            public static System.Func<Date, double, Date> AddDaysImport() => Dates.AddDays;
        }
        {{(includeRetired ? "[ECMAScriptModule(\"retired.mjs\")] public static class Retired { public static int ReadRetired() => 0; }" : "")}}
        """);
    await RequireSuccessAsync(producer, "dotnet", ["pack", project, "--nologo", "-c", "Debug", "-o", feed]);
}

void WriteHost(string project, string output, string revision) =>
    new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"),
        Properties(new XElement("OutputType", "Exe"),
            new XElement("JazorDir", "$(MSBuildProjectDirectory)/" + output + "/"),
            new XElement("DefaultItemExcludes", "$(DefaultItemExcludes);front/**;custom/**")),
        new XElement("ItemGroup", Reference("Jazor", version), Reference(libraryId, revision)))
        .Save(project);

XElement Properties(params XElement[] extra) => new("PropertyGroup",
    new XElement("TargetFramework", "net11.0"),
    new XElement("LangVersion", "preview"),
    new XElement("JazorPnpmStoreDirectory", store),
    extra);

static XElement Reference(string id, string version) =>
    new("PackageReference", new XAttribute("Include", id), new XAttribute("Version", "[" + version + "]"));

async Task BuildHostAsync(string project, string? mode = null, params string[] extra) =>
    await RequireSuccessAsync(Path.GetDirectoryName(project)!, "dotnet",
        new[] { "build", project, "--nologo" }.Concat(mode is null ? [] : ["-p:JazorMode=" + mode]).Concat(extra).ToArray());

async Task VerifyRestoreAsync(string host, string revision)
{
    using var assets = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(host, "obj", "project.assets.json"), stop));
    var entries = assets.RootElement.GetProperty("libraries");
    await VerifyHashAsync("Jazor", version, package);
    await VerifyHashAsync(libraryId, revision, Path.Combine(feed, $"{libraryId}.{revision}.nupkg"));

    async Task VerifyHashAsync(string id, string release, string nupkg)
    {
        using var stream = File.OpenRead(nupkg);
        Require(entries.GetProperty(id + "/" + release).GetProperty("sha512").GetString() ==
            Convert.ToBase64String(await SHA512.HashDataAsync(stream, stop)),
            $"{host} resolved a different package for {id} {release}.");
    }
}

async Task VerifyModulesAsync(string root, string name, int day)
{
    var js = await File.ReadAllTextAsync(Path.Combine(root, name), stop);
    Require(js.Contains("from \"date-fns\"", StringComparison.Ordinal) &&
        js.Contains("from \"./clr/System/StringModule.js\"", StringComparison.Ordinal) &&
        File.Exists(Path.Combine(root, "clr", "System", "StringModule.js")),
        $"Missing real npm or CLR import in {root}/{name}.{Environment.NewLine}{js}");
    VerifyCalendarSourceMap(js, await File.ReadAllTextAsync(Path.Combine(root, name + ".map"), stop));
    using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(root, "package.json"), stop));
    Require(manifest.RootElement.GetProperty("dependencies").GetProperty("date-fns").GetString() == producerNpmVersion,
        "Standalone Emit omitted the exact installed npm dependency version from the producer assembly.");
    await VerifyInstalledDateFnsAsync(root, producerNpmVersion);
    const string probe = "probe.mjs";
    await WriteAsync(root, probe, $$"""
        import { addDays } from 'date-fns';
        import { AddDaysImport, Read } from './{{name}}';
        if (Read() !== 'day:{{day}}') throw new Error(Read());
        if (AddDaysImport() !== addDays) throw new Error('The packed module did not return the original npm export');
        """);
    await NodeModuleRunner.RunAsync(root, Path.Combine(root, probe), cancellationToken: stop);
}

static void VerifyCalendarSourceMap(string js, string json)
{
    var generated = js.ReplaceLineEndings("\n").Split('\n');
    var line = Array.FindIndex(generated, text => text.Contains("return ", StringComparison.Ordinal) &&
        text.Contains("addDays(", StringComparison.Ordinal));
    Require(line >= 0, "The generated calendar npm call was not found.");
    var column = generated[line].IndexOf("addDays(", StringComparison.Ordinal);
    var map = new SourceMapReader().Read(json);
    var segment = map.Segments.Where(segment => segment.GeneratedLine == line && segment.GeneratedColumn <= column)
        .OrderByDescending(segment => segment.GeneratedColumn).FirstOrDefault();
    Require(segment is not null, "The generated calendar npm call has no source mapping.");
    var source = map.Sources[segment!.SourceIndex];
    var authoredLines = (source.Content ?? "").ReplaceLineEndings("\n").Split('\n');
    var authoredLine = Array.FindIndex(authoredLines, text => text.Contains("Dates.AddDays(new Date", StringComparison.Ordinal));
    Require(source.Path.EndsWith("Calendar.cs", StringComparison.Ordinal) && authoredLine >= 0 &&
        segment.SourceLine == authoredLine && segment.SourceColumn < authoredLines[authoredLine].Length,
        $"The generated addDays call maps to {source.Path}:{segment.SourceLine + 1}:{segment.SourceColumn + 1}, expected Calendar.cs:{authoredLine + 1}.");
}

async Task WriteFrontendAsync()
{
    var sample = Path.Combine(repo, "samples", "Jolt.Mixed");
    foreach (var file in new[] { "package.json", "pnpm-lock.yaml", "index.html" })
        File.Copy(Path.Combine(sample, file), Path.Combine(front, file));
    var manifest = JsonNode.Parse(await File.ReadAllTextAsync(Path.Combine(front, "package.json"), stop))!;
    manifest["dependencies"]!["date-fns"] = frontendNpmVersion;
    await WriteAsync(front, "package.json", manifest.ToJsonString());
    await NodeToolchain.RestoreAsync(front, store, stop, Console.Out);
    await WriteAsync(front, "main.ts", """
        import { createApp, h } from 'vue';
        import { addDays } from 'date-fns';
        import Message from './Message.jazor';
        import { AddDaysImport, Read } from './projected/calendar.mjs';
        const source = AddDaysImport() === addDays ? 'frontend' : 'duplicate';
        createApp({ render: () => h(Message, { Text: Read() + ':' + source }) }).mount('#app');
        """);
    await WriteAsync(front, "Message.jazor", """
        <output id="module-value">@Text</output>
        @code { [Parameter] public string Text { get; set; } = ""; }
        """);
}

async Task ReplaceFrontendModuleAsync()
{
    var main = await File.ReadAllTextAsync(Path.Combine(front, "main.ts"), stop);
    await WriteAsync(front, "main.ts", main.Replace(oldModule, newModule, StringComparison.Ordinal));
}

Dictionary<string, byte[]> ReadFrontendFiles() =>
    new[] { "index.html", "main.ts", "Message.jazor", "package.json", "pnpm-lock.yaml" }
        .ToDictionary(file => file, file => File.ReadAllBytes(Path.Combine(front, file)));

async Task BuildAndRunSpaAsync(string expected, Dictionary<string, byte[]> authored)
{
    // Keep both installations present. Chromium compares the actual export identity.
    await RequireSuccessAsync(front, jolt, ["bundle", "."]);
    await VerifyInstalledDateFnsAsync(front, frontendNpmVersion);
    await VerifyInstalledDateFnsAsync(frontOutput, producerNpmVersion);
    foreach (var (name, content) in authored)
        Require(File.ReadAllBytes(Path.Combine(front, name)).SequenceEqual(content), "Jolt/Emit rewrote frontend authored file " + name);

    var dist = Path.Combine(front, "dist");
    var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { WebRootPath = dist });
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
    await using var http = builder.Build();
    http.Urls.Add("http://127.0.0.1:0");
    http.UseDefaultFiles();
    http.UseStaticFiles();
    await http.StartAsync(stop);
    var browserScript = Path.Combine(repo, "samples", "Jolt.Counter", "verify-browser.cs");
    await VerifyBrowserAsync(http.Urls.Single(), expected);
}

async Task VerifyInstalledDateFnsAsync(string directory, string expected)
{
    using var installed = JsonDocument.Parse(await File.ReadAllTextAsync(
        Path.Combine(directory, "node_modules", "date-fns", "package.json"), stop));
    Require(installed.RootElement.GetProperty("version").GetString() == expected,
        $"{directory} did not install date-fns {expected}.");
}

async Task VerifyBrowserAsync(string origin, string expected)
{
    var browserScript = Path.Combine(repo, "samples", "Jolt.Counter", "verify-browser.cs");
    await RequireSuccessAsync(front, "dotnet", ["run", "--file", browserScript, "--", chromium, origin, "--expect-module-value=" + expected + ":frontend"]);
}

async Task VerifyDevelopmentSpaAsync(string expected)
{
    var start = new ProcessStartInfo(jolt)
    {
        WorkingDirectory = front,
        UseShellExecute = false,
        RedirectStandardOutput = true,
    };
    start.Environment["JazorPnpmStoreDirectory"] = store;
    foreach (var argument in new[] { "dev", ".", "--port", "0" })
        start.ArgumentList.Add(argument);
    using var server = Process.Start(start)!;
    Task<string>? remainingOutput = null;
    try
    {
        while (await server.StandardOutput.ReadLineAsync(stop) is { } line)
        {
            Console.WriteLine(line);
            if (!Uri.TryCreate(line, UriKind.Absolute, out var origin) || origin.Scheme != "http")
                continue;

            remainingOutput = server.StandardOutput.ReadToEndAsync();
            await VerifyBrowserAsync(origin.AbsoluteUri, expected);
            await VerifyInstalledDateFnsAsync(front, frontendNpmVersion);
            await VerifyInstalledDateFnsAsync(frontOutput, producerNpmVersion);
            return;
        }
        throw new InvalidOperationException("Jolt exited before reporting its development server origin.");
    }
    finally
    {
        if (!server.HasExited)
            server.Kill(entireProcessTree: true);
        await server.WaitForExitAsync();
        if (remainingOutput is not null)
            Console.Write(await remainingOutput);
    }
}

Task WriteAsync(string directory, string file, string content) => File.WriteAllTextAsync(Path.Combine(directory, file), content, stop);

string SdkVersion()
{
    using var archive = ZipFile.OpenRead(package);
    using var stream = archive.GetEntry("Jazor.nuspec")!.Open();
    var manifest = XElement.Load(stream);
    var ns = manifest.Name.Namespace;
    return manifest.Element(ns + "metadata")!.Element(ns + "version")!.Value;
}

async Task RequireSuccessAsync(string directory, string app, string[] command)
{
    var result = await RunAsync(directory, app, command);
    Require(result.Code == 0, $"{app} {string.Join(' ', command)} failed with exit code {result.Code}.");
}

async Task<(int Code, string Output)> RunAsync(string directory, string app, string[] command)
{
    var start = new ProcessStartInfo(app)
    {
        WorkingDirectory = directory,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };
    start.Environment["JazorPnpmStoreDirectory"] = store;
    foreach (var argument in command)
        start.ArgumentList.Add(argument);
    using var process = Process.Start(start)!;
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    try { await process.WaitForExitAsync(stop); }
    finally
    {
        if (!process.HasExited)
            process.Kill(entireProcessTree: true);
    }
    var output = await stdout + await stderr;
    Console.Write(output);
    return (process.ExitCode, output);
}

static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

static string ScriptPath([CallerFilePath] string path = "") => path;
