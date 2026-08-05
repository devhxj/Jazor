using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Breadcrumb")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
public sealed class TBreadcrumb : TDesignContentComponentBase
{
    [Parameter]
    public int? ItemsAfterCollapse { get; set; }

    [Parameter]
    public int? ItemsBeforeCollapse { get; set; }

    [Parameter]
    public string? MaxItemWidth { get; set; }

    [Parameter]
    public int? MaxItems { get; set; }

    [Parameter]
    public TDesignBreadcrumbTheme? Theme { get; set; }

    [Parameter]
    public RenderFragment? Separator { get; set; }

    [Parameter]
    public RenderFragment? Ellipsis { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "BreadcrumbItem")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
public sealed class TBreadcrumbItem : TDesignContentComponentBase
{
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public string? MaxWidth { get; set; }

    [Parameter]
    public bool Replace { get; set; }

    [Parameter]
    public TDesignTarget? Target { get; set; }

    [Parameter]
    [ECMAScriptName("content")]
    public string? Text { get; set; }

    [Parameter]
    public TDesignMenuRouteTarget? To { get; set; }

    [Parameter]
    public EventCallback<MouseEvent> OnClick { get; set; }

    [Parameter]
    public RenderFragment? Icon { get; set; }
}
