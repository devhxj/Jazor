using Jazor.Emit;

return await RunEmitAsync(args);

static async Task<int> RunEmitAsync(string[] args)
{
    if (!EmitOptions.TryParse(args, out var options, out var error) || options is null)
    {
        Console.Error.WriteLine(error);
        return 1;
    }

    var result = await new EmitPipeline().ExecuteAsync(options).ConfigureAwait(false);
    if (!result.IsSuccess)
    {
        Console.Error.WriteLine(result.Error);
        return result.ExitCode;
    }

    Console.WriteLine(
        $"assemblies={result.AssemblyCount} catalogs={result.CatalogCount} modules={result.ModuleCount} assets={result.AssetCount} written={result.Written} skipped={result.Skipped} deleted={result.Deleted} out={result.OutputDirectory}");
    return 0;
}
