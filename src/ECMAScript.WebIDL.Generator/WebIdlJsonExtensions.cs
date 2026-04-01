using System.Text.Json;

namespace ECMAScript.WebIDL.Generator;

internal static class WebIdlJsonExtensions
{
    public static bool TryGetProperty(this JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    public static string? GetStringOrNull(this JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    public static bool? GetBooleanOrNull(this JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && (value.ValueKind is JsonValueKind.True or JsonValueKind.False)
            ? value.GetBoolean()
            : null;
    }

    public static IReadOnlyList<JsonElement> GetArray(this JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray().ToArray();
    }
}
