using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// 表单校验结果：整体是否有效、按字段路径索引的错误与校验来源。
/// The validation result: overall validity, per-path errors, and the validation source.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VeeValidateValidationResult
{
    /// <summary>全部字段是否通过校验。Whether every field passed validation.</summary>
    [Description("@#valid")]
    public bool Valid { get; init; } = default!;

    /// <summary>按字段路径索引的首条错误消息。The first error message per field path.</summary>
    [Description("@#errors")]
    public Vue.VueDictionary<string> Errors { get; init; } = default!;

    /// <summary>校验来源，例如 <c>schema</c>、<c>field</c> 或 <c>none</c>。The validation source, for example <c>schema</c>, <c>field</c>, or <c>none</c>.</summary>
    [Description("@#source")]
    public string? Source { get; init; }
}

/// <summary>
/// 字段元数据；与 <c>useField</c> 返回的 <c>meta</c> 一致。
/// Field metadata; mirrors the <c>meta</c> returned by <c>useField</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VeeValidateFieldMeta
{
    /// <summary>字段是否通过校验。Whether the field passed validation.</summary>
    [Description("@#valid")]
    public bool Valid { get; init; } = default!;

    /// <summary>字段值是否已改变。Whether the field value changed from its initial value.</summary>
    [Description("@#dirty")]
    public bool Dirty { get; init; } = default!;

    /// <summary>字段是否已被交互过。Whether the field was touched.</summary>
    [Description("@#touched")]
    public bool Touched { get; init; } = default!;

    /// <summary>是否仍有校验在进行。Whether validation is still in flight.</summary>
    [Description("@#pending")]
    public bool Pending { get; init; } = default!;

    /// <summary>字段是否已完成至少一次校验。Whether the field has been validated at least once.</summary>
    [Description("@#validated")]
    public bool Validated { get; init; } = default!;
}

/// <summary>
/// 表单元数据；与 <c>useForm</c> 返回的 <c>meta</c> 一致。
/// Form metadata; mirrors the <c>meta</c> returned by <c>useForm</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VeeValidateFormMeta
{
    /// <summary>表单是否通过校验。Whether the form passed validation.</summary>
    [Description("@#valid")]
    public bool Valid { get; init; } = default!;

    /// <summary>表单值是否已改变。Whether the form values changed from their initial values.</summary>
    [Description("@#dirty")]
    public bool Dirty { get; init; } = default!;

    /// <summary>是否有字段已被交互过。Whether any field was touched.</summary>
    [Description("@#touched")]
    public bool Touched { get; init; } = default!;

    /// <summary>是否仍有校验在进行。Whether validation is still in flight.</summary>
    [Description("@#pending")]
    public bool Pending { get; init; } = default!;

    /// <summary>表单是否已完成至少一次校验。Whether the form has been validated at least once.</summary>
    [Description("@#validated")]
    public bool Validated { get; init; } = default!;
}

/// <summary>
/// 校验失败时提交回调收到的上下文。
/// The context passed to the invalid-submit callback.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VeeValidateInvalidSubmitContext
{
    /// <summary>提交时的表单值。The form values at submission time.</summary>
    [Description("@#values")]
    public Vue.VueDictionary Values { get; init; } = default!;

    /// <summary>按字段路径索引的校验错误。The validation errors per field path.</summary>
    [Description("@#errors")]
    public Vue.VueDictionary<string> Errors { get; init; } = default!;

    /// <summary>触发提交的原生事件；手动调用提交时为 undefined。The originating submit event, undefined for manual submissions.</summary>
    [Description("@#evt")]
    public SubmitEvent? Evt { get; init; }
}

/// <summary>
/// <c>useForm()</c> 的选项对象。
/// Options for <c>useForm()</c>.
/// </summary>
/// <typeparam name="TValues">表单值契约。The form value contract.</typeparam>
[ECMAScript]
[Description("@#")]
public record VeeValidateFormOptions<TValues>
{
    /// <summary>表单显示名，用于 devtools 与诊断消息。The form display name used by devtools and diagnostics.</summary>
    [Description("@#name")]
    public string? Name { get; init; }

    /// <summary>初始值；字段初始值缺省时从此取值。The initial values; fields fall back to them when their own initial value is absent.</summary>
    [Description("@#initialValues")]
    public TValues? InitialValues { get; init; }

    /// <summary>初始错误（按字段路径索引）。The initial errors, keyed by field path.</summary>
    [Description("@#initialErrors")]
    public Vue.VueDictionary<string>? InitialErrors { get; init; }

