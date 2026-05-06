using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.RazorVue.Protocol;

public static class ProtocolJsonSerializer
{
    public static readonly JsonSerializerOptions DefaultOptions = CreateDefaultOptions();

    public static string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, DefaultOptions);

    public static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON payload cannot be null, empty, or whitespace.", nameof(json));
        }

        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
