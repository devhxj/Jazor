using System.ComponentModel;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 显示断点值。
/// Vuetify display breakpoint value.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyDisplayBreakpoint(string, Number)
{
    public string? AsString => Value as string;

    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public static implicit operator VuetifyDisplayBreakpoint(string value)
        => new(value);

    public static implicit operator VuetifyDisplayBreakpoint(Number value)
        => new(value);
}
