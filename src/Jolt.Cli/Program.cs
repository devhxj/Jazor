using Jolt.Compiler;
using Jolt.LanguageServer;
using Jolt.Project;
using Jolt.Vite;
using Microsoft.CodeAnalysis;

using var stop = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) => { eventArgs.Cancel = true; stop.Cancel(); };
const string usage = "Usage: jolt build [project] [--output directory] | dev [project] [--port port] | bundle [project] | lsp";
try
{
    if (args is ["lsp"])
    {
        await Server.RunStdioAsync(stop.Token);
        return 0;
    }
    if (args is [] or ["--help"] or ["-h"])
    {
        Console.WriteLine(usage);
        return 0;
    }
    var index = 1;
    var project = Path.GetFullPath(index < args.Length && !args[index].StartsWith('-') ? args[index++] : ".");
    var output = Path.Combine(project, ".jolt");
    var port = 5173;
    if (args[0] == "build" && args.Length == index + 2 && args[index] == "--output")
    {
        output = Path.GetFullPath(args[index + 1]);
        index += 2;
    }
    if (args[0] == "dev" && args.Length == index + 2 && args[index] == "--port" &&
        int.TryParse(args[index + 1], out var parsedPort) && parsedPort is >= 0 and <= 65535)
    {
        port = parsedPort;
        index += 2;
    }
    if (index != args.Length)
        throw new ArgumentException(usage);
    switch (args[0])
    {
        case "build":
            return await Compile(stop.Token) ? 0 : 1;
        case "bundle":
            return await JoltViteToolchain.BuildAsync(project, Compile, cancellationToken: stop.Token) ? 0 : 1;
        case "dev":
            await using (var server = await JoltViteToolchain.ServeAsync(project, Compile, port: port,
                cancellationToken: stop.Token, reportError: ex => Console.Error.WriteLine(ex.Message)))
            {
                Console.WriteLine(server.Origin);
                await Task.Delay(Timeout.Infinite, stop.Token);
            }
            return 0;
        default:
            throw new ArgumentException(usage);
    }

    async Task<bool> Compile(CancellationToken cancellationToken)
    {
        var workspace = await JoltProjectLoader.LoadAsync(project, cancellationToken: cancellationToken);
        if (HasErrors(workspace.Diagnostics))
            return false;
        var result = await JoltProjectCompiler.CompileAsync(workspace.Sources, workspace.Options, cancellationToken);
        if (HasErrors(result.Diagnostics))
            return false;
        await JoltOutput.WriteAsync(output, result.Modules, cancellationToken);
        Console.WriteLine($"Compiled {result.Modules.Count} component(s) to {output}");
        return true;
    }
}
catch (OperationCanceledException) when (stop.IsCancellationRequested)
{
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine(error.Message);
    return 1;
}

static bool HasErrors(IReadOnlyList<Diagnostic> diagnostics)
{
    foreach (var diagnostic in diagnostics)
        Console.Error.WriteLine(diagnostic);
    return diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}
