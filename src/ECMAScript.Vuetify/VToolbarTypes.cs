namespace ECMAScript.Vuetify;

/// <summary>
/// 工具栏密度枚举。
/// Toolbar density enum.
/// </summary>
[String]
public enum VuetifyToolbarDensity
{
    /// <summary>
    /// 突出显示并增加高度；上游取值为 “prominent”。
    /// </summary>
    [Description("@#prominent")]
    Prominent,

    /// <summary>
    /// 默认配置；上游取值为 “default”。
    /// </summary>
    [Description("@#default")]
    Default,

    /// <summary>
    /// 舒适的间距密度；上游取值为 “comfortable”。
    /// </summary>
    [Description("@#comfortable")]
    Comfortable,

    /// <summary>
    /// 紧凑的间距密度；上游取值为 “compact”。
    /// </summary>
    [Description("@#compact")]
    Compact
}

/// <summary>
/// 工具栏密度值的擦除值联合类型。
/// Erased value union for toolbar density values.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyToolbarDensityValue(VuetifyToolbarDensity, VuetifyDensity)
{
    /// <summary>
    /// 读取当前值的 VuetifyToolbarDensity 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyToolbarDensity? AsToolbarDensity
        => Value is VuetifyToolbarDensity value ? value : default(VuetifyToolbarDensity?);

    /// <summary>
    /// 读取当前值的 VuetifyDensity 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDensity? AsDensity
        => Value is VuetifyDensity value ? value : default(VuetifyDensity?);

    /// <summary>
    /// 构造显式的 JavaScript null 分支，用于向宿主 API 传入空值。
    /// </summary>
    [ECMAScriptInline("null")]
    public extern static VuetifyToolbarDensityValue Null();

    /// <summary>
    /// 将 VuetifyToolbarDensity 值转换为 VuetifyToolbarDensityValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyToolbarDensityValue(VuetifyToolbarDensity value)
        => new(value);

    /// <summary>
    /// 将 VuetifyDensity 值转换为 VuetifyToolbarDensityValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyToolbarDensityValue(VuetifyDensity value)
        => new(value);
}