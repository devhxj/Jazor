namespace ECMAScript.Vuetify;

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyGridSpanValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly Number? _number;
    private readonly string? _string;

    private VuetifyGridSpanValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _number = default;
        _string = default;
    }

    private VuetifyGridSpanValue(Number value)
    {
        _kind = 2;
        _bool = default;
        _number = value;
        _string = default;
    }

    private VuetifyGridSpanValue(string value)
    {
        _kind = 3;
        _bool = default;
        _number = default;
        _string = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public string? AsString => _kind == 3 ? _string : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsNumber,
        3 => AsString,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyGridSpanValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyGridSpanValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyGridSpanValue From(string value);

    public static implicit operator VuetifyGridSpanValue(bool value)
        => new(value);

    public static implicit operator VuetifyGridSpanValue(Number value)
        => new(value);

    public static implicit operator VuetifyGridSpanValue(string value)
        => new(value);

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
