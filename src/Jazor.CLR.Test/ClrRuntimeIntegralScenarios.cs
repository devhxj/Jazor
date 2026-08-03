namespace Jazor.CLR.Test;

internal static class ClrRuntimeIntegralScenarios
{
	private const string Int64ModulePath = "System/Int64Module.js";
	private const string Int32ModulePath = "System/Int32Module.js";
    private const string Int16ModulePath = "System/Int16Module.js";
    private const string UInt16ModulePath = "System/UInt16Module.js";
    private const string UInt32ModulePath = "System/UInt32Module.js";
    private const string ByteModulePath = "System/ByteModule.js";
    private const string SByteModulePath = "System/SByteModule.js";
    private const string UInt64ModulePath = "System/UInt64Module.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("int64.compare.less-than", "long.CompareTo(object)", Int64ModulePath, [BigInt(12), BigInt(17)], Number(-1)),
        Success("int64.compare.null-is-before-value", "long.CompareTo(object)", Int64ModulePath, [BigInt(12), Null()], Number(1)),
        Failure("int64.compare.wrong-type", "long.CompareTo(object)", Int64ModulePath, [BigInt(12), Number(12)], "ArgumentException"),
        Success("int64.parse.maximum", "static long.Parse(string)", Int64ModulePath, [Text("9223372036854775807")], BigInt("9223372036854775807")),
        Failure("int64.parse.null", "static long.Parse(string)", Int64ModulePath, [Null()], "ArgumentNullException"),
        Failure("int64.parse.invalid-format", "static long.Parse(string)", Int64ModulePath, [Text("17px")], "FormatException"),
        Failure("int64.parse.positive-overflow", "static long.Parse(string)", Int64ModulePath, [Text("9223372036854775808")], "OverflowException"),
        Success("int64.try-parse.valid-negative", "static long.TryParse(string, out long)", Int64ModulePath, [Text(" -17 "), BigInt(99)], Array(Bool(true), BigInt(-17))),
		Success("int64.try-parse.null", "static long.TryParse(string, out long)", Int64ModulePath, [Null(), BigInt(99)], Array(Bool(false), BigInt(0))),
		Success("int64.try-parse.char-span", "static long.TryParse(System.ReadOnlySpan<char>, out long)", Int64ModulePath, [Text(" -17 "), BigInt(99)], Array(Bool(true), BigInt(-17))),
        Success("int64.div-rem.negative-dividend-truncates-to-zero", "static long.DivRem(long, long)", Int64ModulePath, [BigInt(-17), BigInt(5)], Record(("quotient", BigInt(-3)), ("remainder", BigInt(-2)))),
        Failure("int64.div-rem.rejects-zero-divisor", "static long.DivRem(long, long)", Int64ModulePath, [BigInt(17), BigInt(0)], "DivideByZeroException"),
        Failure("int64.div-rem.rejects-minimum-overflow", "static long.DivRem(long, long)", Int64ModulePath, [BigInt("-9223372036854775808"), BigInt(-1)], "OverflowException"),
        Success("int64.leading-zero-count.one", "static long.LeadingZeroCount(long)", Int64ModulePath, [BigInt(1)], BigInt(63)),
        Success("int64.leading-zero-count.minus-one", "static long.LeadingZeroCount(long)", Int64ModulePath, [BigInt(-1)], BigInt(0)),
        Success("int64.pop-count.all-bits-set", "static long.PopCount(long)", Int64ModulePath, [BigInt(-1)], BigInt(64)),
        Success("int64.pop-count.single-bit", "static long.PopCount(long)", Int64ModulePath, [BigInt(1)], BigInt(1)),
        Success("int64.rotate-left.moves-sign-bit", "static long.RotateLeft(long, int)", Int64ModulePath, [BigInt(1), Number(63)], BigInt("-9223372036854775808")),
        Success("int64.rotate-left.normalizes-negative-count", "static long.RotateLeft(long, int)", Int64ModulePath, [BigInt(1), Number(-1)], BigInt("-9223372036854775808")),
        Success("int64.rotate-right.restores-low-bit", "static long.RotateRight(long, int)", Int64ModulePath, [BigInt("-9223372036854775808"), Number(63)], BigInt(1)),
        Success("int64.rotate-right.normalizes-negative-count", "static long.RotateRight(long, int)", Int64ModulePath, [BigInt(1), Number(-1)], BigInt(2)),
        Success("int64.trailing-zero-count.zero", "static long.TrailingZeroCount(long)", Int64ModulePath, [BigInt(0)], BigInt(64)),
        Success("int64.trailing-zero-count.low-bit-position", "static long.TrailingZeroCount(long)", Int64ModulePath, [BigInt(8)], BigInt(3)),
        Success("int64.log2.zero", "static long.Log2(long)", Int64ModulePath, [BigInt(0)], BigInt(0)),
        Success("int64.log2.maximum", "static long.Log2(long)", Int64ModulePath, [BigInt("9223372036854775807")], BigInt(62)),
        Failure("int64.log2.rejects-negative", "static long.Log2(long)", Int64ModulePath, [BigInt(-1)], "ArgumentOutOfRangeException"),
        Success("int64.max-magnitude.equal-prefers-positive", "static long.MaxMagnitude(long, long)", Int64ModulePath, [BigInt(-7), BigInt(7)], BigInt(7)),
        Success("int64.max-magnitude.larger-negative-value", "static long.MaxMagnitude(long, long)", Int64ModulePath, [BigInt(-8), BigInt(3)], BigInt(-8)),
        Success("int64.min-magnitude.equal-prefers-negative", "static long.MinMagnitude(long, long)", Int64ModulePath, [BigInt(-7), BigInt(7)], BigInt(-7)),
        Success("int64.min-magnitude.smaller-positive-value", "static long.MinMagnitude(long, long)", Int64ModulePath, [BigInt(-8), BigInt(3)], BigInt(3)),

