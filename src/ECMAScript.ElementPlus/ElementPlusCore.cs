namespace ECMAScript.ElementPlus;

/// <summary>
/// Element Plus root host.
/// </summary>
/// <remarks>
/// 聚合入口是上游的 <c>es/index.mjs</c>（package.json 的 <c>module</c> 字段），
/// 它同时导出组件、指令与 <c>default</c> 插件对象；Jazor 不发明裸包简写。
/// </remarks>
[ECMAScript("element-plus/es/index.mjs")]
[Description("@#")]
public static class ElementPlus
{
    /// <summary>
    /// Element Plus 的默认插件导出，可通过 Vue 应用的 Use 安装。
    /// </summary>
    [ECMAScriptName("default")]
    public extern static ElPlugin Default { get; }

    /// <summary>
    /// 将 Element Plus 插件安装到指定 Vue 应用，注册组件、指令及全局配置。
    /// </summary>
    [Description("@#install")]
    public extern static void Install(VueApp app);

    /// <summary>
    /// 将 Element Plus 插件安装到指定 Vue 应用，注册组件、指令及全局配置。
    /// </summary>
    [Description("@#install")]
    public extern static void Install(VueApp app, ElInstallOptions options);

    /// <summary>
    /// 当前所绑定 Element Plus 运行时导出的版本字符串。
    /// </summary>
    [Description("@#version")]
    public extern static string Version { get; }
}

/// <summary>
/// Marker interface for Element Plus components.
/// </summary>
[ECMAScript]
public interface IElementPlusComponent : IVueComponent
{
}

/// <summary>
/// Marker base type for Element Plus directives.
/// </summary>
[ECMAScript]
public abstract record ElDirective : VueDirective
{
}

/// <summary>
/// Element Plus plugin object.
/// </summary>
[ECMAScript]
public sealed record ElPlugin : VuePlugin
{
    private ElPlugin()
    {
    }
}

/// <summary>
/// Element Plus install options.
/// </summary>
[ECMAScript]
[Description("@#ConfigProviderContext")]
public sealed record ElInstallOptions : VuePluginOptions
{
    /// <summary>
    /// global empty values of components
    /// </summary>
    [Description("@#emptyValues")]
    public VueValue[]? EmptyValues { get; init; }

    /// <summary>
    /// global clear return value
    /// </summary>
    [Description("@#valueOnClear")]
    public ElValueOnClearValue? ValueOnClear { get; init; }

    /// <summary>
    /// 启用组件库提供的可访问性交互支持。
    /// </summary>
    [Description("@#a11y")]
    public bool? A11y { get; init; }

    /// <summary>
    /// Locale Object
    /// </summary>
    [Description("@#locale")]
    public ElLanguage? Locale { get; init; }

    /// <summary>
    /// global component size
    /// </summary>
    [Description("@#size")]
    public ElComponentSize? Size { get; init; }

    /// <summary>
    /// button related configuration, [see the following table](#button-attribute)
    /// </summary>
    [Description("@#button")]
    public ElButtonConfig? Button { get; init; }

    /// <summary>
    /// 应用于本配置作用域的 Card 默认配置。
    /// </summary>
    [Description("@#card")]
    public ElCardConfig? Card { get; init; }

    /// <summary>
    /// dialog related configuration, [see the following table](#dialog-attribute)
    /// </summary>
    [Description("@#dialog")]
    public ElDialogConfig? Dialog { get; init; }

    /// <summary>
    /// link related configuration, [see the following table](#link-attribute)
    /// </summary>
    [Description("@#link")]
    public ElLinkConfig? Link { get; init; }

    /// <summary>
    /// features at experimental stage to be added, all features are default to be set to false
    /// </summary>
    [Description("@#experimentalFeatures")]
    public VueProps? ExperimentalFeatures { get; init; }

    /// <summary>
    /// 是否启用组件库的键盘导航行为。
    /// </summary>
    [Description("@#keyboardNavigation")]
    public bool? KeyboardNavigation { get; init; }

    /// <summary>
    /// message related configuration, [see the following table](#message-attribute)
    /// </summary>
    [Description("@#message")]
    public ElMessageConfig? Message { get; init; }

    /// <summary>
    /// global Initial zIndex
    /// </summary>
    [Description("@#zIndex")]
    public Number? ZIndex { get; init; }

    /// <summary>
    /// global component className prefix (cooperated with [$namespace](https://github.com/element-plus/element-plus/blob/dev/packages/theme-chalk/src/mixins/config.scss#L1))
    /// </summary>
    [Description("@#namespace")]
    public string? Namespace { get; init; }

    /// <summary>
    /// table related configuration, [see the following table](#table-attribute)
    /// </summary>
    [Description("@#table")]
    public ElTableConfig? Table { get; init; }
}