namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VSelectionControl 默认插槽所暴露的插槽上下文。
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
/// Vuetify 选择控件标签插槽所暴露的上下文。
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
/// 传递给 Vuetify VSelectionControl 输入插槽以连接自定义控件的属性对象。
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
/// Vuetify VSelectionControl 输入插槽所暴露的插槽上下文。
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
/// Vuetify VSwitch 滑块和轨道插槽所暴露的上下文。
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
/// 组合 Vuetify VInput 和 VSelectionControl 插槽的组件所暴露的默认插槽上下文。
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
/// Vuetify 颜色组合函数在插槽作用域引用中返回的 CSS 属性集合。
/// CSS property bag returned by Vuetify color composables in slot scoped refs.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyCssProperties : VueDictionary<VueStringNumberValue>
{
}