    /// <summary>挂载后是否立即校验。Whether to validate right after mount.</summary>
    [Description("@#validateOnMount")]
    public bool? ValidateOnMount { get; init; }

    /// <summary>字段卸载后是否保留其值。Whether to keep field values after unmount.</summary>
    [Description("@#keepValuesOnUnmount")]
    public bool? KeepValuesOnUnmount { get; init; }
}

/// <summary>
/// <c>useField()</c> 的选项对象。
/// Options for <c>useField()</c>.
/// </summary>
/// <typeparam name="TValue">字段值类型。The field value type.</typeparam>
[ECMAScript]
[Description("@#")]
public record VeeValidateFieldOptions<TValue>
{
    /// <summary>字段初始值。The field initial value.</summary>
    [Description("@#initialValue")]
    public TValue? InitialValue { get; init; }

    /// <summary>字段显示名，用于错误消息插值。The field display name used in interpolated messages.</summary>
    [Description("@#label")]
    public string? Label { get; init; }

    /// <summary>首次失败后是否停止后续规则。Whether to stop the remaining rules after the first failure.</summary>
    [Description("@#bails")]
    public bool? Bails { get; init; }

    /// <summary>挂载后是否立即校验。Whether to validate right after mount.</summary>
    [Description("@#validateOnMount")]
    public bool? ValidateOnMount { get; init; }

    /// <summary>值更新时是否立即校验。Whether to validate whenever the value updates.</summary>
    [Description("@#validateOnValueUpdate")]
    public bool? ValidateOnValueUpdate { get; init; }

    /// <summary>字段卸载后是否保留其值。Whether to keep the field value after unmount.</summary>
    [Description("@#keepValueOnUnmount")]
    public bool? KeepValueOnUnmount { get; init; }
}

/// <summary>
/// <c>configure()</c> 的全局配置；影响所有字段的默认校验触发方式。
/// Global configuration for <c>configure()</c>; it changes the default validation triggers for every field.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VeeValidateConfig
{
    /// <summary>blur 事件时校验。Validate on the blur event.</summary>
    [Description("@#validateOnBlur")]
    public bool? ValidateOnBlur { get; init; }

    /// <summary>change 事件时校验。Validate on the change event.</summary>
    [Description("@#validateOnChange")]
    public bool? ValidateOnChange { get; init; }

    /// <summary>input 事件时校验。Validate on the input event.</summary>
    [Description("@#validateOnInput")]
    public bool? ValidateOnInput { get; init; }

    /// <summary><c>v-model</c> 更新时校验。Validate when the model updates.</summary>
    [Description("@#validateOnModelUpdate")]
    public bool? ValidateOnModelUpdate { get; init; }
}

/// <summary>
/// 校验通过时的提交回调；接收已校验的表单值。
/// The submit callback invoked for valid submissions; it receives the validated form values.
/// </summary>
/// <typeparam name="TValues">表单值契约。The form value contract.</typeparam>
public delegate void VeeValidateSubmitCallback<TValues>(TValues values);

/// <summary>
/// 校验失败时的提交回调；接收失败上下文。
/// The submit callback invoked for invalid submissions; it receives the failure context.
/// </summary>
public delegate void VeeValidateInvalidSubmitCallback(VeeValidateInvalidSubmitContext context);

/// <summary>
/// <c>handleSubmit()</c> 返回的表单提交处理器；调用后完成一次校验与提交。
/// The form submit handler returned by <c>handleSubmit()</c>; invoking it runs one validation and submission.
/// </summary>
public delegate PromiseResult VeeValidateSubmitHandler(SubmitEvent? submitEvent = null);

/// <summary>
/// 写入字段值的处理器；<c>shouldValidate</c> 控制写入后是否触发校验。
/// The field-value write handler; <c>shouldValidate</c> controls whether writing triggers validation.
/// </summary>
public delegate void VeeValidateSetValueHandler<TValue>(TValue value, bool shouldValidate);

/// <summary>
/// 写入字段 touched 状态的处理器。
/// The field touched-state write handler.
/// </summary>
public delegate void VeeValidateSetTouchedHandler(bool touched);

/// <summary>
/// 写入字段错误消息的处理器；传空消息表示清除错误。
/// The field error write handler; passing an empty message clears the error.
/// </summary>
public delegate void VeeValidateSetErrorHandler(string? message);

/// <summary>
/// 批量写入表单值的处理器。
/// The form-value bulk write handler.
/// </summary>
public delegate void VeeValidateSetFormValuesHandler<TValues>(TValues values, bool shouldValidate);

