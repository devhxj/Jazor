using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// 步骤条项目列表的擦除值联合类型。
/// Erased value union for stepper item lists.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyStepperItemsCollectionBuilder), nameof(VuetifyStepperItemsCollectionBuilder.Create))]
public readonly union VuetifyStepperItems(VuetifyStepperItemValue[]) : IEnumerable<VuetifyStepperItemValue>
{
    /// <summary>
    /// 读取当前值的 VuetifyStepperItemValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyStepperItemValue[]? AsArray => Value as VuetifyStepperItemValue[];

    /// <summary>
    /// 将 VuetifyStepperItemValue[] 值转换为 VuetifyStepperItems，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStepperItems(VuetifyStepperItemValue[] items)
        => new(items);

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStepperItems(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifyStepperItemValue)item));

    /// <summary>
    /// 将 VuetifyStepperItem[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStepperItems(VuetifyStepperItem[] items)
        => new(Array.ConvertAll(items, static item => (VuetifyStepperItemValue)item));

    IEnumerator<VuetifyStepperItemValue> IEnumerable<VuetifyStepperItemValue>.GetEnumerator()
        => ((IEnumerable<VuetifyStepperItemValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyStepperItemValue>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyStepperItemsCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    public static VuetifyStepperItems Create(ReadOnlySpan<VuetifyStepperItemValue> items)
        => items.ToArray();
}

/// <summary>
/// 单个步骤条项目值的擦除值联合类型。
/// Erased value union for a single stepper item value.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyStepperItemValue(string, VuetifyStepperItem)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 VuetifyStepperItem 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyStepperItem? AsItem => Value as VuetifyStepperItem;

    /// <summary>
    /// 将 string 值转换为 VuetifyStepperItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStepperItemValue(string value)
        => new(value);

    /// <summary>
    /// 将 VuetifyStepperItem 值转换为 VuetifyStepperItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStepperItemValue(VuetifyStepperItem value)
        => new(value);
}

/// <summary>
/// Vuetify VStepper 的 items 属性所接受的项目对象。未知键通过继承的字典表面保持可用。
/// Item object accepted by Vuetify VStepper's items prop. Unknown item keys remain available through the inherited dictionary surface.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyStepperItem : VueDictionary
{
    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public VueStringNumberValue? Title { get; init; }

    /// <summary>
    /// 此条目关联的模型值，用于渲染和更新回调。
    /// </summary>
    [Description("@#value")]
    public VuetifyGroupModelValue? Value { get; init; }

    /// <summary>
    /// 条目标题下方的补充文本。
    /// </summary>
    [Description("@#subtitle")]
    public VueStringNumberValue? Subtitle { get; init; }

    /// <summary>
    /// 是否禁止此项的用户交互。
    /// </summary>
    [Description("@#disabled")]
    public bool? Disabled { get; init; }
}

/// <summary>
/// Vuetify VStepper 默认和操作插槽所暴露的插槽上下文。
/// Default/actions slot context exposed by Vuetify VStepper.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VStepperNavigationSlotContext
{
    /// <summary>
    /// 移动到组中的上一个可选项。
    /// </summary>
    [Description("@#prev")]
    public Action? Prev { get; init; }

    /// <summary>
    /// 移动到组中的下一个可选项。
    /// </summary>
    [Description("@#next")]
    public Action? Next { get; init; }
}

/// <summary>
/// Vuetify VStepper 通过 VStepperItem 暴露的头部/图标/标题/副标题插槽上下文。
/// Header/icon/title/subtitle slot context exposed by Vuetify VStepperItem through VStepper.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VStepperItemSlotContext
{
    /// <summary>
    /// 是否允许用户进入并编辑此步骤。
    /// </summary>
    [Description("@#canEdit")]
    public bool CanEdit { get; init; }

    /// <summary>
    /// 当前步骤是否处于错误状态。
    /// </summary>
    [Description("@#hasError")]
    public bool HasError { get; init; }

    /// <summary>
    /// 当前步骤是否已完成。
    /// </summary>
    [Description("@#hasCompleted")]
    public bool HasCompleted { get; init; }

    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public VueStringNumberValue? Title { get; init; }

    /// <summary>
    /// 条目标题下方的补充文本。
    /// </summary>
    [Description("@#subtitle")]
    public VueStringNumberValue? Subtitle { get; init; }

    /// <summary>
    /// 当前步骤关联的模型值，用于识别导航目标。
    /// </summary>
    [Description("@#step")]
    public VuetifyGroupModelValue? Step { get; init; }

    /// <summary>
    /// 此条目关联的模型值，用于渲染和更新回调。
    /// </summary>
    [Description("@#value")]
    public VuetifyGroupModelValue? Value { get; init; }
}

/// <summary>
/// Vuetify VStepper 用于窗口内容的项目插槽上下文。
/// Item slot context exposed by Vuetify VStepper for window content.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VStepperContentItemSlotContext
{
    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public VueValue? Title { get; init; }

    /// <summary>
    /// 此条目关联的模型值，用于渲染和更新回调。
    /// </summary>
    [Description("@#value")]
    public VuetifyGroupModelValue? Value { get; init; }

    /// <summary>
    /// 调用方提供的原始数据，供自定义渲染读取业务字段。
    /// </summary>
    [Description("@#raw")]
    public VuetifyStepperItemValue? Raw { get; init; }
}

/// <summary>
/// Vuetify VStepper 上一步/下一步操作按钮插槽所暴露的属性对象。
/// Props object exposed by Vuetify VStepper prev/next action button slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VStepperActionButtonProps : VueProps
{
    /// <summary>
    /// 自定义渲染时应转发的点击处理函数，用于执行组件默认交互。
    /// </summary>
    [Description("@#onClick")]
    public Action? OnClick { get; init; }
}

/// <summary>
/// Vuetify VStepper 上一步/下一步操作按钮的插槽上下文。
/// Prev/next action button slot context exposed by Vuetify VStepper.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VStepperActionButtonSlotContext
{
    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VStepperActionButtonProps? Props { get; init; }
}