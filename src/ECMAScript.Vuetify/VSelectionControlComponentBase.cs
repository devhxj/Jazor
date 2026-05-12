using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 选择控件组件共享的编写基类。
/// Shared Vuetify selection-control authoring surface.
/// </summary>
public abstract class VSelectionControlComponentBase : ComponentBase
{
    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? BaseColor { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public bool Error { get; set; }

    [Parameter]
    public VuetifyMessagesValue? ErrorMessages { get; set; }

    [Parameter]
    public VuetifyMessagesValue? Messages { get; set; }

    [Parameter]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    [Parameter]
    public VuetifyValidateOn? ValidateOn { get; set; }

    [Parameter]
    public bool Focused { get; set; }

    [Parameter]
    public EventCallback<bool> FocusedChanged { get; set; }

    [Parameter]
    public bool Dirty { get; set; }

    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public VueValue? Value { get; set; }

    [Parameter]
    public VueValue? TrueValue { get; set; }

    [Parameter]
    public VueValue? FalseValue { get; set; }

    [Parameter]
    public bool Indeterminate { get; set; }

    [Parameter]
    public bool Inline { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public VuetifyIconValue? FalseIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? TrueIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? IndeterminateIcon { get; set; }

    [Parameter]
    public RenderFragment<VSelectionControlInputDefaultSlotContext>? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VSelectionControlLabelSlotContext>? LabelContent { get; set; }

    [Parameter]
    public RenderFragment<VInputSlotContext>? Prepend { get; set; }

    [Parameter]
    public RenderFragment<VInputSlotContext>? Append { get; set; }

    [Parameter]
    public RenderFragment<VInputDetailsSlotContext>? Details { get; set; }

    [Parameter]
    public RenderFragment<VMessagesMessageSlotContext>? Message { get; set; }

    [Parameter]
    public RenderFragment<VSelectionControlInputSlotContext>? Input { get; set; }
}
