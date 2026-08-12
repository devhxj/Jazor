namespace Jazor.CLR.Test;

internal static class ClrRuntimeMathScenarios
{
    private const string ModulePath = "System/MathModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "math.sin-cos.zero",
            "static System.Math.SinCos(double)",
            [Number(0)],
            Record(("Sin", Number(0)), ("Cos", Number(1)))),
        Success("math.abs.decimal", "static System.Math.Abs(decimal)", [Text("-123.4500")], Text("123.4500")),
        Success("math.big-mul.uint64.out-high-low", "static System.Math.BigMul(ulong, ulong, out ulong)", [Big("18446744073709551615"), Big("18446744073709551615"), Big(0)], Array(Big("18446744073709551614"), Big(1))),
        Success("math.big-mul.int64.out-signed-low", "static System.Math.BigMul(long, long, out long)", [Big("-9223372036854775808"), Big(-1), Big(0)], Array(Big(0), Big("-9223372036854775808"))),
        Success("math.ceiling.decimal-negative", "static System.Math.Ceiling(decimal)", [Text("-1.2")], Text("-1")),
        Success("math.clamp.decimal-above-maximum", "static System.Math.Clamp(decimal, decimal, decimal)", [Text("100.5"), Text("0"), Text("10")], Text("10")),
        Success("math.floor.decimal-negative", "static System.Math.Floor(decimal)", [Text("-1.2")], Text("-2")),
        Success("math.max.decimal-preserves-selected-scale", "static System.Math.Max(decimal, decimal)", [Text("1.20"), Text("1.2")], Text("1.20")),
        Success("math.min.decimal", "static System.Math.Min(decimal, decimal)", [Text("-5.5"), Text("2")], Text("-5.5")),
        Success("math.max-magnitude.equal-prefers-positive", "static System.Math.MaxMagnitude(double, double)", [Number(-7), Number(7)], Number(7)),
        Success("math.max-magnitude.nan-propagates", "static System.Math.MaxMagnitude(double, double)", [Number(double.NaN), Number(7)], Number(double.NaN)),
        Success("math.min-magnitude.equal-prefers-negative", "static System.Math.MinMagnitude(double, double)", [Number(-7), Number(7)], Number(-7)),
        Success("math.div-rem.int32-out", "static System.Math.DivRem(int, int, out int)", [Number(-17), Number(5), Number(0)], Array(Number(-3), Number(-2))),
        Failure("math.div-rem.int32-out-overflow", "static System.Math.DivRem(int, int, out int)", [Number(-2147483648), Number(-1), Number(0)], "OverflowException"),
        Success("math.div-rem.int64-out", "static System.Math.DivRem(long, long, out long)", [Big(-17), Big(5), Big(0)], Array(Big(-3), Big(-2))),
        Failure("math.div-rem.int64-out-overflow", "static System.Math.DivRem(long, long, out long)", [Big("-9223372036854775808"), Big(-1), Big(0)], "OverflowException"),
        Success("math.div-rem.sbyte", "static System.Math.DivRem(sbyte, sbyte)", [Number(-17), Number(5)], Record(("Quotient", Number(-3)), ("Remainder", Number(-2)))),
        Failure("math.div-rem.sbyte-overflow", "static System.Math.DivRem(sbyte, sbyte)", [Number(-128), Number(-1)], "OverflowException"),
        Success("math.div-rem.byte", "static System.Math.DivRem(byte, byte)", [Number(255), Number(16)], Record(("Quotient", Number(15)), ("Remainder", Number(15)))),
        Success("math.div-rem.int16", "static System.Math.DivRem(short, short)", [Number(-32767), Number(16)], Record(("Quotient", Number(-2047)), ("Remainder", Number(-15)))),
        Failure("math.div-rem.int16-overflow", "static System.Math.DivRem(short, short)", [Number(-32768), Number(-1)], "OverflowException"),
        Success("math.div-rem.uint16", "static System.Math.DivRem(ushort, ushort)", [Number(65535), Number(256)], Record(("Quotient", Number(255)), ("Remainder", Number(255)))),
        Success("math.div-rem.int32", "static System.Math.DivRem(int, int)", [Number(-2147483647), Number(10)], Record(("Quotient", Number(-214748364)), ("Remainder", Number(-7)))),
        Success("math.div-rem.uint32", "static System.Math.DivRem(uint, uint)", [Number(4294967295), Number(65536)], Record(("Quotient", Number(65535)), ("Remainder", Number(65535)))),
        Success("math.div-rem.int64", "static System.Math.DivRem(long, long)", [Big("-9223372036854775807"), Big(10)], Record(("Quotient", Big("-922337203685477580")), ("Remainder", Big(-7)))),
        Success("math.div-rem.uint64", "static System.Math.DivRem(ulong, ulong)", [Big("18446744073709551615"), Big(4294967296)], Record(("Quotient", Big(4294967295)), ("Remainder", Big(4294967295)))),
        Success("math.round.decimal-midpoint-to-even", "static System.Math.Round(decimal)", [Text("2.5")], Text("2")),
        Success("math.round.decimal-with-scale", "static System.Math.Round(decimal, int)", [Text("1.235"), Number(2)], Text("1.24")),
        Success("math.round.decimal-away-from-zero", "static System.Math.Round(decimal, System.MidpointRounding)", [Text("-2.5"), Number(1)], Text("-3")),
        Success("math.round.decimal-scale-toward-zero", "static System.Math.Round(decimal, int, System.MidpointRounding)", [Text("1.239"), Number(2), Number(2)], Text("1.23")),
        Success("math.sign.decimal-negative", "static System.Math.Sign(decimal)", [Text("-0.001")], Number(-1)),
        Success("math.sign.double-positive-infinity", "static System.Math.Sign(double)", [Number(double.PositiveInfinity)], Number(1)),
        Failure("math.sign.double-nan", "static System.Math.Sign(double)", [Number(double.NaN)], "ArithmeticException"),
        Success("math.sign.float-negative", "static System.Math.Sign(float)", [Number(-1.25)], Number(-1)),
        Failure("math.sign.float-nan", "static System.Math.Sign(float)", [Number(double.NaN)], "ArithmeticException"),
        Success("math.truncate.decimal-negative", "static System.Math.Truncate(decimal)", [Text("-12.99")], Text("-12")),
        Success("math.round.double-positive-even-midpoint", "static System.Math.Round(double)", [Number(2.5)], Number(2)),
        Success("math.round.double-negative-even-midpoint", "static System.Math.Round(double)", [Number(-2.5)], Number(-2)),
        Success("math.round.double-decimal-digits", "static System.Math.Round(double, int)", [Number(2.675), Number(2)], Number(Math.Round(2.675, 2))),
        Success("math.round.double-exact-digits", "static System.Math.Round(double, int)", [Number(1.5), Number(1)], Number(Math.Round(1.5, 1))),
        Success("math.round.double-away-from-zero", "static System.Math.Round(double, System.MidpointRounding)", [Number(-2.5), Number(1)], Number(-3)),
        Success("math.round.double-exact-away-from-zero", "static System.Math.Round(double, System.MidpointRounding)", [Number(1), Number(1)], Number(Math.Round(1d, MidpointRounding.AwayFromZero))),
        Success("math.round.double-directed-with-digits", "static System.Math.Round(double, int, System.MidpointRounding)", [Number(-1.234), Number(2), Number(3)], Number(-1.24)),
        Success("math.bit-increment.negative", "static System.Math.BitIncrement(double)", [Number(-1)], Number(double.BitIncrement(-1))),
        Success("math.bit-decrement.negative", "static System.Math.BitDecrement(double)", [Number(-1)], Number(double.BitDecrement(-1))),
        Success("math.ieee-remainder.even-quotient", "static System.Math.IEEERemainder(double, double)", [Number(7), Number(2)], Number(-1)),
        Success("math.ilogb.maximum", "static System.Math.ILogB(double)", [Number(double.MaxValue)], Number(1023)),
        Success("math.ilogb.minimum-subnormal", "static System.Math.ILogB(double)", [Number(double.Epsilon)], Number(-1074))
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string error)
        => new(id, member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Big(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Big(string value)
        => ClrRuntimeValue.BigInt(System.Numerics.BigInteger.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] values) => ClrRuntimeValue.Record(values);
}
