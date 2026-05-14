using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Avatar")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueProp(nameof(CssClass), Name = "class")]
[VueProp(nameof(CssStyle), Name = "style")]
[VueProp(nameof(Text), Name = "content")]
[VueSlot(nameof(Icon), Name = "icon")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
public sealed class TAvatar : TDesignContentComponentBase
{
    [Parameter]
    public string? Alt { get; set; }

    [Parameter]
    public bool HideOnLoadFailed { get; set; }

    [Parameter]
    public string? Image { get; set; }

    [Parameter]
    public TDesignAvatarShape? Shape { get; set; }

    [Parameter]
    public string? Size { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public EventCallback<TDesignAvatarErrorContext> OnError { get; set; }

    [Parameter]
    public RenderFragment? Icon { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "AvatarGroup")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueProp(nameof(CssClass), Name = "class")]
[VueProp(nameof(CssStyle), Name = "style")]
[VueSlot(nameof(CollapseAvatar), Name = "collapseAvatar")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
public sealed class TAvatarGroup : TDesignContentComponentBase
{
    [Parameter]
    public TDesignAvatarGroupCascading? Cascading { get; set; }

    [Parameter]
    public int? Max { get; set; }

    [Parameter]
    public string? Size { get; set; }

    [Parameter]
    public RenderFragment? CollapseAvatar { get; set; }
}
