using System.ComponentModel;

namespace ECMAScript.Vuetify;

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyDisplayBreakpoint : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Number? _number;

    private VuetifyDisplayBreakpoint(string value)
    {
        _kind = 1;
        _string = value;
        _number = default;
    }

    private VuetifyDisplayBreakpoint(Number value)
    {
        _kind = 2;
        _string = default;
        _number = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsNumber,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDisplayBreakpoint From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDisplayBreakpoint From(Number value);

    public static implicit operator VuetifyDisplayBreakpoint(string value)
        => new(value);

    public static implicit operator VuetifyDisplayBreakpoint(Number value)
        => new(value);
}
