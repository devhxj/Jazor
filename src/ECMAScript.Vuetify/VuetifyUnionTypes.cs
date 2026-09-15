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
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 将 string 值转换为 VuetifyDisplayBreakpoint，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDisplayBreakpoint(string value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifyDisplayBreakpoint，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDisplayBreakpoint(Number value)
        => new(value);
}