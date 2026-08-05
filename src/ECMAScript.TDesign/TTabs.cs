using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Tabs")]
[VueLibraryEmit(nameof(ValueChanged), Name = "change")]
public sealed class TTabs : TContentComponentBase
{
    [Parameter]
    public bool Addable { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool DragSort { get; set; }

    [Parameter]
    public TTabsPlacement? Placement { get; set; }

    [Parameter]
    public TTabsScrollPosition? ScrollPosition { get; set; }

    [Parameter]
    public TTabsSize? Size { get; set; }

    [Parameter]
    public TTabsTheme? Theme { get; set; }

    [Parameter]
    public TTabValue? Value { get; set; }

    [Parameter]
    public TTabValue? DefaultValue { get; set; }

    [Parameter]
    public EventCallback<TTabValue> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<TTabAddContext> OnAdd { get; set; }

    [Parameter]
    public EventCallback<TTabsDragSortContext> OnDragSort { get; set; }

    [Parameter]
    public EventCallback<TTabRemoveContext> OnRemove { get; set; }

    [Parameter]
    public RenderFragment? Action { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "TabPanel")]
public sealed class TTabPanel : TContentComponentBase
{
    [Parameter]
    public bool DestroyOnHide { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Draggable { get; set; }

    [Parameter]
    [ECMAScriptName("label")]
    public string? LabelText { get; set; }

    [Parameter]
    public bool Lazy { get; set; }

    [Parameter]
    public bool Removable { get; set; }

    [Parameter]
    public TTabValue? Value { get; set; }

    [Parameter]
    public EventCallback<TTabPanelRemoveContext> OnRemove { get; set; }

    [Parameter]
    public RenderFragment? Label { get; set; }
}
