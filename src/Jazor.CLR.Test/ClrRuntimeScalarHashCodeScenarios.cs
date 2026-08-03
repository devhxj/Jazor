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
        Success("hash.string.uses-deterministic-content", "override string.GetHashCode()", "System/StringModule.js", Text("abc"), Number(602801))
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
}
