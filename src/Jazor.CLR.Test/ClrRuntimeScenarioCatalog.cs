namespace Jazor.CLR.Test;

internal enum ClrRuntimeValueKind
{
    Null,
    String,
    Number,
    Boolean,
    BigInt,
    Array,
	ArrayElement,
	Set,
	Map,
	WeakMap,
	Reference,
	Sequence,
    Record,
    Callable,
    Disposable,
    AsyncDisposable,
    RuntimeInvocation,
    Error,
    Undefined
}

internal enum ClrRuntimeCallableKind
{
    IsEven,
    IsEvenIndex,
    IsPositive,
    DoubleNumber,
    AddIndex,
    ExpandNumber,
    ExpandWithIndex,
    CombineOuterInner,
    CombineOuterGroupCount,
    GroupKeyAndSum,
    CompareDescending,
    AddNumbers,
    ToBigInt,
    ToDecimalText,
    ReturnFactoryText,
    ReturnFactoryArgument,
	ReturnHashCode,
    Identity,
    SameParity,
    ParityHash
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

    public static ClrRuntimeValue ArrayElement(ClrRuntimeValue array, int index)
        => new(
            ClrRuntimeValueKind.ArrayElement,
            index.ToString(System.Globalization.CultureInfo.InvariantCulture),
            [array]);

    public static ClrRuntimeValue Set(params ClrRuntimeValue[] values)
        => new(ClrRuntimeValueKind.Set, Items: values);

    public static ClrRuntimeValue Map(params (ClrRuntimeValue Key, ClrRuntimeValue Value)[] entries)
        => new(ClrRuntimeValueKind.Map, Items: FlattenEntries(entries));

	public static ClrRuntimeValue WeakMap(params (ClrRuntimeValue Key, ClrRuntimeValue Value)[] entries)
		=> new(ClrRuntimeValueKind.WeakMap, Items: FlattenEntries(entries));

	public static ClrRuntimeValue Reference(string id, ClrRuntimeValue value)
		=> new(ClrRuntimeValueKind.Reference, id, [value]);

	public static ClrRuntimeValue Sequence(params ClrRuntimeValue[] steps)
		=> new(ClrRuntimeValueKind.Sequence, Items: steps);

