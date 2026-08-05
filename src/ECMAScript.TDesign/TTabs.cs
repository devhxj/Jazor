using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Tabs")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryEmit(nameof(ValueChanged), VueEmitKind.ModelUpdate, Name = "change")]
public sealed class TTabs : TDesignContentComponentBase
{
    [Parameter]
    public bool Addable { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool DragSort { get; set; }

    [Parameter]
    public TDesignTabsPlacement? Placement { get; set; }

    [Parameter]
    public TDesignTabsScrollPosition? ScrollPosition { get; set; }

    [Parameter]
    public TDesignTabsSize? Size { get; set; }

    [Parameter]
    public TDesignTabsTheme? Theme { get; set; }

    [Parameter]
    public TDesignTabValue? Value { get; set; }

    [Parameter]
    public TDesignTabValue? DefaultValue { get; set; }

    [Parameter]
    public EventCallback<TDesignTabValue> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<TDesignTabAddContext> OnAdd { get; set; }

    [Parameter]
    public EventCallback<TDesignTabsDragSortContext> OnDragSort { get; set; }

    [Parameter]
    public EventCallback<TDesignTabRemoveContext> OnRemove { get; set; }

    [Parameter]
    public RenderFragment? Action { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "TabPanel")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
public sealed class TTabPanel : TDesignContentComponentBase
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
    public TDesignTabValue? Value { get; set; }

    [Parameter]
    public EventCallback<TDesignTabPanelRemoveContext> OnRemove { get; set; }

    [Parameter]
    public RenderFragment? Label { get; set; }
}
