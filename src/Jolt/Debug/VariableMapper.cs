namespace Jolt.Debug;

internal sealed class VariableMapper
{
    private const int MaxDisplayValueLength = 4096;

    public DapEvaluationResult ToEvaluationResult(
        CdpRemoteObject? remoteObject,
        int variablesReference = 0)
    {
        if (remoteObject is null)
        {
            return new DapEvaluationResult("undefined", "undefined", 0);
        }

        return new DapEvaluationResult(
            FormatValue(remoteObject),
            remoteObject.Type,
            variablesReference);
    }

    public DapVariable ToVariable(
        string name,
        CdpRemoteObject remoteObject,
        int variablesReference = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(remoteObject);

        return new DapVariable
        {
            Name = name,
            Value = FormatValue(remoteObject),
            Type = remoteObject.Type,
            VariablesReference = variablesReference
        };
    }

    internal static string FormatValue(CdpRemoteObject remoteObject)
    {
        ArgumentNullException.ThrowIfNull(remoteObject);

        if (!string.IsNullOrWhiteSpace(remoteObject.UnserializableValue))
        {
            return TruncateDisplayValue(remoteObject.UnserializableValue!);
        }

        var value = remoteObject.Type switch
        {
            "string" => remoteObject.Value ?? remoteObject.Description ?? string.Empty,
            "number" or "bigint" or "boolean" => remoteObject.Value ?? remoteObject.Description ?? "0",
            "undefined" => "undefined",
            "symbol" => remoteObject.Description ?? "symbol",
            "function" => remoteObject.Description ?? "function",
            "object" when string.Equals(remoteObject.SubType, "null", StringComparison.Ordinal) => "null",
            "object" => remoteObject.Description ?? "Object",
            _ => remoteObject.Value ?? remoteObject.Description ?? "undefined"
        };
        return TruncateDisplayValue(value);
    }

    private static string TruncateDisplayValue(string value)
        => value.Length <= MaxDisplayValueLength
            ? value
            : value[..MaxDisplayValueLength] + "...";
}
