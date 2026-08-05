namespace ECMAScript.Vuetify;

/// <summary>
/// 网格跨度值，支持布尔、数字或字符串表示。
/// Grid span value supporting boolean, numeric, or string representation.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyGridSpanValue(bool, Number, string)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public string? AsString => Value as string;

    public static implicit operator VuetifyGridSpanValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyGridSpanValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyGridSpanValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyGridSpanValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyGridSpanValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyGridSpanValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyGridSpanValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyGridSpanValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyGridSpanValue(decimal value)
        => new((Number)value);
}
