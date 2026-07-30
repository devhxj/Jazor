using System.Numerics;

namespace Jazor.CLR.Test;

internal static class ClrRuntimeNumericWidthScenarios
{
	private const string HalfModulePath = "System/HalfModule.js";
	private const string Int128ModulePath = "System/Int128Module.js";
	private const string UInt128ModulePath = "System/UInt128Module.js";

	private static readonly BigInteger Int128Min = BigInteger.Parse("-170141183460469231731687303715884105728");
	private static readonly BigInteger Int128Max = BigInteger.Parse("170141183460469231731687303715884105727");
	private static readonly BigInteger UInt128Max = BigInteger.Parse("340282366920938463463374607431768211455");
	private static readonly double NegativeZero = BitConverter.Int64BitsToDouble(long.MinValue);

	public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
	[
		Success("half.compare.nan-before-number", "System.Half.CompareTo(object)", HalfModulePath, [Number(double.NaN), Number(1)], Number(-1)),
		Failure("half.compare.wrong-type", "System.Half.CompareTo(object)", HalfModulePath, [Number(1), Text("1")], "ArgumentException"),
		Success("half.equals.nan-values", "override System.Half.Equals(object)", HalfModulePath, [Number(double.NaN), Number(double.NaN)], Bool(true)),
		Success("half.parse.decimal", "static System.Half.Parse(string)", HalfModulePath, [Text(" -1.5 ")], Number(-1.5)),
		Success("half.parse.binary16-rounding", "static System.Half.Parse(string)", HalfModulePath, [Text("1.0001")], Number(1)),
		Failure("half.parse.invalid", "static System.Half.Parse(string)", HalfModulePath, [Text("1.5f")], "FormatException"),
		Success("half.parse.provider", "static System.Half.Parse(string, System.IFormatProvider)", HalfModulePath, [Text("2.5"), Null()], Number(2.5)),
		Success("half.try-parse.invalid", "static System.Half.TryParse(string, out System.Half)", HalfModulePath, [Text("half"), Number(7)], Array(Bool(false), Number(0))),
		Success("half.try-parse.provider-infinity", "static System.Half.TryParse(string, System.IFormatProvider, out System.Half)", HalfModulePath, [Text("Infinity"), Null(), Number(0)], Array(Bool(true), Number(double.PositiveInfinity))),
		Success("half.is-pow2.fraction", "static System.Half.IsPow2(System.Half)", HalfModulePath, [Number(0.5)], Bool(true)),
		Success("half.round.positive-midpoint-to-even", "static System.Half.Round(System.Half)", HalfModulePath, [Number(2.5)], Number(2)),
		Success("half.round.negative-midpoint-to-even", "static System.Half.Round(System.Half)", HalfModulePath, [Number(-1.5)], Number(-2)),
		Success("half.ieee-remainder.midpoint-to-even", "static System.Half.Ieee754Remainder(System.Half, System.Half)", HalfModulePath, [Number(5), Number(2)], Number(1)),
		Success("half.ilogb.normal", "static System.Half.ILogB(System.Half)", HalfModulePath, [Number(8)], Number(3)),
		Success("half.ilogb.zero-sentinel", "static System.Half.ILogB(System.Half)", HalfModulePath, [Number(0)], Number(int.MinValue)),
		Success("half.clamp.above-maximum", "static System.Half.Clamp(System.Half, System.Half, System.Half)", HalfModulePath, [Number(5), Number(1), Number(4)], Number(4)),
		Failure("half.clamp.inverted-range", "static System.Half.Clamp(System.Half, System.Half, System.Half)", HalfModulePath, [Number(1), Number(2), Number(0)], "ArgumentException"),
		Failure("half.sign.nan", "static System.Half.Sign(System.Half)", HalfModulePath, [Number(double.NaN)], "ArithmeticException"),
		Success("half.max-magnitude.tie-prefers-positive", "static System.Half.MaxMagnitude(System.Half, System.Half)", HalfModulePath, [Number(-7), Number(7)], Number(7)),
		Success("half.max-magnitude-number.skips-nan", "static System.Half.MaxMagnitudeNumber(System.Half, System.Half)", HalfModulePath, [Number(double.NaN), Number(7)], Number(7)),
		Success("half.min-magnitude.tie-prefers-negative", "static System.Half.MinMagnitude(System.Half, System.Half)", HalfModulePath, [Number(-7), Number(7)], Number(-7)),
		Success("half.min-magnitude-number.skips-nan", "static System.Half.MinMagnitudeNumber(System.Half, System.Half)", HalfModulePath, [Number(4), Number(double.NaN)], Number(4)),
		Success("half.sin-cos.zero", "static System.Half.SinCos(System.Half)", HalfModulePath, [Number(0)], Record(("sin", Number(0)), ("cos", Number(1)))),
		Success("half.sin-cos.binary16-rounding", "static System.Half.SinCos(System.Half)", HalfModulePath, [Number(1)], Record(("sin", Number(0.84130859375)), ("cos", Number(0.54052734375)))),
		Success("half.sin-cos-pi.zero", "static System.Half.SinCosPi(System.Half)", HalfModulePath, [Number(0)], Record(("sinPi", Number(0)), ("cosPi", Number(1)))),
		Success("half.sin-cos-pi.binary16-rounding", "static System.Half.SinCosPi(System.Half)", HalfModulePath, [Number(0.25)], Record(("sinPi", Number(0.70703125)), ("cosPi", Number(0.70703125)))),
		Success("half.root-n.negative-odd", "static System.Half.RootN(System.Half, int)", HalfModulePath, [Number(-8), Number(3)], Number(-2)),
		Success("half.root-n.negative-even-is-nan", "static System.Half.RootN(System.Half, int)", HalfModulePath, [Number(-8), Number(2)], Number(double.NaN)),
		Success("half.root-n.negative-zero-odd", "static System.Half.RootN(System.Half, int)", HalfModulePath, [Number(NegativeZero), Number(3)], Number(NegativeZero)),
		Success("half.root-n.negative-zero-even", "static System.Half.RootN(System.Half, int)", HalfModulePath, [Number(NegativeZero), Number(2)], Number(0)),
		Success("half.root-n.negative-zero-negative-odd", "static System.Half.RootN(System.Half, int)", HalfModulePath, [Number(NegativeZero), Number(-3)], Number(double.NegativeInfinity)),
		Success("half.root-n.zero-degree-is-nan", "static System.Half.RootN(System.Half, int)", HalfModulePath, [Number(8), Number(0)], Number(double.NaN)),

		Success("int128.compare.null-is-before-value", "System.Int128.CompareTo(object)", Int128ModulePath, [Big(12), Null()], Number(1)),
		Success("int128.parse.minimum", "static System.Int128.Parse(string)", Int128ModulePath, [Text(Int128Min.ToString())], Big(Int128Min)),
		Failure("int128.parse.positive-overflow", "static System.Int128.Parse(string)", Int128ModulePath, [Text((Int128Max + 1).ToString())], "OverflowException"),
		Success("int128.parse.provider-maximum", "static System.Int128.Parse(string, System.IFormatProvider)", Int128ModulePath, [Text(Int128Max.ToString()), Null()], Big(Int128Max)),
		Success("int128.try-parse.invalid", "static System.Int128.TryParse(string, out System.Int128)", Int128ModulePath, [Text("0x10"), Big(9)], Array(Bool(false), Big(0))),
		Success("int128.try-parse.provider", "static System.Int128.TryParse(string, System.IFormatProvider, out System.Int128)", Int128ModulePath, [Text("-17"), Null(), Big(0)], Array(Bool(true), Big(-17))),
		Success("int128.div-rem.negative-dividend", "static System.Int128.DivRem(System.Int128, System.Int128)", Int128ModulePath, [Big(-17), Big(5)], Record(("quotient", Big(-3)), ("remainder", Big(-2)))),
		Failure("int128.div-rem.zero-divisor", "static System.Int128.DivRem(System.Int128, System.Int128)", Int128ModulePath, [Big(17), Big(0)], "DivideByZeroException"),
		Failure("int128.div-rem.minimum-overflow", "static System.Int128.DivRem(System.Int128, System.Int128)", Int128ModulePath, [Big(Int128Min), Big(-1)], "OverflowException"),
		Success("int128.operator.divide.truncates-toward-zero", "static System.Int128.operator /(System.Int128, System.Int128)", Int128ModulePath, [Big(-17), Big(5)], Big(-3)),
		Failure("int128.operator.divide.minimum-overflow", "static System.Int128.operator /(System.Int128, System.Int128)", Int128ModulePath, [Big(Int128Min), Big(-1)], "OverflowException"),
		Failure("int128.operator.remainder.minimum-overflow", "static System.Int128.operator %(System.Int128, System.Int128)", Int128ModulePath, [Big(Int128Min), Big(-1)], "OverflowException"),
		Failure("int128.operator.remainder.zero-divisor", "static System.Int128.operator %(System.Int128, System.Int128)", Int128ModulePath, [Big(17), Big(0)], "DivideByZeroException"),
		Success("int128.leading-zero-count.one", "static System.Int128.LeadingZeroCount(System.Int128)", Int128ModulePath, [Big(1)], Big(127)),
		Success("int128.pop-count.minus-one", "static System.Int128.PopCount(System.Int128)", Int128ModulePath, [Big(-1)], Big(128)),
		Success("int128.rotate-left.into-sign-bit", "static System.Int128.RotateLeft(System.Int128, int)", Int128ModulePath, [Big(1), Number(127)], Big(Int128Min)),
		Success("int128.rotate-right.restores-low-bit", "static System.Int128.RotateRight(System.Int128, int)", Int128ModulePath, [Big(Int128Min), Number(127)], Big(1)),
		Success("int128.trailing-zero-count.zero", "static System.Int128.TrailingZeroCount(System.Int128)", Int128ModulePath, [Big(0)], Big(128)),
		Success("int128.max-magnitude.tie-prefers-positive", "static System.Int128.MaxMagnitude(System.Int128, System.Int128)", Int128ModulePath, [Big(-7), Big(7)], Big(7)),
		Success("int128.min-magnitude.tie-prefers-negative", "static System.Int128.MinMagnitude(System.Int128, System.Int128)", Int128ModulePath, [Big(-7), Big(7)], Big(-7)),
		Success("int128.abs.negative", "static System.Int128.Abs(System.Int128)", Int128ModulePath, [Big(-7)], Big(7)),
		Failure("int128.abs.minimum-overflow", "static System.Int128.Abs(System.Int128)", Int128ModulePath, [Big(Int128Min)], "OverflowException"),
		Success("int128.copy-sign.positive", "static System.Int128.CopySign(System.Int128, System.Int128)", Int128ModulePath, [Big(-7), Big(1)], Big(7)),
		Failure("int128.copy-sign.minimum-overflow", "static System.Int128.CopySign(System.Int128, System.Int128)", Int128ModulePath, [Big(Int128Min), Big(1)], "OverflowException"),
		Success("int128.clamp.above-maximum", "static System.Int128.Clamp(System.Int128, System.Int128, System.Int128)", Int128ModulePath, [Big(5), Big(1), Big(4)], Big(4)),
		Failure("int128.clamp.inverted-range", "static System.Int128.Clamp(System.Int128, System.Int128, System.Int128)", Int128ModulePath, [Big(1), Big(2), Big(0)], "ArgumentException"),

		Success("uint128.compare.less-than", "System.UInt128.CompareTo(object)", UInt128ModulePath, [Big(12), Big(17)], Number(-1)),
		Success("uint128.parse.maximum", "static System.UInt128.Parse(string)", UInt128ModulePath, [Text(UInt128Max.ToString())], Big(UInt128Max)),
		Failure("uint128.parse.negative-overflow", "static System.UInt128.Parse(string)", UInt128ModulePath, [Text("-1")], "OverflowException"),
		Success("uint128.parse.provider", "static System.UInt128.Parse(string, System.IFormatProvider)", UInt128ModulePath, [Text("17"), Null()], Big(17)),
		Success("uint128.try-parse.overflow", "static System.UInt128.TryParse(string, out System.UInt128)", UInt128ModulePath, [Text((UInt128Max + 1).ToString()), Big(9)], Array(Bool(false), Big(0))),
		Success("uint128.try-parse.provider", "static System.UInt128.TryParse(string, System.IFormatProvider, out System.UInt128)", UInt128ModulePath, [Text("17"), Null(), Big(0)], Array(Bool(true), Big(17))),
		Success("uint128.div-rem", "static System.UInt128.DivRem(System.UInt128, System.UInt128)", UInt128ModulePath, [Big(17), Big(5)], Record(("quotient", Big(3)), ("remainder", Big(2)))),
		Success("uint128.operator.divide", "static System.UInt128.operator /(System.UInt128, System.UInt128)", UInt128ModulePath, [Big(17), Big(5)], Big(3)),
		Failure("uint128.operator.divide.zero-divisor", "static System.UInt128.operator /(System.UInt128, System.UInt128)", UInt128ModulePath, [Big(17), Big(0)], "DivideByZeroException"),
		Success("uint128.operator.remainder", "static System.UInt128.operator %(System.UInt128, System.UInt128)", UInt128ModulePath, [Big(17), Big(5)], Big(2)),
		Failure("uint128.operator.remainder.zero-divisor", "static System.UInt128.operator %(System.UInt128, System.UInt128)", UInt128ModulePath, [Big(17), Big(0)], "DivideByZeroException"),
		Success("uint128.leading-zero-count.one", "static System.UInt128.LeadingZeroCount(System.UInt128)", UInt128ModulePath, [Big(1)], Big(127)),
		Success("uint128.pop-count.maximum", "static System.UInt128.PopCount(System.UInt128)", UInt128ModulePath, [Big(UInt128Max)], Big(128)),
		Success("uint128.rotate-left.high-bit", "static System.UInt128.RotateLeft(System.UInt128, int)", UInt128ModulePath, [Big(1), Number(127)], Big(Int128Max + 1)),
		Success("uint128.rotate-right.restores-low-bit", "static System.UInt128.RotateRight(System.UInt128, int)", UInt128ModulePath, [Big(Int128Max + 1), Number(127)], Big(1)),
		Success("uint128.trailing-zero-count.zero", "static System.UInt128.TrailingZeroCount(System.UInt128)", UInt128ModulePath, [Big(0)], Big(128)),
		Success("uint128.clamp.above-maximum", "static System.UInt128.Clamp(System.UInt128, System.UInt128, System.UInt128)", UInt128ModulePath, [Big(5), Big(1), Big(4)], Big(4)),
		Failure("uint128.clamp.inverted-range", "static System.UInt128.Clamp(System.UInt128, System.UInt128, System.UInt128)", UInt128ModulePath, [Big(1), Big(2), Big(0)], "ArgumentException")
	];

	private static ClrRuntimeScenario Success(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
		=> new(id, member, modulePath, arguments, expected);

	private static ClrRuntimeScenario Failure(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, string error)
		=> new(id, member, modulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

	private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
	private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
	private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
	private static ClrRuntimeValue Big(long value) => ClrRuntimeValue.BigInt(value);
	private static ClrRuntimeValue Big(BigInteger value) => ClrRuntimeValue.BigInt(value);
	private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
	private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
	private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] values) => ClrRuntimeValue.Record(values);
}
