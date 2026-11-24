#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;

if (args.Length is < 1 or > 2)
    throw new ArgumentException("Usage: verify-jolt-dev-dependencies.cs <Jolt CLI apphost> [Chromium executable]");
var appHost = args[0];
var repository = new DirectoryInfo(Directory.GetCurrentDirectory());
while (repository is not null && !File.Exists(Path.Combine(repository.FullName, "Jazor.slnx")))
    repository = repository.Parent;
var sample = Path.Combine(repository?.FullName ?? throw new InvalidOperationException("Cannot locate Jazor.slnx."), "samples", "Jolt.Counter");
var scratch = Directory.CreateDirectory(Path.Combine(repository!.FullName, ".tmp", $"jolt-dev-dependencies-{Guid.NewGuid():N}"));
try
{
    var project = scratch.CreateSubdirectory("frontend").FullName;
    var linked = scratch.CreateSubdirectory("linked").FullName;
    await File.WriteAllTextAsync(Path.Combine(linked, "package.json"), """
        {"name":"linked-watch-probe","version":"1.0.0","type":"module","exports":{"types":"./index.d.ts","import":"./index.mjs"}}
        """);
    await File.WriteAllTextAsync(Path.Combine(linked, "index.mjs"), "export function format(value) { return value; }");
    await File.WriteAllTextAsync(Path.Combine(linked, "index.d.ts"), "export { format } from './first.js';");
    var firstDeclaration = Path.Combine(linked, "first.d.ts");
    var secondDeclaration = Path.Combine(linked, "second.d.ts");
    const string signature = "export declare function format(value: string): string;";
    await File.WriteAllTextAsync(firstDeclaration, signature);
    var vueLinked = scratch.CreateSubdirectory("vue-linked").FullName;
    await File.WriteAllTextAsync(Path.Combine(vueLinked, "package.json"), """
        {"name":"linked-vue-props","version":"1.0.0","types":"./index.d.ts"}
        """);
    await File.WriteAllTextAsync(Path.Combine(vueLinked, "index.d.ts"), "export type { CardProps } from './props.js';");
    var externalCardTypes = Path.Combine(vueLinked, "props.d.ts");
    const string cardProps = "export interface CardProps { title: string }";
    await File.WriteAllTextAsync(externalCardTypes, cardProps);
    foreach (var name in new[] { "CounterButton.jazor", "package.json", "pnpm-lock.yaml", "index.html", "main.js" })
        File.Copy(Path.Combine(sample, name), Path.Combine(project, name));
    var app = Path.Combine(project, "App.jazor");
    const string appSource = """
        @using Text = Npm.EsToolkit.String.Exports
        @using Linked = Npm.LinkedWatchProbe.Exports
        @using Shared
        <p>@(Text.CamelCase("Order Total"))</p>
        <p id="linked">@(Linked.Format("external declaration"))</p>
        <LocalCard Title="Hello" />
        <p>@Entry.Label</p>
        <p id="left">@Display.Value</p>
        <Sibling />
        @code { private Row Entry = new("Shared DTO"); }
        """;
    await File.WriteAllTextAsync(app, appSource);
    var vuePath = Path.Combine(project, "LocalCard.vue");
    const string vueSource = """
        <script setup lang="ts">
        import type { CardProps } from './card-props'
        defineProps<CardProps>()
        </script>
        <template><p id="card">{{ title }}</p></template>
        """;
    await File.WriteAllTextAsync(vuePath, vueSource);
    var cardTypes = Path.Combine(project, "card-props.d.ts");
    await File.WriteAllTextAsync(cardTypes, cardProps);
    var contract = Path.Combine(project, "Contract.cs");
    const string contractSource = """
        namespace Shared;
        public sealed record Row(string Label);
        public static class Display { public const string Value = "before"; }
        """;
    await File.WriteAllTextAsync(contract, contractSource);
    await File.WriteAllTextAsync(Path.Combine(project, "Sibling.jazor"), """
        @using Shared
        <p id="right">@Display.Value</p>
        """);
    var manifestPath = Path.Combine(project, "package.json");
    var packageManifest = JsonNode.Parse(await File.ReadAllTextAsync(manifestPath))!;
    packageManifest["devDependencies"]!["@vitejs/plugin-vue"] = "6.0.7";
    packageManifest["dependencies"]!["linked-watch-probe"] = "link:../linked";
    packageManifest["dependencies"]!["linked-vue-props"] = "link:../vue-linked";
    packageManifest["jolt"] = JsonNode.Parse("""{"csharpSources":["Contract.cs"]}""");
    var manifest = packageManifest.ToJsonString();
    await File.WriteAllTextAsync(manifestPath, manifest);
    await File.WriteAllTextAsync(Path.Combine(project, "vite.config.js"), """
        import vue from '@vitejs/plugin-vue';
        export default { plugins: [vue()] };
        """);
    var lockfile = Path.Combine(project, "pnpm-lock.yaml");
    var start = new ProcessStartInfo(Path.GetFullPath(appHost))
    {
        WorkingDirectory = project,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };
    start.Environment["JazorPnpmStoreDirectory"] =
        Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(project, ".npm-store");
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    foreach (var argument in new[] { "dev", ".", "--port", port.ToString() })
        start.ArgumentList.Add(argument);
    using var server = Process.Start(start) ?? throw new InvalidOperationException("Could not start Jolt.");
    var events = Channel.CreateUnbounded<string>();
    var compilations = 0;
    var expectedDiagnostics = 0;
    server.OutputDataReceived += (_, message) =>
    {
        if (message.Data is not null)
        {
            if (IsCompile(message.Data))
                Interlocked.Increment(ref compilations);
            Console.WriteLine(message.Data);
            events.Writer.TryWrite(message.Data);
        }
        else
            events.Writer.TryComplete();
    };
    server.ErrorDataReceived += (_, message) =>
    {
        if (message.Data is not null)
        {
            if (IsPropError(message.Data) || IsDtoError(message.Data))
                Interlocked.Increment(ref expectedDiagnostics);
            Console.Error.WriteLine(message.Data);
            events.Writer.TryWrite(message.Data);
        }
    };
    server.BeginOutputReadLine();
    server.BeginErrorReadLine();
    using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(2));
    Process? browser = null;
    var liveContract = args.Length == 2 ? contractSource.Replace("\"before\"", "\"after\"", StringComparison.Ordinal) : contractSource;
    var recoveredVue = vueSource.Replace("{{ title }}", "Recovered {{ title }}", StringComparison.Ordinal);
    try
    {
        await WaitFor(IsCompile);
        var origin = new Uri(await WaitFor(line => Uri.TryCreate(line, UriKind.Absolute, out var uri) && uri.Scheme == "http"));
        await CheckVersion("1.53.2");
        await CheckIdle();

        if (args.Length == 2)
        {
            await File.WriteAllTextAsync(contract + ".next", liveContract);
            var browserStart = new ProcessStartInfo("dotnet") { UseShellExecute = false };
            foreach (var argument in new[] { "run", "--file", Path.Combine(sample, "verify-browser.cs"), "--",
                Path.GetFullPath(args[1]), origin.AbsoluteUri, contract, "--correlated" })
                browserStart.ArgumentList.Add(argument);
            browser = Process.Start(browserStart)!;
            await WaitFor(IsCompile);
            while (!File.Exists(contract + ".browser-ready") && !browser.HasExited)
                await Task.Delay(50, deadline.Token);
            if (browser.HasExited)
                throw new InvalidOperationException("The correlated browser update failed.");
            await CheckIdle();
        }

        // Change the manifest while the CLI owns Vite. pnpm must derive a new lockfile.
        await ReplaceAsync(manifestPath, manifest.Replace("1.53.2", "1.53.1", StringComparison.Ordinal));
        await WaitFor(IsCompile);
        await CheckVersion("1.53.1");
        await CheckIdle();

        // A shared DTO error must bind to the actual source and preserve the prior module.
        var generated = Path.Combine(project, ".jolt", "App.mjs");
        var lastGood = await File.ReadAllTextAsync(generated);
        await ReplaceAsync(contract, liveContract.Replace("Row(string Label)", "Row(MissingType Label)", StringComparison.Ordinal));
        await WaitFor(IsDtoError);
        await CheckIdle();
        if (await File.ReadAllTextAsync(generated) != lastGood)
            throw new InvalidOperationException("A failed DTO compilation replaced the last good module.");
        await ReplaceAsync(contract, liveContract.Replace("Row(string Label);",
            "Row(string Label) { public string Detail => Label; }", StringComparison.Ordinal));
        await WaitFor(IsCompile);
        await CheckIdle();

        // Editing the SFC's own public surface also invalidates the consuming template.
        await ReplaceAsync(vuePath, vueSource.Replace("CardProps", "OtherProps", StringComparison.Ordinal)
            .Replace("import type { OtherProps } from './card-props'", "interface OtherProps { title: number }", StringComparison.Ordinal));
        await WaitFor(IsPropError);
        await CheckIdle();
        await ReplaceAsync(vuePath, recoveredVue);
        await WaitFor(IsCompile);
        await CheckIdle();

        // Edit the transitive prop declaration without writing either component.
        await ReplaceAsync(cardTypes, cardProps.Replace("string", "number", StringComparison.Ordinal));
        await WaitFor(IsPropError);
        await ReplaceAsync(cardTypes, cardProps);
        await WaitFor(IsCompile);
        await CheckIdle();

        // A lockfile-only change also invalidates the project.
        await ReplaceAsync(lockfile, await File.ReadAllTextAsync(lockfile) + "\n# Updated dependency lock\n");
        await WaitFor(IsCompile);
        await CheckVersion("1.53.1");
        await CheckIdle();

        foreach (var output in new[] { ".jolt", ".jazor", "bin", "obj", "dist" })
        {
            var directory = Directory.CreateDirectory(Path.Combine(project, output));
            await File.WriteAllTextAsync(Path.Combine(directory.FullName, "WatchProbe.g.cs"), "generated");
        }
        await CheckIdle();

        // This package's declaration paths are outside the frontend recursive watcher.
        await ReplaceAsync(firstDeclaration, signature.Replace("value: string", "value: number", StringComparison.Ordinal));
        await WaitFor(line => IsPropError(line) && line.Contains("double", StringComparison.Ordinal));
        await CheckIdle();
        await ReplaceAsync(secondDeclaration, signature);
        await ReplaceAsync(Path.Combine(linked, "index.d.ts"), "export { format } from './second.js';");
        await WaitFor(IsCompile);
        await CheckIdle();
        await ReplaceAsync(firstDeclaration, signature);
        await CheckIdle();
        await ReplaceAsync(secondDeclaration, signature.Replace("value: string", "value: number", StringComparison.Ordinal));
        await WaitFor(line => IsPropError(line) && line.Contains("double", StringComparison.Ordinal));
        await CheckIdle();
        await ReplaceAsync(secondDeclaration, signature);
        await WaitFor(IsCompile);
        await CheckIdle();

        var externalVue = recoveredVue.Replace("'./card-props'", "'linked-vue-props'", StringComparison.Ordinal);
        await ReplaceAsync(vuePath, externalVue);
        await WaitFor(IsCompile);
        await CheckIdle();
        lastGood = await File.ReadAllTextAsync(generated);
        // Only the link target behind the package's type re-export changes here.
        await ReplaceAsync(externalCardTypes, cardProps.Replace("string", "number", StringComparison.Ordinal));
        using (var propDeadline = new CancellationTokenSource(TimeSpan.FromSeconds(12)))
            await WaitFor(line => IsPropError(line) && line.Contains("double", StringComparison.Ordinal), propDeadline.Token);
        await CheckIdle();
        if (await File.ReadAllTextAsync(generated) != lastGood)
            throw new InvalidOperationException("An invalid linked SFC prop declaration replaced the last good module.");
        await ReplaceAsync(externalCardTypes, cardProps);
        await WaitFor(IsCompile);
        await CheckIdle();

        if (browser is not null)
        {
            await File.WriteAllTextAsync(contract + ".browser-recover", "", deadline.Token);
            await browser.WaitForExitAsync(deadline.Token);
            if (browser.ExitCode != 0)
                throw new InvalidOperationException("The browser did not recover after the SFC and linked package edits.");
        }

        using var client = new HttpClient();
        using var response = await client.GetAsync(new Uri(origin, ".jolt/App.mjs"), deadline.Token);
        response.EnsureSuccessStatusCode();
        using var vueResponse = await client.GetAsync(new Uri(origin, "LocalCard.vue"), deadline.Token);
        vueResponse.EnsureSuccessStatusCode();
        if (await File.ReadAllTextAsync(app) != appSource || await File.ReadAllTextAsync(vuePath) != externalVue)
            throw new InvalidOperationException("The dependency check modified a component source.");
        var expectedCompilations = args.Length == 2 ? 11 : 10;
        if (Volatile.Read(ref compilations) != expectedCompilations || Volatile.Read(ref expectedDiagnostics) != 6)
            throw new InvalidOperationException($"Expected {expectedCompilations} successful compilations and six primary diagnostics. Observed {compilations} and {expectedDiagnostics}.");
        Console.WriteLine($"Verified {compilations} successful compilations and six rejected edits in one Jolt dev process without extra successful compilations.");
    }
    finally
    {
        if (browser is not null)
        {
            if (!browser.HasExited)
                browser.Kill(entireProcessTree: true);
            await browser.WaitForExitAsync();
            browser.Dispose();
        }
        if (!server.HasExited)
            server.Kill(entireProcessTree: true);
        await server.WaitForExitAsync();
    }

    async Task<string> WaitFor(Func<string, bool> predicate, CancellationToken cancellationToken = default)
    {
        using var wait = CancellationTokenSource.CreateLinkedTokenSource(deadline.Token, cancellationToken);
        await foreach (var line in events.Reader.ReadAllAsync(wait.Token))
            if (predicate(line))
                return line;
        throw new InvalidOperationException("Jolt exited before reporting the expected dev event.");
    }

    async Task CheckVersion(string expected)
    {
        var package = Path.Combine(project, "node_modules", "es-toolkit");
        using var metadata = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(package, "package.json")));
        if (metadata.RootElement.GetProperty("version").GetString() != expected ||
            !File.Exists(Path.Combine(package, "string.d.ts")) ||
            !(await File.ReadAllTextAsync(Path.Combine(project, ".jolt", "App.mjs"))).Contains("es-toolkit/string", StringComparison.Ordinal))
            throw new InvalidOperationException($"Expected the installed es-toolkit {expected} declarations and an emitted npm function import.");
    }

    async Task CheckIdle()
    {
        await Task.Delay(750, deadline.Token);
        while (events.Reader.TryRead(out var line))
            if (IsCompile(line))
                throw new InvalidOperationException("A dependency restore caused another Jolt compilation.");
    }
}
finally
{
    scratch.Delete(recursive: true);
}

static bool IsCompile(string line) => line.StartsWith("Compiled ", StringComparison.Ordinal);

static bool IsPropError(string line) => line.Contains("App.jazor", StringComparison.Ordinal) &&
    line.Contains("error CS1503", StringComparison.Ordinal) && !line.Contains("MissingType", StringComparison.Ordinal);

static bool IsDtoError(string line) => line.Contains("Contract.cs", StringComparison.Ordinal) &&
    line.Contains("error CS0246", StringComparison.Ordinal);

static async Task ReplaceAsync(string path, string text)
{
    var next = path + ".next";
    await File.WriteAllTextAsync(next, text);
    File.Move(next, path, overwrite: true);
}
