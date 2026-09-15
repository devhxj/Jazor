using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 强制选择模式。
/// Vuetify mandatory selection mode.
/// </summary>
[String]
public enum VuetifyMandatoryMode
{
    /// <summary>
    /// 强制保持至少一个条目处于选中状态；上游取值为 “force”。
    /// </summary>
    [Description("@#force")]
    Force
}

/// <summary>
/// Vuetify 项目标签位置。
/// Vuetify item label position.
/// </summary>
[String]
public enum VuetifyItemLabelPosition
{
    /// <summary>
    /// 顶部；上游取值为 “top”。
    /// </summary>
    [Description("@#top")]
    Top,

    /// <summary>
    /// 底部；上游取值为 “bottom”。
    /// </summary>
    [Description("@#bottom")]
    Bottom
}

/// <summary>
/// Vuetify 箭头显示模式。
/// Vuetify show-arrows mode.
/// </summary>
[String]
public enum VuetifyShowArrowsMode
{
    /// <summary>
    /// 始终启用或显示；上游取值为 “always”。
    /// </summary>
    [Description("@#always")]
    Always,

    /// <summary>
    /// 仅在桌面显示模式显示箭头；上游取值为 “desktop”。
    /// </summary>
    [Description("@#desktop")]
    Desktop,

    /// <summary>
    /// 仅在移动显示模式显示箭头；上游取值为 “mobile”。
    /// </summary>
    [Description("@#mobile")]
    Mobile
}

/// <summary>
/// 用于 VChipGroup.ValueComparator、VItemGroup.ValueComparator、VSelectionControl.ValueComparator、VSelectionControlGroup.ValueComparator 的回调签名。
/// Apply a custom comparison algorithm to compare **model-value** and values contains in the **items** prop.
/// 用于比较条目值的函数。
/// Value comparator for item selection.
/// </summary>
public delegate bool VuetifyValueComparator(VueValue? first, VueValue? second);

/// <summary>
/// 用于 VBottomNavigation.Mandatory、VBtnToggle.Mandatory、VCarousel.Mandatory、VChipGroup.Mandatory、VItemGroup.Mandatory、VSlideGroup.Mandatory、VStepper.Mandatory、VStepperVertical.Mandatory、VWindow.Mandatory 的参数类型。
/// Forces at least one item to always be selected (if available).
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyMandatoryValue(bool, VuetifyMandatoryMode)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VuetifyMandatoryMode 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyMandatoryMode? AsMode
        => Value is VuetifyMandatoryMode value ? value : default(VuetifyMandatoryMode?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyMandatoryValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyMandatoryValue(bool value)
        => new(value);

    /// <summary>
    /// 将 VuetifyMandatoryMode 值转换为 VuetifyMandatoryValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyMandatoryValue(VuetifyMandatoryMode value)
        => new(value);
}

/// <summary>
/// 用于 VChipGroup.ShowArrows、VSlideGroup.ShowArrows 的参数类型。
/// Force the display of the pagination arrows.
/// Change when the overflow arrow indicators are shown. By **default**, arrows *always* display on Desktop when the container is overflowing. When the container overflows on mobile, arrows are not shown by default. A **show-arrows** value of `true` allows these arrows to show on Mobile if the container overflowing. A value of `desktop` *always* displays arrows on Desktop while a value of `mobile` always displays arrows on Mobile. A value of `always` always displays arrows on Desktop *and* Mobile. Use **never** to turn arrows off. Find more information on how to customize breakpoint thresholds on the [breakpoints page](/customizing/breakpoints).
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyShowArrowsValue(bool, VuetifyShowArrowsMode)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VuetifyShowArrowsMode 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyShowArrowsMode? AsMode
        => Value is VuetifyShowArrowsMode value ? value : default(VuetifyShowArrowsMode?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyShowArrowsValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyShowArrowsValue(bool value)
        => new(value);

    /// <summary>
    /// 将 VuetifyShowArrowsMode 值转换为 VuetifyShowArrowsValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyShowArrowsValue(VuetifyShowArrowsMode value)
        => new(value);
}

