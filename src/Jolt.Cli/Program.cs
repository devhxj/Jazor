using Jolt.Compiler;
using Jolt.LanguageServer;
using Jolt.Project;
using Jolt.Vite;
using Microsoft.CodeAnalysis;

using var stop = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) => { eventArgs.Cancel = true; stop.Cancel(); };
const string usage = "Usage: jolt build [project] [--output directory] | dev [project] [--port port] | bundle [project] [--ssr server-entry] | library [project] [--output directory] | lsp\n       " + NpmBindingsCommand.Usage;
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
    if (args[0] == "bindings")
        return await NpmBindingsCommand.RunAsync(args[1..], stop.Token);
    var index = 1;
    var project = Path.GetFullPath(index < args.Length && !args[index].StartsWith('-') ? args[index++] : ".");
    var output = Path.Combine(project, args[0] == "library" ? "dist" : ".jolt");
    var port = JoltViteToolchain.DefaultPort;
    string? serverEntry = null;
    if (args[0] is "build" or "library" && args.Length == index + 2 && args[index] == "--output")
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
    if (args[0] == "bundle" && args.Length == index + 2 && args[index] == "--ssr" &&
        !string.IsNullOrWhiteSpace(args[index + 1]) && !args[index + 1].StartsWith('-'))
    {
        serverEntry = args[index + 1];
        index += 2;
    }
    if (index != args.Length)
        throw new ArgumentException(usage);
    switch (args[0])
    {
        case "build":
            return await Compile(stop.Token) ? 0 : 1;
        case "library":
            return await BuildLibrary(stop.Token) ? 0 : 1;
        case "bundle":
            if (serverEntry is null)
                return await JoltViteToolchain.BuildAsync(project, Compile, cancellationToken: stop.Token) ? 0 : 1;
            if (!await JoltViteToolchain.BuildSsrAsync(project, serverEntry, Compile, cancellationToken: stop.Token))
                return 1;
            Console.WriteLine($"Bundled SSR client and server to {Path.Combine(project, "dist")}");
            return 0;
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
        var result = await CompileModules(workspace, output, cancellationToken);
        if (result is null)
            return false;
        await JoltOutput.WriteAsync(project, output, result.Modules, cancellationToken, result.RouterModule);
        Console.WriteLine($"Compiled {result.Modules.Count} component(s) to {output}");
        return true;
    }

    async Task<bool> BuildLibrary(CancellationToken cancellationToken)
    {
        var manifest = await JoltLibrary.ReadManifestAsync(project, cancellationToken);
        await JoltViteToolchain.RestoreAsync(project, cancellationToken: cancellationToken);
        var workspace = await JoltProjectLoader.LoadAsync(project, cancellationToken: cancellationToken);
        var temporary = Path.Combine(project, ".jazor", "library-" + Guid.NewGuid().ToString("N"));
        var generated = Path.Combine(temporary, "generated");
        try
        {
            var result = await CompileModules(workspace, generated, cancellationToken);
            if (result is null)
                return false;
            var modules = result.Modules;
            var bundled = modules.Any(module => module.LocalVueImports.Count != 0)
                ? Path.Combine(temporary, "bundled")
                : null;
            if (bundled is not null)
            {
                await JoltOutput.WriteAsync(project, generated, modules, cancellationToken);
                await JoltViteToolchain.BuildLibraryAsync(project, generated, bundled,
                    modules.Select(module => module.Path).ToArray(), cancellationToken);
            }
            await JoltLibrary.WriteAsync(project, output, manifest, modules, cancellationToken, bundled);
            Console.WriteLine($"Packaged {modules.Count} component(s) to {output}");
            return true;
        }
        finally
        {
            if (Directory.Exists(temporary))
                Directory.Delete(temporary, recursive: true);
        }
    }

    async Task<JoltCompileResult?> CompileModules(JoltWorkspace workspace, string outputDirectory, CancellationToken cancellationToken)
    {
        if (HasErrors(workspace.Diagnostics))
            return null;
        var result = await JoltProjectCompiler.CompileAsync(workspace.Sources,
            workspace.Options with { VueSourceRootRelative = Path.GetRelativePath(outputDirectory, project) }, cancellationToken);
        return HasErrors(result.Diagnostics) ? null : result;
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
