namespace Jazor.CLR.Test;

internal enum ClrRuntimeValueKind
{
    Null,
    String,
    Number,
    Boolean,
    BigInt,
    Array,
    Record,
    Callable,
    Undefined
}

internal enum ClrRuntimeCallableKind
{
    IsEven,
    IsPositive,
    DoubleNumber,
    CompareDescending
}

internal sealed record ClrRuntimeValue(
    ClrRuntimeValueKind Kind,
    string? Scalar = null,
    IReadOnlyList<ClrRuntimeValue>? Items = null,
    IReadOnlyDictionary<string, ClrRuntimeValue>? Properties = null)
{
    public static ClrRuntimeValue Null() => new(ClrRuntimeValueKind.Null);

    public static ClrRuntimeValue Text(string value) => new(ClrRuntimeValueKind.String, value);

    public static ClrRuntimeValue Number(double value)
        => new(ClrRuntimeValueKind.Number, value.ToString("R", System.Globalization.CultureInfo.InvariantCulture));

    public static ClrRuntimeValue BigInt(long value)
        => BigInt(new System.Numerics.BigInteger(value));

    public static ClrRuntimeValue BigInt(System.Numerics.BigInteger value)
        => new(ClrRuntimeValueKind.BigInt, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

    public static ClrRuntimeValue Boolean(bool value)
        => new(ClrRuntimeValueKind.Boolean, value ? "true" : "false");

    public static ClrRuntimeValue Array(params ClrRuntimeValue[] values)
        => new(ClrRuntimeValueKind.Array, Items: values);

    public static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] properties)
        => new(
            ClrRuntimeValueKind.Record,
            Properties: properties.ToDictionary(
                static property => property.Name,
                static property => property.Value,
                StringComparer.Ordinal));

    public static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind)
        => new(ClrRuntimeValueKind.Callable, kind.ToString());

    public static ClrRuntimeValue Undefined() => new(ClrRuntimeValueKind.Undefined);
}

internal sealed record ClrRuntimeScenario(
    string Id,
    string Member,
    string ModulePath,
    IReadOnlyList<ClrRuntimeValue> Arguments,
    ClrRuntimeValue? ExpectedValue,
    string? ExpectedErrorContains = null,
    IReadOnlyList<ClrRuntimeValue>? ExpectedArguments = null);

internal static class ClrRuntimeScenarioCatalog
{
    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        .. ClrRuntimeGuidScenarios.All,
        .. ClrRuntimeBooleanScenarios.All,
        .. ClrRuntimeInt32Scenarios.All,
        .. ClrRuntimeCharScenarios.All,
        .. ClrRuntimeStringScenarios.All,
        .. ClrRuntimeArrayScenarios.All,
        .. ClrRuntimeListScenarios.All,
        .. ClrRuntimeDoubleScenarios.All,
        .. ClrRuntimeMathScenarios.All,
        .. ClrRuntimeBigIntegerScenarios.All,
        .. ClrRuntimeBigIntegerBinaryScenarios.All
    ];

    public static ClrRuntimeScenario Get(string id)
        => All.Single(scenario => string.Equals(scenario.Id, id, StringComparison.Ordinal));
}
