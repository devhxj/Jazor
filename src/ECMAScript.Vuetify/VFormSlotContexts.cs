namespace ECMAScript.Vuetify;

public delegate IPromise<string[]> VuetifyFormFieldValidateCallback();

public delegate IPromise VuetifyFormFieldResetCallback();

public delegate IPromise<VuetifyFormValidationResult> VuetifyFormValidateCallback();

/// <summary>
/// Vuetify VForm 验证 API 返回的验证错误项。
/// Validation error item returned by Vuetify VForm validation APIs.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyFormFieldValidationResult
{
    [Description("@#id")]
    public VueStringNumberValue? Id { get; init; }

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
    [Description("@#valid")]
    public bool Valid { get; init; }

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
    [Description("@#id")]
    public VueStringNumberValue? Id { get; init; }

    [Description("@#validate")]
    public VuetifyFormFieldValidateCallback? Validate { get; init; }

    [Description("@#reset")]
    public VuetifyFormFieldResetCallback? Reset { get; init; }

    [Description("@#resetValidation")]
    public VuetifyFormFieldResetCallback? ResetValidation { get; init; }

    [Description("@#isValid")]
    public bool? IsValid { get; init; }

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
    public extern VFormSubmitEvent(string type, SubmitEventInit? eventInitDict = default);

    [Description("@#catch")]
    public extern IPromise Catch(Action<Error> onError);

    [Description("@#finally")]
    public extern IPromise<VuetifyFormValidationResult> Finally(Action onFinal);

    IPromise IPromise.Finally(Action onFinal)
        => Finally(onFinal);

    [Description("@#then")]
    public extern IPromise Then(Action onFulfilled);

    [Description("@#then")]
    public extern IPromise Then(Action onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise Then(Action onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<TResult> onFulfilled);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<TResult> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<TResult> onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise Then(Func<IPromise> onFulfilled);

    [Description("@#then")]
    public extern IPromise Then(Func<IPromise> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise Then(Func<IPromise> onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<IPromise<TResult>> onFulfilled);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<IPromise<TResult>> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<IPromise<TResult>> onFulfilled, Action<Error> onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<PromiseResult> onFulfilled);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<PromiseResult> onFulfilled, Action onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<PromiseResult> onFulfilled, Action<Error> onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<PromiseResult<TResult>> onFulfilled);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<PromiseResult<TResult>> onFulfilled, Action onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<PromiseResult<TResult>> onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise Then(Action<VuetifyFormValidationResult> onFulfilled);

    [Description("@#then")]
    public extern IPromise Then(Action<VuetifyFormValidationResult> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise Then(Action<VuetifyFormValidationResult> onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, TResult> onFulfilled);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, TResult> onFulfilled, Action onRejected);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, TResult> onFulfilled, Action<Error> onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, PromiseResult<TResult>> onFulfilled);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, PromiseResult<TResult>> onFulfilled, Action onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, PromiseResult<TResult>> onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise Then(Func<VuetifyFormValidationResult, IPromise> onResolve);

    [Description("@#then")]
    public extern IPromise Then(Func<VuetifyFormValidationResult, IPromise> onResolve, Action onRejected);

    [Description("@#then")]
    public extern IPromise Then(Func<VuetifyFormValidationResult, IPromise> onResolve, Action<Error> onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<VuetifyFormValidationResult, PromiseResult> onFulfilled);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<VuetifyFormValidationResult, PromiseResult> onFulfilled, Action onRejected);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("@#then")]
    public extern IPromise Then(Func<VuetifyFormValidationResult, PromiseResult> onFulfilled, Action<Error> onRejected);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, IPromise<TResult>> onFulfilled);

    [Description("@#then")]
    public extern IPromise<TResult> Then<TResult>(Func<VuetifyFormValidationResult, IPromise<TResult>> onFulfilled, Action onRejected);

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
    [Description("@#errors")]
    public IVueRef<VuetifyFormFieldValidationResult[]>? Errors { get; init; }

    [Description("@#isDisabled")]
    public VueComputedRef<bool>? IsDisabled { get; init; }

    [Description("@#isReadonly")]
    public VueComputedRef<bool>? IsReadonly { get; init; }

    [Description("@#isValidating")]
    public VueShallowRef<bool>? IsValidating { get; init; }

    [Description("@#isValid")]
    public IVueRef<bool?>? IsValid { get; init; }

    [Description("@#items")]
    public IVueRef<VuetifyFormField[]>? Items { get; init; }

    [Description("@#validate")]
    public VuetifyFormValidateCallback? Validate { get; init; }

    [Description("@#reset")]
    public Action? Reset { get; init; }

    [Description("@#resetValidation")]
    public Action? ResetValidation { get; init; }
}