    public static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] properties)
        => new(
            ClrRuntimeValueKind.Record,
            Properties: properties.ToDictionary(
                static property => property.Name,
                static property => property.Value,
                StringComparer.Ordinal));

    public static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind)
        => new(ClrRuntimeValueKind.Callable, kind.ToString());

    public static ClrRuntimeValue Disposable(int count = 0)
        => new(ClrRuntimeValueKind.Disposable, count.ToString(System.Globalization.CultureInfo.InvariantCulture));

    public static ClrRuntimeValue AsyncDisposable(int count = 0)
        => new(ClrRuntimeValueKind.AsyncDisposable, count.ToString(System.Globalization.CultureInfo.InvariantCulture));

    public static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments)
    {
        var mapping = ClrRuntimeMappingCatalog.GetImport(member);
        return new(
            ClrRuntimeValueKind.RuntimeInvocation,
            Invocation: new(member, mapping.ModulePath, mapping.ExportName, arguments));
    }

    public static ClrRuntimeValue Error(string message, ClrRuntimeValue? cause = null)
        => new(ClrRuntimeValueKind.Error, message, [cause ?? Null()]);

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
        .. ClrRuntimeCollectionDiscardScenarios.All,
        .. ClrRuntimeIntegralScenarios.All,
        .. ClrRuntimeScalarHashCodeScenarios.All,
        .. ClrRuntimeReadOnlyCollectionScenarios.All,
        .. ClrRuntimeEnumerableSelectManyScenarios.All,
        .. ClrRuntimeEnumerableCountScenarios.All,
		.. ClrRuntimeEnumerableLongCountScenarios.All,
		.. ClrRuntimeEnumerableIndexScenarios.All,
		.. ClrRuntimeEnumerableTryGetNonEnumeratedCountScenarios.All,
		.. ClrRuntimeEnumerableSumScenarios.All,
		.. ClrRuntimeEnumerableAverageScenarios.All,
		.. ClrRuntimeEnumerableNullableNumericScenarios.All,
		.. ClrRuntimeEnumerableNullableMinMaxScenarios.All,
		.. ClrRuntimeEnumerableNullableNumericSelectorScenarios.All,
		.. ClrRuntimeEnumerableNumericSelectorScenarios.All,
		.. ClrRuntimeEnumerableMinMaxScenarios.All,
        .. ClrRuntimeEnumerableConcatScenarios.All,
		.. ClrRuntimeEnumerableAppendPrependScenarios.All,
		.. ClrRuntimeEnumerableWhileScenarios.All,
		.. ClrRuntimeEnumerableFactoryScenarios.All,
		.. ClrRuntimeEnumerableSkipTakeLastScenarios.All,
		.. ClrRuntimeEnumerableTakeRangeScenarios.All,
		.. ClrRuntimeEnumerableDefaultIfEmptyScenarios.All,
        .. ClrRuntimeEnumerableDefaultTerminalScenarios.All,
        .. ClrRuntimeEnumerableElementAtScenarios.All,
        .. ClrRuntimeEnumerableDistinctByScenarios.All,
        .. ClrRuntimeEnumerableMinMaxByScenarios.All,
        .. ClrRuntimeEnumerableChunkScenarios.All,
        .. ClrRuntimeEnumerableReverseScenarios.All,
        .. ClrRuntimeEnumerableTerminalScenarios.All,
		.. ClrRuntimeEnumerableSequenceEqualScenarios.All,
		.. ClrRuntimeEnumerableAggregateScenarios.All,
		.. ClrRuntimeEnumerableAggregateByScenarios.All,
		.. ClrRuntimeEnumerableSetByScenarios.All,
		.. ClrRuntimeMemoryExtensionsSequenceEqualScenarios.All,
		.. ClrRuntimeMemoryExtensionsTrimScenarios.All,
        .. ClrRuntimeEnumerableSetScenarios.All,
		.. ClrRuntimeEnumerableComparerScenarios.All,
		.. ClrRuntimeEnumerableGroupByScenarios.All,
		.. ClrRuntimeEnumerableLookupScenarios.All,
		.. ClrRuntimeEnumerableJoinScenarios.All,
        .. ClrRuntimeQueueStackScenarios.All,
        .. ClrRuntimeComparerScenarios.All,
        .. ClrRuntimeIndexRangeScenarios.All,
        .. ClrRuntimeTailScenarios.All,
        .. ClrRuntimeBooleanScenarios.All,
        .. ClrRuntimeExceptionScenarios.All,
        .. ClrRuntimeInt32Scenarios.All,
        .. ClrRuntimeCharScenarios.All,
        .. ClrRuntimeStringScenarios.All,
        .. ClrRuntimeStringExtendedScenarios.All,
        .. ClrRuntimeStringBuilderScenarios.All,
        .. ClrRuntimeArrayScenarios.All,
        .. ClrRuntimeArrayExtendedScenarios.All,
        .. ClrRuntimeListScenarios.All,
        .. ClrRuntimeDoubleScenarios.All,
        .. ClrRuntimeSingleScenarios.All,
		.. ClrRuntimeNumericWidthScenarios.All,
		.. ClrRuntimeUtf8NumericParsingScenarios.All,
        .. ClrRuntimeNullableScenarios.All,
		.. ClrRuntimeMathScenarios.All,
		.. ClrRuntimeWeakReferenceScenarios.All,
		.. ClrRuntimeReadOnlyArrayViewScenarios.All,
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
        .. ClrRuntimeCultureInfoScenarios.All,
        .. ClrRuntimeNavigationScenarios.All
    ];

    public static ClrRuntimeScenario Get(string id)
        => All.Single(scenario => string.Equals(scenario.Id, id, StringComparison.Ordinal));
}
