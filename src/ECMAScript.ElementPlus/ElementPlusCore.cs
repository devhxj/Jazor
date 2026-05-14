namespace ECMAScript.ElementPlus;

/// <summary>
/// Element Plus root host.
/// </summary>
[ECMAScript("npm:element-plus")]
[Description("@#")]
public static class ElementPlus
{
    [ECMAScriptName("default")]
    public extern static ElementPlusPlugin Default { get; }

    [Description("@#install")]
    public extern static void Install(VueApp app);

    [Description("@#install")]
    public extern static void Install(VueApp app, ElementPlusInstallOptions options);

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
public abstract record ElementPlusDirective : VueDirective
{
}

/// <summary>
/// Element Plus plugin object.
/// </summary>
[ECMAScript]
public sealed record ElementPlusPlugin : VuePlugin
{
    private ElementPlusPlugin()
    {
    }
}

/// <summary>
/// Element Plus install options.
/// </summary>
[ECMAScript]
[Description("@#ConfigProviderContext")]
public sealed record ElementPlusInstallOptions : VuePluginOptions
{
    [Description("@#emptyValues")]
    public VueValue[]? EmptyValues { get; init; }

    [Description("@#valueOnClear")]
    public VueValue? ValueOnClear { get; init; }

    [Description("@#a11y")]
    public bool? A11y { get; init; }

    [Description("@#locale")]
    public VueProps? Locale { get; init; }

    [Description("@#size")]
    public ElementPlusComponentSize? Size { get; init; }

    [Description("@#button")]
    public ElementPlusButtonConfig? Button { get; init; }

    [Description("@#card")]
    public ElementPlusCardConfig? Card { get; init; }

    [Description("@#dialog")]
    public ElementPlusDialogConfig? Dialog { get; init; }

    [Description("@#link")]
    public ElementPlusLinkConfig? Link { get; init; }

    [Description("@#experimentalFeatures")]
    public VueProps? ExperimentalFeatures { get; init; }

    [Description("@#keyboardNavigation")]
    public bool? KeyboardNavigation { get; init; }

    [Description("@#message")]
    public ElementPlusMessageConfig? Message { get; init; }

    [Description("@#zIndex")]
    public Number? ZIndex { get; init; }

    [Description("@#namespace")]
    public string? Namespace { get; init; }

    [Description("@#table")]
    public ElementPlusTableConfig? Table { get; init; }
}
