namespace Jazor.CLR.Test;

internal static class ClrRuntimeScalarHashCodeScenarios
{
    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("hash.long.folds-high-and-low-words", "override long.GetHashCode()", "System/Int64Module.js", BigInt(4294967296), Number(1)),
        Success("hash.ulong.folds-high-and-low-words", "override ulong.GetHashCode()", "System/UInt64Module.js", BigInt(4294967296), Number(1)),
        Success("hash.int128.folds-all-words", "override System.Int128.GetHashCode()", "System/Int128Module.js", BigInt(4294967296), Number(1)),
        Success("hash.uint128.folds-all-words", "override System.UInt128.GetHashCode()", "System/UInt128Module.js", BigInt(4294967296), Number(1)),
        Success("hash.half.normalizes-number", "override System.Half.GetHashCode()", "System/HalfModule.js", Number(42), Number(42)),
        Success("hash.single.normalizes-number", "override float.GetHashCode()", "System/SingleModule.js", Number(42), Number(42)),
        Success("hash.double.normalizes-number", "override double.GetHashCode()", "System/DoubleModule.js", Number(42), Number(42)),
        Success("hash.string.uses-deterministic-content", "override string.GetHashCode()", "System/StringModule.js", Text("abc"), Number(602801)),

        Failure("hash.object.rejects-null-receiver", "virtual object.GetHashCode()", "System/ObjectModule.js", Null(), "NullReferenceException"),
        Success("hash.object.reuses-scalar-carrier-contract", "virtual object.GetHashCode()", "System/ObjectModule.js", Number(42), Number(42)),
        Success("hash.object.dispatches-compiled-override", "virtual object.GetHashCode()", "System/ObjectModule.js", Record(("getHashCode", Callable(ClrRuntimeCallableKind.ReturnHashCode))), Number(713)),
        Success("hash.object.assigns-first-reference-identity", "virtual object.GetHashCode()", "System/ObjectModule.js", Record(("id", Number(1))), Number(1)),
        Success("hash.object.assigns-distinct-reference-identity", "virtual object.GetHashCode()", "System/ObjectModule.js", Record(("id", Number(2))), Number(2))
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        string modulePath,
        ClrRuntimeValue argument,
        ClrRuntimeValue expected)
        => new(id, member, modulePath, [argument], expected);

    private static ClrRuntimeValue BigInt(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] properties) => ClrRuntimeValue.Record(properties);
    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);

    private static ClrRuntimeScenario Failure(
        string id,
        string member,
        string modulePath,
        ClrRuntimeValue argument,
        string error)
        => new(id, member, modulePath, [argument], ExpectedValue: null, ExpectedErrorContains: error);
}
