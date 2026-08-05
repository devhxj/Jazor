using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Avatar")]
public sealed class TAvatar : TContentComponentBase
{
    [Parameter]
    public string? Alt { get; set; }

    [Parameter]
    public bool HideOnLoadFailed { get; set; }

    [Parameter]
    public string? Image { get; set; }

    [Parameter]
    public TAvatarShape? Shape { get; set; }

    [Parameter]
    public string? Size { get; set; }

    [Parameter]
    [ECMAScriptName("content")]
    public string? Text { get; set; }

    [Parameter]
    public EventCallback<TAvatarErrorContext> OnError { get; set; }

    [Parameter]
    public RenderFragment? Icon { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "AvatarGroup")]
public sealed class TAvatarGroup : TContentComponentBase
{
    [Parameter]
    public TAvatarGroupCascading? Cascading { get; set; }

    [Parameter]
    public int? Max { get; set; }

    [Parameter]
    public string? Size { get; set; }

    [Parameter]
    [ECMAScriptName("collapseAvatar")]
    public RenderFragment? CollapseAvatar { get; set; }
}
