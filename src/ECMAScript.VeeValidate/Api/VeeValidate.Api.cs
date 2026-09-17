using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class VeeValidate
{
    // ---------- 表单与字段上下文 Form & field contexts ----------

    /// <summary>
    /// 创建表单上下文；返回值同时是可写状态与提交入口。
    /// Creates the form context; the returned object carries both writable state and submission entries.
    /// </summary>
    /// <param name="options">表单选项。The form options.</param>
    /// <typeparam name="TValues">表单值契约。The form value contract.</typeparam>
    [Description("@#useForm")]
    public extern static VeeValidateFormReturn<TValues> UseForm<TValues>(VeeValidateFormOptions<TValues>? options = null);

    /// <summary>
    /// 读取由祖先 <c>useForm()</c> 或 <c>&lt;Form&gt;</c> 提供的表单上下文。
    /// Reads the form context provided by an ancestor <c>useForm()</c> or <c>&lt;Form&gt;</c>.
    /// </summary>
    /// <typeparam name="TValues">表单值契约。The form value contract.</typeparam>
    [Description("@#useFormContext")]
    public extern static VeeValidateFormReturn<TValues> UseFormContext<TValues>();

    /// <summary>
    /// 创建字段上下文；<c>rules</c> 使用 vee-validate 的字符串规则语法（例如 <c>required|email</c>）。
    /// Creates a field context; <c>rules</c> uses the vee-validate string rule syntax (for example <c>required|email</c>).
    /// </summary>
    /// <param name="path">字段路径。The field path.</param>
    /// <param name="rules">字符串规则（可选）。The string rules (optional).</param>
    /// <param name="options">字段选项。The field options.</param>
    /// <typeparam name="TValue">字段值类型。The field value type.</typeparam>
    [Description("@#useField")]
    public extern static VeeValidateFieldReturn<TValue> UseField<TValue>(
        string path,
        string? rules = null,
        VeeValidateFieldOptions<TValue>? options = null);

    /// <summary>
    /// 创建数组字段上下文；条目 key 在重排后保持稳定。
    /// Creates an array field context; entry keys stay stable across reordering.
    /// </summary>
    /// <param name="arrayPath">数组字段路径。The array field path.</param>
    /// <typeparam name="TItem">数组元素类型。The array element type.</typeparam>
    [Description("@#useFieldArray")]
    public extern static VeeValidateFieldArrayReturn<TItem> UseFieldArray<TItem>(string arrayPath);

    /// <summary>
    /// 修改全局默认配置；影响之后创建的所有字段。
    /// Changes the global default configuration; it affects every field created afterwards.
    /// </summary>
    /// <param name="config">配置值。The configuration values.</param>
    [Description("@#configure")]
    public extern static void Configure(VeeValidateConfig config);

    // ---------- 状态读取 State reads ----------

    /// <summary>
    /// 读取单个字段的当前错误消息。
    /// Reads the current error message of a single field.
    /// </summary>
    /// <param name="path">字段路径；为空时取当前字段上下文。The field path; when omitted the current field context is used.</param>
    [Description("@#useFieldError")]
    public extern static Vue.VueComputedRef<string?> UseFieldError(string? path = null);

    /// <summary>
    /// 读取单个字段的当前值。
    /// Reads the current value of a single field.
    /// </summary>
    /// <param name="path">字段路径；为空时取当前字段上下文。The field path; when omitted the current field context is used.</param>
    /// <typeparam name="TValue">字段值类型。The field value type.</typeparam>
    [Description("@#useFieldValue")]
    public extern static Vue.VueComputedRef<TValue> UseFieldValue<TValue>(string? path = null);

    /// <summary>
    /// 读取表单的全部错误（按字段路径索引）。
    /// Reads all form errors, keyed by field path.
    /// </summary>
    [Description("@#useFormErrors")]
    public extern static Vue.VueComputedRef<Vue.VueDictionary<string>> UseFormErrors();

    /// <summary>
    /// 读取表单的全部当前值。
    /// Reads all current form values.
    /// </summary>
    /// <typeparam name="TValues">表单值契约。The form value contract.</typeparam>
    [Description("@#useFormValues")]
    public extern static Vue.VueComputedRef<TValues> UseFormValues<TValues>();

    /// <summary>
    /// 字段值是否已被修改。
    /// Whether the field value changed from its initial value.
    /// </summary>
    /// <param name="path">字段路径。The field path.</param>
    [Description("@#useIsFieldDirty")]
    public extern static Vue.VueComputedRef<bool> UseIsFieldDirty(string? path = null);

    /// <summary>
    /// 字段是否已被交互过。
    /// Whether the field was touched.
    /// </summary>
    /// <param name="path">字段路径。The field path.</param>
    [Description("@#useIsFieldTouched")]
    public extern static Vue.VueComputedRef<bool> UseIsFieldTouched(string? path = null);

    /// <summary>
    /// 字段是否通过校验。
    /// Whether the field passed validation.
    /// </summary>
    /// <param name="path">字段路径。The field path.</param>
    [Description("@#useIsFieldValid")]
    public extern static Vue.VueComputedRef<bool> UseIsFieldValid(string? path = null);

    /// <summary>
    /// 表单是否已被修改。
    /// Whether the form was modified.
    /// </summary>
    [Description("@#useIsFormDirty")]
    public extern static Vue.VueComputedRef<bool> UseIsFormDirty();

    /// <summary>
    /// 表单是否有字段被交互过。
    /// Whether any field of the form was touched.
    /// </summary>
    [Description("@#useIsFormTouched")]
    public extern static Vue.VueComputedRef<bool> UseIsFormTouched();

    /// <summary>
    /// 表单是否全部通过校验。
    /// Whether the whole form passed validation.
    /// </summary>
    [Description("@#useIsFormValid")]
    public extern static Vue.VueComputedRef<bool> UseIsFormValid();

    /// <summary>
    /// 表单是否正在提交。
    /// Whether the form is submitting.
    /// </summary>
    [Description("@#useIsSubmitting")]
    public extern static Vue.VueComputedRef<bool> UseIsSubmitting();

    /// <summary>
    /// 表单是否正在校验。
    /// Whether the form is validating.
    /// </summary>
    [Description("@#useIsValidating")]
    public extern static Vue.VueComputedRef<bool> UseIsValidating();

    /// <summary>
    /// 已尝试提交的次数。
    /// The number of submission attempts.
    /// </summary>
    [Description("@#useSubmitCount")]
    public extern static Vue.VueComputedRef<Number> UseSubmitCount();

    // ---------- 状态写入 State writes ----------

    /// <summary>
    /// 获取写入字段错误消息的处理器。
    /// Gets a handler that writes a field's error message.
    /// </summary>
    /// <param name="path">字段路径；为空时取当前字段上下文。The field path; when omitted the current field context is used.</param>
    [Description("@#useSetFieldError")]
    public extern static VeeValidateSetErrorHandler UseSetFieldError(string? path = null);

    /// <summary>
    /// 获取写入字段 touched 状态的处理器。
    /// Gets a handler that writes a field's touched state.
    /// </summary>
    /// <param name="path">字段路径；为空时取当前字段上下文。The field path; when omitted the current field context is used.</param>
    [Description("@#useSetFieldTouched")]
    public extern static VeeValidateSetTouchedHandler UseSetFieldTouched(string? path = null);

    /// <summary>
    /// 获取写入字段值的处理器。
    /// Gets a handler that writes a field value.
    /// </summary>
    /// <param name="path">字段路径；为空时取当前字段上下文。The field path; when omitted the current field context is used.</param>
    /// <typeparam name="TValue">字段值类型。The field value type.</typeparam>
    [Description("@#useSetFieldValue")]
    public extern static VeeValidateSetValueHandler<TValue> UseSetFieldValue<TValue>(string? path = null);

    /// <summary>
    /// 获取批量写入表单错误的处理器。
    /// Gets a handler that writes form errors in bulk.
    /// </summary>
    [Description("@#useSetFormErrors")]
    public extern static VeeValidateSetFormErrorsHandler UseSetFormErrors();

    /// <summary>
    /// 获取批量写入表单 touched 状态的处理器。
    /// Gets a handler that writes the form touched state in bulk.
    /// </summary>
    [Description("@#useSetFormTouched")]
    public extern static VeeValidateSetFormTouchedHandler UseSetFormTouched();

    /// <summary>
    /// 获取批量写入表单值的处理器。
    /// Gets a handler that writes form values in bulk.
    /// </summary>
    /// <typeparam name="TValues">表单值契约。The form value contract.</typeparam>
    [Description("@#useSetFormValues")]
    public extern static VeeValidateSetFormValuesHandler<TValues> UseSetFormValues<TValues>();

    /// <summary>
    /// 获取重置表单的处理器。
    /// Gets a handler that resets the form.
    /// </summary>
    [Description("@#useResetForm")]
    public extern static VeeValidateResetFormHandler UseResetForm();

    // ---------- 校验与提交 Validation & submission ----------

    /// <summary>
    /// 获取触发单个字段校验的处理器。
    /// Gets a handler that validates a single field.
    /// </summary>
    /// <param name="path">字段路径；为空时取当前字段上下文。The field path; when omitted the current field context is used.</param>
    [Description("@#useValidateField")]
    public extern static VeeValidateValidateHandler UseValidateField(string? path = null);

    /// <summary>
    /// 获取触发整个表单校验的处理器。
    /// Gets a handler that validates the whole form.
    /// </summary>
    [Description("@#useValidateForm")]
    public extern static VeeValidateValidateHandler UseValidateForm();

    /// <summary>
    /// 将回调注册为祖先表单的提交处理器；返回的函数可直接绑定到提交事件。
    /// Registers a callback as the ancestor form's submit handler; the returned function can be bound to the submit event.
    /// </summary>
    /// <param name="callback">校验通过时的提交回调。The callback invoked for valid submissions.</param>
    /// <typeparam name="TValues">表单值契约。The form value contract.</typeparam>
    [Description("@#useSubmitForm")]
    public extern static VeeValidateSubmitHandler UseSubmitForm<TValues>(VeeValidateSubmitCallback<TValues> callback);

    /// <summary>
    /// 将异步回调注册为祖先表单的提交处理器；返回的 promise 会被表单等待。
    /// Registers an async callback as the ancestor form's submit handler; its promise is awaited by the form.
    /// </summary>
    /// <param name="callback">校验通过时的异步提交回调。The async callback invoked for valid submissions.</param>
    /// <typeparam name="TValues">表单值契约。The form value contract.</typeparam>
    [Description("@#useSubmitForm")]
    public extern static VeeValidateSubmitHandler UseSubmitForm<TValues>(Func<TValues, PromiseResult> callback);
}
