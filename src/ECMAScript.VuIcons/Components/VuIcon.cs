namespace ECMAScript.VuIcons;

/// <summary>
/// Shared wrapper props for the static <c>VuUser</c>/<c>VuSearch</c> component catalog。
/// Static components each import one browser-ready SVG module, preserving the library's on-demand path.
/// </summary>
public abstract class VuIconComponentBase : ComponentBase
{
    /// <summary>Icon size，number maps to px while string keeps the upstream numeric parsing behavior.</summary>
    [Parameter]
    [ECMAScriptName("size")]
    public Vue.VueStringNumberValue? Size { get; set; }

    /// <summary>CSS color token。</summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>Forwarded CSS class for the static wrapper component。</summary>
    [Parameter]
    [ECMAScriptName("className")]
    public string? ClassName { get; set; }

    /// <summary>Whether the icon uses the upstream spin animation。</summary>
    [Parameter]
    [ECMAScriptName("spin")]
    public bool? Spin { get; set; }
}

/// <summary>
/// Dynamic vu-icons renderer。Use this when the icon changes at runtime; for a known icon, prefer its
/// generated static component such as <c>VuUser</c> so Emit only materializes that SVG module.
/// </summary>
[VueLibraryComponent("vu-icons", "VuIcon")]
public sealed class VuIcon : ComponentBase
{
    /// <summary>Canonical required Razor icon token。The closed enum prevents misspelled upstream names.</summary>
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("name")]
    public VuIconName Name { get; set; }

    [Parameter]
    [ECMAScriptName("size")]
    public Vue.VueStringNumberValue? Size { get; set; }

    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>Falls through to the rendered element's CSS class, matching the upstream core component.</summary>
    [Parameter]
    [ECMAScriptName("class")]
    public string? Class { get; set; }

    [Parameter]
    [ECMAScriptName("spin")]
    public bool? Spin { get; set; }
}
