using Jazor.Emit;

if (args.Length > 0 && string.Equals(args[0], "bundle", StringComparison.OrdinalIgnoreCase))
{
    return await RunBundleAsync(args[1..]);
}

if (args.Length > 0 && string.Equals(args[0], "razorvue-diff", StringComparison.OrdinalIgnoreCase))
{
    return RunRazorVueDiff(args[1..]);
}

if (args.Length > 0 && string.Equals(args[0], "razorvue-sfc-bridge", StringComparison.OrdinalIgnoreCase))
{
    return await RunRazorVueSfcBridgeAsync(args[1..]);
}

if (args.Length > 0 && string.Equals(args[0], "razorvue-consumer-entry", StringComparison.OrdinalIgnoreCase))
{
    return await RunRazorVueConsumerEntryAsync(args[1..]);
}

return await RunEmitAsync(args);

static async Task<int> RunEmitAsync(string[] args)
{
    if (!EmitOptions.TryParse(args, out var options, out var error) || options is null)
    {
        Console.Error.WriteLine(error);
        return 1;
    }

    try
    {
        var loadContext = new EmitLoadContext(options.RootAssemblyPath);
        var collector = new ModuleCollector(loadContext);
        collector.AddAssembly(options.RootAssemblyPath);

        foreach (var assemblyPath in options.AssemblyPaths)
            collector.AddAssembly(assemblyPath);

        var collectResult = collector.Collect(options.FailOnPathConflict);
        if (!collectResult.IsSuccess)
        {
            Console.Error.WriteLine(collectResult.Error);
            return collectResult.ExitCode;
        }

        var writer = new ModuleWriter();
        var writeResult = writer.Write(
            options.RootAssemblyPath,
            options.OutputDirectory,
            options.ManifestPath,
            collectResult.Modules,
            options.Clean);

        if (!writeResult.IsSuccess)
        {
            Console.Error.WriteLine(writeResult.Error);
            return writeResult.ExitCode;
        }

        var razorVueWriteResult = WriteResult.Success(0, 0, 0);
        if (collectResult.RazorVueSfcArtifactCount > 0)
        {
            var razorVueWriter = new RazorVueSfcModuleWriter();
            razorVueWriteResult = razorVueWriter.Write(
                options.RootAssemblyPath,
                options.OutputDirectory,
                options.ManifestPath,
                collectResult.RazorVueSfcCatalogs,
                options.Clean);

            if (!razorVueWriteResult.IsSuccess)
            {
                Console.Error.WriteLine(razorVueWriteResult.Error);
                return razorVueWriteResult.ExitCode;
            }
        }
        else if (collectResult.RazorVueArtifactCount > 0 ||
                 (options.Clean && ManifestModel.TryLoad(options.ManifestPath)?.ToRazorVueManifest(ManifestComponentModel.H).Modules.Count > 0))
        {
            var razorVueWriter = new RazorVueModuleWriter();
            razorVueWriteResult = razorVueWriter.Write(
                options.RootAssemblyPath,
                options.OutputDirectory,
                options.ManifestPath,
                collectResult.RazorVueCatalogs,
                options.Clean);

            if (!razorVueWriteResult.IsSuccess)
            {
                Console.Error.WriteLine(razorVueWriteResult.Error);
                return razorVueWriteResult.ExitCode;
            }
        }

        Console.WriteLine(
            $"assemblies={collectResult.AssemblyCount} catalogs={collectResult.CatalogCount} razorvueCatalogs={collectResult.RazorVueCatalogCount} razorvueSfcCatalogs={collectResult.RazorVueSfcCatalogCount} modules={collectResult.Modules.Count} razorvueArtifacts={collectResult.RazorVueArtifactCount} razorvueSfcArtifacts={collectResult.RazorVueSfcArtifactCount} written={writeResult.Written + razorVueWriteResult.Written} skipped={writeResult.Skipped + razorVueWriteResult.Skipped} deleted={writeResult.Deleted + razorVueWriteResult.Deleted}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 5;
    }
}

static async Task<int> RunBundleAsync(string[] args)
{
    if (!BundleOptions.TryParse(args, out var options, out var error) || options is null)
    {
        Console.Error.WriteLine(error);
        return 1;
    }

    try
    {
        var bundler = new ModuleBundler();
        var result = await bundler.BundleAsync(options);
        if (!result.IsSuccess)
        {
            Console.Error.WriteLine(result.Error);
            return result.ExitCode;
        }

        Console.WriteLine($"bundled={result.ModuleCount} out={result.OutputPath}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 5;
    }
}

static int RunRazorVueDiff(string[] args)
{
    if (!RazorVueDiffOptions.TryParse(args, out var options, out var error) || options is null)
    {
        Console.Error.WriteLine(error);
        return 1;
    }

    try
    {
        var previous = RazorVueManifestSerializer.TryLoad(options.PreviousManifestPath);
        if (previous is null)
        {
            Console.Error.WriteLine($"Previous RazorVue manifest was not found: '{options.PreviousManifestPath}'.");
            return 6;
        }

        var current = RazorVueManifestSerializer.TryLoad(options.CurrentManifestPath);
        if (current is null)
        {
            Console.Error.WriteLine($"Current RazorVue manifest was not found: '{options.CurrentManifestPath}'.");
            return 7;
        }

        var diff = RazorVueManifestDiffer.Diff(previous, current);
        var writer = new RazorVueUpdatePlanWriter();
        writer.Write(options.OutputPath, previous, current, diff);

        Console.WriteLine($"action={diff.Action} modules={diff.Modules.Count} out={options.OutputPath}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 8;
    }
}

static async Task<int> RunRazorVueSfcBridgeAsync(string[] args)
{
    if (!RazorVueSfcBridgeOptions.TryParse(args, out var options, out var error) || options is null)
    {
        Console.Error.WriteLine(error);
        return 1;
    }

    try
    {
        var compiler = new RazorVueSfcBridgeCompiler();
        var result = await compiler.CompileAsync(options);
        if (!result.IsSuccess)
        {
            Console.Error.WriteLine(result.Error);
            return result.ExitCode;
        }

        Console.WriteLine($"razorvue-sfc-bridge result={result.ResultPath}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 10;
    }
}

static async Task<int> RunRazorVueConsumerEntryAsync(string[] args)
{
    if (!RazorVueConsumerEntryOptions.TryParse(args, out var options, out var error) || options is null)
    {
        Console.Error.WriteLine(error);
        return 1;
    }

    try
    {
        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(options);
        if (!result.IsSuccess)
        {
            Console.Error.WriteLine(result.Error);
            return result.ExitCode;
        }

        Console.WriteLine($"razorvue-consumer-entry components={result.ComponentCount} result={result.ResultPath ?? string.Empty}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 20;
    }
}
