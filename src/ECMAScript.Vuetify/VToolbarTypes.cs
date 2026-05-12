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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyToolbarDensityValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly VuetifyToolbarDensity? _toolbarDensity;
    private readonly VuetifyDensity? _density;

    private VuetifyToolbarDensityValue(VuetifyToolbarDensity value)
    {
        _kind = 1;
        _toolbarDensity = value;
        _density = default;
    }

    private VuetifyToolbarDensityValue(VuetifyDensity value)
    {
        _kind = 2;
        _toolbarDensity = default;
        _density = value;
    }

    public VuetifyToolbarDensity? AsToolbarDensity => _kind == 1 ? _toolbarDensity : default;

    public VuetifyDensity? AsDensity => _kind == 2 ? _density : default;

    public object? Value => _kind switch
    {
        1 => AsToolbarDensity,
        2 => AsDensity,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyToolbarDensityValue From(VuetifyToolbarDensity value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyToolbarDensityValue From(VuetifyDensity value);

    [ECMAScriptInline("null")]
    public extern static VuetifyToolbarDensityValue Null();

    public static implicit operator VuetifyToolbarDensityValue(VuetifyToolbarDensity value)
        => new(value);

    public static implicit operator VuetifyToolbarDensityValue(VuetifyDensity value)
        => new(value);
}
