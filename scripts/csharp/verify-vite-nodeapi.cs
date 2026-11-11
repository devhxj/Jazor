#!/usr/bin/env dotnet run
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using Jazor.NodeHost;
using System.Text.Json;

var repositoryRoot = Directory.GetCurrentDirectory();
var projectDirectory = args.Length == 0
    ? Path.Combine(repositoryRoot, "artifacts", "nodeapi-vite", Guid.NewGuid().ToString("N"))
    : Path.GetFullPath(args[0]);
if (args.Length == 0)
{
    Directory.CreateDirectory(projectDirectory);
    await File.WriteAllTextAsync(Path.Combine(projectDirectory, "package.json"), """
        {"name":"@jazor/nodeapi-vite-probe","private":true,"type":"module","dependencies":{"vite":"8.3.0","vue":"3.5.42"}}
        """);
    await File.WriteAllTextAsync(Path.Combine(projectDirectory, "entry.js"), "import './probe.css'; import { ref } from 'vue'; globalThis.probe = ref(42).value;");
    await File.WriteAllTextAsync(Path.Combine(projectDirectory, "probe.css"), ".probe { color: rgb(255, 0, 0); padding: 0px 0px 0px 0px; }");
}
await NodeToolchain.RestoreAsync(projectDirectory, Path.Combine(projectDirectory, ".store"), CancellationToken.None, Console.Out);
using var environment = new NodeEnvironment(projectDirectory);
var result = await environment.InvokeAsync(NodeToolchain.MaterializeBridge(projectDirectory), "verifyVite", projectDirectory);
using var document = JsonDocument.Parse(result);
if (document.RootElement.GetProperty("pid").GetInt32() != Environment.ProcessId)
    throw new InvalidOperationException("Vite did not run inside the C# host process.");
Console.WriteLine(result);
if (args.Length == 0)
{
    var css = Directory.GetFiles(Path.Combine(projectDirectory, "dist"), "*.css", SearchOption.AllDirectories);
    if (css.Length != 1 || !File.ReadAllText(css[0]).Contains("padding:0", StringComparison.Ordinal))
        throw new InvalidOperationException("Vite did not produce the minified CSS artifact.");
}
