using Jolt.Compiler;
using Jolt.LanguageServer;
using Jolt.Project;
using Jolt.Vite;
using Microsoft.CodeAnalysis;

using var stop = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) => { eventArgs.Cancel = true; stop.Cancel(); };
const string usage = "Usage: jolt build [project] [--output directory] | dev [project] [--port port] [--stop-on-stdin-close] | bundle [project] [--ssr server-entry] | library [project] [--output directory] | lsp";
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
    var generatedRoot = Environment.GetEnvironmentVariable("JAZOR_GENERATED_ROOT") is { Length: > 0 } selectedRoot
        ? Path.GetFullPath(selectedRoot, project)
        : await JoltProjectLoader.GetGeneratedRootAsync(project, stop.Token);
    var output = args[0] == "library" ? Path.Combine(project, "dist") : generatedRoot;
    var port = JoltViteToolchain.DefaultPort;
    var stopOnStdinClose = false;
    using var externalDeclarations = args[0] == "dev" ? new JoltExternalDeclarations(project) : null;
    var devMetadata = externalDeclarations is null ? null : JoltProjectLoader.CreateDevMetadataProvider(externalDeclarations.Observe);
    var devCompilation = devMetadata is null ? null : new JoltDevCompilation();
    IReadOnlyList<JoltSource>? previousDevSources = null;
    using var moduleRefresh = args[0] == "dev" && Environment.GetEnvironmentVariable("JOLT_DEVELOPMENT_CONFIGURATION") is { } configuration
        ? new JoltDevelopmentModules(project, generatedRoot, configuration) : null;
    string? serverEntry = null;
    if (args[0] is "build" or "library" && args.Length == index + 2 && args[index] == "--output")
    {
        output = Path.GetFullPath(args[index + 1]);
        index += 2;
    }
    if (args[0] == "dev")
    {
        var portSpecified = false;
        while (index < args.Length)
        {
            if (args[index] == "--stop-on-stdin-close" && !stopOnStdinClose)
            {
                stopOnStdinClose = true;
                index++;
            }
            else if (!portSpecified && args[index] == "--port" && index + 1 < args.Length &&
                int.TryParse(args[index + 1], out var parsedPort) && parsedPort is >= 0 and <= 65535)
            {
                port = parsedPort;
                portSpecified = true;
                index += 2;
            }
            else
                throw new ArgumentException(usage);
        }
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
                return await JoltViteToolchain.BuildAsync(project, generatedRoot, Compile, cancellationToken: stop.Token) ? 0 : 1;
            if (!await JoltViteToolchain.BuildSsrAsync(project, generatedRoot, serverEntry, Compile, cancellationToken: stop.Token))
                return 1;
            Console.WriteLine($"Bundled SSR client and server to {Path.Combine(generatedRoot, "dist")}");
            return 0;
        case "dev":
            if (stopOnStdinClose)
                JoltDevInput.StopOnClose(stop);
            await using (var server = await JoltViteToolchain.ServeAsync(project, generatedRoot, Compile, notify =>
            {
                externalDeclarations!.Changed = notify;
                if (moduleRefresh is not null)
                    moduleRefresh.Changed = notify;
            }, port: port,
                cancellationToken: stop.Token, reportError: ex => Console.Error.WriteLine(ex.Message),
                basePath: Environment.GetEnvironmentVariable("JOLT_VITE_BASE")))
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
        externalDeclarations?.Begin();
        JoltWorkspace workspace;
        var loaded = false;
        try
        {
            workspace = devMetadata is null
                ? await JoltProjectLoader.LoadAsync(project, cancellationToken: cancellationToken)
                : await JoltProjectLoader.LoadAsync(project, devMetadata, cancellationToken: cancellationToken, previousSources: previousDevSources);
            if (devMetadata is not null)
                previousDevSources = workspace.Sources;
            loaded = !workspace.Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        }
        finally
        {
            externalDeclarations?.Complete(loaded);
        }
        var result = await CompileModules(workspace, output, cancellationToken);
        if (result is null)
            return false;
        if (moduleRefresh is not null && !await moduleRefresh.UpdateAsync(cancellationToken))
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
        var temporary = Path.Combine(generatedRoot, "library-" + Guid.NewGuid().ToString("N"));
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
        var options = workspace.Options with { VueSourceRootRelative = Path.GetRelativePath(outputDirectory, project) };
        var result = devCompilation is null
            ? await JoltProjectCompiler.CompileAsync(workspace.Sources, options, cancellationToken)
            : await devCompilation.CompileAsync(workspace.Sources, options, cancellationToken);
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
