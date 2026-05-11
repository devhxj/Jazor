namespace ECMAScript.Vuetify;

/// <summary>
/// Default slot context exposed by Vuetify VSelectionControl.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSelectionControlDefaultSlotContext
{
    [Description("@#backgroundColorClasses")]
    public IVueRef<string[]>? BackgroundColorClasses { get; init; }

    [Description("@#backgroundColorStyles")]
    public IVueRef<VuetifyCssProperties>? BackgroundColorStyles { get; init; }
}

/// <summary>
/// Context exposed by Vuetify selection-control label slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSelectionControlLabelSlotContext
{
    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#props")]
    public VueProps? Props { get; init; }
}

/// <summary>
/// Props passed to Vuetify VSelectionControl input slots for wiring custom controls.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSelectionControlInputProps
{
    [Description("@#onBlur")]
    public Action<Event>? OnBlur { get; init; }

    [Description("@#onFocus")]
    public Action<FocusEvent>? OnFocus { get; init; }

    [Description("@#id")]
    public string? Id { get; init; }
}

/// <summary>
/// Input slot context exposed by Vuetify VSelectionControl.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSelectionControlInputSlotContext
{
    [Description("@#model")]
    public VueWritableComputedRef<bool>? Model { get; init; }

    [Description("@#textColorClasses")]
    public IVueRef<string[]>? TextColorClasses { get; init; }

    [Description("@#textColorStyles")]
    public IVueRef<VuetifyCssProperties>? TextColorStyles { get; init; }

    [Description("@#backgroundColorClasses")]
    public IVueRef<string[]>? BackgroundColorClasses { get; init; }

    [Description("@#backgroundColorStyles")]
    public IVueRef<VuetifyCssProperties>? BackgroundColorStyles { get; init; }

    [Description("@#inputNode")]
    public IVNode? InputNode { get; init; }

    [Description("@#icon")]
    public VuetifyIconValue? Icon { get; init; }

    [Description("@#props")]
    public VSelectionControlInputProps? Props { get; init; }
}

/// <summary>
/// Context exposed by Vuetify VSwitch thumb and track slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSwitchSlotContext
{
    [Description("@#model")]
    public IVueRef<bool>? Model { get; init; }

    [Description("@#isValid")]
    public VueComputedRef<bool?>? IsValid { get; init; }

    [Description("@#icon")]
    public VuetifyIconValue? Icon { get; init; }
}

/// <summary>
/// Default slot context exposed by components that combine Vuetify VInput and VSelectionControl slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSelectionControlInputDefaultSlotContext : VInputSlotContext
{
    [Description("@#backgroundColorClasses")]
    public IVueRef<string[]>? BackgroundColorClasses { get; init; }

    [Description("@#backgroundColorStyles")]
    public IVueRef<VuetifyCssProperties>? BackgroundColorStyles { get; init; }
}

/// <summary>
/// CSS property bag returned by Vuetify color composables in slot scoped refs.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyCssProperties : VueDictionary<VueStringNumberValue>
{
}
