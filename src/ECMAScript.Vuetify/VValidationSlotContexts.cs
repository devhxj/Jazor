namespace ECMAScript.Vuetify;

public delegate IPromise VuetifyValidationResetCallback();

public delegate IPromise<string[]> VuetifyValidationValidateCallback(bool silent = false);

/// <summary>
/// Vuetify VValidation 暴露的默认插槽上下文。
/// Default slot context exposed by Vuetify VValidation.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VValidationSlotContext
{
    [Description("@#errorMessages")]
    public VueComputedRef<string[]>? ErrorMessages { get; init; }

    [Description("@#isDirty")]
    public VueComputedRef<bool>? IsDirty { get; init; }

    [Description("@#isDisabled")]
    public VueComputedRef<bool>? IsDisabled { get; init; }

    [Description("@#isReadonly")]
    public VueComputedRef<bool>? IsReadonly { get; init; }

    [Description("@#isPristine")]
    public VueShallowRef<bool>? IsPristine { get; init; }

    [Description("@#isValid")]
    public VueComputedRef<bool?>? IsValid { get; init; }

    [Description("@#isValidating")]
    public VueShallowRef<bool>? IsValidating { get; init; }

    [Description("@#reset")]
    public VuetifyValidationResetCallback? Reset { get; init; }

    [Description("@#resetValidation")]
    public VuetifyValidationResetCallback? ResetValidation { get; init; }

    [Description("@#validate")]
    public VuetifyValidationValidateCallback? Validate { get; init; }

    [Description("@#validationClasses")]
    public VueComputedRef<VuetifyValidationClasses>? ValidationClasses { get; init; }
}

/// <summary>
/// Vuetify 验证状态的 CSS 类名映射。
/// CSS class map for Vuetify validation states.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyValidationClasses : VueDictionary<bool>
{
}
