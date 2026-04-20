using System.Text.Json;
using Jolt.Rpc;

namespace Jolt.Analysis;

public static class VueAnalysisRpcSerializer
{
    public static JsonSerializerOptions DefaultOptions => JoltRpcSerializer.DefaultOptions;

    public static string Serialize<T>(T value)
        => JoltRpcSerializer.Serialize(value);

    public static T? Deserialize<T>(string json)
        => JoltRpcSerializer.Deserialize<T>(json);
}
