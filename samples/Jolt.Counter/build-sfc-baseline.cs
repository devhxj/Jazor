#!/usr/bin/env dotnet run
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.Net;
using Jazor.NodeHost;

if (args is ["--serve", var dist, var port])
{
    // Vite's production output uses absolute asset URLs. Serve each build at its own origin.
    var distRoot = Path.GetFullPath(dist);
    var files = Directory.EnumerateFiles(distRoot, "*", SearchOption.AllDirectories)
        .ToDictionary(path => "/" + Path.GetRelativePath(distRoot, path).Replace(Path.DirectorySeparatorChar, '/'));
    files.Add("/", files["/index.html"]);
    using var listener = new HttpListener();
    listener.Prefixes.Add($"http://127.0.0.1:{port}/");
    listener.Start();
    Console.WriteLine(listener.Prefixes.Single());
    while (true)
    {
        var request = await listener.GetContextAsync();
        using var response = request.Response;
        if (!files.TryGetValue(request.Request.Url!.AbsolutePath, out var path))
        {
            response.StatusCode = 404;
            continue;
        }
        response.ContentType = Path.GetExtension(path) switch
        {
            ".html" => "text/html",
            ".js" => "text/javascript",
            ".css" => "text/css",
            _ => "application/octet-stream",
        };
        await using var stream = File.OpenRead(path);
        response.ContentLength64 = stream.Length;
        await stream.CopyToAsync(response.OutputStream);
    }
}

var jolt = args is ["--prepare-jolt", _];
if (!jolt && args.Length != 1)
    throw new ArgumentException("Usage: build-sfc-baseline.cs [--prepare-jolt] <scratch-directory> | --serve <dist-directory> <port>");

var sample = Path.GetDirectoryName(ScriptPath())!;
var fixtures = Path.Combine(sample, ".fixtures");
var project = Path.GetFullPath(args[^1]);
Directory.CreateDirectory(project);
foreach (var path in new[] { "package.json", "pnpm-lock.yaml", "index.html" })
    File.Copy(Path.Combine(sample, path), Path.Combine(project, path), overwrite: true);
if (jolt)
{
    File.Copy(Path.Combine(sample, "vite.config.js"), Path.Combine(project, "vite.config.js"), overwrite: true);
    File.Copy(Path.Combine(fixtures, "_Imports.jazor"), Path.Combine(project, "_Imports.jazor"), overwrite: true);
}
else
{
    File.Copy(Path.Combine(sample, "vite.config.js"), Path.Combine(project, "vite.shared.config.js"), overwrite: true);
    File.Copy(Path.Combine(fixtures, "sfc-vite.config.js"), Path.Combine(project, "vite.config.js"), overwrite: true);
}
var component = jolt ? "EditableList.jazor" : "EditableList.vue";
File.Copy(Path.Combine(fixtures, component), Path.Combine(project, component), overwrite: true);
await File.WriteAllTextAsync(Path.Combine(project, "main.js"), $$"""
    import { createApp } from 'vue';
    import EditableList from './{{component}}';
    createApp(EditableList).mount('#app');
    """);

if (jolt)
    return;

var repository = Path.GetFullPath(Path.Combine(sample, "..", ".."));
var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(repository, ".tmp", "nodeapi-store");
await NodeToolchain.RestoreAsync(project, store, CancellationToken.None, Console.Out);
await NodeToolchain.BuildAsync(project, ssr: false, CancellationToken.None, Console.Out);

static string ScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;