/// <summary>
/// 批量写入表单错误的处理器。
/// The form-error bulk write handler.
/// </summary>
public delegate void VeeValidateSetFormErrorsHandler(Vue.VueDictionary<string> errors);

/// <summary>
/// 批量写入表单 touched 状态的处理器。
/// The form touched-state bulk write handler.
/// </summary>
public delegate void VeeValidateSetFormTouchedHandler(Vue.VueDictionary<bool> touched);

/// <summary>
/// 重置表单的处理器；<c>state</c> 为空时回到初始值。
/// The form reset handler; an empty <c>state</c> restores the initial values.
/// </summary>
public delegate void VeeValidateResetFormHandler(VeeValidateFormResetState? state);

/// <summary>
/// 触发校验的处理器。
/// The validation trigger handler.
/// </summary>
public delegate PromiseResult<VeeValidateValidationResult> VeeValidateValidateHandler();

/// <summary>
/// <c>useForm()</c> 与 <c>useFormContext()</c> 共享的表单上下文投影。
/// The shared form-context projection returned by <c>useForm()</c> and <c>useFormContext()</c>.
/// </summary>
/// <typeparam name="TValues">表单值契约。The form value contract.</typeparam>
[ECMAScript]
[Description("@#")]
public abstract class VeeValidateFormReturn<TValues>
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VeeValidateFormReturn()
    {
    }

    /// <summary>表单显示名。The form display name.</summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>表单当前值（响应式对象）。The current form values as a reactive object.</summary>
    [Description("@#values")]
    public extern TValues Values { get; }

    /// <summary>按字段路径索引的首条错误消息。The first error message per field path.</summary>
    [Description("@#errors")]
    public extern Vue.VueComputedRef<Vue.VueDictionary<string>> Errors { get; }

    /// <summary>按字段路径索引的全部错误消息。All error messages per field path.</summary>
    [Description("@#errorBag")]
    public extern Vue.VueComputedRef<Vue.VueDictionary<string[]>> ErrorBag { get; }

    /// <summary>表单聚合元数据。The aggregated form metadata.</summary>
    [Description("@#meta")]
    public extern Vue.VueComputedRef<VeeValidateFormMeta> Meta { get; }

    /// <summary>是否正在提交。Whether a submission is in progress.</summary>
    [Description("@#isSubmitting")]
    public extern Vue.VueComputedRef<bool> IsSubmitting { get; }

    /// <summary>是否正在校验。Whether validation is in progress.</summary>
    [Description("@#isValidating")]
    public extern Vue.VueComputedRef<bool> IsValidating { get; }

    /// <summary>已尝试提交的次数。The number of submission attempts.</summary>
    [Description("@#submitCount")]
    public extern Vue.VueComputedRef<Number> SubmitCount { get; }

    /// <summary>校验整个表单。Validates the whole form.</summary>
    [Description("@#validate")]
    public extern PromiseResult<VeeValidateValidationResult> Validate();

    /// <summary>校验单个字段。Validates a single field.</summary>
    /// <param name="path">字段路径。The field path.</param>
    [Description("@#validateField")]
    public extern PromiseResult<VeeValidateValidationResult> ValidateField(string path);

    /// <summary>重置表单；<c>state</c> 为空时回到初始值。Resets the form; an empty state restores the initial values.</summary>
    /// <param name="state">可选的重置目标状态。An optional reset target state.</param>
    [Description("@#resetForm")]
    public extern void ResetForm(VeeValidateFormResetState<TValues>? state = null);

    /// <summary>写入单个字段值。Writes a single field value.</summary>
    /// <param name="path">字段路径。The field path.</param>
    /// <param name="value">要写入的值。The value to write.</param>
    [Description("@#setFieldValue")]
    public extern void SetFieldValue<TValue>(string path, TValue value);

    /// <summary>写入单个字段的错误消息；传 null 表示清除。Writes a single field error; <c>null</c> clears it.</summary>
    /// <param name="path">字段路径。The field path.</param>
    /// <param name="message">错误消息或 null。The error message, or null to clear.</param>
    [Description("@#setFieldError")]
    public extern void SetFieldError(string path, string? message);

    /// <summary>写入单个字段的 touched 状态。Writes a single field's touched state.</summary>
    /// <param name="path">字段路径。The field path.</param>
    /// <param name="touched">是否已交互。Whether the field was touched.</param>
    [Description("@#setFieldTouched")]
    public extern void SetFieldTouched(string path, bool touched);

    /// <summary>批量写入表单值。Writes form values in bulk.</summary>
    /// <param name="values">要写入的表单值。The form values to write.</param>
    [Description("@#setValues")]
    public extern void SetValues(TValues values);

    /// <summary>批量写入表单错误。Writes form errors in bulk.</summary>
    /// <param name="errors">按字段路径索引的错误。The errors keyed by field path.</param>
    [Description("@#setErrors")]
    public extern void SetErrors(Vue.VueDictionary<string> errors);

    /// <summary>为字段路径创建隔离的字段绑定。Creates an isolated field binding for a path.</summary>
    /// <param name="path">字段路径。The field path.</param>
    /// <typeparam name="TValue">字段值类型。The field value type.</typeparam>
    [Description("@#defineField")]
    public extern VeeValidateFieldReturn<TValue> DefineField<TValue>(string path);

    /// <summary>创建提交处理器；返回的函数每次调用都会先校验再提交。Creates a submit handler that validates before each submission.</summary>
    /// <param name="onSubmit">校验通过时的提交回调。The callback invoked for valid submissions.</param>
    /// <param name="onInvalid">校验失败时的回调。The callback invoked for invalid submissions.</param>
    [Description("@#handleSubmit")]
    public extern VeeValidateSubmitHandler HandleSubmit(
        VeeValidateSubmitCallback<TValues> onSubmit,
        VeeValidateInvalidSubmitCallback? onInvalid = null);

    /// <summary>创建异步提交处理器；回调返回的 promise 会被表单等待。Creates an async submit handler whose returned promise is awaited by the form.</summary>
    /// <param name="onSubmit">校验通过时的异步提交回调。The async callback invoked for valid submissions.</param>
    /// <param name="onInvalid">校验失败时的回调。The callback invoked for invalid submissions.</param>
    [Description("@#handleSubmit")]
    public extern VeeValidateSubmitHandler HandleSubmit(
        Func<TValues, PromiseResult> onSubmit,
        VeeValidateInvalidSubmitCallback? onInvalid = null);
}

