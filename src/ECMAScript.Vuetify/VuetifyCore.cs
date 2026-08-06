namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 核心入口，提供 createVuetify 工厂方法。
/// Vuetify core entry point providing the createVuetify factory.
/// </summary>
[ECMAScript("vuetify")]
[Description("@#")]
public static class Vuetify
{
    [Description("@#createVuetify")]
    public extern static VuetifyPlugin CreateVuetify();

    [Description("@#createVuetify")]
    public extern static VuetifyPlugin CreateVuetify(VuetifyOptions options);
}

/// <summary>
/// Vuetify 组件的标记接口。
/// Marker interface for Vuetify components.
/// </summary>
[ECMAScript]
public interface IVuetifyComponent : IVueComponent { }

/// <summary>
/// Vuetify 插件实例。
/// Vuetify plugin instance.
/// </summary>
[ECMAScript]
public sealed record VuetifyPlugin : VuePlugin
{
    private VuetifyPlugin()
    {
    }
}

/// <summary>
/// Vuetify 插件配置选项。
/// Vuetify plugin configuration options.
/// </summary>
[ECMAScript]
[Description("@#VuetifyOptions")]
public sealed record VuetifyOptions : VuePluginOptions
{
    [Description("@#components")]
    public VueComponentRegistry? Components { get; init; }

    [Description("@#directives")]
    public VueDirectiveRegistry? Directives { get; init; }

    [Description("@#display")]
    public VuetifyDisplayOptions? Display { get; init; }

    [Description("@#theme")]
    public VuetifyThemeOptions? Theme { get; init; }

    [Description("@#icons")]
    public VuetifyIconOptions? Icons { get; init; }

    [Description("@#locale")]
    public VuetifyLocaleOptions? Locale { get; init; }

    [Description("@#date")]
    public VuetifyDateOptions? Date { get; init; }

    [Description("@#ssr")]
    public bool? Ssr { get; init; }
}

/// <summary>
/// Vuetify 主题配置选项。
/// Vuetify theme configuration options.
/// </summary>
[ECMAScript]
[Description("@#VuetifyThemeOptions")]
public sealed record VuetifyThemeOptions
{
    [Description("@#defaultTheme")]
    public string? DefaultTheme { get; init; }

    [Description("@#variations")]
    public VuetifyThemeVariationOptions? Variations { get; init; }
}

/// <summary>
/// Vuetify 主题变体配置选项。
/// Vuetify theme variation options.
/// </summary>
[ECMAScript]
[Description("@#VuetifyThemeVariationOptions")]
public sealed record VuetifyThemeVariationOptions
{
    [Description("@#colors")]
    public string[]? Colors { get; init; }

    [Description("@#lighten")]
    public int? Lighten { get; init; }

    [Description("@#darken")]
    public int? Darken { get; init; }
}

/// <summary>
/// Vuetify 显示配置选项。
/// Vuetify display configuration options.
/// </summary>
[ECMAScript]
[Description("@#VuetifyDisplayOptions")]
public sealed record VuetifyDisplayOptions
{
    [Description("@#mobileBreakpoint")]
    public VuetifyDisplayBreakpoint? MobileBreakpoint { get; init; }

    [Description("@#thresholds")]
    public VuetifyDisplayThresholds? Thresholds { get; init; }
}

/// <summary>
/// Vuetify 显示断点阈值配置。
/// Vuetify display breakpoint thresholds.
/// </summary>
[ECMAScript]
[Description("@#VuetifyDisplayThresholds")]
public sealed record VuetifyDisplayThresholds
{
    [Description("@#xs")]
    public int? Xs { get; init; }

    [Description("@#sm")]
    public int? Sm { get; init; }

    [Description("@#md")]
    public int? Md { get; init; }

    [Description("@#lg")]
    public int? Lg { get; init; }

    [Description("@#xl")]
    public int? Xl { get; init; }

    [Description("@#xxl")]
    public int? Xxl { get; init; }
}

/// <summary>
/// Vuetify 图标配置选项。
/// Vuetify icon configuration options.
/// </summary>
[ECMAScript]
[Description("@#VuetifyIconOptions")]
public sealed record VuetifyIconOptions
{
    [Description("@#defaultSet")]
    public string? DefaultSet { get; init; }
}

/// <summary>
/// Vuetify 区域设置配置选项。
/// Vuetify locale configuration options.
/// </summary>
[ECMAScript]
[Description("@#VuetifyLocaleOptions")]
public sealed record VuetifyLocaleOptions
{
    [Description("@#locale")]
    public string? Locale { get; init; }

    [Description("@#fallback")]
    public string? Fallback { get; init; }
}

/// <summary>
/// Vuetify 日期配置选项。
/// Vuetify date configuration options.
/// </summary>
[ECMAScript]
[Description("@#VuetifyDateOptions")]
public sealed record VuetifyDateOptions
{
    [Description("@#locale")]
    public string? Locale { get; init; }
}
