using Jazor.Emit;

return await RunEmitAsync(args);

static async Task<int> RunEmitAsync(string[] args)
{
    if (!EmitOptions.TryParse(args, out var options, out var error) || options is null)
    {
        Console.Error.WriteLine(error);
        return 1;
    }

    using var cancellation = new CancellationTokenSource();
    ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        cancellation.Cancel();
    };
    Console.CancelKeyPress += cancelHandler;
    EmitPipelineResult result;
    try
    {
        result = await new EmitPipeline(Console.Out).ExecuteAsync(options, cancellation.Token).ConfigureAwait(false);
    }
    finally
    {
        Console.CancelKeyPress -= cancelHandler;
    }
    if (!result.IsSuccess)
    {
        Console.Error.WriteLine(result.Error);
        return result.ExitCode;
    }

    Console.WriteLine(
        $"assemblies={result.AssemblyCount} catalogs={result.CatalogCount} modules={result.ModuleCount} assets={result.AssetCount} written={result.Written} skipped={result.Skipped} deleted={result.Deleted} out={result.OutputDirectory}");
    return 0;
}
