using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 树形视图节点激活策略。
/// Vuetify treeview node activation strategy.
/// </summary>
[String]
public enum VuetifyTreeviewActiveStrategy
{
    [Description("@#single-leaf")]
    SingleLeaf,

    [Description("@#leaf")]
    Leaf,

    [Description("@#independent")]
    Independent,

    [Description("@#single-independent")]
    SingleIndependent
}

/// <summary>
/// Vuetify 树形视图节点选择策略。
/// Vuetify treeview node selection strategy.
/// </summary>
[String]
public enum VuetifyTreeviewSelectStrategy
{
    [Description("@#single-leaf")]
    SingleLeaf,

    [Description("@#leaf")]
    Leaf,

    [Description("@#independent")]
    Independent,

    [Description("@#single-independent")]
    SingleIndependent,

    [Description("@#classic")]
    Classic,

    [Description("@#trunk")]
    Trunk
}

/// <summary>
/// Vuetify 树形视图节点选择状态。
/// Vuetify treeview node selection state.
/// </summary>
[String]
public enum VuetifyTreeviewSelectionState
{
    [Description("@#on")]
    On,

    [Description("@#off")]
    Off,

    [Description("@#indeterminate")]
    Indeterminate
}

/// <summary>
/// Vuetify 树形视图选中值的擦除值联合类型。
/// Erased value union for Vuetify treeview selected values.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyTreeviewValuesCollectionBuilder), nameof(VuetifyTreeviewValuesCollectionBuilder.Create))]
public readonly union VuetifyTreeviewValues(VueValue[]) : IEnumerable<VueValue>
{
    public VueValue[]? AsArray => Value as VueValue[];

    public static implicit operator VuetifyTreeviewValues(VueValue[] values)
        => new(values);

    public static implicit operator VuetifyTreeviewValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyTreeviewValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyTreeviewValues(bool[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyTreeviewValues(VueProps[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyTreeviewValues(VueDictionary[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyTreeviewValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyTreeviewValues(double[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    IEnumerator<VueValue> IEnumerable<VueValue>.GetEnumerator()
        => ((IEnumerable<VueValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyTreeviewValuesCollectionBuilder
{
    public static VuetifyTreeviewValues Create(ReadOnlySpan<VueValue> values)
        => values.ToArray();
}

/// <summary>
/// Vuetify 树形视图项目列表的擦除值联合类型。
/// Erased value union for Vuetify treeview item collections.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyTreeviewItemsCollectionBuilder), nameof(VuetifyTreeviewItemsCollectionBuilder.Create))]
public readonly union VuetifyTreeviewItems(VuetifyTreeviewItemValue[]) : IEnumerable<VuetifyTreeviewItemValue>
{
    public VuetifyTreeviewItemValue[]? AsArray => Value as VuetifyTreeviewItemValue[];

    public static implicit operator VuetifyTreeviewItems(VuetifyTreeviewItemValue[] items)
        => new(items);

    public static implicit operator VuetifyTreeviewItems(VuetifyTreeviewItem[] items)
        => new(Array.ConvertAll(items, static item => (VuetifyTreeviewItemValue)item));

    public static implicit operator VuetifyTreeviewItems(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifyTreeviewItemValue)item));

    IEnumerator<VuetifyTreeviewItemValue> IEnumerable<VuetifyTreeviewItemValue>.GetEnumerator()
        => ((IEnumerable<VuetifyTreeviewItemValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyTreeviewItemValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyTreeviewItemsCollectionBuilder
{
    public static VuetifyTreeviewItems Create(ReadOnlySpan<VuetifyTreeviewItemValue> items)
        => items.ToArray();
}

/// <summary>
/// Vuetify 树形视图单个项目的擦除值联合类型。
/// Erased value union for a single Vuetify treeview item.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyTreeviewItemValue(
    string,
    VuetifyTreeviewItem,
    Number,
    bool,
    VueProps)
{
    public string? AsString => Value as string;

    public VuetifyTreeviewItem? AsItem => Value as VuetifyTreeviewItem;

    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public VueProps? AsObject => Value as VueProps;

    public static implicit operator VuetifyTreeviewItemValue(string value)
        => new(value);

    public static implicit operator VuetifyTreeviewItemValue(VuetifyTreeviewItem value)
        => new(value);

    public static implicit operator VuetifyTreeviewItemValue(Number value)
        => new(value);

    public static implicit operator VuetifyTreeviewItemValue(bool value)
        => new(value);

    public static implicit operator VuetifyTreeviewItemValue(VueProps value)
        => new(value);

    public static implicit operator VuetifyTreeviewItemValue(VueDictionary value)
        => new(value);

    public static implicit operator VuetifyTreeviewItemValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyTreeviewItemValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyTreeviewItemValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyTreeviewItemValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyTreeviewItemValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyTreeviewItemValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyTreeviewItemValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyTreeviewItemValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyTreeviewItemValue(decimal value)
        => new((Number)value);
}

/// <summary>
/// Vuetify 树形视图项目定义。
/// Vuetify treeview item definition.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VuetifyTreeviewItem
{
    [Description("@#title")]
    public VuetifyTextValue? Title { get; init; }

    [Description("@#value")]
    public VueValue? Value { get; init; }

    [Description("@#props")]
    public VuetifySelectItemPropsValue? Props { get; init; }

    [Description("@#children")]
    public VuetifyTreeviewItems? Children { get; init; }

    [Description("@#raw")]
    public VueValue? Raw { get; init; }
}

public delegate IPromise VuetifyTreeviewLoadChildrenCallback(VueValue? item);

public delegate VuetifyTreeviewActiveStrategyDefinition VuetifyTreeviewActiveStrategyFactory(bool mandatory);

public delegate VuetifyTreeviewSelectStrategyDefinition VuetifyTreeviewSelectStrategyFactory(bool mandatory);

/// <summary>
/// Vuetify 树形视图激活策略值的擦除值联合类型。
/// Erased value union for Vuetify treeview active strategy.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyTreeviewActiveStrategyValue(
    VuetifyTreeviewActiveStrategy,
    VuetifyTreeviewActiveStrategyDefinition,
    VuetifyTreeviewActiveStrategyFactory)
{
    public VuetifyTreeviewActiveStrategy? AsName
        => Value is VuetifyTreeviewActiveStrategy value ? value : default(VuetifyTreeviewActiveStrategy?);

    public VuetifyTreeviewActiveStrategyDefinition? AsDefinition
        => Value as VuetifyTreeviewActiveStrategyDefinition;

    public VuetifyTreeviewActiveStrategyFactory? AsFactory
        => Value as VuetifyTreeviewActiveStrategyFactory;

    public static implicit operator VuetifyTreeviewActiveStrategyValue(VuetifyTreeviewActiveStrategy value)
        => new(value);

    public static implicit operator VuetifyTreeviewActiveStrategyValue(VuetifyTreeviewActiveStrategyDefinition value)
        => new(value);

    public static implicit operator VuetifyTreeviewActiveStrategyValue(VuetifyTreeviewActiveStrategyFactory value)
        => new(value);
}

/// <summary>
/// Vuetify 树形视图选择策略值的擦除值联合类型。
/// Erased value union for Vuetify treeview select strategy.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyTreeviewSelectStrategyValue(
    VuetifyTreeviewSelectStrategy,
    VuetifyTreeviewSelectStrategyDefinition,
    VuetifyTreeviewSelectStrategyFactory)
{
    public VuetifyTreeviewSelectStrategy? AsName
        => Value is VuetifyTreeviewSelectStrategy value ? value : default(VuetifyTreeviewSelectStrategy?);

    public VuetifyTreeviewSelectStrategyDefinition? AsDefinition
        => Value as VuetifyTreeviewSelectStrategyDefinition;

    public VuetifyTreeviewSelectStrategyFactory? AsFactory
        => Value as VuetifyTreeviewSelectStrategyFactory;

    public static implicit operator VuetifyTreeviewSelectStrategyValue(VuetifyTreeviewSelectStrategy value)
        => new(value);

    public static implicit operator VuetifyTreeviewSelectStrategyValue(VuetifyTreeviewSelectStrategyDefinition value)
        => new(value);

    public static implicit operator VuetifyTreeviewSelectStrategyValue(VuetifyTreeviewSelectStrategyFactory value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public sealed record VuetifyTreeviewActiveStrategyDefinition : VueProps
{
    [Description("@#activate")]
    public VuetifyTreeviewActiveStrategyActivateCallback? Activate { get; init; }

    [Description("@#in")]
    public VuetifyTreeviewActiveStrategyTransformInCallback? In { get; init; }

    [Description("@#out")]
    public VuetifyTreeviewActiveStrategyTransformOutCallback? Out { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record VuetifyTreeviewSelectStrategyDefinition : VueProps
{
    [Description("@#select")]
    public VuetifyTreeviewSelectStrategySelectCallback? Select { get; init; }

    [Description("@#in")]
    public VuetifyTreeviewSelectStrategyTransformInCallback? In { get; init; }

    [Description("@#out")]
    public VuetifyTreeviewSelectStrategyTransformOutCallback? Out { get; init; }
}

public delegate Set<VueValue> VuetifyTreeviewActiveStrategyActivateCallback(VuetifyTreeviewActiveStrategyActivateContext context);

public delegate Set<VueValue> VuetifyTreeviewActiveStrategyTransformInCallback(
    VuetifyTreeviewValues? value,
    Map<VueValue, VueValue[]> children,
    Map<VueValue, VueValue> parents);

public delegate VuetifyTreeviewValues VuetifyTreeviewActiveStrategyTransformOutCallback(
    Set<VueValue> value,
    Map<VueValue, VueValue[]> children,
    Map<VueValue, VueValue> parents);

public delegate Map<VueValue, VuetifyTreeviewSelectionState> VuetifyTreeviewSelectStrategySelectCallback(
    VuetifyTreeviewSelectStrategySelectContext context);

public delegate Map<VueValue, VuetifyTreeviewSelectionState> VuetifyTreeviewSelectStrategyTransformInCallback(
    VuetifyTreeviewValues? value,
    Map<VueValue, VueValue[]> children,
    Map<VueValue, VueValue> parents);

public delegate VuetifyTreeviewValues VuetifyTreeviewSelectStrategyTransformOutCallback(
    Map<VueValue, VuetifyTreeviewSelectionState> value,
    Map<VueValue, VueValue[]> children,
    Map<VueValue, VueValue> parents);

[ECMAScript]
[Description("@#")]
public sealed record VuetifyTreeviewActiveStrategyActivateContext : VueProps
{
    [Description("@#id")]
    public VueValue? Id { get; init; }

    [Description("@#value")]
    public bool Value { get; init; }

    [Description("@#activated")]
    public Set<VueValue>? Activated { get; init; }

    [Description("@#children")]
    public Map<VueValue, VueValue[]>? Children { get; init; }

    [Description("@#parents")]
    public Map<VueValue, VueValue>? Parents { get; init; }

    [Description("@#event")]
    public Event? Event { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record VuetifyTreeviewSelectStrategySelectContext : VueProps
{
    [Description("@#id")]
    public VueValue? Id { get; init; }

    [Description("@#value")]
    public bool Value { get; init; }

    [Description("@#selected")]
    public Map<VueValue, VuetifyTreeviewSelectionState>? Selected { get; init; }

    [Description("@#children")]
    public Map<VueValue, VueValue[]>? Children { get; init; }

    [Description("@#parents")]
    public Map<VueValue, VueValue>? Parents { get; init; }

    [Description("@#event")]
    public Event? Event { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record VuetifyTreeviewClickPayload : VueProps
{
    [Description("@#id")]
    public VueValue? Id { get; init; }

    [Description("@#value")]
    public bool Value { get; init; }

    [Description("@#path")]
    public VueValue[]? Path { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record VuetifyTreeviewInternalItem : VueProps
{
    [Description("@#type")]
    public string? Type { get; init; }

    [Description("@#title")]
    public VuetifyTextValue? Title { get; init; }

    [Description("@#value")]
    public VueValue? Value { get; init; }

    [Description("@#props")]
    public VuetifyItemProps? Props { get; init; }

    [Description("@#children")]
    public VuetifyTreeviewInternalItem[]? Children { get; init; }

    [Description("@#raw")]
    public VueValue? Raw { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record VTreeviewNodeSlotContext
{
    [Description("@#isActive")]
    public bool IsActive { get; init; }

    [Description("@#isOpen")]
    public bool IsOpen { get; init; }

    [Description("@#isSelected")]
    public bool IsSelected { get; init; }

    [Description("@#isIndeterminate")]
    public bool IsIndeterminate { get; init; }

    [Description("@#select")]
    public VListItemSelectCallback? Select { get; init; }

    [Description("@#item")]
    public VueValue? Item { get; init; }

    [Description("@#internalItem")]
    public VuetifyTreeviewInternalItem? InternalItem { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record VTreeviewTitleSlotContext
{
    [Description("@#title")]
    public VuetifyTextValue? Title { get; init; }

    [Description("@#item")]
    public VueValue? Item { get; init; }

    [Description("@#internalItem")]
    public VuetifyTreeviewInternalItem? InternalItem { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record VTreeviewSubtitleSlotContext
{
    [Description("@#subtitle")]
    public VuetifyTextValue? Subtitle { get; init; }

    [Description("@#item")]
    public VueValue? Item { get; init; }

    [Description("@#internalItem")]
    public VuetifyTreeviewInternalItem? InternalItem { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record VTreeviewItemSlotContext : VueProps
{
    [Description("@#props")]
    public VuetifyItemProps? Props { get; init; }

    [Description("@#item")]
    public VueValue? Item { get; init; }

    [Description("@#internalItem")]
    public VuetifyTreeviewInternalItem? InternalItem { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record VTreeviewStructuralItemSlotContext : VueProps
{
    [Description("@#props")]
    public VuetifyItemProps? Props { get; init; }
}
