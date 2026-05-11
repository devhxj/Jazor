namespace ECMAScript.Vuetify;

/// <summary>
/// Context shared by Vuetify text-field and textarea adornment slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VFieldSlotContext
{
    [Description("@#id")]
    public string? Id { get; init; }

    [Description("@#props")]
    public VueProps? Props { get; init; }

    [Description("@#isActive")]
    public IVueRef<bool>? IsActive { get; init; }

    [Description("@#isFocused")]
    public IVueRef<bool>? IsFocused { get; init; }

    [Description("@#isDirty")]
    public VueComputedRef<bool>? IsDirty { get; init; }

    [Description("@#isDisabled")]
    public VueComputedRef<bool>? IsDisabled { get; init; }

    [Description("@#isReadonly")]
    public VueComputedRef<bool>? IsReadonly { get; init; }

    [Description("@#isValid")]
    public VueComputedRef<bool?>? IsValid { get; init; }

    [Description("@#controlRef")]
    public IVueRef<Element?>? ControlRef { get; init; }

    [Description("@#focus")]
    public Action? Focus { get; init; }

    [Description("@#blur")]
    public Action? Blur { get; init; }
}

/// <summary>
/// Context exposed by Vuetify field label slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VFieldLabelSlotContext
{
    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#props")]
    public VueProps? Props { get; init; }

    [Description("@#isActive")]
    public IVueRef<bool>? IsActive { get; init; }

    [Description("@#isFocused")]
    public IVueRef<bool>? IsFocused { get; init; }

    [Description("@#controlRef")]
    public IVueRef<Element?>? ControlRef { get; init; }

    [Description("@#focus")]
    public Action? Focus { get; init; }

    [Description("@#blur")]
    public Action? Blur { get; init; }
}

/// <summary>
/// Context exposed by Vuetify input scoped slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VInputSlotContext
{
    [Description("@#id")]
    public VueComputedRef<string>? Id { get; init; }

    [Description("@#messagesId")]
    public VueComputedRef<string>? MessagesId { get; init; }

    [Description("@#isDirty")]
    public VueComputedRef<bool>? IsDirty { get; init; }

    [Description("@#isDisabled")]
    public VueComputedRef<bool>? IsDisabled { get; init; }

    [Description("@#isReadonly")]
    public VueComputedRef<bool>? IsReadonly { get; init; }

    [Description("@#isPristine")]
    public IVueRef<bool>? IsPristine { get; init; }

    [Description("@#isValid")]
    public VueComputedRef<bool?>? IsValid { get; init; }

    [Description("@#isValidating")]
    public IVueRef<bool>? IsValidating { get; init; }

    [Description("@#reset")]
    public Action? Reset { get; init; }

    [Description("@#resetValidation")]
    public Action? ResetValidation { get; init; }

    [Description("@#validate")]
    public Action? Validate { get; init; }
}

/// <summary>
/// Context exposed by Vuetify input details slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VInputDetailsSlotContext : VInputSlotContext
{
}

/// <summary>
/// Context exposed by text-field counter slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VCounterSlotContext
{
    [Description("@#max")]
    public VuetifyCounterValueSource? Max { get; init; }

    [Description("@#value")]
    public string? Value { get; init; }
}
