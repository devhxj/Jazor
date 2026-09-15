namespace ECMAScript.Vuetify;

/// <summary>
/// 用于 VuetifyFormField.Validate 的回调签名。
/// 执行当前字段或表单的校验，异步结果见回调的 Promise 返回类型。
/// </summary>
public delegate IPromise<string[]> VuetifyFormFieldValidateCallback();

/// <summary>
/// 用于 VuetifyFormField.Reset、VuetifyFormField.ResetValidation 的回调签名。
/// 将模型值和校验状态恢复到组件的重置状态。
/// 清除校验错误并重置校验状态，保留当前模型值。
/// </summary>
public delegate IPromise VuetifyFormFieldResetCallback();

/// <summary>
/// 用于 VFormDefaultSlotContext.Validate 的回调签名。
/// 执行当前字段或表单的校验，异步结果见回调的 Promise 返回类型。
/// </summary>
public delegate IPromise<VuetifyFormValidationResult> VuetifyFormValidateCallback();

/// <summary>
/// Vuetify VForm 验证 API 返回的验证错误项。
/// Validation error item returned by Vuetify VForm validation APIs.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyFormFieldValidationResult
{
    /// <summary>
    /// 当前条目的标识，用于组件内部关联及状态更新。
    /// </summary>
    [Description("@#id")]
    public VueStringNumberValue? Id { get; init; }

    /// <summary>
    /// 当前字段的校验错误文本。
    /// </summary>
    [Description("@#errorMessages")]
    public string[]? ErrorMessages { get; init; }
}

/// <summary>
/// Vuetify VForm validate 和 submit promise 返回的验证结果。
/// Validation result returned by Vuetify VForm validate and submit promise payloads.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyFormValidationResult
{
    /// <summary>
    /// 所有参与校验的字段是否均通过校验。
    /// </summary>
    [Description("@#valid")]
    public bool Valid { get; init; }

    /// <summary>
    /// 表单中未通过校验的字段及各自的错误信息。
    /// </summary>
    [Description("@#errors")]
    public VuetifyFormFieldValidationResult[]? Errors { get; init; }
}

/// <summary>
/// 通过 Vuetify VForm 默认插槽上下文暴露的已注册字段项。
/// Registered field item exposed through Vuetify VForm default slot context.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyFormField
{
    /// <summary>
    /// 当前条目的标识，用于组件内部关联及状态更新。
    /// </summary>
    [Description("@#id")]
    public VueStringNumberValue? Id { get; init; }

    /// <summary>
    /// 执行当前字段或表单的校验，异步结果见回调的 Promise 返回类型。
    /// </summary>
    [Description("@#validate")]
    public VuetifyFormFieldValidateCallback? Validate { get; init; }

    /// <summary>
    /// 将模型值和校验状态恢复到组件的重置状态。
    /// </summary>
    [Description("@#reset")]
    public VuetifyFormFieldResetCallback? Reset { get; init; }

    /// <summary>
    /// 清除校验错误并重置校验状态，保留当前模型值。
    /// </summary>
    [Description("@#resetValidation")]
    public VuetifyFormFieldResetCallback? ResetValidation { get; init; }

    /// <summary>
    /// 当前校验是否通过；可空状态中的 null 表示尚无确定校验结果。
    /// </summary>
    [Description("@#isValid")]
    public bool? IsValid { get; init; }

    /// <summary>
    /// 当前字段的校验错误文本。
    /// </summary>
    [Description("@#errorMessages")]
    public string[]? ErrorMessages { get; init; }
}

