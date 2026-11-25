using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Jero.Core.Service;

internal static class ApiJsonValue
{
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    internal static JsonObject ParseObject(string json) =>
        JsonNode.Parse(json, documentOptions: DocumentOptions) as JsonObject
        ?? throw new JsonException("Expected an APIJSON object.");

    internal static JsonArray ParseArray(string json) =>
        JsonNode.Parse(json, documentOptions: DocumentOptions) as JsonArray
        ?? throw new JsonException("Expected an APIJSON array.");

    internal static int? ReadInt(JsonNode? node) =>
        node is null ? null : Convert.ToInt32(ToClrValue(node), CultureInfo.InvariantCulture);

    internal static bool IsEmpty(JsonNode? node) => ToText(node).Length == 0;

    internal static string ToText(JsonNode? node) =>
        node is JsonValue ? Convert.ToString(ToClrValue(node), CultureInfo.CurrentCulture) ?? string.Empty
            : node?.ToJsonString() ?? string.Empty;

    internal static IEnumerable<string> ConditionItems(JsonNode? node) =>
        node switch
        {
            JsonArray { Count: > 0 } array => array.Select(ToText),
            JsonObject { Count: > 0 } obj => obj.Select(property =>
                $"{JsonSerializer.Serialize(property.Key)}: {property.Value?.ToJsonString() ?? "null"}"),
            _ => [ToText(node)]
        };

    internal static object? ToClrValue(JsonNode? node, bool sqlParameter = false)
    {
        if (node is not JsonValue value)
            return node;

        // Preserve SQL scalar types instead of passing JsonElement into SqlSugar.
        var element = value.TryGetValue<JsonElement>(out var parsed) ? parsed : JsonSerializer.SerializeToElement(value);
        return element.ValueKind switch
        {
            JsonValueKind.Number when element.TryGetInt64(out var integer) => integer,
            JsonValueKind.Number when element.GetRawText().IndexOfAny(['.', 'e', 'E']) < 0 => element.GetInt64(),
            JsonValueKind.Number when sqlParameter => Convert.ToDecimal(element.GetDouble()),
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when element.TryGetDateTime(out var date) => date,
            JsonValueKind.String => element.GetString(),
            _ => null
        };
    }
}