/// <summary>
/// C# 联合参数，允许 VuetifyGroupModelValue[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyGroupModelValuesCollectionBuilder), nameof(VuetifyGroupModelValuesCollectionBuilder.Create))]
public readonly union VuetifyGroupModelValues(VuetifyGroupModelValue[]) : IEnumerable<VuetifyGroupModelValue>
{
    /// <summary>
    /// 读取当前值的 VuetifyGroupModelValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyGroupModelValue[]? AsArray => Value as VuetifyGroupModelValue[];

    /// <summary>
    /// 将 VuetifyGroupModelValue[] 值转换为 VuetifyGroupModelValues，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValues(VuetifyGroupModelValue[] values)
        => new(values);

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyGroupModelValue)value));

    /// <summary>
    /// 将 Number[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyGroupModelValue)value));

    /// <summary>
    /// 将 bool[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValues(bool[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyGroupModelValue)value));

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyGroupModelValue)value));

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValues(double[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyGroupModelValue)value));

    IEnumerator<VuetifyGroupModelValue> IEnumerable<VuetifyGroupModelValue>.GetEnumerator()
        => ((IEnumerable<VuetifyGroupModelValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyGroupModelValue>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyGroupModelValuesCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    public static VuetifyGroupModelValues Create(ReadOnlySpan<VuetifyGroupModelValue> values)
        => values.ToArray();
}

/// <summary>
/// 用于 VBottomNavigation.ModelValue、VBottomNavigation.ModelValueChanged、VBtnToggle.ModelValue、VBtnToggle.ModelValueChanged、VCarousel.ModelValue、VCarousel.ModelValueChanged、VChip.Value、VChipGroup.ModelValue、VChipGroup.ModelValueChanged、VItemGroup.ModelValue、VItemGroup.ModelValueChanged、VSelectionControl.ModelValue、VSelectionControl.ModelValueChanged、VSelectionControl.Value、VSelectionControl.TrueValue、VSelectionControl.FalseValue、VSelectionControlGroup.ModelValue、VSelectionControlGroup.ModelValueChanged、VSlideGroup.ModelValue、VSlideGroup.ModelValueChanged、VStepper.ModelValue、VStepper.ModelValueChanged、VStepperVertical.ModelValue、VStepperVertical.ModelValueChanged、VTabsWindow.ModelValue、VTabsWindow.ModelValueChanged、VTabsWindowItem.Value、VWindow.ModelValue、VWindow.ModelValueChanged 的参数类型。
/// The v-model value of the component. If component supports the **multiple** prop, this defaults to an empty array.
/// 模型值变化时触发的事件。
/// Event fired when model value changes.
/// 激活项变更时的回调。
/// Callback invoked when the active item changes.
/// The value used when a child of a [v-chip-group](/components/chip-groups).
/// 绑定值变更回调。
/// Callback invoked when the bound value changes.
/// 绑定值变化时的回调。
/// Callback when the bound value changes.
/// 绑定值变更时触发的回调。
/// Callback invoked when the bound value changes.
/// The value used when the component is selected in a group. If not provided, a unique ID will be used.
/// Sets value for truthy state.
/// Sets value for falsy state.
/// 选中值变更回调。
/// Callback when the selected value changes.
/// 选中步骤变化时触发的回调。
/// Callback invoked when the selected step changes.
/// Controls expanded panel(s). Defaults to an empty array when using **multiple** prop. It is recommended to set unique `value` prop for the panels inside, otherwise index is used instead.
/// 模型值变化事件。
/// Model value changed event.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyGroupModelValue(
    string,
    Number,
    bool,
    Symbol,
    VueProps,
    VuetifyGroupModelValues)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 Symbol 分支；不属于该分支时返回 null。
    /// </summary>
    public Symbol? AsSymbol => Value as Symbol;

    /// <summary>
    /// 读取当前值的 VueProps 分支；不属于该分支时返回 null。
    /// </summary>
    public VueProps? AsObject => Value as VueProps;

    /// <summary>
    /// 读取当前值的 VuetifyGroupModelValues 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyGroupModelValues? AsValues
        => Value is VuetifyGroupModelValues value ? value : default(VuetifyGroupModelValues?);

    /// <summary>
    /// 将 string 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(string value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(Number value)
        => new(value);

    /// <summary>
    /// 将 bool 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(bool value)
        => new(value);

    /// <summary>
    /// 将 Symbol 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(Symbol value)
        => new(value);

    /// <summary>
    /// 将 VueProps 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(VueProps value)
        => new(value);

    /// <summary>
    /// 将 VueDictionary 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(VueDictionary value)
        => new(value);

    /// <summary>
    /// 将 VuetifyGroupModelValues 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(VuetifyGroupModelValues value)
        => new(value);

    /// <summary>
    /// 将 VuetifyGroupModelValue[] 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(VuetifyGroupModelValue[] value)
        => new((VuetifyGroupModelValues)value);

    /// <summary>
    /// 将 string[] 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(string[] value)
        => new((VuetifyGroupModelValues)value);

    /// <summary>
    /// 将 Number[] 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(Number[] value)
        => new((VuetifyGroupModelValues)value);

    /// <summary>
    /// 将 bool[] 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(bool[] value)
        => new((VuetifyGroupModelValues)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 int[] 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(int[] value)
        => new((VuetifyGroupModelValues)value);

    /// <summary>
    /// 将 double[] 值转换为 VuetifyGroupModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyGroupModelValue(double[] value)
        => new((VuetifyGroupModelValues)value);
}