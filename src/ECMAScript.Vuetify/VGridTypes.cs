namespace ECMAScript.Vuetify;

/// <summary>
/// 网格跨度值，支持布尔、数字或字符串表示。
/// Grid span value supporting boolean, numeric, or string representation.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyGridSpanValue(bool, Number, string)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 将 byte 值转换为 VuetifyGridSpanValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGridSpanValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyGridSpanValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGridSpanValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyGridSpanValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGridSpanValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyGridSpanValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGridSpanValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyGridSpanValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGridSpanValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyGridSpanValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGridSpanValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyGridSpanValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGridSpanValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyGridSpanValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGridSpanValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyGridSpanValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGridSpanValue(decimal value)
        => new((Number)value);
}