using Jazor.Emit;
using Jazor.Npm;

return await RunEmitAsync(args);

static async Task<int> RunEmitAsync(string[] args)
{
    using var cancellation = new CancellationTokenSource();
    ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        cancellation.Cancel();
    };
    Console.CancelKeyPress += cancelHandler;
    try
    {
        // The SDK reuses this packaged CET-compatible apphost for its pre-compiler step.
        // The request file is private build transport and does not participate in Emit modes.
        if (args is ["--sdk-npm-inputs", var inputs])
            return await NpmBuildProjection.RunAsync(inputs, Console.Error, cancellation.Token);
        if (args is ["--sdk-npm-completion", var indexOutput])
            return await NpmValueMetadataProvider.WriteEditorExportsAsync(
                await ReadNpmRequestAsync(cancellation.Token), indexOutput, Console.Error, cancellation.Token);
        if (args is ["--sdk-npm-generate", var sourceOutput])
            return await NpmBuildProjection.RunGeneratorAsync(
                await ReadNpmRequestAsync(cancellation.Token), sourceOutput, cancellation.Token);
        if (args is ["--sdk-npm-editor-inputs", var projectDirectory, var editorInputs])
        {
            await NpmLocalDeclarationInputs.WriteAsync(projectDirectory, editorInputs, cancellation.Token);
            return 0;
        }

        if (!EmitOptions.TryParse(args, out var options, out var error) || options is null)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
        var result = await new EmitPipeline(Console.Out).ExecuteAsync(options, cancellation.Token).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            Console.Error.WriteLine(result.Error);
            return result.ExitCode;
        }

        Console.WriteLine(
            $"assemblies={result.AssemblyCount} catalogs={result.CatalogCount} modules={result.ModuleCount} assets={result.AssetCount} written={result.Written} skipped={result.Skipped} deleted={result.Deleted} out={result.OutputDirectory}");
        return 0;
    }
    finally
    {
        Console.CancelKeyPress -= cancelHandler;
    }
}

static async Task<string> ReadNpmRequestAsync(CancellationToken cancellationToken)
{
    using var input = new StreamReader(Console.OpenStandardInput());
    return await input.ReadToEndAsync(cancellationToken);
}
