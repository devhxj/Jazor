namespace ECMAScript.Vuetify;

/// <summary>
/// 用于 VValidationSlotContext.Reset、VValidationSlotContext.ResetValidation 的回调签名。
/// 将模型值和校验状态恢复到组件的重置状态。
/// 清除校验错误并重置校验状态，保留当前模型值。
/// </summary>
public delegate IPromise VuetifyValidationResetCallback();

/// <summary>
/// 用于 VValidationSlotContext.Validate 的回调签名。
/// 执行当前字段或表单的校验，异步结果见回调的 Promise 返回类型。
/// </summary>
public delegate IPromise<string[]> VuetifyValidationValidateCallback(bool silent = false);

/// <summary>
/// Vuetify VValidation 暴露的默认插槽上下文。
/// Default slot context exposed by Vuetify VValidation.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VValidationSlotContext
{
    /// <summary>
    /// 当前字段的校验错误文本。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#errorMessages")]
    public VueComputedRef<string[]>? ErrorMessages { get; init; }

    /// <summary>
    /// 模型当前是否包含有效输入内容。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isDirty")]
    public VueComputedRef<bool>? IsDirty { get; init; }

    /// <summary>
    /// 当前控件是否被禁用。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isDisabled")]
    public VueComputedRef<bool>? IsDisabled { get; init; }

    /// <summary>
    /// 当前控件是否只读。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isReadonly")]
    public VueComputedRef<bool>? IsReadonly { get; init; }

    /// <summary>
    /// 当前编辑或校验状态是否尚未被用户修改或触发。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isPristine")]
    public VueShallowRef<bool>? IsPristine { get; init; }

    /// <summary>
    /// 当前校验是否通过；可空状态中的 null 表示尚无确定校验结果。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isValid")]
    public VueComputedRef<bool?>? IsValid { get; init; }

    /// <summary>
    /// 是否仍有尚未完成的异步校验。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isValidating")]
    public VueShallowRef<bool>? IsValidating { get; init; }

    /// <summary>
    /// 将模型值和校验状态恢复到组件的重置状态。
    /// </summary>
    [Description("@#reset")]
    public VuetifyValidationResetCallback? Reset { get; init; }

    /// <summary>
    /// 清除校验错误并重置校验状态，保留当前模型值。
    /// </summary>
    [Description("@#resetValidation")]
    public VuetifyValidationResetCallback? ResetValidation { get; init; }

    /// <summary>
    /// 执行当前字段或表单的校验，异步结果见回调的 Promise 返回类型。
    /// </summary>
    [Description("@#validate")]
    public VuetifyValidationValidateCallback? Validate { get; init; }

    /// <summary>
    /// 按当前校验状态计算的 CSS 类名开关映射。 通过引用的 Value 读取最新状态。
    /// </summary>
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