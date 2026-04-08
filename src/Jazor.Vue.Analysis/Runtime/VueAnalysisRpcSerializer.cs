using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.Vue.Analysis.Runtime;

public static class VueAnalysisRpcSerializer
{
    public static readonly JsonSerializerOptions DefaultOptions = CreateDefaultOptions();

    public static string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, DefaultOptions);

    public static T? Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json, DefaultOptions);

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