/// <summary>
/// <c>resetForm()</c> 的重置目标状态。
/// The reset target state used by <c>resetForm()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VeeValidateFormResetState
{
    /// <summary>按字段路径索引的 touched 覆盖值。Touched overrides keyed by field path.</summary>
    [Description("@#touched")]
    public Vue.VueDictionary<bool>? Touched { get; init; }

    /// <summary>按字段路径索引的错误覆盖值。Error overrides keyed by field path.</summary>
    [Description("@#errors")]
    public Vue.VueDictionary<string>? Errors { get; init; }
}

/// <summary>
/// <c>resetForm()</c> 的重置目标状态，附带类型化的表单值。
/// The reset target state used by <c>resetForm()</c>, with typed form values.
/// </summary>
/// <typeparam name="TValues">表单值契约。The form value contract.</typeparam>
[ECMAScript]
[Description("@#")]
public record VeeValidateFormResetState<TValues> : VeeValidateFormResetState
{
    /// <summary>重置后的表单值。The form values after the reset.</summary>
    [Description("@#values")]
    public TValues? Values { get; init; }
}

/// <summary>
/// <c>useField()</c> 返回的字段上下文投影。
/// The field-context projection returned by <c>useField()</c>.
/// </summary>
/// <typeparam name="TValue">字段值类型。The field value type.</typeparam>
[ECMAScript]
[Description("@#")]
public abstract class VeeValidateFieldReturn<TValue>
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VeeValidateFieldReturn()
    {
    }

    /// <summary>字段路径。The field path.</summary>
    [Description("@#name")]
    public extern Vue.VueComputedRef<string> Name { get; }

    /// <summary>字段值（可写计算引用）。The field value as a writable computed ref.</summary>
    [Description("@#value")]
    public extern Vue.IVueRef<TValue> Value { get; }

    /// <summary>字段元数据。The field metadata.</summary>
    [Description("@#meta")]
    public extern Vue.VueComputedRef<VeeValidateFieldMeta> Meta { get; }

    /// <summary>字段的全部错误消息。All error messages for the field.</summary>
    [Description("@#errors")]
    public extern Vue.VueComputedRef<string[]> Errors { get; }

    /// <summary>字段的首条错误消息。The first error message for the field.</summary>
    [Description("@#errorMessage")]
    public extern Vue.VueComputedRef<string?> ErrorMessage { get; }

    /// <summary>校验该字段。Validates the field.</summary>
    [Description("@#validate")]
    public extern PromiseResult<VeeValidateValidationResult> Validate();

    /// <summary>重置字段到初始值。Resets the field to its initial value.</summary>
    [Description("@#resetField")]
    public extern void ResetField();

    /// <summary>写入字段值。Writes the field value.</summary>
    /// <param name="value">要写入的值。The value to write.</param>
    [Description("@#setValue")]
    public extern void SetValue(TValue value);

    /// <summary>写入字段 touched 状态。Writes the field touched state.</summary>
    /// <param name="touched">是否已交互。Whether the field was touched.</param>
    [Description("@#setTouched")]
    public extern void SetTouched(bool touched);

    /// <summary>写入字段错误信息。Writes the field error messages.</summary>
    /// <param name="errors">错误消息数组。The error messages.</param>
    [Description("@#setErrors")]
    public extern void SetErrors(string[] errors);
}

