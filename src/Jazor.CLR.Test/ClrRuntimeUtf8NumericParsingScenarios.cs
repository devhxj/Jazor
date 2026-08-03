using System.Numerics;
using System.Text;

namespace Jazor.CLR.Test;

internal static class ClrRuntimeUtf8NumericParsingScenarios
{
	private static readonly BigInteger Int128Min = BigInteger.Parse("-170141183460469231731687303715884105728");
	private static readonly BigInteger UInt128Max = BigInteger.Parse("340282366920938463463374607431768211455");

	public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
	[
		Success("utf8-parse.byte.maximum", "static byte.TryParse(System.ReadOnlySpan<byte>, out byte)", "System/ByteModule.js", Utf8("255"), Number(9), Number(255)),
		Success("utf8-parse.sbyte.minimum", "static sbyte.TryParse(System.ReadOnlySpan<byte>, out sbyte)", "System/SByteModule.js", Utf8("-128"), Number(9), Number(-128)),
		Success("utf8-parse.int16.minimum", "static short.TryParse(System.ReadOnlySpan<byte>, out short)", "System/Int16Module.js", Utf8("-32768"), Number(9), Number(-32768)),
		Success("utf8-parse.uint16.maximum", "static ushort.TryParse(System.ReadOnlySpan<byte>, out ushort)", "System/UInt16Module.js", Utf8("65535"), Number(9), Number(65535)),
		Success("utf8-parse.int32.minimum", "static int.TryParse(System.ReadOnlySpan<byte>, out int)", "System/Int32Module.js", Utf8("-2147483648"), Number(9), Number(-2147483648)),
		Success("utf8-parse.uint32.maximum", "static uint.TryParse(System.ReadOnlySpan<byte>, out uint)", "System/UInt32Module.js", Utf8("4294967295"), Number(9), Number(4294967295)),
		Success("utf8-parse.int64.minimum", "static long.TryParse(System.ReadOnlySpan<byte>, out long)", "System/Int64Module.js", Utf8(long.MinValue.ToString()), Big(9), Big(long.MinValue)),
		Success("utf8-parse.uint64.maximum", "static ulong.TryParse(System.ReadOnlySpan<byte>, out ulong)", "System/UInt64Module.js", Utf8(ulong.MaxValue.ToString()), Big(9), Big(ulong.MaxValue)),
		Success("utf8-parse.int128.minimum", "static System.Int128.TryParse(System.ReadOnlySpan<byte>, out System.Int128)", "System/Int128Module.js", Utf8(Int128Min.ToString()), Big(9), Big(Int128Min)),
		Success("utf8-parse.uint128.maximum", "static System.UInt128.TryParse(System.ReadOnlySpan<byte>, out System.UInt128)", "System/UInt128Module.js", Utf8(UInt128Max.ToString()), Big(9), Big(UInt128Max)),
		Success("utf8-parse.half.rounds-binary16", "static System.Half.TryParse(System.ReadOnlySpan<byte>, out System.Half)", "System/HalfModule.js", Utf8("1.0001"), Number(9), Number(1)),
		Success("utf8-parse.single.fraction", "static float.TryParse(System.ReadOnlySpan<byte>, out float)", "System/SingleModule.js", Utf8("-12.5"), Number(9), Number(-12.5)),
		Success("utf8-parse.double.exponent", "static double.TryParse(System.ReadOnlySpan<byte>, out double)", "System/DoubleModule.js", Utf8("1.25e2"), Number(9), Number(125)),
		Success("utf8-parse.decimal.preserves-scale", "static decimal.TryParse(System.ReadOnlySpan<byte>, out decimal)", "System/DecimalModule.js", Utf8("123.4500"), Text("9"), Text("123.4500")),

		Failure("utf8-parse.int32.rejects-malformed-encoding", "static int.TryParse(System.ReadOnlySpan<byte>, out int)", "System/Int32Module.js", Bytes(0x31, 0xff), Number(9), Number(0)),
		Failure("utf8-parse.byte.rejects-leading-bom", "static byte.TryParse(System.ReadOnlySpan<byte>, out byte)", "System/ByteModule.js", Bytes(0xef, 0xbb, 0xbf, 0x31), Number(9), Number(0)),
		Failure("utf8-parse.uint64.rejects-overflow", "static ulong.TryParse(System.ReadOnlySpan<byte>, out ulong)", "System/UInt64Module.js", Utf8("18446744073709551616"), Big(9), Big(0)),
		Failure("utf8-parse.decimal.rejects-exponent-by-default", "static decimal.TryParse(System.ReadOnlySpan<byte>, out decimal)", "System/DecimalModule.js", Utf8("1e2"), Text("9"), Text("0")),
		DirectSuccess(
			"utf8-parse.decimal.style-provider",
			"static decimal.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)",
			[Utf8("1e2"), Number(167), Text("")],
			Text("100")),
		DirectSuccess(
			"utf8-parse.decimal.try-style-provider",
			"static decimal.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)",
			[Utf8("1.234,50"), Number(111), Text("de-DE"), Text("9")],
			Array(Bool(true), Text("1234.50"))),
		DirectSuccess(
			"utf8-parse.decimal.provider",
			"static decimal.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)",
			[Utf8("1.234,50"), Text("de-DE")],
			Text("1234.50")),
		DirectSuccess(
			"utf8-parse.decimal.try-provider-malformed",
			"static decimal.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out decimal)",
			[Bytes(0x31, 0xff), Text(""), Text("9")],
			Array(Bool(false), Text("0"))),
		Throws(
			"utf8-parse.decimal.parse-rejects-malformed",
			"static decimal.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)",
			[Bytes(0x31, 0xff), Text("")],
			"FormatException")
	];

	private static ClrRuntimeScenario Success(
		string id,
		string member,
		string modulePath,
		ClrRuntimeValue input,
		ClrRuntimeValue initialResult,
		ClrRuntimeValue parsed)
		=> new(id, member, modulePath, [input, initialResult], Array(Bool(true), parsed));

	private static ClrRuntimeScenario Failure(
		string id,
		string member,
		string modulePath,
		ClrRuntimeValue input,
		ClrRuntimeValue initialResult,
		ClrRuntimeValue zero)
		=> new(id, member, modulePath, [input, initialResult], Array(Bool(false), zero));

	private static ClrRuntimeScenario DirectSuccess(
		string id,
		string member,
		IReadOnlyList<ClrRuntimeValue> arguments,
		ClrRuntimeValue expected)
		=> new(id, member, "System/DecimalModule.js", arguments, expected);

	private static ClrRuntimeScenario Throws(
		string id,
		string member,
		IReadOnlyList<ClrRuntimeValue> arguments,
		string error)
		=> new(id, member, "System/DecimalModule.js", arguments, ExpectedValue: null, ExpectedErrorContains: error);

	private static ClrRuntimeValue Utf8(string value) => Bytes(Encoding.UTF8.GetBytes(value));
	private static ClrRuntimeValue Bytes(params byte[] values)
		=> ClrRuntimeValue.Array(values.Select(static value => Number(value)).ToArray());
	private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
	private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
	private static ClrRuntimeValue Big(long value) => ClrRuntimeValue.BigInt(value);
	private static ClrRuntimeValue Big(ulong value) => ClrRuntimeValue.BigInt(value);
	private static ClrRuntimeValue Big(BigInteger value) => ClrRuntimeValue.BigInt(value);
	private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
	private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
