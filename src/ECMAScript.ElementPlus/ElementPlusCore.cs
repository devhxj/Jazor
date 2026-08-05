namespace ECMAScript.ElementPlus;

/// <summary>
/// Element Plus root host.
/// </summary>
[ECMAScript("npm:element-plus")]
[Description("@#")]
public static class ElementPlus
{
    [ECMAScriptName("default")]
    public extern static ElPlugin Default { get; }

    [Description("@#install")]
    public extern static void Install(VueApp app);

    [Description("@#install")]
    public extern static void Install(VueApp app, ElInstallOptions options);

    [Description("@#version")]
    public extern static string Version { get; }
}

/// <summary>
/// Marker interface for Element Plus components.
/// </summary>
[ECMAScript]
public interface IElComponent : IVueComponent
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
    [Description("@#emptyValues")]
    public VueValue[]? EmptyValues { get; init; }

    [Description("@#valueOnClear")]
    public ElValueOnClearValue? ValueOnClear { get; init; }

    [Description("@#a11y")]
    public bool? A11y { get; init; }

    [Description("@#locale")]
    public ElLanguage? Locale { get; init; }

    [Description("@#size")]
    public ElComponentSize? Size { get; init; }

    [Description("@#button")]
    public ElButtonConfig? Button { get; init; }

    [Description("@#card")]
    public ElCardConfig? Card { get; init; }

    [Description("@#dialog")]
    public ElDialogConfig? Dialog { get; init; }

    [Description("@#link")]
    public ElLinkConfig? Link { get; init; }

    [Description("@#experimentalFeatures")]
    public VueProps? ExperimentalFeatures { get; init; }

    [Description("@#keyboardNavigation")]
    public bool? KeyboardNavigation { get; init; }

    [Description("@#message")]
    public ElMessageConfig? Message { get; init; }

    [Description("@#zIndex")]
    public Number? ZIndex { get; init; }

    [Description("@#namespace")]
    public string? Namespace { get; init; }

    [Description("@#table")]
    public ElTableConfig? Table { get; init; }
}
