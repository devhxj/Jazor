using System.Globalization;
using System.Text.Json;

namespace Jero.Core;

internal static class LoggingJsonSerializer
{
    public static JsonElement Deserialize(string json)
    {
        var monitor = JsonSerializer.Deserialize<JsonElement>(json);
        return monitor.ValueKind == JsonValueKind.Object
            ? monitor
            : throw new JsonException("日志 JSON 必须为对象");
    }

    public static string SerializeException(Exception exception, JsonSerializerOptions options)
    {
        return JsonSerializer.Serialize(GetExceptionDetails(exception), options);
    }

    // Exception.TargetSite includes reflection metadata that System.Text.Json cannot serialize.
    public static ExceptionDetails GetExceptionDetails(Exception exception) =>
        new(exception.GetType().FullName, exception.Message, exception.StackTrace, exception.Source, exception.HResult,
            exception.InnerException is { } inner ? GetExceptionDetails(inner) : null);

    public static JsonElement Property(this JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var property)
            ? property
            : default;

    public static bool HasValue(this JsonElement element) =>
        element.ValueKind is not (JsonValueKind.Undefined or JsonValueKind.Null);

    public static string? Text(this JsonElement element) =>
        element.HasValue() ? element.ToString() : null;

    public static long? Integer(this JsonElement element) =>
        element.HasValue() ? long.Parse(element.Text()!, CultureInfo.InvariantCulture) : null;

    public static IEnumerable<JsonElement> Items(this JsonElement element) =>
        element.ValueKind == JsonValueKind.Array ? element.EnumerateArray() : Enumerable.Empty<JsonElement>();

    public static Dictionary<string, string> Claims(JsonElement monitor) =>
        monitor.Property("authorizationClaims").Items()
            .Select(claim => (Type: claim.Property("type").Text(), Value: claim.Property("value").Text()))
            .Where(claim => !string.IsNullOrWhiteSpace(claim.Type))
            .GroupBy(claim => claim.Type!)
            .ToDictionary(claims => claims.Key, claims => claims.Last().Value ?? string.Empty);

    public static string? RequestParam(JsonElement monitor, JsonSerializerOptions options)
    {
        var parameter = monitor.Property("parameters").Items().FirstOrDefault();
        // A parameter with a missing or explicit null value is persisted as a JSON null.
        return parameter.HasValue() ? SerializePayload(parameter.Property("value"), options) : null;
    }

    public static string SerializePayload(JsonElement value, JsonSerializerOptions options) =>
        value.HasValue() ? JsonSerializer.Serialize(value, options) : "null";

    internal sealed record ExceptionDetails(
        string? ClassName,
        string Message,
        string? StackTraceString,
        string? Source,
        int HResult,
        ExceptionDetails? InnerException);
}
