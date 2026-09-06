namespace ECMAScript.VuIcons;

/// <summary>
/// Shared wrapper props for the static <c>VuUser</c>/<c>VuSearch</c> component catalog。
/// Static components each import one browser-ready SVG module, preserving the library's on-demand path.
/// </summary>
public abstract class VuIconComponentBase : ComponentBase, ECMAScript.Vue.IVueComponent
{
    /// <summary>Icon size</summary>
    [Parameter]
    [ECMAScriptName("size")]
    public Vue.VueStringNumberValue? Size { get; set; }

    /// <summary>Icon color</summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>Custom class name</summary>
    [Parameter]
    [ECMAScriptName("className")]
    public string? ClassName { get; set; }

    /// <summary>Spin animation</summary>
    [Parameter]
    [ECMAScriptName("spin")]
    public bool? Spin { get; set; }
}

/// <summary>
/// Dynamic vu-icons renderer。Use this when the icon changes at runtime; for a known icon, prefer its
/// generated static component such as <c>VuUser</c> so Emit only materializes that SVG module.
/// </summary>
[ECMAScript("vu-icons", Transform.Component, "VuIcon")]
public sealed class VuIcon : ComponentBase, ECMAScript.Vue.IVueComponent
{
    /// <summary>Icon name. The closed enum prevents misspelled upstream names.</summary>
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("name")]
    public VuIconName Name { get; set; }

    /// <summary>Icon size</summary>
    [Parameter]
    [ECMAScriptName("size")]
    public Vue.VueStringNumberValue? Size { get; set; }

    /// <summary>Icon color</summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>Custom class name</summary>
    [Parameter]
    [ECMAScriptName("class")]
    public string? Class { get; set; }

    /// <summary>Spin animation</summary>
    [Parameter]
    [ECMAScriptName("spin")]
    public bool? Spin { get; set; }
}
