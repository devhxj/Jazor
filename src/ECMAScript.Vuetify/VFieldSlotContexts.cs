namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 文本字段和文本区域装饰插槽共享的上下文。
/// Context shared by Vuetify text-field and textarea adornment slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VFieldSlotContext
{
    /// <summary>
    /// 当前条目的标识，用于组件内部关联及状态更新。
    /// </summary>
    [Description("@#id")]
    public string? Id { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VueProps? Props { get; init; }

    /// <summary>
    /// 当前目标是否处于激活状态；在浮层上下文中表示是否显示。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isActive")]
    public IVueRef<bool>? IsActive { get; init; }

    /// <summary>
    /// 控件当前是否拥有输入焦点。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isFocused")]
    public IVueRef<bool>? IsFocused { get; init; }

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
    /// 当前校验是否通过；可空状态中的 null 表示尚无确定校验结果。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isValid")]
    public VueComputedRef<bool?>? IsValid { get; init; }

    /// <summary>
    /// 控件主体元素的响应式引用，供聚焦和定位使用。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#controlRef")]
    public IVueRef<Element?>? ControlRef { get; init; }

    /// <summary>
    /// 将焦点移入当前控件。
    /// </summary>
    [Description("@#focus")]
    public Action? Focus { get; init; }

    /// <summary>
    /// 让当前控件失去焦点。
    /// </summary>
    [Description("@#blur")]
    public Action? Blur { get; init; }
}

/// <summary>
/// Vuetify 字段标签插槽上下文。
/// Context exposed by Vuetify field label slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VFieldLabelSlotContext
{
    /// <summary>
    /// 供当前控件或数据项显示的标签文本。
    /// </summary>
    [Description("@#label")]
    public string? Label { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VueProps? Props { get; init; }

    /// <summary>
    /// 当前目标是否处于激活状态；在浮层上下文中表示是否显示。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isActive")]
    public IVueRef<bool>? IsActive { get; init; }

    /// <summary>
    /// 控件当前是否拥有输入焦点。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isFocused")]
    public IVueRef<bool>? IsFocused { get; init; }

    /// <summary>
    /// 控件主体元素的响应式引用，供聚焦和定位使用。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#controlRef")]
    public IVueRef<Element?>? ControlRef { get; init; }

    /// <summary>
    /// 将焦点移入当前控件。
    /// </summary>
    [Description("@#focus")]
    public Action? Focus { get; init; }

    /// <summary>
    /// 让当前控件失去焦点。
    /// </summary>
    [Description("@#blur")]
    public Action? Blur { get; init; }
}

/// <summary>
/// Vuetify 输入作用域插槽上下文。
/// Context exposed by Vuetify input scoped slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VInputSlotContext
{
    /// <summary>
    /// 当前条目的标识，用于组件内部关联及状态更新。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#id")]
    public VueComputedRef<string>? Id { get; init; }

    /// <summary>
    /// 消息区域的元素 id，可用于控件的 aria-describedby 关联。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#messagesId")]
    public VueComputedRef<string>? MessagesId { get; init; }

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
    public IVueRef<bool>? IsPristine { get; init; }

    /// <summary>
    /// 当前校验是否通过；可空状态中的 null 表示尚无确定校验结果。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isValid")]
    public VueComputedRef<bool?>? IsValid { get; init; }

    /// <summary>
    /// 是否仍有尚未完成的异步校验。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isValidating")]
    public IVueRef<bool>? IsValidating { get; init; }

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

    /// <summary>
    /// 执行当前字段或表单的校验，异步结果见回调的 Promise 返回类型。
    /// </summary>
    [Description("@#validate")]
    public Action? Validate { get; init; }
}

/// <summary>
/// Vuetify 输入详细信息插槽上下文。
/// Context exposed by Vuetify input details slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VInputDetailsSlotContext : VInputSlotContext
{
}

/// <summary>
/// 文本字段计数器插槽上下文。
/// Context exposed by text-field counter slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VCounterSlotContext
{
    /// <summary>
    /// 计数器的最大允许值，用于显示上限提示。
    /// </summary>
    [Description("@#max")]
    public VuetifyCounterValueSource? Max { get; init; }

    /// <summary>
    /// 此条目关联的模型值，用于渲染和更新回调。
    /// </summary>
    [Description("@#value")]
    public string? Value { get; init; }
}