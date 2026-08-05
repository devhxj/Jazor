namespace ECMAScript.Vuetify;

/// <summary>
/// 工具栏密度枚举。
/// Toolbar density enum.
/// </summary>
[String]
public enum VuetifyToolbarDensity
{
    [Description("@#prominent")]
    Prominent,

    [Description("@#default")]
    Default,

    [Description("@#comfortable")]
    Comfortable,

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
    public VuetifyToolbarDensity? AsToolbarDensity
        => Value is VuetifyToolbarDensity value ? value : default(VuetifyToolbarDensity?);

    public VuetifyDensity? AsDensity
        => Value is VuetifyDensity value ? value : default(VuetifyDensity?);

    [ECMAScriptInline("null")]
    public extern static VuetifyToolbarDensityValue Null();

    public static implicit operator VuetifyToolbarDensityValue(VuetifyToolbarDensity value)
        => new(value);

    public static implicit operator VuetifyToolbarDensityValue(VuetifyDensity value)
        => new(value);
}
