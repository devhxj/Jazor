using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

/// <summary>
/// TDesign button authoring proxy.
/// </summary>
[VueLibraryComponent("tdesign-vue-next", "Button")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibraryProp(nameof(Text), Name = "content")]
[VueLibrarySlot(nameof(Icon), Name = "icon")]
[VueLibrarySlot(nameof(Suffix), Name = "suffix")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class TButton : TDesignContentComponentBase
{
    [Parameter]
    public bool Block { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? Form { get; set; }

    [Parameter]
    public bool Ghost { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public TDesignButtonShape? Shape { get; set; }

    [Parameter]
    public TDesignSize? Size { get; set; }

    [Parameter]
    public TDesignButtonTag? Tag { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public TDesignButtonTheme? Theme { get; set; }

    [Parameter]
    public TDesignButtonType? Type { get; set; }

    [Parameter]
    public TDesignButtonVariant? Variant { get; set; }

    [Parameter]
    public EventCallback<MouseEvent> OnClick { get; set; }

    [Parameter]
    public RenderFragment? Icon { get; set; }

    [Parameter]
    public RenderFragment? Suffix { get; set; }
}
