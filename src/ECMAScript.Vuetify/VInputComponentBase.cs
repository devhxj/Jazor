using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Shared Vuetify text input authoring surface for field and input props.
/// </summary>
public abstract class VInputComponentBase : ComponentBase
{
    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public string? Hint { get; set; }

    [Parameter]
    public bool PersistentHint { get; set; }

    [Parameter]
    public bool PersistentPlaceholder { get; set; }

    [Parameter]
    public string? Prefix { get; set; }

    [Parameter]
    public string? Suffix { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? BaseColor { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public bool Focused { get; set; }

    [Parameter]
    public EventCallback<bool> FocusedChanged { get; set; }

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
    public string? PrependIcon { get; set; }

    [Parameter]
    public string? AppendIcon { get; set; }

    [Parameter]
    public string? PrependInnerIcon { get; set; }

    [Parameter]
    public string? AppendInnerIcon { get; set; }

    [Parameter]
    public string? ClearIcon { get; set; }

    [Parameter]
    public bool Clearable { get; set; }

    [Parameter]
    public bool PersistentClear { get; set; }

    [Parameter]
    public bool Dirty { get; set; }

    [Parameter]
    public VuetifyFieldVariant? Variant { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public VuetifyCounterValue? Counter { get; set; }

    [Parameter]
    public VuetifyCounterValueSource? CounterValue { get; set; }

    [Parameter]
    public string? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifyTextModelModifiers? ModelModifiers { get; set; }

    [Parameter]
    public RenderFragment<VFieldSlotContext>? Prepend { get; set; }

    [Parameter]
    public RenderFragment<VFieldSlotContext>? Append { get; set; }

    [Parameter]
    public RenderFragment<VFieldSlotContext>? PrependInner { get; set; }

    [Parameter]
    public RenderFragment<VFieldSlotContext>? AppendInner { get; set; }

    [Parameter]
    public RenderFragment<VFieldSlotContext>? Clear { get; set; }

    [Parameter]
    public RenderFragment<VFieldSlotContext>? LabelContent { get; set; }

    [Parameter]
    public RenderFragment<VInputDetailsSlotContext>? Details { get; set; }

    [Parameter]
    public RenderFragment<VCounterSlotContext>? CounterContent { get; set; }
}
