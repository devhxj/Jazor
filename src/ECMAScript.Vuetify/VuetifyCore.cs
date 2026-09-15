namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 核心入口，提供 createVuetify 工厂方法。
/// Vuetify core entry point providing the createVuetify factory.
/// </summary>
[ECMAScript("vuetify")]
[Description("@#")]
public static class Vuetify
{
    /// <summary>
    /// 创建 Vuetify 插件实例；通过 Vue 应用的 Use 安装，使配置的组件、主题、指令和服务可用。
    /// </summary>
    [Description("@#createVuetify")]
    public extern static VuetifyPlugin CreateVuetify();

    /// <summary>
    /// 创建 Vuetify 插件实例；通过 Vue 应用的 Use 安装，使配置的组件、主题、指令和服务可用。
    /// </summary>
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
    /// <summary>
    /// 在安装 Vuetify 插件时注册的组件映射。
    /// </summary>
    [Description("@#components")]
    public VueComponentRegistry? Components { get; init; }

    /// <summary>
    /// 在安装 Vuetify 插件时注册的指令映射。
    /// </summary>
    [Description("@#directives")]
    public VueDirectiveRegistry? Directives { get; init; }

    /// <summary>
    /// 显示断点与移动端判定的配置。
    /// </summary>
    [Description("@#display")]
    public VuetifyDisplayOptions? Display { get; init; }

    /// <summary>
    /// 主题名称、颜色和颜色变体生成的配置。
    /// </summary>
    [Description("@#theme")]
    public VuetifyThemeOptions? Theme { get; init; }

    /// <summary>
    /// 图标集与默认图标集的配置。
    /// </summary>
    [Description("@#icons")]
    public VuetifyIconOptions? Icons { get; init; }

    /// <summary>
    /// 应用的语言、后备语言和翻译配置。
    /// </summary>
    [Description("@#locale")]
    public VuetifyLocaleOptions? Locale { get; init; }

    /// <summary>
    /// 日期适配器与日期本地化配置。
    /// </summary>
    [Description("@#date")]
    public VuetifyDateOptions? Date { get; init; }

    /// <summary>
    /// 是否启用面向服务端渲染的初始化方式。
    /// </summary>
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
    /// <summary>
    /// 应用初始化时使用的主题名称。
    /// </summary>
    [Description("@#defaultTheme")]
    public string? DefaultTheme { get; init; }

    /// <summary>
    /// 为指定主题颜色生成明亮和暗色变体的配置。
    /// </summary>
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
    /// <summary>
    /// 需要生成颜色变体的主题颜色名称数组。
    /// </summary>
    [Description("@#colors")]
    public string[]? Colors { get; init; }

    /// <summary>
    /// 为每个指定颜色生成的明亮变体数量。
    /// </summary>
    [Description("@#lighten")]
    public int? Lighten { get; init; }

    /// <summary>
    /// 为每个指定颜色生成的暗色变体数量。
    /// </summary>
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
    /// <summary>
    /// 低于该断点时将显示环境视为移动端；支持断点名称或像素值。
    /// </summary>
    [Description("@#mobileBreakpoint")]
    public VuetifyDisplayBreakpoint? MobileBreakpoint { get; init; }

    /// <summary>
    /// 各命名显示断点的像素阈值。
    /// </summary>
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
    /// <summary>
    /// xs 显示断点的像素阈值。
    /// </summary>
    [Description("@#xs")]
    public int? Xs { get; init; }

    /// <summary>
    /// sm 显示断点的像素阈值。
    /// </summary>
    [Description("@#sm")]
    public int? Sm { get; init; }

    /// <summary>
    /// md 显示断点的像素阈值。
    /// </summary>
    [Description("@#md")]
    public int? Md { get; init; }

    /// <summary>
    /// lg 显示断点的像素阈值。
    /// </summary>
    [Description("@#lg")]
    public int? Lg { get; init; }

    /// <summary>
    /// xl 显示断点的像素阈值。
    /// </summary>
    [Description("@#xl")]
    public int? Xl { get; init; }

    /// <summary>
    /// xxl 显示断点的像素阈值。
    /// </summary>
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
    /// <summary>
    /// 默认图标集的名称。
    /// </summary>
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
    /// <summary>
    /// 当前使用的 locale 标识。
    /// </summary>
    [Description("@#locale")]
    public string? Locale { get; init; }

    /// <summary>
    /// 当前语言缺少翻译时使用的后备 locale。
    /// </summary>
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
    /// <summary>
    /// 当前使用的 locale 标识。
    /// </summary>
    [Description("@#locale")]
    public string? Locale { get; init; }
}