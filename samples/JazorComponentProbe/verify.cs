#!/usr/bin/env dotnet run
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Jazor.NodeHost;

var sample = SampleDirectory();
var fixture = Path.Combine(sample, "Npm", "current");
Directory.CreateDirectory(Path.Combine(fixture, ".jazor"));
var scratch = Directory.CreateTempSubdirectory("jazor-component-");
var output = Path.Combine(fixture, ".jazor", $"verify-{Guid.NewGuid():N}");
Directory.CreateDirectory(output);
try
{
    var authorPath = Path.Combine(sample, "Counter.jazor");
    var module = Path.Combine(output, "Counter.mjs");
    var valid = await CompileAsync(authorPath, module);
    if (valid.Status != 0)
        throw new InvalidOperationException(valid.Error);

    var runner = Path.Combine(output, "render.mjs");
    await File.WriteAllTextAsync(runner, """
        import { createSSRApp } from 'vue';
        import { renderToString } from 'vue/server-renderer';
        import Counter from './Counter.mjs';
        console.log(await renderToString(createSSRApp(Counter)));
        """);
    var invalid = Path.Combine(scratch.FullName, "Invalid.jazor");
    var original = await File.ReadAllTextAsync(authorPath);
    await File.WriteAllTextAsync(invalid, original.Replace("Count > 0", "Count", StringComparison.Ordinal));
    var rejectedModule = Path.Combine(output, "Invalid.mjs");
    var rejected = await CompileAsync(invalid, rejectedModule);
    if (rejected.Status != 1 || !rejected.Error.Contains("Invalid.jazor(3,26): CS0029", StringComparison.Ordinal) || File.Exists(rejectedModule))
        throw new InvalidOperationException($"Expected a mapped integer-to-bool diagnostic. {rejected.Error}");
    var html = await NodeModuleRunner.RunAsync(fixture, runner);
    if (!html.Contains("data-state=\"open\"", StringComparison.Ordinal))
        throw new InvalidOperationException($"Expected Reka UI to render its open state, received {html}");
    Console.WriteLine(html);
    Console.WriteLine("Verified direct npm component rendering and Invalid.jazor(3,26): CS0029.");
}
finally
{
    Directory.Delete(output, recursive: true);
    scratch.Delete(recursive: true);
}

async Task<(int Status, string Error)> CompileAsync(string input, string outputFile)
{
    var start = new ProcessStartInfo("dotnet") { RedirectStandardError = true };
    foreach (var argument in new[] { "run", "--project", sample, "--", input, fixture, outputFile })
        start.ArgumentList.Add(argument);
    using var child = Process.Start(start)!;
    var error = await child.StandardError.ReadToEndAsync();
    await child.WaitForExitAsync();
    return (child.ExitCode, error);
}

static string SampleDirectory([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
