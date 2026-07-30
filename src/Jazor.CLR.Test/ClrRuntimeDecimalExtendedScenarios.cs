namespace Jazor.CLR.Test;

internal static class ClrRuntimeDecimalExtendedScenarios
{
    private const string ModulePath = "System/DecimalModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("decimal.ctor.float-finite", "decimal.Decimal(float)", [Number(1.25)], Text("1.25")),
        Success("decimal.compare-to-typed", "decimal.CompareTo(decimal)", [Text("1.20"), Text("2.0")], Number(-1)),
        Success("decimal.equals-typed", "decimal.Equals(decimal)", [Text("1.20"), Text("1.2")], Bool(true)),
        Success("decimal.hash-code-normalized", "override decimal.GetHashCode()", [Text("1.20")], Number(48565)),
        Success("decimal.to-string.provider", "decimal.ToString(System.IFormatProvider)", [Text("123.4500"), Text("")], Text("123.4500")),
        Success("decimal.is-negative", "static decimal.IsNegative(decimal)", [Text("-0.01")], Bool(true)),

        Success("decimal.parse.span-style-provider", "static decimal.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)", [Text("1e2"), Number(167), Text("")], Text("100")),
        Success("decimal.parse.span-provider", "static decimal.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)", [Text("123.4500"), Text("")], Text("123.4500")),
        Success("decimal.try-parse.span", "static decimal.TryParse(System.ReadOnlySpan<char>, out decimal)", [Text("123.4500"), Text("0")], Array(Bool(true), Text("123.4500"))),
        Success("decimal.try-parse.span-style-provider", "static decimal.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)", [Text("1e2"), Number(167), Text(""), Text("0")], Array(Bool(true), Text("100"))),
        Success("decimal.try-parse.string-provider", "static decimal.TryParse(string, System.IFormatProvider, out decimal)", [Text("1.234,50"), Text("de-DE"), Text("0")], Array(Bool(true), Text("1234.50"))),
        Success("decimal.try-parse.span-provider", "static decimal.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out decimal)", [Text("123.4500"), Text(""), Text("0")], Array(Bool(true), Text("123.4500"))),

        Success("decimal.to-sbyte-truncates", "static decimal.ToSByte(decimal)", [Text("-12.9")], Number(-12)),
        Success("decimal.to-int16-truncates", "static decimal.ToInt16(decimal)", [Text("-1234.5")], Number(-1234)),
        Success("decimal.to-uint16-truncates", "static decimal.ToUInt16(decimal)", [Text("65535.9")], Number(65535)),
        Success("decimal.to-uint32-truncates", "static decimal.ToUInt32(decimal)", [Text("4294967295.9")], Number(4294967295)),
        Success("decimal.to-uint64-exact", "static decimal.ToUInt64(decimal)", [Text("9007199254740993.9")], Big(9007199254740993)),
        Success("decimal.to-single", "static decimal.ToSingle(decimal)", [Text("1.25")], Number(1.25)),
        Success("decimal.truncate-preserves-integral-value", "static decimal.Truncate(decimal)", [Text("-123.90")], Text("-123")),

        Success("decimal.implicit-byte", "static decimal.implicit operator decimal(byte)", [Number(255)], Text("255")),
        Success("decimal.implicit-sbyte", "static decimal.implicit operator decimal(sbyte)", [Number(-128)], Text("-128")),
        Success("decimal.implicit-short", "static decimal.implicit operator decimal(short)", [Number(-32768)], Text("-32768")),
        Success("decimal.implicit-ushort", "static decimal.implicit operator decimal(ushort)", [Number(65535)], Text("65535")),
        Success("decimal.implicit-char", "static decimal.implicit operator decimal(char)", [Number(65)], Text("65")),
        Success("decimal.implicit-int", "static decimal.implicit operator decimal(int)", [Number(-2147483648)], Text("-2147483648")),
        Success("decimal.implicit-uint", "static decimal.implicit operator decimal(uint)", [Number(4294967295)], Text("4294967295")),
        Success("decimal.implicit-ulong", "static decimal.implicit operator decimal(ulong)", [Big(9007199254740993)], Text("9007199254740993")),

        Success("decimal.explicit-decimal-from-float", "static decimal.explicit operator decimal(float)", [Number(1.25)], Text("1.25")),
        Success("decimal.explicit-decimal-from-double", "static decimal.explicit operator decimal(double)", [Number(1.5)], Text("1.5")),
        Success("decimal.explicit-byte", "static decimal.explicit operator byte(decimal)", [Text("255.9")], Number(255)),
        Success("decimal.explicit-sbyte", "static decimal.explicit operator sbyte(decimal)", [Text("-128.9")], Number(-128)),
        Success("decimal.explicit-char", "static decimal.explicit operator char(decimal)", [Text("65.9")], Number(65)),
        Success("decimal.explicit-short", "static decimal.explicit operator short(decimal)", [Text("-32768.9")], Number(-32768)),
        Success("decimal.explicit-ushort", "static decimal.explicit operator ushort(decimal)", [Text("65535.9")], Number(65535)),
        Success("decimal.explicit-int", "static decimal.explicit operator int(decimal)", [Text("-2147483648.9")], Number(-2147483648)),
        Success("decimal.explicit-uint", "static decimal.explicit operator uint(decimal)", [Text("4294967295.9")], Number(4294967295)),
        Success("decimal.explicit-long", "static decimal.explicit operator long(decimal)", [Text("9007199254740993.9")], Big(9007199254740993)),
        Success("decimal.explicit-ulong", "static decimal.explicit operator ulong(decimal)", [Text("9007199254740993.9")], Big(9007199254740993)),
        Success("decimal.explicit-float", "static decimal.explicit operator float(decimal)", [Text("1.25")], Number(1.25)),
        Success("decimal.explicit-double", "static decimal.explicit operator double(decimal)", [Text("1.25")], Number(1.25)),

        Success("decimal.operator-unary-minus", "static decimal.operator -(decimal)", [Text("1.20")], Text("-1.20")),
        Success("decimal.operator-decrement", "static decimal.operator --(decimal)", [Text("1.20")], Text("0.20")),
        Success("decimal.operator-add", "static decimal.operator +(decimal, decimal)", [Text("1.20"), Text("2.3")], Text("3.50")),
        Success("decimal.operator-subtract", "static decimal.operator -(decimal, decimal)", [Text("1.20"), Text("2.3")], Text("-1.10")),
        Success("decimal.operator-multiply", "static decimal.operator *(decimal, decimal)", [Text("1.20"), Text("2.0")], Text("2.400")),
        Success("decimal.operator-divide", "static decimal.operator /(decimal, decimal)", [Text("1.20"), Text("2.0")], Text("0.6")),
        Success("decimal.operator-remainder", "static decimal.operator %(decimal, decimal)", [Text("5.50"), Text("2.0")], Text("1.50")),
        Success("decimal.operator-not-equal", "static decimal.operator !=(decimal, decimal)", [Text("1.20"), Text("1.3")], Bool(true)),
        Success("decimal.operator-less-than", "static decimal.operator <(decimal, decimal)", [Text("1.2"), Text("1.3")], Bool(true)),
        Success("decimal.operator-less-than-or-equal", "static decimal.operator <=(decimal, decimal)", [Text("1.20"), Text("1.2")], Bool(true)),
        Success("decimal.operator-greater-than", "static decimal.operator >(decimal, decimal)", [Text("1.3"), Text("1.2")], Bool(true)),
        Success("decimal.operator-greater-than-or-equal", "static decimal.operator >=(decimal, decimal)", [Text("1.20"), Text("1.2")], Bool(true))
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Big(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
