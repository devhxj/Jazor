using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

/// <summary>
/// TDesign button authoring proxy.
/// </summary>
[VueLibraryComponent("tdesign-vue-next", "Button")]
public sealed class TButton : TContentComponentBase
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
    public TButtonShape? Shape { get; set; }

    [Parameter]
    public TSize? Size { get; set; }

    [Parameter]
    public TButtonTag? Tag { get; set; }

    [Parameter]
    [ECMAScriptName("content")]
    public string? Text { get; set; }

    [Parameter]
    public TButtonTheme? Theme { get; set; }

    [Parameter]
    public TButtonType? Type { get; set; }

    [Parameter]
    public TButtonVariant? Variant { get; set; }

    [Parameter]
    public EventCallback<MouseEvent> OnClick { get; set; }

    [Parameter]
    public RenderFragment? Icon { get; set; }

    [Parameter]
    public RenderFragment? Suffix { get; set; }
}
