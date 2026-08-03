using System.Numerics;

namespace Jazor.CLR.Test;

internal static class ClrRuntimeBigIntegerScenarios
{
    private const string ModulePath = "System/Numerics/BigIntegerModule.js";
    private static readonly BigInteger TwoPow100 = BigInteger.One << 100;
    private static readonly BigInteger TenPow20 = BigInteger.Pow(10, 20);
    private static readonly BigInteger DecimalMax = BigInteger.Parse("79228162514264337593543950335");
    private static readonly BigInteger Int128Min = BigInteger.Parse("-170141183460469231731687303715884105728");
    private static readonly BigInteger Int128Max = BigInteger.Parse("170141183460469231731687303715884105727");
    private static readonly BigInteger UInt128Max = BigInteger.Parse("340282366920938463463374607431768211455");

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("big-integer.sign.negative", "System.Numerics.BigInteger.Sign.get", [Big(-99)], Number(-1)),
        Success("big-integer.sign.zero", "System.Numerics.BigInteger.Sign.get", [Big(0)], Number(0)),
        Success("big-integer.sign.positive", "System.Numerics.BigInteger.Sign.get", [Big(99)], Number(1)),
        Success("big-integer.hash-code.zero", "override System.Numerics.BigInteger.GetHashCode()", [Big(0)], Number(575)),
        Success("big-integer.parse.large-decimal", "static System.Numerics.BigInteger.Parse(string)", [Text(TwoPow100.ToString())], Big(TwoPow100)),
        Success("big-integer.parse.signed-whitespace", "static System.Numerics.BigInteger.Parse(string)", [Text("  +123  ")], Big(123)),
        Failure("big-integer.parse.javascript-hex-is-invalid", "static System.Numerics.BigInteger.Parse(string)", [Text("0x10")], "FormatException"),
        Failure("big-integer.parse.group-separator-is-invalid", "static System.Numerics.BigInteger.Parse(string)", [Text("1,000")], "FormatException"),
        Failure("big-integer.parse.fraction-is-invalid", "static System.Numerics.BigInteger.Parse(string)", [Text("1.0")], "FormatException"),
        Failure("big-integer.parse.null", "static System.Numerics.BigInteger.Parse(string)", [Null()], "ArgumentNullException"),
        Success(
            "big-integer.try-parse.large-decimal",
            "static System.Numerics.BigInteger.TryParse(string, out System.Numerics.BigInteger)",
            [Text(TwoPow100.ToString()), Big(7)],
            Array(Bool(true), Big(TwoPow100))),
        Success(
            "big-integer.try-parse.javascript-hex-is-invalid",
            "static System.Numerics.BigInteger.TryParse(string, out System.Numerics.BigInteger)",
            [Text("0x10"), Big(7)],
            Array(Bool(false), Big(0))),
        Success(
            "big-integer.try-parse.whitespace",
            "static System.Numerics.BigInteger.TryParse(string, out System.Numerics.BigInteger)",
            [Text("   "), Big(7)],
            Array(Bool(false), Big(0))),
        Success(
            "big-integer.try-parse.null",
            "static System.Numerics.BigInteger.TryParse(string, out System.Numerics.BigInteger)",
            [Null(), Big(7)],
            Array(Bool(false), Big(0))),
        Success(
            "big-integer.try-parse.span-large-decimal",
            "static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, out System.Numerics.BigInteger)",
            [Text(TwoPow100.ToString()), Big(7)],
            Array(Bool(true), Big(TwoPow100))),
        Success(
            "big-integer.div-rem.out-negative-dividend",
            "static System.Numerics.BigInteger.DivRem(System.Numerics.BigInteger, System.Numerics.BigInteger, out System.Numerics.BigInteger)",
            [Big(-17), Big(5), Big(0)],
            Array(Big(-3), Big(-2))),
        Failure(
            "big-integer.div-rem.out-zero-divisor",
            "static System.Numerics.BigInteger.DivRem(System.Numerics.BigInteger, System.Numerics.BigInteger, out System.Numerics.BigInteger)",
            [Big(17), Big(0), Big(0)],
            "DivideByZeroException"),
        Success(
            "big-integer.div-rem.tuple-negative-dividend",
            "static System.Numerics.BigInteger.DivRem(System.Numerics.BigInteger, System.Numerics.BigInteger)",
            [Big(-17), Big(5)],
            Array(Big(-3), Big(-2))),
        Failure(
            "big-integer.div-rem.tuple-zero-divisor",
            "static System.Numerics.BigInteger.DivRem(System.Numerics.BigInteger, System.Numerics.BigInteger)",
            [Big(17), Big(0)],
            "DivideByZeroException"),
        Success("big-integer.log.zero", "static System.Numerics.BigInteger.Log(System.Numerics.BigInteger)", [Big(0)], Number(double.NegativeInfinity)),
        Success("big-integer.log.negative", "static System.Numerics.BigInteger.Log(System.Numerics.BigInteger)", [Big(-1)], Number(double.NaN)),
        Success("big-integer.log.base-two-large", "static System.Numerics.BigInteger.Log(System.Numerics.BigInteger, double)", [Big(TwoPow100), Number(2)], Number(100)),
        Success("big-integer.log.invalid-zero-base", "static System.Numerics.BigInteger.Log(System.Numerics.BigInteger, double)", [Big(10), Number(0)], Number(double.NaN)),
        Success("big-integer.log10.large-power", "static System.Numerics.BigInteger.Log10(System.Numerics.BigInteger)", [Big(TenPow20)], Number(20)),
        Success("big-integer.gcd.signed-operands", "static System.Numerics.BigInteger.GreatestCommonDivisor(System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(-48), Big(18)], Big(6)),
        Success("big-integer.gcd.both-zero", "static System.Numerics.BigInteger.GreatestCommonDivisor(System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(0), Big(0)], Big(0)),
        Success("big-integer.mod-pow.negative-base-odd-exponent", "static System.Numerics.BigInteger.ModPow(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(-3), Big(3), Big(5)], Big(-2)),
        Success("big-integer.mod-pow.negative-modulus", "static System.Numerics.BigInteger.ModPow(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(-3), Big(3), Big(-5)], Big(-2)),
        Success("big-integer.mod-pow.zero-exponent", "static System.Numerics.BigInteger.ModPow(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(9), Big(0), Big(7)], Big(1)),
        Failure("big-integer.mod-pow.negative-exponent", "static System.Numerics.BigInteger.ModPow(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(2), Big(-1), Big(5)], "ArgumentOutOfRangeException"),
        Failure("big-integer.mod-pow.zero-modulus", "static System.Numerics.BigInteger.ModPow(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(2), Big(3), Big(0)], "DivideByZeroException"),
        Success("big-integer.pow.large-result", "static System.Numerics.BigInteger.Pow(System.Numerics.BigInteger, int)", [Big(2), Number(100)], Big(TwoPow100)),
        Success("big-integer.pow.zero-exponent", "static System.Numerics.BigInteger.Pow(System.Numerics.BigInteger, int)", [Big(-99), Number(0)], Big(1)),
        Failure("big-integer.pow.negative-exponent", "static System.Numerics.BigInteger.Pow(System.Numerics.BigInteger, int)", [Big(2), Number(-1)], "ArgumentOutOfRangeException"),
        Success("big-integer.compare-object.null", "System.Numerics.BigInteger.CompareTo(object)", [Big(1), Null()], Number(1)),
        Success("big-integer.compare-object.big-int", "System.Numerics.BigInteger.CompareTo(object)", [Big(-5), Big(7)], Number(-1)),
        Failure("big-integer.compare-object.wrong-type", "System.Numerics.BigInteger.CompareTo(object)", [Big(1), Text("1")], "ArgumentException"),
        Success("big-integer.bit-length.zero", "System.Numerics.BigInteger.GetBitLength()", [Big(0)], Big(0)),
        Success("big-integer.bit-length.negative", "System.Numerics.BigInteger.GetBitLength()", [Big(-3)], Big(2)),
        Success("big-integer.bit-length.large", "System.Numerics.BigInteger.GetBitLength()", [Big(TwoPow100)], Big(101)),
        Success("big-integer.leading-zero-count.zero", "static System.Numerics.BigInteger.LeadingZeroCount(System.Numerics.BigInteger)", [Big(0)], Big(32)),
        Success("big-integer.leading-zero-count.word-boundary", "static System.Numerics.BigInteger.LeadingZeroCount(System.Numerics.BigInteger)", [Big(BigInteger.One << 32)], Big(31)),
        Success("big-integer.pop-count.positive", "static System.Numerics.BigInteger.PopCount(System.Numerics.BigInteger)", [Big(255)], Big(8)),
        Success("big-integer.pop-count.negative-one", "static System.Numerics.BigInteger.PopCount(System.Numerics.BigInteger)", [Big(-1)], Big(32)),
        Success("big-integer.pop-count.negative-value", "static System.Numerics.BigInteger.PopCount(System.Numerics.BigInteger)", [Big(-8)], Big(29)),
        Success("big-integer.trailing-zero-count.zero", "static System.Numerics.BigInteger.TrailingZeroCount(System.Numerics.BigInteger)", [Big(0)], Big(32)),
        Success("big-integer.trailing-zero-count.negative", "static System.Numerics.BigInteger.TrailingZeroCount(System.Numerics.BigInteger)", [Big(-8)], Big(3)),
        Success("big-integer.trailing-zero-count.large", "static System.Numerics.BigInteger.TrailingZeroCount(System.Numerics.BigInteger)", [Big(TwoPow100)], Big(100)),
        Success("big-integer.is-pow2.large", "static System.Numerics.BigInteger.IsPow2(System.Numerics.BigInteger)", [Big(TwoPow100)], Bool(true)),
        Success("big-integer.is-pow2.non-power", "static System.Numerics.BigInteger.IsPow2(System.Numerics.BigInteger)", [Big(12)], Bool(false)),
        Success("big-integer.is-pow2.negative", "static System.Numerics.BigInteger.IsPow2(System.Numerics.BigInteger)", [Big(-8)], Bool(false)),
        Success("big-integer.log2.zero", "static System.Numerics.BigInteger.Log2(System.Numerics.BigInteger)", [Big(0)], Big(0)),
        Success("big-integer.log2.floor", "static System.Numerics.BigInteger.Log2(System.Numerics.BigInteger)", [Big(255)], Big(7)),
        Success("big-integer.log2.large", "static System.Numerics.BigInteger.Log2(System.Numerics.BigInteger)", [Big(TwoPow100)], Big(100)),
        Failure("big-integer.log2.negative", "static System.Numerics.BigInteger.Log2(System.Numerics.BigInteger)", [Big(-1)], "ArgumentOutOfRangeException"),
        Success("big-integer.clamp.below-minimum", "static System.Numerics.BigInteger.Clamp(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(-10), Big(0), Big(5)], Big(0)),
        Success("big-integer.clamp.within-range", "static System.Numerics.BigInteger.Clamp(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(3), Big(0), Big(5)], Big(3)),
        Success("big-integer.clamp.above-maximum", "static System.Numerics.BigInteger.Clamp(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(10), Big(0), Big(5)], Big(5)),
        Failure("big-integer.clamp.invalid-range", "static System.Numerics.BigInteger.Clamp(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(1), Big(2), Big(0)], "ArgumentException"),
        Success("big-integer.max-magnitude.tie-prefers-positive", "static System.Numerics.BigInteger.MaxMagnitude(System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(-3), Big(3)], Big(3)),
        Success("big-integer.max-magnitude.dominant-negative", "static System.Numerics.BigInteger.MaxMagnitude(System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(-9), Big(4)], Big(-9)),
        Success("big-integer.min-magnitude.tie-prefers-negative", "static System.Numerics.BigInteger.MinMagnitude(System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(3), Big(-3)], Big(-3)),
        Success("big-integer.min-magnitude.smaller-positive", "static System.Numerics.BigInteger.MinMagnitude(System.Numerics.BigInteger, System.Numerics.BigInteger)", [Big(-9), Big(4)], Big(4)),
        Success("big-integer.ctor-float.truncates", "System.Numerics.BigInteger.BigInteger(float)", [Number(-123.75)], Big(-123)),
        Success("big-integer.ctor-double.truncates", "System.Numerics.BigInteger.BigInteger(double)", [Number(123.75)], Big(123)),
        Failure("big-integer.ctor-double.rejects-nan", "System.Numerics.BigInteger.BigInteger(double)", [Number(double.NaN)], "OverflowException"),
        Success("big-integer.ctor-decimal.truncates", "System.Numerics.BigInteger.BigInteger(decimal)", [Text("-123.75")], Big(-123)),
        Success("big-integer.from-double.truncates", "static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(double)", [Number(-17.875)], Big(-17)),
        Failure("big-integer.from-double.rejects-infinity", "static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(double)", [Number(double.PositiveInfinity)], "OverflowException"),
        Success("big-integer.from-float.truncates", "static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(float)", [Number(17.875)], Big(17)),
        Success("big-integer.from-half.truncates", "static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(System.Half)", [Number(-7.5)], Big(-7)),
        Success("big-integer.from-decimal.truncates", "static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(decimal)", [Text("17.875")], Big(17)),
        Success("big-integer.to-half.maximum-finite-boundary", "static System.Numerics.BigInteger.explicit operator System.Half(System.Numerics.BigInteger)", [Big(65519)], Number((double)Half.MaxValue)),
        Success("big-integer.to-half.overflow-boundary", "static System.Numerics.BigInteger.explicit operator System.Half(System.Numerics.BigInteger)", [Big(65520)], Number(double.PositiveInfinity)),
        Success("big-integer.to-byte.maximum", "static System.Numerics.BigInteger.explicit operator byte(System.Numerics.BigInteger)", [Big(255)], Number(255)),
        Failure("big-integer.to-byte.overflow", "static System.Numerics.BigInteger.explicit operator byte(System.Numerics.BigInteger)", [Big(256)], "OverflowException"),
        Success("big-integer.to-char.maximum", "static System.Numerics.BigInteger.explicit operator char(System.Numerics.BigInteger)", [Big(65535)], Number(65535)),
        Success("big-integer.to-sbyte.minimum", "static System.Numerics.BigInteger.explicit operator sbyte(System.Numerics.BigInteger)", [Big(-128)], Number(-128)),
        Success("big-integer.to-short.minimum", "static System.Numerics.BigInteger.explicit operator short(System.Numerics.BigInteger)", [Big(-32768)], Number(-32768)),
        Success("big-integer.to-ushort.maximum", "static System.Numerics.BigInteger.explicit operator ushort(System.Numerics.BigInteger)", [Big(65535)], Number(65535)),
        Success("big-integer.to-int.maximum", "static System.Numerics.BigInteger.explicit operator int(System.Numerics.BigInteger)", [Big(2147483647)], Number(2147483647)),
        Success("big-integer.to-uint.maximum", "static System.Numerics.BigInteger.explicit operator uint(System.Numerics.BigInteger)", [Big(BigInteger.Parse("4294967295"))], Number(4294967295)),
        Success("big-integer.to-long.maximum", "static System.Numerics.BigInteger.explicit operator long(System.Numerics.BigInteger)", [Big(long.MaxValue)], Big(long.MaxValue)),
        Failure("big-integer.to-long.overflow", "static System.Numerics.BigInteger.explicit operator long(System.Numerics.BigInteger)", [Big((BigInteger)long.MaxValue + 1)], "OverflowException"),
        Success("big-integer.to-ulong.maximum", "static System.Numerics.BigInteger.explicit operator ulong(System.Numerics.BigInteger)", [Big(BigInteger.Parse("18446744073709551615"))], Big(BigInteger.Parse("18446744073709551615"))),
        Success("big-integer.to-int128.minimum", "static System.Numerics.BigInteger.explicit operator System.Int128(System.Numerics.BigInteger)", [Big(Int128Min)], Big(Int128Min)),
        Failure("big-integer.to-int128.overflow", "static System.Numerics.BigInteger.explicit operator System.Int128(System.Numerics.BigInteger)", [Big(Int128Max + 1)], "OverflowException"),
        Success("big-integer.to-uint128.maximum", "static System.Numerics.BigInteger.explicit operator System.UInt128(System.Numerics.BigInteger)", [Big(UInt128Max)], Big(UInt128Max)),
        Failure("big-integer.to-uint128.negative-overflow", "static System.Numerics.BigInteger.explicit operator System.UInt128(System.Numerics.BigInteger)", [Big(-1)], "OverflowException"),
        Success("big-integer.to-decimal.maximum", "static System.Numerics.BigInteger.explicit operator decimal(System.Numerics.BigInteger)", [Big(DecimalMax)], Text(DecimalMax.ToString())),
        Failure("big-integer.to-decimal.overflow", "static System.Numerics.BigInteger.explicit operator decimal(System.Numerics.BigInteger)", [Big(DecimalMax + 1)], "OverflowException")
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
    private static ClrRuntimeValue Big(BigInteger value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
