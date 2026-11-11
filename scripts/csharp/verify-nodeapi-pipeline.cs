#!/usr/bin/env dotnet run
#:sdk Microsoft.NET.Sdk.Web
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.Emit/Jazor.Emit.csproj
#:project ../../src/Jazor.AspNetCore/Jazor.AspNetCore.csproj

using Jazor.AspNetCore;
using Jazor.Emit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Jazor.slnx"))) directory = directory.Parent;
var repoRoot = directory?.FullName ?? throw new DirectoryNotFoundException("Repository root was not found.");
var root = Path.GetFullPath(args.Length == 0 ? Path.Combine(repoRoot, ".tmp", "nodeapi-pipeline") : args[0]);
Directory.CreateDirectory(Path.Combine(root, "components"));
var libraries = new LibraryMaterializer().Materialize(
    [Path.Combine(repoRoot, "src", "ECMAScript.Vue", "manifest.json")], root, BuildMode.Production,
    ["vue", "@vue/server-renderer"]);
await File.WriteAllTextAsync(Path.Combine(root, "components", "counter.js"), """
    import { defineComponent, h, onServerPrefetch, ref } from 'vue';
    export default defineComponent({
      props: ['Title'],
      setup(props) {
        const phase = ref('before');
        onServerPrefetch(async () => { phase.value = 'prefetched'; });
        return () => h('main', { id: 'nodeapi-ssr' }, props.Title + '|' + phase.value);
      }
    });
    """);
var entries = ProjectEntryWriter.Write(root, ["components/counter.js"], enableSsr: true);
LibraryPackageWriter.WritePackageProject(root, libraries, hasSsrEntry: true);
await NodePackageRestorer.RestoreAndCheckAsync(root, entries, CancellationToken.None, Console.Out);
await RenderAsync(root, Environments.Development);
foreach (var task in new[] { "build", "build:ssr" })
{
    var built = await new JavaScriptProjectBuilder().BuildAsync(root, cancellationToken: CancellationToken.None, taskName: task, progress: Console.Out);
    if (!built.IsSuccess) throw new InvalidOperationException(built.Diagnostic?.Message);
}
ProjectEntryWriter.WriteSsrRuntimePackage(root);
var deployed = Path.Combine(repoRoot, "artifacts", "nodeapi-pipeline", Guid.NewGuid().ToString("N"));
foreach (var file in Directory.GetFiles(Path.Combine(root, "ssr"), "*", SearchOption.AllDirectories))
{
    var destination = Path.Combine(deployed, "ssr", Path.GetRelativePath(Path.Combine(root, "ssr"), file));
    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
    File.Copy(file, destination);
}
await RenderAsync(deployed, Environments.Production);
Console.WriteLine("NodeApi restore, source SSR, browser build, SSR build and runtime-only deployed SSR passed.");

static async Task RenderAsync(string root, string environment)
{
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = root, EnvironmentName = environment });
    builder.Services.AddJazorSsr(options => { options.ArtifactRootPath = root; options.WorkerCount = 1; });
    await using var host = builder.Build();
    var renderer = host.Services.GetRequiredService<IJazorSsrRenderer>();
    var result = await renderer.RenderAsync(new JazorSsrRequest("components/counter.js", new { Title = "NodeApi <same process>" }));
    if (!result.Html.Contains("NodeApi &lt;same process&gt;|prefetched", StringComparison.Ordinal))
        throw new InvalidOperationException("SSR content mismatch: " + result.Html);
    Console.WriteLine(environment + " SSR passed (.NET PID " + Environment.ProcessId + ").");
}
