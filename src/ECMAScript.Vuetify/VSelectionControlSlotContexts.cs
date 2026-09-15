namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VSelectionControl 默认插槽所暴露的插槽上下文。
/// Default slot context exposed by Vuetify VSelectionControl.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSelectionControlDefaultSlotContext
{
    /// <summary>
    /// Vuetify 为背景颜色计算的 CSS 类名。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#backgroundColorClasses")]
    public IVueRef<string[]>? BackgroundColorClasses { get; init; }

    /// <summary>
    /// Vuetify 为背景颜色计算的内联样式。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#backgroundColorStyles")]
    public IVueRef<VuetifyCssProperties>? BackgroundColorStyles { get; init; }
}

/// <summary>
/// Vuetify 选择控件标签插槽所暴露的上下文。
/// Context exposed by Vuetify selection-control label slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSelectionControlLabelSlotContext
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
}

/// <summary>
/// 传递给 Vuetify VSelectionControl 输入插槽以连接自定义控件的属性对象。
/// Props passed to Vuetify VSelectionControl input slots for wiring custom controls.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSelectionControlInputProps
{
    /// <summary>
    /// 输入元素失去焦点时调用的处理函数。
    /// </summary>
    [Description("@#onBlur")]
    public Action<EventRef>? OnBlur { get; init; }

    /// <summary>
    /// 输入元素获得焦点时调用的处理函数。
    /// </summary>
    [Description("@#onFocus")]
    public Action<FocusEvent>? OnFocus { get; init; }

    /// <summary>
    /// 当前条目的标识，用于组件内部关联及状态更新。
    /// </summary>
    [Description("@#id")]
    public string? Id { get; init; }
}

/// <summary>
/// Vuetify VSelectionControl 输入插槽所暴露的插槽上下文。
/// Input slot context exposed by Vuetify VSelectionControl.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSelectionControlInputSlotContext
{
    /// <summary>
    /// 控件内部正在编辑的模型引用；自定义插槽应与此引用同步值。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#model")]
    public VueWritableComputedRef<bool>? Model { get; init; }

    /// <summary>
    /// Vuetify 为文字颜色计算的 CSS 类名。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#textColorClasses")]
    public IVueRef<string[]>? TextColorClasses { get; init; }

    /// <summary>
    /// Vuetify 为文字颜色计算的内联样式。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#textColorStyles")]
    public IVueRef<VuetifyCssProperties>? TextColorStyles { get; init; }

    /// <summary>
    /// Vuetify 为背景颜色计算的 CSS 类名。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#backgroundColorClasses")]
    public IVueRef<string[]>? BackgroundColorClasses { get; init; }

    /// <summary>
    /// Vuetify 为背景颜色计算的内联样式。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#backgroundColorStyles")]
    public IVueRef<VuetifyCssProperties>? BackgroundColorStyles { get; init; }

    /// <summary>
    /// Vuetify 已创建的原生输入节点，供自定义插槽组合使用。
    /// </summary>
    [Description("@#inputNode")]
    public IVNode? InputNode { get; init; }

    /// <summary>
    /// 当前状态所用的图标，支持 Vuetify 图标别名或相应图标值。
    /// </summary>
    [Description("@#icon")]
    public VuetifyIconValue? Icon { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VSelectionControlInputProps? Props { get; init; }
}

/// <summary>
/// Vuetify VSwitch 滑块和轨道插槽所暴露的上下文。
/// Context exposed by Vuetify VSwitch thumb and track slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSwitchSlotContext
{
    /// <summary>
    /// 控件内部正在编辑的模型引用；自定义插槽应与此引用同步值。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#model")]
    public IVueRef<bool>? Model { get; init; }

    /// <summary>
    /// 当前校验是否通过；可空状态中的 null 表示尚无确定校验结果。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isValid")]
    public VueComputedRef<bool?>? IsValid { get; init; }

    /// <summary>
    /// 当前状态所用的图标，支持 Vuetify 图标别名或相应图标值。
    /// </summary>
    [Description("@#icon")]
    public VuetifyIconValue? Icon { get; init; }
}

/// <summary>
/// 组合 Vuetify VInput 和 VSelectionControl 插槽的组件所暴露的默认插槽上下文。
/// Default slot context exposed by components that combine Vuetify VInput and VSelectionControl slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSelectionControlInputDefaultSlotContext : VInputSlotContext
{
    /// <summary>
    /// Vuetify 为背景颜色计算的 CSS 类名。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#backgroundColorClasses")]
    public IVueRef<string[]>? BackgroundColorClasses { get; init; }

    /// <summary>
    /// Vuetify 为背景颜色计算的内联样式。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#backgroundColorStyles")]
    public IVueRef<VuetifyCssProperties>? BackgroundColorStyles { get; init; }
}

/// <summary>
/// Vuetify 颜色组合函数在插槽作用域引用中返回的 CSS 属性集合。
/// CSS property bag returned by Vuetify color composables in slot scoped refs.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyCssProperties : VueDictionary<VueStringNumberValue>
{
}
