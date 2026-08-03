namespace Jazor.CLR.Test;

internal static class ClrRuntimeDecimalScenarios
{
    private const string ModulePath = "System/DecimalModule.js";
    private const string MaxValue = "79228162514264337593543950335";
    private const string MinValue = "-79228162514264337593543950335";
    private const int NumberStyle = 111;
    private const int FloatStyle = 167;

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("decimal.constant.zero", "static readonly decimal.Zero", [], Text("0")),
        Success("decimal.constant.one", "static readonly decimal.One", [], Text("1")),
        Success("decimal.constant.minus-one", "static readonly decimal.MinusOne", [], Text("-1")),
        Success("decimal.constant.max-value", "static readonly decimal.MaxValue", [], Text(MaxValue)),
        Success("decimal.constant.min-value", "static readonly decimal.MinValue", [], Text(MinValue)),
        Success("decimal.ctor.double-finite", "decimal.Decimal(double)", [Number(1.25)], Text("1.25")),
        Failure("decimal.ctor.double-infinity", "decimal.Decimal(double)", [Number(double.PositiveInfinity)], "OverflowException"),
        Success("decimal.from-oa-currency.zero-keeps-four-digits", "static decimal.FromOACurrency(long)", [Big(0)], Text("0.0000")),
        Success("decimal.from-oa-currency.normalizes-trailing-zeros", "static decimal.FromOACurrency(long)", [Big(1000)], Text("0.1")),
        Success("decimal.from-oa-currency.maximum", "static decimal.FromOACurrency(long)", [Big(long.MaxValue)], Text("922337203685477.5807")),
        Success("decimal.from-oa-currency.minimum", "static decimal.FromOACurrency(long)", [Big(long.MinValue)], Text("-922337203685477.5808")),
        Success("decimal.to-oa-currency-midpoint-to-even-down", "static decimal.ToOACurrency(decimal)", [Text("0.00005")], Big(0)),
        Success("decimal.to-oa-currency-midpoint-to-even-up", "static decimal.ToOACurrency(decimal)", [Text("0.00015")], Big(2)),
        Success("decimal.to-oa-currency-negative-rounding", "static decimal.ToOACurrency(decimal)", [Text("-0.00006")], Big(-1)),
        Success("decimal.to-oa-currency-maximum", "static decimal.ToOACurrency(decimal)", [Text("922337203685477.5807")], Big(long.MaxValue)),
        Success("decimal.to-oa-currency-minimum", "static decimal.ToOACurrency(decimal)", [Text("-922337203685477.5808")], Big(long.MinValue)),
        Failure("decimal.to-oa-currency-overflow-after-rounding", "static decimal.ToOACurrency(decimal)", [Text("922337203685477.58075")], "OverflowException"),
        Success("decimal.scale.trailing-zeros", "decimal.Scale.get", [Text("123.4500")], Number(4)),
        Success("decimal.scale.scaled-zero", "decimal.Scale.get", [Text("0.00")], Number(2)),
        Success("decimal.add.preserves-maximum-scale", "static decimal.Add(decimal, decimal)", [Text("1.20"), Text("1.3")], Text("2.50")),
        Failure("decimal.add.overflow", "static decimal.Add(decimal, decimal)", [Text(MaxValue), Text("1")], "OverflowException"),
        Success("decimal.ceiling.negative", "static decimal.Ceiling(decimal)", [Text("-1.20")], Text("-1")),
        Success("decimal.compare.equal-different-scale", "static decimal.Compare(decimal, decimal)", [Text("1.20"), Text("1.2")], Number(0)),
        Success("decimal.compare.large-exact", "static decimal.Compare(decimal, decimal)", [Text("9007199254740993"), Text("9007199254740992")], Number(1)),
        Success("decimal.compare-to-object.null", "decimal.CompareTo(object)", [Text("1"), Null()], Number(1)),
        Success("decimal.compare-to-object.decimal", "decimal.CompareTo(object)", [Text("-2"), Text("1")], Number(-1)),
        Failure("decimal.compare-to-object.wrong-type", "decimal.CompareTo(object)", [Text("1"), Number(1)], "ArgumentException"),
        Success("decimal.divide.exact", "static decimal.Divide(decimal, decimal)", [Text("1.20"), Text("2.0")], Text("0.6")),
        Success("decimal.divide.repeating", "static decimal.Divide(decimal, decimal)", [Text("1"), Text("3")], Text("0.3333333333333333333333333333")),
        Failure("decimal.divide.zero", "static decimal.Divide(decimal, decimal)", [Text("1"), Text("0.00")], "DivideByZeroException"),
        Success("decimal.equals-object.same-value", "override decimal.Equals(object)", [Text("1.20"), Text("1.2")], Bool(true)),
        Success("decimal.equals-object.wrong-type", "override decimal.Equals(object)", [Text("1.20"), Number(1.2)], Bool(false)),
        Success("decimal.equals-static.same-value", "static decimal.Equals(decimal, decimal)", [Text("1.20"), Text("1.2")], Bool(true)),
        Success("decimal.floor.negative", "static decimal.Floor(decimal)", [Text("-1.20")], Text("-2")),
        Success("decimal.to-string.preserves-scale", "override decimal.ToString()", [Text("123.4500")], Text("123.4500")),
        Success("decimal.to-string.general-preserves-scale", "decimal.ToString(string)", [Text("123.4500"), Text("G")], Text("123.4500")),
        Success("decimal.to-string.fixed-rounding", "decimal.ToString(string, System.IFormatProvider)", [Text("123.456"), Text("F2"), Text("")], Text("123.46")),
        Success("decimal.to-string.number-grouping", "decimal.ToString(string, System.IFormatProvider)", [Text("1234.5"), Text("N2"), Text("")], Text("1,234.50")),
        Success("decimal.to-string.custom", "decimal.ToString(string, System.IFormatProvider)", [Text("123.456"), Text("0.00"), Text("")], Text("123.46")),
        Failure("decimal.to-string.invalid-format", "decimal.ToString(string)", [Text("1.2"), Text("X")], "FormatException"),
        Success("decimal.parse.preserves-scale", "static decimal.Parse(string)", [Text("  123.4500  ")], Text("123.4500")),
        Success("decimal.parse.reduces-only-unrepresentable-scale", "static decimal.Parse(string)", [Text("1.23000000000000000000000000000")], Text("1.2300000000000000000000000000")),
        Success("decimal.parse.preserves-scaled-zero", "static decimal.Parse(string)", [Text("0.00")], Text("0.00")),
        Success("decimal.parse.allows-thousands", "static decimal.Parse(string)", [Text("1,234")], Text("1234")),
        Failure("decimal.parse.rejects-default-exponent", "static decimal.Parse(string)", [Text("1e2")], "FormatException"),
        Failure("decimal.parse.null", "static decimal.Parse(string)", [Null()], "ArgumentNullException"),
        Success("decimal.parse.float-style-exponent", "static decimal.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)", [Text("1e2"), Number(FloatStyle), Text("")], Text("100")),
        Failure("decimal.parse.number-style-exponent", "static decimal.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)", [Text("1e2"), Number(NumberStyle), Text("")], "FormatException"),
        Failure("decimal.parse.hex-style", "static decimal.Parse(string, System.Globalization.NumberStyles)", [Text("10"), Number(512)], "ArgumentException"),
        Success("decimal.parse.explicit-german-provider", "static decimal.Parse(string, System.IFormatProvider)", [Text("1.234,50"), Text("de-DE")], Text("1234.50")),
        Success("decimal.try-parse.valid", "static decimal.TryParse(string, out decimal)", [Text("123.4500"), Text("9")], Array(Bool(true), Text("123.4500"))),
        Success("decimal.try-parse.invalid", "static decimal.TryParse(string, out decimal)", [Text("1e2"), Text("9")], Array(Bool(false), Text("0"))),
        Success("decimal.try-parse.null", "static decimal.TryParse(string, out decimal)", [Null(), Text("9")], Array(Bool(false), Text("0"))),
        Success("decimal.try-parse.float-style", "static decimal.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)", [Text("1e2"), Number(FloatStyle), Text(""), Text("9")], Array(Bool(true), Text("100"))),
        Success("decimal.remainder.preserves-scale", "static decimal.Remainder(decimal, decimal)", [Text("5.50"), Text("2.0")], Text("1.50")),
        Failure("decimal.remainder.zero", "static decimal.Remainder(decimal, decimal)", [Text("5.50"), Text("0")], "DivideByZeroException"),
        Success("decimal.multiply.combines-scale", "static decimal.Multiply(decimal, decimal)", [Text("1.20"), Text("2.0")], Text("2.400")),
        Success("decimal.multiply.reduces-scale-to-fit", "static decimal.Multiply(decimal, decimal)", [Text(MaxValue), Text("1.0")], Text(MaxValue)),
        Success("decimal.negate.preserves-scale", "static decimal.Negate(decimal)", [Text("1.20")], Text("-1.20")),
        Success("decimal.negate.preserves-zero-scale", "static decimal.Negate(decimal)", [Text("0.00")], Text("0.00")),
        Success("decimal.round.midpoint-to-even", "static decimal.Round(decimal)", [Text("2.5")], Text("2")),
        Success("decimal.round.scale", "static decimal.Round(decimal, int)", [Text("1.2350"), Number(2)], Text("1.24")),
        Success("decimal.round.no-scale-increase", "static decimal.Round(decimal, int)", [Text("1.20"), Number(4)], Text("1.20")),
        Success("decimal.round.away-from-zero", "static decimal.Round(decimal, System.MidpointRounding)", [Text("-2.5"), Number(1)], Text("-3")),
        Success("decimal.round.toward-zero", "static decimal.Round(decimal, int, System.MidpointRounding)", [Text("1.239"), Number(2), Number(2)], Text("1.23")),
        Failure("decimal.round.invalid-scale", "static decimal.Round(decimal, int)", [Text("1.2"), Number(29)], "ArgumentOutOfRangeException"),
        Failure("decimal.round.invalid-mode", "static decimal.Round(decimal, System.MidpointRounding)", [Text("1.2"), Number(5)], "ArgumentException"),
        Success("decimal.subtract.preserves-scale", "static decimal.Subtract(decimal, decimal)", [Text("1.20"), Text("1.3")], Text("-0.10")),
        Success("decimal.to-byte.truncates", "static decimal.ToByte(decimal)", [Text("255.9")], Number(255)),
        Failure("decimal.to-byte.overflow", "static decimal.ToByte(decimal)", [Text("256")], "OverflowException"),
        Success("decimal.to-int32.truncates-negative", "static decimal.ToInt32(decimal)", [Text("-123.9")], Number(-123)),
        Success("decimal.to-int64.large", "static decimal.ToInt64(decimal)", [Text("9007199254740993.9")], Big(9007199254740993)),
        Success("decimal.to-double", "static decimal.ToDouble(decimal)", [Text("1.25")], Number(1.25)),
        Success("decimal.implicit-long", "static decimal.implicit operator decimal(long)", [Big(9007199254740993)], Text("9007199254740993")),
        Success("decimal.unary-plus.preserves-scale", "static decimal.operator +(decimal)", [Text("1.20")], Text("1.20")),
        Success("decimal.increment", "static decimal.operator ++(decimal)", [Text("1.20")], Text("2.20")),
        Success("decimal.operator-equality", "static decimal.operator ==(decimal, decimal)", [Text("1.20"), Text("1.2")], Bool(true)),
        Success("decimal.clamp.below-minimum", "static decimal.Clamp(decimal, decimal, decimal)", [Text("-1"), Text("0.00"), Text("10.0")], Text("0.00")),
        Success("decimal.clamp.within-range", "static decimal.Clamp(decimal, decimal, decimal)", [Text("5.50"), Text("0"), Text("10")], Text("5.50")),
        Failure("decimal.clamp.invalid-range", "static decimal.Clamp(decimal, decimal, decimal)", [Text("1"), Text("2"), Text("0")], "ArgumentException"),
        Success("decimal.copy-sign.preserves-scale", "static decimal.CopySign(decimal, decimal)", [Text("1.20"), Text("-1")], Text("-1.20")),
        Success("decimal.max.tie-preserves-first", "static decimal.Max(decimal, decimal)", [Text("1.20"), Text("1.2")], Text("1.20")),
        Success("decimal.min.tie-selects-second", "static decimal.Min(decimal, decimal)", [Text("1.20"), Text("1.2")], Text("1.2")),
        Success("decimal.sign.negative", "static decimal.Sign(decimal)", [Text("-0.01")], Number(-1)),
        Success("decimal.abs.preserves-scale", "static decimal.Abs(decimal)", [Text("-1.20")], Text("1.20")),
        Success("decimal.is-canonical.normalized", "static decimal.IsCanonical(decimal)", [Text("1.2")], Bool(true)),
        Success("decimal.is-canonical.trailing-zero", "static decimal.IsCanonical(decimal)", [Text("1.20")], Bool(false)),
        Success("decimal.is-canonical.scaled-zero", "static decimal.IsCanonical(decimal)", [Text("0.00")], Bool(false)),
        Success("decimal.is-integer.scaled", "static decimal.IsInteger(decimal)", [Text("1.00")], Bool(true)),
        Success("decimal.is-integer.fraction", "static decimal.IsInteger(decimal)", [Text("1.01")], Bool(false)),
        Success("decimal.is-even-integer.scaled", "static decimal.IsEvenInteger(decimal)", [Text("2.00")], Bool(true)),
        Success("decimal.is-even-integer.fraction", "static decimal.IsEvenInteger(decimal)", [Text("2.50")], Bool(false)),
        Success("decimal.is-odd-integer.scaled", "static decimal.IsOddInteger(decimal)", [Text("3.00")], Bool(true)),
        Success("decimal.is-positive-zero", "static decimal.IsPositive(decimal)", [Text("0.00")], Bool(true)),
        Success("decimal.max-magnitude.tie-selects-positive", "static decimal.MaxMagnitude(decimal, decimal)", [Text("-1.20"), Text("1.2")], Text("1.2")),
        Success("decimal.min-magnitude.tie-selects-negative", "static decimal.MinMagnitude(decimal, decimal)", [Text("1.20"), Text("-1.2")], Text("-1.2"))
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

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Big(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
