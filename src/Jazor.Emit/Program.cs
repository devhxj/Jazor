using Jazor.Emit;

if (args.Length > 0 && string.Equals(args[0], "toolchain", StringComparison.OrdinalIgnoreCase))
{
    return await RunToolchainAsync(args[1..]);
}

if (args.Length > 1 && string.Equals(args[0], "manifest", StringComparison.OrdinalIgnoreCase) && string.Equals(args[1], "materialize", StringComparison.OrdinalIgnoreCase))
{
    return await RunManifestMaterializeAsync(args[2..]);
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
        var writeResult = ModuleWriter.Write(
            options.RootAssemblyPath,
            options.OutputDirectory,
            options.ManifestPath,
            collectResult.Modules,
            options.Clean,
            collectResult.Assets,
            collectResult.ImportMapEntries);

        if (!writeResult.IsSuccess)
        {
            Console.Error.WriteLine(writeResult.Error);
            return writeResult.ExitCode;
        }

        Console.WriteLine(
            $"assemblies={collectResult.AssemblyCount} catalogs={collectResult.CatalogCount} modules={collectResult.Modules.Count} assets={collectResult.Assets.Count} written={writeResult.Written} skipped={writeResult.Skipped} deleted={writeResult.Deleted}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 5;
    }
}

static async Task<int> RunToolchainAsync(string[] args)
{
    if (!ToolchainCommand.TryParse(args, out var command, out var error) || command is null)
    {
        Console.Error.WriteLine(error);
        return 1;
    }

    try
    {
        var runner = new Toolchain();
        var result = await runner.BuildAsync(command.Request);
        if (!result.IsSuccess)
        {
            Console.Error.WriteLine($"{result.Diagnostic?.Code}: {result.Diagnostic?.Message}");
            return result.ExitCode;
        }

        Console.WriteLine($"mode={command.Mode} modules={result.ModuleCount} out={result.OutputPath}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 5;
    }
}

static async Task<int> RunManifestMaterializeAsync(string[] args)
{
    var outputRoot = string.Empty;
    var manifestPath = string.Empty;
    var mode = BuildMode.Development;
    var manifests = new List<string>();
    for (var index = 0; index < args.Length; index++)
    {
        if (index + 1 >= args.Length)
        {
            Console.Error.WriteLine($"Missing value for argument '{args[index]}'.");
            return 1;
        }

        var value = args[++index];
        switch (args[index - 1])
        {
            case "--out-root":
                outputRoot = value;
                break;
            case "--manifest":
                manifestPath = value;
                break;
            case "--mode" when Enum.TryParse<BuildMode>(value, ignoreCase: true, out var parsedMode):
                mode = parsedMode;
                break;
            case "--library-manifest":
                manifests.Add(value);
                break;
            default:
                Console.Error.WriteLine($"Unknown manifest materialize argument '{args[index - 1]}'.");
                return 1;
        }
    }

    if (string.IsNullOrWhiteSpace(outputRoot))
    {
        Console.Error.WriteLine("Missing required argument --out-root.");
        return 1;
    }

    try
    {
        outputRoot = Path.GetFullPath(outputRoot);
        Directory.CreateDirectory(outputRoot);
        var manifest = string.IsNullOrWhiteSpace(manifestPath)
            ? null
            : ManifestModel.TryLoad(Path.GetFullPath(manifestPath))
                ?? throw new FileNotFoundException("Manifest was not found.", manifestPath);
        var requiredImports = manifest?.Modules
            .SelectMany(static module => module.PackageImports ?? [])
            .ToArray() ?? [];
        var providedModulePaths = manifest?.Modules
            .Select(static module => module.RelativePath)
            .ToArray() ?? [];
        var materialization = new LibraryMaterializer().Materialize(
            manifests,
            outputRoot,
            mode,
            requiredImports,
            providedModulePaths);
        await ImportMapWriter.WriteAsync(
            outputRoot,
            materialization,
            manifest?.ImportMapEntries);
        Console.WriteLine($"manifests={materialization.ManifestPaths.Count} imports={materialization.ImportPaths.Count} out={outputRoot}");
        return 0;
    }
    catch (LibraryException ex)
    {
        Console.Error.WriteLine($"{ex.Code}: {ex.Message}");
        return 5;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.Message);
        return 5;
    }
}