        Success("int16.compare.null-is-before-value", "short.CompareTo(object)", Int16ModulePath, [Number(0), Null()], Number(1)),
        Failure("int16.compare.wrong-type", "short.CompareTo(object)", Int16ModulePath, [Number(0), Text("0")], "ArgumentException"),
        Success("int16.parse.minimum", "static short.Parse(string)", Int16ModulePath, [Text("-32768")], Number(-32768)),
        Failure("int16.parse.null", "static short.Parse(string)", Int16ModulePath, [Null()], "ArgumentNullException"),
        Success("int16.try-parse.trailing-text", "static short.TryParse(string, out short)", Int16ModulePath, [Text("12px"), Number(99)], Array(Bool(false), Number(0))),
		Success("int16.try-parse.null", "static short.TryParse(string, out short)", Int16ModulePath, [Null(), Number(99)], Array(Bool(false), Number(0))),
		Success("int16.try-parse.char-span", "static short.TryParse(System.ReadOnlySpan<char>, out short)", Int16ModulePath, [Text("-32768"), Number(99)], Array(Bool(true), Number(-32768))),
        Success("int16.div-rem.negative-dividend-truncates-to-zero", "static short.DivRem(short, short)", Int16ModulePath, [Number(-17), Number(5)], Record(("quotient", Number(-3)), ("remainder", Number(-2)))),
        Failure("int16.div-rem.rejects-zero-divisor", "static short.DivRem(short, short)", Int16ModulePath, [Number(17), Number(0)], "DivideByZeroException"),
        Failure("int16.div-rem.rejects-minimum-overflow", "static short.DivRem(short, short)", Int16ModulePath, [Number(-32768), Number(-1)], "OverflowException"),
        Success("int16.pop-count.all-bits-set", "static short.PopCount(short)", Int16ModulePath, [Number(-1)], Number(16)),
        Success("int16.pop-count.single-bit", "static short.PopCount(short)", Int16ModulePath, [Number(1)], Number(1)),
        Success("int16.max-magnitude.equal-prefers-positive", "static short.MaxMagnitude(short, short)", Int16ModulePath, [Number(-7), Number(7)], Number(7)),
        Success("int16.min-magnitude.equal-prefers-negative", "static short.MinMagnitude(short, short)", Int16ModulePath, [Number(-7), Number(7)], Number(-7)),

