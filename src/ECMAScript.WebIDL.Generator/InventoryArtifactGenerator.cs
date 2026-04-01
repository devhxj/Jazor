using System.Text;
using System.Text.Json;

namespace ECMAScript.WebIDL.Generator;

internal sealed class InventoryArtifactGenerator
{
    private readonly GeneratorOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public InventoryArtifactGenerator(GeneratorOptions options, JsonSerializerOptions jsonOptions)
    {
        _options = options;
        _jsonOptions = jsonOptions;
    }

    public async Task WriteAsync(WebIdlInventory inventory, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_options.OutputDirectory);

        var inventoryPath = Path.IsPathRooted(_options.InventoryFileName)
            ? _options.InventoryFileName
            : Path.Combine(_options.OutputDirectory, _options.InventoryFileName);
        await File.WriteAllTextAsync(
            inventoryPath,
            JsonSerializer.Serialize(inventory, _jsonOptions) + Environment.NewLine,
            cancellationToken);

        var reportPath = Path.Combine(_options.OutputDirectory, "webidl.inventory.md");
        await File.WriteAllTextAsync(reportPath, BuildReport(inventory), cancellationToken);
    }

    private static string BuildReport(WebIdlInventory inventory)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# WebIDL Inventory");
        builder.AppendLine();
        builder.AppendLine($"- Generated: `{inventory.GeneratedAt:O}`");
        builder.AppendLine($"- Files: `{inventory.Stats.FileCount}`");
        builder.AppendLine($"- Declarations: `{inventory.Stats.DeclarationCount}`");
        builder.AppendLine($"- Event targets: `{inventory.Stats.InterfaceEventTargetCount}`");
        builder.AppendLine();
        builder.AppendLine("## Sources");
        builder.AppendLine();
        builder.AppendLine($"- Parser: `{inventory.Source.Parser}`");
        builder.AppendLine($"- WebRef IDL: `{inventory.Source.WebrefIdl}`");
        builder.AppendLine($"- WebRef CSS: `{inventory.Source.WebrefCss}`");
        builder.AppendLine($"- WebRef Events: `{inventory.Source.WebrefEvents}`");
        builder.AppendLine();
        builder.AppendLine("## Declaration Kinds");
        builder.AppendLine();

        foreach (var pair in inventory.Stats.DeclarationsByKind.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            builder.AppendLine($"- `{pair.Key}`: `{pair.Value}`");
        }

        builder.AppendLine();
        builder.AppendLine("## Next Step");
        builder.AppendLine();
        builder.AppendLine("This inventory is the stable interchange format between the Deno collection layer and the future C# binding emitter.");
        builder.AppendLine("A preview emitter currently writes typedef, enum, callback, callback interface, and dictionary bindings under `csharp-preview/`.");
        return builder.ToString();
    }
}
