namespace ECMAScript.TDesign;

/// <summary>
/// TDesign Vue Next 的入口，提供插件安装和运行时版本信息。
/// </summary>
[ECMAScript("tdesign-vue-next")]
[Description("@#")]
public static class TDesign
{
    /// <summary>
    /// TDesign 的默认 Vue 插件，可传给 Vue 应用的 Use 方法。
    /// </summary>
    [ECMAScriptName("default")]
    public extern static TPlugin Default { get; }

    /// <summary>
    /// 将 TDesign 组件和服务安装到 Vue 应用。
    /// </summary>
    /// <param name="app">接收组件和服务注册的 Vue 应用。</param>
    [Description("@#install")]
    public extern static void Install(VueApp app);

    /// <summary>
    /// 使用指定配置将 TDesign 组件和服务安装到 Vue 应用。
    /// </summary>
    /// <param name="app">接收组件和服务注册的 Vue 应用。</param>
    /// <param name="options">插件安装配置。</param>
    [Description("@#install")]
    public extern static void Install(VueApp app, TInstallOptions options);

    /// <summary>
    /// 获取上游 tdesign-vue-next 运行时版本；此值与 ECMAScript.TDesign 的 NuGet 包版本独立。
    /// </summary>
    [Description("@#version")]
    public extern static string Version { get; }
}

/// <summary>
/// TDesign Vue 组件引用的公共契约，用于组件注册和动态组件传递。
/// </summary>
[ECMAScript]
public interface ITDesignComponent : IVueComponent
{
}

/// <summary>
/// TDesign 默认导出的 Vue 插件对象。
/// </summary>
[ECMAScript]
public sealed record TPlugin : VuePlugin
{
    private TPlugin()
    {
    }
}

/// <summary>
/// TDesign 插件安装选项；当前绑定仅提供配置对象，不额外声明配置字段。
/// </summary>
[ECMAScript]
[Description("@#TInstallOptions")]
public sealed record TInstallOptions : VuePluginOptions
{
}

/// <summary>
/// ConfigProvider 使用的全局配置，当前提供 CSS 类名前缀设置。
/// </summary>
[ECMAScript]
[Description("@#GlobalConfigProvider")]
public sealed record TGlobalConfig : VueProps
{
    /// <summary>
    /// TDesign 生成的 CSS 类名前缀。使用自定义前缀时，需提供使用相同前缀构建的样式。
    /// </summary>
    [Description("@#classPrefix")]
    public string? ClassPrefix { get; init; }
}