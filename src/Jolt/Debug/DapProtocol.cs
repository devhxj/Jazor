using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jolt.Debug;

internal sealed class DapRequest
{
    public int Seq { get; init; }

    public string Type { get; init; } = "request";

    public required string Command { get; init; }

    public JsonElement? Arguments { get; init; }
}

internal sealed class DapResponse
{
    public required int Seq { get; init; }

    public string Type => "response";

    public required int RequestSeq { get; init; }

    public required string Command { get; init; }

    public bool Success { get; init; } = true;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Body { get; init; }
}

internal sealed class DapEvent
{
    public required int Seq { get; init; }

    public string Type => "event";

    public required string Event { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Body { get; init; }
}

internal sealed class DapBreakpoint
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Id { get; init; }

    public required bool Verified { get; init; }

    public required int Line { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Column { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; init; }
}

internal sealed class DapThread
{
    public required int Id { get; init; }

    public required string Name { get; init; }
}

internal sealed class DapScope
{
    public required string Name { get; init; }

    public required int VariablesReference { get; init; }

    public required bool Expensive { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PresentationHint { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? NamedVariables { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? IndexedVariables { get; init; }
}

internal sealed class DapVariable
{
    public required string Name { get; init; }

    public required string Value { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; init; }

    public int VariablesReference { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? NamedVariables { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? IndexedVariables { get; init; }
}

internal sealed class DapDispatchResult
{
    public required DapResponse Response { get; init; }

    public IReadOnlyList<DapEvent> Events { get; init; } = [];

    public bool ShouldTerminate { get; init; }
}

internal static class DapProtocolSerializer
{
    public static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, DefaultOptions);

    public static T? Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json, DefaultOptions);
}
