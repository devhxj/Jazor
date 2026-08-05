namespace ECMAScript.TDesign;

/// <summary>
/// TDesign Vue Next root host.
/// </summary>
[ECMAScript("npm:tdesign-vue-next")]
[Description("@#")]
public static class TDesign
{
    [ECMAScriptName("default")]
    public extern static TPlugin Default { get; }

    [Description("@#install")]
    public extern static void Install(VueApp app);

    [Description("@#install")]
    public extern static void Install(VueApp app, TInstallOptions options);

    [Description("@#version")]
    public extern static string Version { get; }
}

/// <summary>
/// Marker interface for TDesign components.
/// </summary>
[ECMAScript]
public interface ITComponent : IVueComponent
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
