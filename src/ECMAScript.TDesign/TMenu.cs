using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Menu")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibrarySlot(nameof(Logo), Name = "logo")]
[VueLibrarySlot(nameof(Operations), Name = "operations")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class TMenu : TDesignContentComponentBase
{
    [Parameter]
    public bool Collapsed { get; set; }

    [Parameter]
    public TDesignMenuValue[]? Expanded { get; set; }

    [Parameter]
    public TDesignMenuValue[]? DefaultExpanded { get; set; }

    [Parameter]
    public bool ExpandMutex { get; set; }

    [Parameter]
    public TDesignMenuExpandType? ExpandType { get; set; }

    [Parameter]
    public TDesignMenuTheme? Theme { get; set; }

    [Parameter]
    public TDesignMenuValue? Value { get; set; }

    [Parameter]
    public TDesignMenuValue? DefaultValue { get; set; }

    [Parameter]
    public TDesignMenuWidthValue? Width { get; set; }

    [Parameter]
    public EventCallback<TDesignMenuValue> OnChange { get; set; }

    [Parameter]
    public EventCallback<TDesignMenuValue[]> OnExpand { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    [Parameter]
    public RenderFragment? Operations { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "HeadMenu")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibrarySlot(nameof(Logo), Name = "logo")]
[VueLibrarySlot(nameof(Operations), Name = "operations")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class THeadMenu : TDesignContentComponentBase
{
    [Parameter]
    public TDesignMenuValue[]? Expanded { get; set; }

    [Parameter]
    public TDesignMenuValue[]? DefaultExpanded { get; set; }

    [Parameter]
    public TDesignMenuExpandType? ExpandType { get; set; }

    [Parameter]
    public TDesignMenuTheme? Theme { get; set; }

    [Parameter]
    public TDesignMenuValue? Value { get; set; }

    [Parameter]
    public TDesignMenuValue? DefaultValue { get; set; }

    [Parameter]
    public EventCallback<TDesignMenuValue> OnChange { get; set; }

    [Parameter]
    public EventCallback<TDesignMenuValue[]> OnExpand { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    [Parameter]
    public RenderFragment? Operations { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "Submenu")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibrarySlot(nameof(Icon), Name = "icon")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class TSubmenu : TDesignContentComponentBase
{
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public TDesignMenuValue? Value { get; set; }

    [Parameter]
    public RenderFragment? Icon { get; set; }

    [Parameter]
    public RenderFragment? TitleContent { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "MenuItem")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibraryProp(nameof(Text), Name = "content")]
[VueLibrarySlot(nameof(Icon), Name = "icon")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class TMenuItem : TDesignContentComponentBase
{
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public bool Replace { get; set; }

    [Parameter]
    public bool RouterLink { get; set; }

    [Parameter]
    public TDesignTarget? Target { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public TDesignMenuRouteTarget? To { get; set; }

    [Parameter]
    public TDesignMenuValue? Value { get; set; }

    [Parameter]
    public EventCallback<TDesignMenuItemClickContext> OnClick { get; set; }

    [Parameter]
    public RenderFragment? Icon { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "MenuGroup")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class TMenuGroup : TDesignContentComponentBase
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public RenderFragment? TitleContent { get; set; }
}