        Success("uint16.compare.null-is-before-value", "ushort.CompareTo(object)", UInt16ModulePath, [Number(0), Null()], Number(1)),
        Failure("uint16.compare.wrong-type", "ushort.CompareTo(object)", UInt16ModulePath, [Number(0), Text("0")], "ArgumentException"),
        Success("uint16.parse.maximum", "static ushort.Parse(string)", UInt16ModulePath, [Text("65535")], Number(65535)),
        Failure("uint16.parse.null", "static ushort.Parse(string)", UInt16ModulePath, [Null()], "ArgumentNullException"),
        Success("uint16.try-parse.negative-is-invalid", "static ushort.TryParse(string, out ushort)", UInt16ModulePath, [Text("-1"), Number(99)], Array(Bool(false), Number(0))),
		Success("uint16.try-parse.null", "static ushort.TryParse(string, out ushort)", UInt16ModulePath, [Null(), Number(99)], Array(Bool(false), Number(0))),
		Success("uint16.try-parse.char-span", "static ushort.TryParse(System.ReadOnlySpan<char>, out ushort)", UInt16ModulePath, [Text("65535"), Number(99)], Array(Bool(true), Number(65535))),
        Success("uint16.div-rem.positive-operands", "static ushort.DivRem(ushort, ushort)", UInt16ModulePath, [Number(17), Number(5)], Record(("quotient", Number(3)), ("remainder", Number(2)))),
        Failure("uint16.div-rem.rejects-zero-divisor", "static ushort.DivRem(ushort, ushort)", UInt16ModulePath, [Number(17), Number(0)], "DivideByZeroException"),
        Success("uint16.pop-count.all-bits-set", "static ushort.PopCount(ushort)", UInt16ModulePath, [Number(65535)], Number(16)),
        Success("uint16.pop-count.single-bit", "static ushort.PopCount(ushort)", UInt16ModulePath, [Number(1)], Number(1)),

        Success("uint32.compare.null-is-before-value", "uint.CompareTo(object)", UInt32ModulePath, [Number(0), Null()], Number(1)),
        Failure("uint32.compare.wrong-type", "uint.CompareTo(object)", UInt32ModulePath, [Number(0), Text("0")], "ArgumentException"),
        Success("uint32.parse.maximum", "static uint.Parse(string)", UInt32ModulePath, [Text("4294967295")], Number(4294967295)),
        Failure("uint32.parse.null", "static uint.Parse(string)", UInt32ModulePath, [Null()], "ArgumentNullException"),
        Success("uint32.try-parse.overflow", "static uint.TryParse(string, out uint)", UInt32ModulePath, [Text("4294967296"), Number(99)], Array(Bool(false), Number(0))),
		Success("uint32.try-parse.null", "static uint.TryParse(string, out uint)", UInt32ModulePath, [Null(), Number(99)], Array(Bool(false), Number(0))),
		Success("uint32.try-parse.char-span", "static uint.TryParse(System.ReadOnlySpan<char>, out uint)", UInt32ModulePath, [Text("4294967295"), Number(99)], Array(Bool(true), Number(4294967295))),
        Success("uint32.div-rem.positive-operands", "static uint.DivRem(uint, uint)", UInt32ModulePath, [Number(17), Number(5)], Record(("quotient", Number(3)), ("remainder", Number(2)))),
        Failure("uint32.div-rem.rejects-zero-divisor", "static uint.DivRem(uint, uint)", UInt32ModulePath, [Number(17), Number(0)], "DivideByZeroException"),
        Success("uint32.pop-count.all-bits-set", "static uint.PopCount(uint)", UInt32ModulePath, [Number(-1)], Number(32)),
        Success("uint32.pop-count.single-bit", "static uint.PopCount(uint)", UInt32ModulePath, [Number(1)], Number(1)),

        Success("byte.compare.null-is-before-value", "byte.CompareTo(object)", ByteModulePath, [Number(0), Null()], Number(1)),
        Failure("byte.compare.wrong-type", "byte.CompareTo(object)", ByteModulePath, [Number(0), Text("0")], "ArgumentException"),
        Success("byte.parse.maximum", "static byte.Parse(string)", ByteModulePath, [Text("255")], Number(255)),
        Failure("byte.parse.null", "static byte.Parse(string)", ByteModulePath, [Null()], "ArgumentNullException"),
        Success("byte.try-parse.trailing-text", "static byte.TryParse(string, out byte)", ByteModulePath, [Text("12px"), Number(99)], Array(Bool(false), Number(0))),
		Success("byte.try-parse.null", "static byte.TryParse(string, out byte)", ByteModulePath, [Null(), Number(99)], Array(Bool(false), Number(0))),
		Success("byte.try-parse.char-span", "static byte.TryParse(System.ReadOnlySpan<char>, out byte)", ByteModulePath, [Text("255"), Number(99)], Array(Bool(true), Number(255))),
        Success("byte.div-rem.positive-operands", "static byte.DivRem(byte, byte)", ByteModulePath, [Number(17), Number(5)], Record(("quotient", Number(3)), ("remainder", Number(2)))),
        Failure("byte.div-rem.rejects-zero-divisor", "static byte.DivRem(byte, byte)", ByteModulePath, [Number(17), Number(0)], "DivideByZeroException"),

