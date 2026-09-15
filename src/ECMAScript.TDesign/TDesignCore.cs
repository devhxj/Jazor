namespace ECMAScript.TDesign;

/// <summary>
/// TDesign Vue Next root host.
/// </summary>
[ECMAScript("tdesign-vue-next")]
[Description("@#")]
public static class TDesign
{
    /// <summary>
    /// The TDesign Vue plugin instance.
    /// </summary>
    [ECMAScriptName("default")]
    public extern static TPlugin Default { get; }

    /// <summary>
    /// Installs TDesign components and services into a Vue application.
    /// </summary>
    [Description("@#install")]
    public extern static void Install(VueApp app);

    /// <summary>
    /// Installs TDesign components and services with global configuration.
    /// </summary>
    [Description("@#install")]
    public extern static void Install(VueApp app, TInstallOptions options);

    /// <summary>
    /// Gets the runtime TDesign package version.
    /// </summary>
    [Description("@#version")]
    public extern static string Version { get; }
}

/// <summary>
/// Marker interface for TDesign components.
/// </summary>
[ECMAScript]
public interface ITDesignComponent : IVueComponent
{
}

/// <summary>
/// TDesign plugin object.
/// </summary>
[ECMAScript]
public sealed record TPlugin : VuePlugin
{
    private TPlugin()
    {
    }
}

/// <summary>
/// TDesign install options.
/// The published package only guarantees a plain config object shape.
/// </summary>
[ECMAScript]
[Description("@#TInstallOptions")]
public sealed record TInstallOptions : VuePluginOptions
{
}

/// <summary>
/// Minimal verified global config surface for ConfigProvider.
/// </summary>
[ECMAScript]
[Description("@#GlobalConfigProvider")]
public sealed record TGlobalConfig : VueProps
{
    [Description("@#classPrefix")]
    public string? ClassPrefix { get; init; }
}
