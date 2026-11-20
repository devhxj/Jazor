#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;

if (args is not [var appHost])
    throw new ArgumentException("Usage: verify-jolt-dev-dependencies.cs <Jolt CLI apphost>");
var repository = new DirectoryInfo(Directory.GetCurrentDirectory());
while (repository is not null && !File.Exists(Path.Combine(repository.FullName, "Jazor.slnx")))
    repository = repository.Parent;
var sample = Path.Combine(repository?.FullName ?? throw new InvalidOperationException("Cannot locate Jazor.slnx."), "samples", "Jolt.Counter");
var scratch = Directory.CreateDirectory(Path.Combine(repository!.FullName, ".tmp", $"jolt-dev-dependencies-{Guid.NewGuid():N}"));
try
{
    var project = scratch.FullName;
    foreach (var name in new[] { "CounterButton.jazor", "package.json", "pnpm-lock.yaml", "index.html", "main.js" })
        File.Copy(Path.Combine(sample, name), Path.Combine(project, name));
    var app = Path.Combine(project, "App.jazor");
    const string appSource = """
        @using Text = Npm.EsToolkit.String.Exports
        <p>@(Text.CamelCase("Order Total"))</p>
        <LocalCard Title="Hello" />
        """;
    await File.WriteAllTextAsync(app, appSource);
    var vuePath = Path.Combine(project, "LocalCard.vue");
    const string vueSource = """
        <script setup lang="ts">
        import type { CardProps } from './card-props'
        defineProps<CardProps>()
        </script>
        <template><p>{{ title }}</p></template>
        """;
    await File.WriteAllTextAsync(vuePath, vueSource);
    var cardTypes = Path.Combine(project, "card-props.d.ts");
    const string cardProps = "export interface CardProps { title: string }";
    await File.WriteAllTextAsync(cardTypes, cardProps);
    var manifestPath = Path.Combine(project, "package.json");
    var packageManifest = JsonNode.Parse(await File.ReadAllTextAsync(manifestPath))!;
    packageManifest["devDependencies"]!["@vitejs/plugin-vue"] = "6.0.7";
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
    server.OutputDataReceived += (_, message) =>
    {
        if (message.Data is not null)
        {
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
            Console.Error.WriteLine(message.Data);
            events.Writer.TryWrite(message.Data);
        }
    };
    server.BeginOutputReadLine();
    server.BeginErrorReadLine();
    using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(2));
    try
    {
        await WaitFor(IsCompile);
        var origin = new Uri(await WaitFor(line => Uri.TryCreate(line, UriKind.Absolute, out var uri) && uri.Scheme == "http"));
        await CheckVersion("1.53.2");

        // Change the manifest while the CLI owns Vite. pnpm must derive a new lockfile.
        await ReplaceAsync(manifestPath, manifest.Replace("1.53.2", "1.53.1", StringComparison.Ordinal));
        await WaitFor(IsCompile);
        await CheckVersion("1.53.1");
        await CheckIdle();

        // Edit the transitive prop declaration without writing either component.
        await ReplaceAsync(cardTypes, cardProps.Replace("string", "number", StringComparison.Ordinal));
        await WaitFor(line => line.Contains("App.jazor", StringComparison.Ordinal) && line.Contains("error CS1503", StringComparison.Ordinal));
        await ReplaceAsync(cardTypes, cardProps);
        await WaitFor(IsCompile);
        await CheckIdle();

        // A lockfile-only change also invalidates the project.
        await ReplaceAsync(lockfile, await File.ReadAllTextAsync(lockfile) + "\n# Updated dependency lock\n");
        await WaitFor(IsCompile);
        await CheckVersion("1.53.1");
        await CheckIdle();

        using var client = new HttpClient();
        using var response = await client.GetAsync(new Uri(origin, ".jolt/App.mjs"), deadline.Token);
        response.EnsureSuccessStatusCode();
        using var vueResponse = await client.GetAsync(new Uri(origin, "LocalCard.vue"), deadline.Token);
        vueResponse.EnsureSuccessStatusCode();
        if (await File.ReadAllTextAsync(app) != appSource || await File.ReadAllTextAsync(vuePath) != vueSource)
            throw new InvalidOperationException("The dependency check modified a component source.");
        Console.WriteLine("Verified live npm version, lockfile, and Vue prop declaration invalidation with the same Jolt dev process and source.");
    }
    finally
    {
        if (!server.HasExited)
            server.Kill(entireProcessTree: true);
        await server.WaitForExitAsync();
    }

    async Task<string> WaitFor(Func<string, bool> predicate)
    {
        await foreach (var line in events.Reader.ReadAllAsync(deadline.Token))
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

static async Task ReplaceAsync(string path, string text)
{
    var next = path + ".next";
    await File.WriteAllTextAsync(next, text);
    File.Move(next, path, overwrite: true);
}
