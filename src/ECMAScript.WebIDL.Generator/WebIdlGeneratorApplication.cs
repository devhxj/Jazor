using System.Text.Json;

namespace ECMAScript.WebIDL.Generator;

internal static class WebIdlGeneratorApplication
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
        => RunAsync(GeneratorOptions.Parse(args, RepositoryLayout.Discover(AppContext.BaseDirectory)), cancellationToken);

    public static async Task<int> RunAsync(GeneratorOptions options, CancellationToken cancellationToken)
    {
        if (options.TestCollector)
        {
            Console.WriteLine(await new NodeWebIdlCollector(options).TestAsync(cancellationToken));
            return 0;
        }
        var inventory = options.InputInventoryPath is { } inputInventoryPath
            ? await ReadInventoryAsync(inputInventoryPath, cancellationToken)
            : await CollectAsync(options, cancellationToken);

        if (options.InputInventoryPath is null)
        {
            var generator = new InventoryArtifactGenerator(options, JsonOptions);
            await generator.WriteAsync(inventory, cancellationToken);
        }

        var previewEmitter = new PreviewBindingEmitter(options);
        await previewEmitter.EmitAsync(inventory, cancellationToken);

        Console.WriteLine($"Generated bindings for {inventory.Stats.FileCount} WebIDL files.");
        return 0;
    }

    private static async Task<WebIdlInventory> CollectAsync(GeneratorOptions options, CancellationToken cancellationToken)
    {
        var collector = new NodeWebIdlCollector(options);
        return await collector.CollectAsync(cancellationToken);
    }

    private static async Task<WebIdlInventory> ReadInventoryAsync(string inventoryPath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(inventoryPath);
        var inventory = await JsonSerializer.DeserializeAsync<WebIdlInventory>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken);
        return inventory ?? throw new InvalidOperationException($"The inventory file '{inventoryPath}' is invalid.");
    }
}
