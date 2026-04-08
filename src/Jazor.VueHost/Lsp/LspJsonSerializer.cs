using System.Text.Json;

namespace Jazor.VueHost.Lsp;

internal static class LspJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static string Serialize<TValue>(TValue value)
        => JsonSerializer.Serialize(value, Options);

    public static TValue? Deserialize<TValue>(string json)
        => JsonSerializer.Deserialize<TValue>(json, Options);
}
