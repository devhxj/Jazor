namespace Jazor.CLR.Test;

internal static class ClrRuntimeIntegralScenarios
{
    private const string Int64ModulePath = "System/Int64Module.js";
    private const string Int16ModulePath = "System/Int16Module.js";
    private const string UInt16ModulePath = "System/UInt16Module.js";
    private const string UInt32ModulePath = "System/UInt32Module.js";
    private const string ByteModulePath = "System/ByteModule.js";
    private const string SByteModulePath = "System/SByteModule.js";
    private const string UInt64ModulePath = "System/UInt64Module.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("int64.compare.less-than", "long.CompareTo(object)", Int64ModulePath, [BigInt(12), BigInt(17)], Number(-1)),
        Failure("int64.compare.wrong-type", "long.CompareTo(object)", Int64ModulePath, [BigInt(12), Number(12)], "ArgumentException"),
        Success("int64.parse.maximum", "static long.Parse(string)", Int64ModulePath, [Text("9223372036854775807")], BigInt("9223372036854775807")),
        Failure("int64.parse.positive-overflow", "static long.Parse(string)", Int64ModulePath, [Text("9223372036854775808")], "OverflowException"),
        Success("int64.try-parse.valid-negative", "static long.TryParse(string, out long)", Int64ModulePath, [Text(" -17 "), BigInt(99)], Array(Bool(true), BigInt(-17))),
        Success("int64.div-rem.negative-dividend-truncates-to-zero", "static long.DivRem(long, long)", Int64ModulePath, [BigInt(-17), BigInt(5)], Record(("quotient", BigInt(-3)), ("remainder", BigInt(-2)))),
        Success("int64.pop-count.all-bits-set", "static long.PopCount(long)", Int64ModulePath, [BigInt(-1)], BigInt(64)),
        Success("int64.rotate-left.moves-sign-bit", "static long.RotateLeft(long, int)", Int64ModulePath, [BigInt(1), Number(63)], BigInt("-9223372036854775808")),
        Success("int64.rotate-right.restores-low-bit", "static long.RotateRight(long, int)", Int64ModulePath, [BigInt("-9223372036854775808"), Number(63)], BigInt(1)),
        Success("int64.trailing-zero-count.zero", "static long.TrailingZeroCount(long)", Int64ModulePath, [BigInt(0)], BigInt(64)),
        Success("int64.max-magnitude.equal-prefers-positive", "static long.MaxMagnitude(long, long)", Int64ModulePath, [BigInt(-7), BigInt(7)], BigInt(7)),
        Success("int64.min-magnitude.equal-prefers-negative", "static long.MinMagnitude(long, long)", Int64ModulePath, [BigInt(-7), BigInt(7)], BigInt(-7)),

        Success("int16.compare.null-is-before-value", "short.CompareTo(object)", Int16ModulePath, [Number(0), Null()], Number(1)),
        Success("int16.parse.minimum", "static short.Parse(string)", Int16ModulePath, [Text("-32768")], Number(-32768)),
        Success("int16.try-parse.trailing-text", "static short.TryParse(string, out short)", Int16ModulePath, [Text("12px"), Number(99)], Array(Bool(false), Number(0))),
        Success("int16.div-rem.negative-dividend-truncates-to-zero", "static short.DivRem(short, short)", Int16ModulePath, [Number(-17), Number(5)], Record(("quotient", Number(-3)), ("remainder", Number(-2)))),
        Success("int16.pop-count.all-bits-set", "static short.PopCount(short)", Int16ModulePath, [Number(-1)], Number(16)),
        Success("int16.max-magnitude.equal-prefers-positive", "static short.MaxMagnitude(short, short)", Int16ModulePath, [Number(-7), Number(7)], Number(7)),
        Success("int16.min-magnitude.equal-prefers-negative", "static short.MinMagnitude(short, short)", Int16ModulePath, [Number(-7), Number(7)], Number(-7)),

        Success("uint16.compare.null-is-before-value", "ushort.CompareTo(object)", UInt16ModulePath, [Number(0), Null()], Number(1)),
        Success("uint16.parse.maximum", "static ushort.Parse(string)", UInt16ModulePath, [Text("65535")], Number(65535)),
        Success("uint16.try-parse.negative-is-invalid", "static ushort.TryParse(string, out ushort)", UInt16ModulePath, [Text("-1"), Number(99)], Array(Bool(false), Number(0))),
        Success("uint16.div-rem.positive-operands", "static ushort.DivRem(ushort, ushort)", UInt16ModulePath, [Number(17), Number(5)], Record(("quotient", Number(3)), ("remainder", Number(2)))),
        Success("uint16.pop-count.all-bits-set", "static ushort.PopCount(ushort)", UInt16ModulePath, [Number(65535)], Number(16)),

        Success("uint32.compare.null-is-before-value", "uint.CompareTo(object)", UInt32ModulePath, [Number(0), Null()], Number(1)),
        Success("uint32.parse.maximum", "static uint.Parse(string)", UInt32ModulePath, [Text("4294967295")], Number(4294967295)),
        Success("uint32.try-parse.overflow", "static uint.TryParse(string, out uint)", UInt32ModulePath, [Text("4294967296"), Number(99)], Array(Bool(false), Number(0))),
        Success("uint32.div-rem.positive-operands", "static uint.DivRem(uint, uint)", UInt32ModulePath, [Number(17), Number(5)], Record(("quotient", Number(3)), ("remainder", Number(2)))),
        Success("uint32.pop-count.all-bits-set", "static uint.PopCount(uint)", UInt32ModulePath, [Number(-1)], Number(32)),

        Success("byte.compare.null-is-before-value", "byte.CompareTo(object)", ByteModulePath, [Number(0), Null()], Number(1)),
        Success("byte.parse.maximum", "static byte.Parse(string)", ByteModulePath, [Text("255")], Number(255)),
        Success("byte.try-parse.trailing-text", "static byte.TryParse(string, out byte)", ByteModulePath, [Text("12px"), Number(99)], Array(Bool(false), Number(0))),
        Success("byte.div-rem.positive-operands", "static byte.DivRem(byte, byte)", ByteModulePath, [Number(17), Number(5)], Record(("quotient", Number(3)), ("remainder", Number(2)))),

        Success("sbyte.compare.null-is-before-value", "sbyte.CompareTo(object)", SByteModulePath, [Number(0), Null()], Number(1)),
        Success("sbyte.parse.minimum", "static sbyte.Parse(string)", SByteModulePath, [Text("-128")], Number(-128)),
        Success("sbyte.try-parse.overflow", "static sbyte.TryParse(string, out sbyte)", SByteModulePath, [Text("128"), Number(99)], Array(Bool(false), Number(0))),

        Success("uint64.compare.less-than", "ulong.CompareTo(object)", UInt64ModulePath, [BigInt(12), BigInt(17)], Number(-1)),
        Success("uint64.parse.maximum", "static ulong.Parse(string)", UInt64ModulePath, [Text("18446744073709551615")], BigInt("18446744073709551615")),
        Success("uint64.try-parse.negative-is-invalid", "static ulong.TryParse(string, out ulong)", UInt64ModulePath, [Text("-1"), BigInt(99)], Array(Bool(false), BigInt(0)))
    ];

    private static ClrRuntimeScenario Success(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, modulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, string error)
        => new(id, member, modulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue BigInt(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue BigInt(string value) => new(ClrRuntimeValueKind.BigInt, value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] values) => ClrRuntimeValue.Record(values);
}
