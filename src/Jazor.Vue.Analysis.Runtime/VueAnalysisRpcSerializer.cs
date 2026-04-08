using System.Text.Json;
using Jazor.VueContracts.Protocol;

namespace Jazor.Vue.Analysis.Runtime;

public static class VueAnalysisRpcSerializer
{
    public static JsonSerializerOptions DefaultOptions => ProtocolJsonSerializer.DefaultOptions;

    public static string Serialize<T>(T value)
        => ProtocolJsonSerializer.Serialize(value);

    public static T? Deserialize<T>(string json)
        => ProtocolJsonSerializer.Deserialize<T>(json);
}