        Success("sbyte.compare.null-is-before-value", "sbyte.CompareTo(object)", SByteModulePath, [Number(0), Null()], Number(1)),
        Failure("sbyte.compare.wrong-type", "sbyte.CompareTo(object)", SByteModulePath, [Number(0), Text("0")], "ArgumentException"),
        Success("sbyte.parse.minimum", "static sbyte.Parse(string)", SByteModulePath, [Text("-128")], Number(-128)),
        Failure("sbyte.parse.null", "static sbyte.Parse(string)", SByteModulePath, [Null()], "ArgumentNullException"),
        Success("sbyte.try-parse.overflow", "static sbyte.TryParse(string, out sbyte)", SByteModulePath, [Text("128"), Number(99)], Array(Bool(false), Number(0))),
		Success("sbyte.try-parse.null", "static sbyte.TryParse(string, out sbyte)", SByteModulePath, [Null(), Number(99)], Array(Bool(false), Number(0))),
		Success("sbyte.try-parse.char-span", "static sbyte.TryParse(System.ReadOnlySpan<char>, out sbyte)", SByteModulePath, [Text("-128"), Number(99)], Array(Bool(true), Number(-128))),

		Success("int32.try-parse.char-span", "static int.TryParse(System.ReadOnlySpan<char>, out int)", Int32ModulePath, [Text("-2147483648"), Number(99)], Array(Bool(true), Number(-2147483648))),

        Success("uint64.compare.less-than", "ulong.CompareTo(object)", UInt64ModulePath, [BigInt(12), BigInt(17)], Number(-1)),
        Success("uint64.compare.null-is-before-value", "ulong.CompareTo(object)", UInt64ModulePath, [BigInt(12), Null()], Number(1)),
        Failure("uint64.compare.wrong-type", "ulong.CompareTo(object)", UInt64ModulePath, [BigInt(12), Number(12)], "ArgumentException"),
        Success("uint64.parse.maximum", "static ulong.Parse(string)", UInt64ModulePath, [Text("18446744073709551615")], BigInt("18446744073709551615")),
        Failure("uint64.parse.null", "static ulong.Parse(string)", UInt64ModulePath, [Null()], "ArgumentNullException"),
        Failure("uint64.parse.positive-overflow", "static ulong.Parse(string)", UInt64ModulePath, [Text("18446744073709551616")], "OverflowException"),
        Success("uint64.try-parse.negative-is-invalid", "static ulong.TryParse(string, out ulong)", UInt64ModulePath, [Text("-1"), BigInt(99)], Array(Bool(false), BigInt(0))),
		Success("uint64.try-parse.null", "static ulong.TryParse(string, out ulong)", UInt64ModulePath, [Null(), BigInt(99)], Array(Bool(false), BigInt(0))),
		Success("uint64.try-parse.char-span", "static ulong.TryParse(System.ReadOnlySpan<char>, out ulong)", UInt64ModulePath, [Text("18446744073709551615"), BigInt(99)], Array(Bool(true), BigInt("18446744073709551615"))),
        Success("uint64.div-rem.positive-operands", "static ulong.DivRem(ulong, ulong)", UInt64ModulePath, [BigInt(17), BigInt(5)], Record(("quotient", BigInt(3)), ("remainder", BigInt(2)))),
        Failure("uint64.div-rem.rejects-zero-divisor", "static ulong.DivRem(ulong, ulong)", UInt64ModulePath, [BigInt(17), BigInt(0)], "DivideByZeroException"),
        Success("uint64.leading-zero-count.one", "static ulong.LeadingZeroCount(ulong)", UInt64ModulePath, [BigInt(1)], BigInt(63)),
        Success("uint64.pop-count.maximum", "static ulong.PopCount(ulong)", UInt64ModulePath, [BigInt("18446744073709551615")], BigInt(64)),
        Success("uint64.rotate-left.moves-high-bit", "static ulong.RotateLeft(ulong, int)", UInt64ModulePath, [BigInt(1), Number(63)], BigInt("9223372036854775808")),
        Success("uint64.rotate-left.normalizes-negative-count", "static ulong.RotateLeft(ulong, int)", UInt64ModulePath, [BigInt(1), Number(-1)], BigInt("9223372036854775808")),
        Success("uint64.rotate-right-restores-low-bit", "static ulong.RotateRight(ulong, int)", UInt64ModulePath, [BigInt("9223372036854775808"), Number(63)], BigInt(1)),
        Success("uint64.trailing-zero-count.zero", "static ulong.TrailingZeroCount(ulong)", UInt64ModulePath, [BigInt(0)], BigInt(64)),
        Success("uint64.trailing-zero-count.low-bit-position", "static ulong.TrailingZeroCount(ulong)", UInt64ModulePath, [BigInt(8)], BigInt(3))
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
