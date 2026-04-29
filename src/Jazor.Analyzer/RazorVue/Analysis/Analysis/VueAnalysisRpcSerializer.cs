using System.Text.Json;
using ECMAScript.Contract.VueContracts.Protocol;

namespace Jazor.Vue;

public static class VueAnalysisRpcSerializer
{
    public static JsonSerializerOptions DefaultOptions => ProtocolJsonSerializer.DefaultOptions;

    public static string Serialize<T>(T value)
        => ProtocolJsonSerializer.Serialize(value);

    public static T? Deserialize<T>(string json)
        => ProtocolJsonSerializer.Deserialize<T>(json);
}

