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
    await File.WriteAllTextAsync(Path.Combine(projectDirectory, "entry.js"), "import { ref } from 'vue'; globalThis.probe = ref(42).value;");
}
await NodeToolchain.RestoreAsync(projectDirectory, Path.Combine(projectDirectory, ".store"), CancellationToken.None, Console.Out);
using var environment = new NodeEnvironment(projectDirectory);
var result = await environment.InvokeAsync(NodeToolchain.MaterializeBridge(projectDirectory), "verifyVite", projectDirectory);
using var document = JsonDocument.Parse(result);
if (document.RootElement.GetProperty("pid").GetInt32() != Environment.ProcessId)
    throw new InvalidOperationException("Vite did not run inside the C# host process.");
Console.WriteLine(result);