/// <summary>
/// Vuetify VForm 发出的提交事件。Vuetify 在原生
/// <see cref="SubmitEvent"/> 上增加了表单验证结果的 promise。
/// Submit event emitted by Vuetify VForm. Vuetify augments the native
/// <see cref="SubmitEvent"/> with a promise for the form validation result.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VFormSubmitEvent(string type, EventInit eventInitDict) :
    SubmitEvent(type, eventInitDict),
    IPromise<VuetifyFormValidationResult>
{
    /// <summary>
    /// 构造带异步校验结果契约的提交事件投影，供绑定类型使用；实际表单提交事件由 Vuetify 提供。
    /// </summary>
    public extern VFormSubmitEvent(string type, SubmitEventInit? eventInitDict = default);

    /// <summary>
    /// 处理表单校验 Promise 的拒绝结果，返回继续链式处理的新 Promise。
    /// </summary>
    [Description("@#catch")]
    public extern IPromise Catch(Action<Error> onError);

    /// <summary>
    /// 在表单校验 Promise 完成或拒绝后运行清理回调，保留原结果或拒绝原因继续传递。
    /// </summary>
    [Description("@#finally")]
    public extern IPromise<VuetifyFormValidationResult> Finally(Action onFinal);

    IPromise IPromise.Finally(Action onFinal)
        => Finally(onFinal);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise Then(Action onFulfilled);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise Then(Action onFulfilled, Action onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise Then(Action onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<TResult> onFulfilled);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<TResult> onFulfilled, Action onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<TResult> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise Then(Func<IPromise> onFulfilled);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise Then(Func<IPromise> onFulfilled, Action onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise Then(Func<IPromise> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<IPromise<TResult>> onFulfilled);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<IPromise<TResult>> onFulfilled, Action onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<IPromise<TResult>> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<PromiseResult> onFulfilled);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<PromiseResult> onFulfilled, Action onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<PromiseResult> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<PromiseResult<TResult>> onFulfilled);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<PromiseResult<TResult>> onFulfilled, Action onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<PromiseResult<TResult>> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise Then(Action<VuetifyFormValidationResult> onFulfilled);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise Then(Action<VuetifyFormValidationResult> onFulfilled, Action onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise Then(Action<VuetifyFormValidationResult> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, TResult> onFulfilled);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, TResult> onFulfilled, Action onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, TResult> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, PromiseResult<TResult>> onFulfilled);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, PromiseResult<TResult>> onFulfilled, Action onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, PromiseResult<TResult>> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise Then(Func<VuetifyFormValidationResult, IPromise> onResolve);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise Then(Func<VuetifyFormValidationResult, IPromise> onResolve, Action onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise Then(Func<VuetifyFormValidationResult, IPromise> onResolve, Action<Error> onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<VuetifyFormValidationResult, PromiseResult> onFulfilled);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<VuetifyFormValidationResult, PromiseResult> onFulfilled, Action onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<VuetifyFormValidationResult, PromiseResult> onFulfilled, Action<Error> onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, IPromise<TResult>> onFulfilled);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, IPromise<TResult>> onFulfilled, Action onRejected);

    /// <summary>
    /// 在表单异步校验完成后处理校验结果；成功和拒绝回调遵循 Promise.then 的链式调用语义，返回新的 Promise。
    /// </summary>
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, IPromise<TResult>> onFulfilled, Action<Error> onRejected);
}

/// <summary>
/// Vuetify VForm 默认插槽上下文。
/// Default slot context exposed by Vuetify VForm.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VFormDefaultSlotContext
{
    /// <summary>
    /// 表单中未通过校验的字段及各自的错误信息。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#errors")]
    public IVueRef<VuetifyFormFieldValidationResult[]>? Errors { get; init; }

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
    /// 是否仍有尚未完成的异步校验。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isValidating")]
    public VueShallowRef<bool>? IsValidating { get; init; }

    /// <summary>
    /// 当前校验是否通过；可空状态中的 null 表示尚无确定校验结果。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isValid")]
    public IVueRef<bool?>? IsValid { get; init; }

    /// <summary>
    /// 当前组件处理后的条目集合，顺序与当前渲染顺序一致。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#items")]
    public IVueRef<VuetifyFormField[]>? Items { get; init; }

    /// <summary>
    /// 执行当前字段或表单的校验，异步结果见回调的 Promise 返回类型。
    /// </summary>
    [Description("@#validate")]
    public VuetifyFormValidateCallback? Validate { get; init; }

    /// <summary>
    /// 将模型值和校验状态恢复到组件的重置状态。
    /// </summary>
    [Description("@#reset")]
    public Action? Reset { get; init; }

    /// <summary>
    /// 清除校验错误并重置校验状态，保留当前模型值。
    /// </summary>
    [Description("@#resetValidation")]
    public Action? ResetValidation { get; init; }
}