/// <summary>
/// 字段数组中的一项；<c>key</c> 在数组重排后保持稳定，可直接用于渲染 key。
/// One entry of a field array; <c>key</c> stays stable across reordering and can be used as the render key.
/// </summary>
/// <typeparam name="TItem">数组元素类型。The array element type.</typeparam>
[ECMAScript]
[Description("@#")]
public abstract class VeeValidateFieldArrayEntry<TItem>
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VeeValidateFieldArrayEntry()
    {
    }

    /// <summary>稳定的条目键。The stable entry key.</summary>
    [Description("@#key")]
    public extern Number Key { get; }

    /// <summary>条目值（可写计算引用）。The entry value as a writable computed ref.</summary>
    [Description("@#value")]
    public extern Vue.IVueRef<TItem> Value { get; }

    /// <summary>是否为首项。Whether this is the first entry.</summary>
    [Description("@#isFirst")]
    public extern bool IsFirst { get; }

    /// <summary>是否为末项。Whether this is the last entry.</summary>
    [Description("@#isLast")]
    public extern bool IsLast { get; }
}

/// <summary>
/// <c>useFieldArray()</c> 返回的数组字段上下文投影。
/// The array-field-context projection returned by <c>useFieldArray()</c>.
/// </summary>
/// <typeparam name="TItem">数组元素类型。The array element type.</typeparam>
[ECMAScript]
[Description("@#")]
public abstract class VeeValidateFieldArrayReturn<TItem>
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VeeValidateFieldArrayReturn()
    {
    }

    /// <summary>当前条目列表。The current entry list.</summary>
    [Description("@#fields")]
    public extern Vue.VueShallowRef<VeeValidateFieldArrayEntry<TItem>[]> Fields { get; }

    /// <summary>数组字段路径。The array field path.</summary>
    [Description("@#path")]
    public extern Vue.VueComputedRef<string> Path { get; }

    /// <summary>移除指定下标的条目。Removes the entry at the given index.</summary>
    /// <param name="index">要移除的下标。The index to remove.</param>
    [Description("@#remove")]
    public extern void Remove(Number index);

    /// <summary>在末尾追加一个条目。Appends an entry at the end.</summary>
    /// <param name="item">要追加的元素。The element to append.</param>
    [Description("@#push")]
    public extern void Push(TItem item);

    /// <summary>在开头插入一个条目。Inserts an entry at the beginning.</summary>
    /// <param name="item">要插入的元素。The element to insert.</param>
    [Description("@#prepend")]
    public extern void Prepend(TItem item);

    /// <summary>在指定下标插入一个条目。Inserts an entry at the given index.</summary>
    /// <param name="index">插入位置。The insertion index.</param>
    /// <param name="item">要插入的元素。The element to insert.</param>
    [Description("@#insert")]
    public extern void Insert(Number index, TItem item);

    /// <summary>更新指定下标的条目。Updates the entry at the given index.</summary>
    /// <param name="index">要更新的下标。The index to update.</param>
    /// <param name="item">新的元素值。The new element value.</param>
    [Description("@#update")]
    public extern void Update(Number index, TItem item);

    /// <summary>替换整个数组。Replaces the whole array.</summary>
    /// <param name="items">新的元素集合。The new element collection.</param>
    [Description("@#replace")]
    public extern void Replace(TItem[] items);

    /// <summary>交换两个下标的元素。Swaps the elements at two indices.</summary>
    /// <param name="from">源下标。The source index.</param>
    /// <param name="to">目标下标。The destination index.</param>
    [Description("@#swap")]
    public extern void Swap(Number from, Number to);

    /// <summary>移动一个下标的条目到目标下标。Moves the entry at one index to another.</summary>
    /// <param name="from">源下标。The source index.</param>
    /// <param name="to">目标下标。The destination index.</param>
    [Description("@#move")]
    public extern void Move(Number from, Number to);
}
