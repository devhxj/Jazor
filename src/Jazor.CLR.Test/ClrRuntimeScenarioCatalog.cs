namespace Jazor.CLR.Test;

internal enum ClrRuntimeValueKind
{
    Null,
    String,
    Number,
    Boolean,
    BigInt,
    Array,
    Set,
    Map,
    WeakMap,
    Reference,
    Record,
    Callable,
    RuntimeInvocation,
    Undefined
}

internal enum ClrRuntimeCallableKind
{
    IsEven,
    IsEvenIndex,
    IsPositive,
    DoubleNumber,
    AddIndex,
    CompareDescending
}

internal sealed record ClrRuntimeInvocationValue(
    string Member,
    string ModulePath,
    string ExportName,
    IReadOnlyList<ClrRuntimeValue> Arguments);

internal sealed record ClrRuntimeValue(
    ClrRuntimeValueKind Kind,
    string? Scalar = null,
    IReadOnlyList<ClrRuntimeValue>? Items = null,
    IReadOnlyDictionary<string, ClrRuntimeValue>? Properties = null,
    ClrRuntimeInvocationValue? Invocation = null)
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

    public static ClrRuntimeValue Set(params ClrRuntimeValue[] values)
        => new(ClrRuntimeValueKind.Set, Items: values);

    public static ClrRuntimeValue Map(params (ClrRuntimeValue Key, ClrRuntimeValue Value)[] entries)
        => new(ClrRuntimeValueKind.Map, Items: FlattenEntries(entries));

    public static ClrRuntimeValue WeakMap(params (ClrRuntimeValue Key, ClrRuntimeValue Value)[] entries)
        => new(ClrRuntimeValueKind.WeakMap, Items: FlattenEntries(entries));

    public static ClrRuntimeValue Reference(string id, ClrRuntimeValue value)
        => new(ClrRuntimeValueKind.Reference, id, [value]);

    public static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] properties)
        => new(
            ClrRuntimeValueKind.Record,
            Properties: properties.ToDictionary(
                static property => property.Name,
                static property => property.Value,
                StringComparer.Ordinal));

    public static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind)
        => new(ClrRuntimeValueKind.Callable, kind.ToString());

    public static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments)
    {
        var mapping = ClrRuntimeMappingCatalog.GetImport(member);
        return new(
            ClrRuntimeValueKind.RuntimeInvocation,
            Invocation: new(member, mapping.ModulePath, mapping.ExportName, arguments));
    }

    public static ClrRuntimeValue Undefined() => new(ClrRuntimeValueKind.Undefined);

    private static IReadOnlyList<ClrRuntimeValue> FlattenEntries(
        IEnumerable<(ClrRuntimeValue Key, ClrRuntimeValue Value)> entries)
        => entries.SelectMany(static entry => new[] { entry.Key, entry.Value }).ToArray();
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
        .. ClrRuntimeSetScenarios.HashSet,
        .. ClrRuntimeSetScenarios.InterfaceSet,
        .. ClrRuntimeDictionaryScenarios.All,
        .. ClrRuntimeIntegralScenarios.All,
        .. ClrRuntimeReadOnlyCollectionScenarios.All,
        .. ClrRuntimeQueueStackScenarios.All,
        .. ClrRuntimeBooleanScenarios.All,
        .. ClrRuntimeInt32Scenarios.All,
        .. ClrRuntimeCharScenarios.All,
        .. ClrRuntimeStringScenarios.All,
        .. ClrRuntimeStringExtendedScenarios.All,
        .. ClrRuntimeArrayScenarios.All,
        .. ClrRuntimeArrayExtendedScenarios.All,
        .. ClrRuntimeListScenarios.All,
        .. ClrRuntimeDoubleScenarios.All,
        .. ClrRuntimeSingleScenarios.All,
        .. ClrRuntimeMathScenarios.All,
        .. ClrRuntimeBigIntegerScenarios.All,
        .. ClrRuntimeBigIntegerBinaryScenarios.All,
        .. ClrRuntimeDecimalScenarios.All,
        .. ClrRuntimeDecimalExtendedScenarios.All,
        .. ClrRuntimeDateTimeScenarios.All,
        .. ClrRuntimeTimeSpanScenarios.All,
        .. ClrRuntimeDateTimeOffsetScenarios.All,
        .. ClrRuntimeTimeOnlyScenarios.All,
        .. ClrRuntimeDateOnlyScenarios.All,
        .. ClrRuntimeCalendarScenarios.All,
        .. ClrRuntimeCultureInfoScenarios.All
    ];

    public static ClrRuntimeScenario Get(string id)
        => All.Single(scenario => string.Equals(scenario.Id, id, StringComparison.Ordinal));
}
