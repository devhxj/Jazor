using System.Text.Json;
using Jazor.VueHost.Rpc;

namespace Jazor.VueHost.Analysis;

public static class VueAnalysisRpcSerializer
{
    public static JsonSerializerOptions DefaultOptions => VueHostRpcSerializer.DefaultOptions;

    public static string Serialize<T>(T value)
        => VueHostRpcSerializer.Serialize(value);

    public static T? Deserialize<T>(string json)
        => VueHostRpcSerializer.Deserialize<T>(json);
}
