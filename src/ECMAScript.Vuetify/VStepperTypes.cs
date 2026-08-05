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
    public VuetifyStepperItemValue[]? AsArray => Value as VuetifyStepperItemValue[];

    public static implicit operator VuetifyStepperItems(VuetifyStepperItemValue[] items)
        => new(items);

    public static implicit operator VuetifyStepperItems(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifyStepperItemValue)item));

    public static implicit operator VuetifyStepperItems(VuetifyStepperItem[] items)
        => new(Array.ConvertAll(items, static item => (VuetifyStepperItemValue)item));

    IEnumerator<VuetifyStepperItemValue> IEnumerable<VuetifyStepperItemValue>.GetEnumerator()
        => ((IEnumerable<VuetifyStepperItemValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyStepperItemValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyStepperItemsCollectionBuilder
{
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
    public string? AsString => Value as string;

    public VuetifyStepperItem? AsItem => Value as VuetifyStepperItem;

    public static implicit operator VuetifyStepperItemValue(string value)
        => new(value);

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
    [Description("@#title")]
    public VueStringNumberValue? Title { get; init; }

    [Description("@#value")]
    public VuetifyGroupModelValue? Value { get; init; }

    [Description("@#subtitle")]
    public VueStringNumberValue? Subtitle { get; init; }

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
    [Description("@#prev")]
    public Action? Prev { get; init; }

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
    [Description("@#canEdit")]
    public bool CanEdit { get; init; }

    [Description("@#hasError")]
    public bool HasError { get; init; }

    [Description("@#hasCompleted")]
    public bool HasCompleted { get; init; }

    [Description("@#title")]
    public VueStringNumberValue? Title { get; init; }

    [Description("@#subtitle")]
    public VueStringNumberValue? Subtitle { get; init; }

    [Description("@#step")]
    public VuetifyGroupModelValue? Step { get; init; }

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
    [Description("@#title")]
    public VueValue? Title { get; init; }

    [Description("@#value")]
    public VuetifyGroupModelValue? Value { get; init; }

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
    [Description("@#props")]
    public VStepperActionButtonProps? Props { get; init; }
}
