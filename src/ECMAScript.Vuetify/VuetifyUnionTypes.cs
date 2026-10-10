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
    #region Generated union numeric conversions
    /// <summary>Accepts a numeric value through the declared Number branch; avoids two chained C# user conversions. 经现有 Number 分支接受数值。</summary>
    public static implicit operator VuetifyDisplayBreakpoint(byte value) => (VuetifyDisplayBreakpoint)(ECMAScript.Number)value;
    /// <summary>Accepts a numeric value through the declared Number branch; avoids two chained C# user conversions. 经现有 Number 分支接受数值。</summary>
    public static implicit operator VuetifyDisplayBreakpoint(decimal value) => (VuetifyDisplayBreakpoint)(ECMAScript.Number)value;
    /// <summary>Accepts a numeric value through the declared Number branch; avoids two chained C# user conversions. 经现有 Number 分支接受数值。</summary>
    public static implicit operator VuetifyDisplayBreakpoint(double value) => (VuetifyDisplayBreakpoint)(ECMAScript.Number)value;
    /// <summary>Accepts a numeric value through the declared Number branch; avoids two chained C# user conversions. 经现有 Number 分支接受数值。</summary>
    public static implicit operator VuetifyDisplayBreakpoint(float value) => (VuetifyDisplayBreakpoint)(ECMAScript.Number)value;
    /// <summary>Accepts a numeric value through the declared Number branch; avoids two chained C# user conversions. 经现有 Number 分支接受数值。</summary>
    public static implicit operator VuetifyDisplayBreakpoint(int value) => (VuetifyDisplayBreakpoint)(ECMAScript.Number)value;
    /// <summary>Accepts a numeric value through the declared Number branch; avoids two chained C# user conversions. 经现有 Number 分支接受数值。</summary>
    public static implicit operator VuetifyDisplayBreakpoint(sbyte value) => (VuetifyDisplayBreakpoint)(ECMAScript.Number)value;
    /// <summary>Accepts a numeric value through the declared Number branch; avoids two chained C# user conversions. 经现有 Number 分支接受数值。</summary>
    public static implicit operator VuetifyDisplayBreakpoint(short value) => (VuetifyDisplayBreakpoint)(ECMAScript.Number)value;
    /// <summary>Accepts a numeric value through the declared Number branch; avoids two chained C# user conversions. 经现有 Number 分支接受数值。</summary>
    public static implicit operator VuetifyDisplayBreakpoint(uint value) => (VuetifyDisplayBreakpoint)(ECMAScript.Number)value;
    /// <summary>Accepts a numeric value through the declared Number branch; avoids two chained C# user conversions. 经现有 Number 分支接受数值。</summary>
    public static implicit operator VuetifyDisplayBreakpoint(ushort value) => (VuetifyDisplayBreakpoint)(ECMAScript.Number)value;
    #endregion
}