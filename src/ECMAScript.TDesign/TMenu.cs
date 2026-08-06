using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Menu", StyleUrls = [TDesignLibraryAssets.StyleUrl])]
public sealed class TMenu : TContentComponentBase
{
    [Parameter]
    public bool Collapsed { get; set; }

    [Parameter]
    public TMenuValue[]? Expanded { get; set; }

    [Parameter]
    public TMenuValue[]? DefaultExpanded { get; set; }

    [Parameter]
    public bool ExpandMutex { get; set; }

    [Parameter]
    public TMenuExpandType? ExpandType { get; set; }

    [Parameter]
    public TMenuTheme? Theme { get; set; }

    [Parameter]
    public TMenuValue? Value { get; set; }

    [Parameter]
    public TMenuValue? DefaultValue { get; set; }

    [Parameter]
    public TMenuWidthValue? Width { get; set; }

    [Parameter]
    public EventCallback<TMenuValue> OnChange { get; set; }

    [Parameter]
    public EventCallback<TMenuValue[]> OnExpand { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    [Parameter]
    public RenderFragment? Operations { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "HeadMenu", StyleUrls = [TDesignLibraryAssets.StyleUrl])]
public sealed class THeadMenu : TContentComponentBase
{
    [Parameter]
    public TMenuValue[]? Expanded { get; set; }

    [Parameter]
    public TMenuValue[]? DefaultExpanded { get; set; }

    [Parameter]
    public TMenuExpandType? ExpandType { get; set; }

    [Parameter]
    public TMenuTheme? Theme { get; set; }

    [Parameter]
    public TMenuValue? Value { get; set; }

    [Parameter]
    public TMenuValue? DefaultValue { get; set; }

    [Parameter]
    public EventCallback<TMenuValue> OnChange { get; set; }

    [Parameter]
    public EventCallback<TMenuValue[]> OnExpand { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    [Parameter]
    public RenderFragment? Operations { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "Submenu", StyleUrls = [TDesignLibraryAssets.StyleUrl])]
public sealed class TSubmenu : TContentComponentBase
{
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public TMenuValue? Value { get; set; }

    [Parameter]
    public RenderFragment? Icon { get; set; }

    [Parameter]
    public RenderFragment? TitleContent { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "MenuItem", StyleUrls = [TDesignLibraryAssets.StyleUrl])]
public sealed class TMenuItem : TContentComponentBase
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
    public TTarget? Target { get; set; }

    [Parameter]
    [ECMAScriptName("content")]
    public string? Text { get; set; }

    [Parameter]
    public TMenuRouteTarget? To { get; set; }

    [Parameter]
    public TMenuValue? Value { get; set; }

    [Parameter]
    public EventCallback<TMenuItemClickContext> OnClick { get; set; }

    [Parameter]
    public RenderFragment? Icon { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "MenuGroup", StyleUrls = [TDesignLibraryAssets.StyleUrl])]
public sealed class TMenuGroup : TContentComponentBase
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public RenderFragment? TitleContent { get; set; }
}
