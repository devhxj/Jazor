using System.Text.Json;

namespace ECMAScript.WebIDL.Generator;

internal static class WebIdlGeneratorApplication
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        var options = GeneratorOptions.Parse(args, RepositoryLayout.Discover(AppContext.BaseDirectory));
        var collector = new DenoWebIdlCollector(options);
        var inventory = await collector.CollectAsync(cancellationToken);

        var generator = new InventoryArtifactGenerator(options, JsonOptions);
        await generator.WriteAsync(inventory, cancellationToken);

        var previewEmitter = new PreviewBindingEmitter(options);
        await previewEmitter.EmitAsync(inventory, cancellationToken);

        Console.WriteLine($"Collected {inventory.Stats.FileCount} WebIDL files.");
        Console.WriteLine($"Wrote inventory artifacts to '{options.OutputDirectory}'.");
        return 0;
    }
}
