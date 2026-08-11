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
            NormalizeLineEndings(JsonSerializer.Serialize(inventory, _jsonOptions) + Environment.NewLine),
            cancellationToken);

        var reportPath = Path.Combine(_options.OutputDirectory, "webidl.inventory.md");
        await File.WriteAllTextAsync(reportPath, NormalizeLineEndings(BuildReport(inventory)), cancellationToken);
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
        if (!string.IsNullOrWhiteSpace(inventory.Source.WebrefXref))
        {
            builder.AppendLine($"- WebRef XRef: `{inventory.Source.WebrefXref}`");
        }
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
        builder.AppendLine("This inventory is the stable interchange format between the Deno collection layer and the C# binding emitter.");
        builder.AppendLine("The preview emitter writes typedef, enum, callback, callback interface, dictionary, interface, and namespace bindings under `webidl/`.");
        builder.AppendLine("When WebRef XRef can match a declaration, member, or argument to a specification definition, the inventory also carries the source anchor, heading, source-authored prose, and available specification usage expressions for XML documentation emission.");
        return builder.ToString();
    }

    private static string NormalizeLineEndings(string text)
    {
        return text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Replace("\n", "\r\n", StringComparison.Ordinal);
    }
}
