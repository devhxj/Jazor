using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

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

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyStyleValuesCollectionBuilder), nameof(VuetifyStyleValuesCollectionBuilder.Create))]
public readonly struct VuetifyStyleValues : IEnumerable<VuetifyStyleValue>
{
    private readonly VuetifyStyleValue[]? _values;

    private VuetifyStyleValues(VuetifyStyleValue[] values)
    {
        _values = values;
    }

    public VuetifyStyleValue[]? AsArray => _values;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyStyleValues From(VuetifyStyleValue[] values);

    public static implicit operator VuetifyStyleValues(VuetifyStyleValue[] values)
        => new(values);

    public static implicit operator VuetifyStyleValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyStyleValue)value));

    public static implicit operator VuetifyStyleValues(VueProps[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyStyleValue)value));

    public static implicit operator VuetifyStyleValues(VueDictionary[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyStyleValue)value));

    IEnumerator<VuetifyStyleValue> IEnumerable<VuetifyStyleValue>.GetEnumerator()
        => ((IEnumerable<VuetifyStyleValue>)(_values ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyStyleValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyStyleValuesCollectionBuilder
{
    public static VuetifyStyleValues Create(ReadOnlySpan<VuetifyStyleValue> values)
        => values.ToArray();
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyStyleValue
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly VueProps? _props;
    private readonly VuetifyStyleValues? _values;

    private VuetifyStyleValue(string value)
    {
        _kind = 1;
        _string = value;
        _props = default;
        _values = default;
    }

    private VuetifyStyleValue(VueProps value)
    {
        _kind = 2;
        _string = default;
        _props = value;
        _values = default;
    }

    private VuetifyStyleValue(VuetifyStyleValues value)
    {
        _kind = 3;
        _string = default;
        _props = default;
        _values = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public VueProps? AsProps => _kind == 2 ? _props : default;

    public VuetifyStyleValues? AsValues => _kind == 3 ? _values : default;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyStyleValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyStyleValue From(VueProps value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyStyleValue From(VuetifyStyleValues value);

    public static implicit operator VuetifyStyleValue(string value)
        => new(value);

    public static implicit operator VuetifyStyleValue(VueProps value)
        => new(value);

    public static implicit operator VuetifyStyleValue(VueDictionary value)
        => new(value);

    public static implicit operator VuetifyStyleValue(VuetifyStyleValues value)
        => new(value);

    public static implicit operator VuetifyStyleValue(VuetifyStyleValue[] value)
        => new((VuetifyStyleValues)value);

    public static implicit operator VuetifyStyleValue(string[] value)
        => new((VuetifyStyleValues)value);

    public static implicit operator VuetifyStyleValue(VueProps[] value)
        => new((VuetifyStyleValues)value);

    public static implicit operator VuetifyStyleValue(VueDictionary[] value)
        => new((VuetifyStyleValues)value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyTreeviewValuesCollectionBuilder), nameof(VuetifyTreeviewValuesCollectionBuilder.Create))]
public readonly struct VuetifyTreeviewValues : IEnumerable<VueValue>
{
    private readonly VueValue[]? _values;

    private VuetifyTreeviewValues(VueValue[] values)
    {
        _values = values;
    }

    public VueValue[]? AsArray => _values;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTreeviewValues From(VueValue[] values);

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
        => ((IEnumerable<VueValue>)(_values ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyTreeviewValuesCollectionBuilder
{
    public static VuetifyTreeviewValues Create(ReadOnlySpan<VueValue> values)
        => values.ToArray();
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyTreeviewItemsCollectionBuilder), nameof(VuetifyTreeviewItemsCollectionBuilder.Create))]
public readonly struct VuetifyTreeviewItems : IEnumerable<VuetifyTreeviewItemValue>
{
    private readonly VuetifyTreeviewItemValue[]? _items;

    private VuetifyTreeviewItems(VuetifyTreeviewItemValue[] items)
    {
        _items = items;
    }

    public VuetifyTreeviewItemValue[]? AsArray => _items;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTreeviewItems From(VuetifyTreeviewItemValue[] items);

    public static implicit operator VuetifyTreeviewItems(VuetifyTreeviewItemValue[] items)
        => new(items);

    public static implicit operator VuetifyTreeviewItems(VuetifyTreeviewItem[] items)
        => new(Array.ConvertAll(items, static item => (VuetifyTreeviewItemValue)item));

    public static implicit operator VuetifyTreeviewItems(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifyTreeviewItemValue)item));

    IEnumerator<VuetifyTreeviewItemValue> IEnumerable<VuetifyTreeviewItemValue>.GetEnumerator()
        => ((IEnumerable<VuetifyTreeviewItemValue>)(_items ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyTreeviewItemValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyTreeviewItemsCollectionBuilder
{
    public static VuetifyTreeviewItems Create(ReadOnlySpan<VuetifyTreeviewItemValue> items)
        => items.ToArray();
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyTreeviewItemValue
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly VuetifyTreeviewItem? _item;
    private readonly Number? _number;
    private readonly bool? _boolean;
    private readonly VueProps? _object;

    private VuetifyTreeviewItemValue(string value)
    {
        _kind = 1;
        _string = value;
        _item = default;
        _number = default;
        _boolean = default;
        _object = default;
    }

    private VuetifyTreeviewItemValue(VuetifyTreeviewItem value)
    {
        _kind = 2;
        _string = default;
        _item = value;
        _number = default;
        _boolean = default;
        _object = default;
    }

    private VuetifyTreeviewItemValue(Number value)
    {
        _kind = 3;
        _string = default;
        _item = default;
        _number = value;
        _boolean = default;
        _object = default;
    }

    private VuetifyTreeviewItemValue(bool value)
    {
        _kind = 4;
        _string = default;
        _item = default;
        _number = default;
        _boolean = value;
        _object = default;
    }

    private VuetifyTreeviewItemValue(VueProps value)
    {
        _kind = 5;
        _string = default;
        _item = default;
        _number = default;
        _boolean = default;
        _object = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public VuetifyTreeviewItem? AsItem => _kind == 2 ? _item : default;

    public Number? AsNumber => _kind == 3 ? _number : default;

    public bool? AsBool => _kind == 4 ? _boolean : default;

    public VueProps? AsObject => _kind == 5 ? _object : default;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTreeviewItemValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTreeviewItemValue From(VuetifyTreeviewItem value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTreeviewItemValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTreeviewItemValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTreeviewItemValue From(VueProps value);

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

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyTreeviewActiveStrategyValue
{
    private readonly byte _kind;
    private readonly VuetifyTreeviewActiveStrategy? _name;
    private readonly VuetifyTreeviewActiveStrategyDefinition? _definition;
    private readonly VuetifyTreeviewActiveStrategyFactory? _factory;

    private VuetifyTreeviewActiveStrategyValue(VuetifyTreeviewActiveStrategy value)
    {
        _kind = 1;
        _name = value;
        _definition = default;
        _factory = default;
    }

    private VuetifyTreeviewActiveStrategyValue(VuetifyTreeviewActiveStrategyDefinition value)
    {
        _kind = 2;
        _name = default;
        _definition = value;
        _factory = default;
    }

    private VuetifyTreeviewActiveStrategyValue(VuetifyTreeviewActiveStrategyFactory value)
    {
        _kind = 3;
        _name = default;
        _definition = default;
        _factory = value;
    }

    public VuetifyTreeviewActiveStrategy? AsName => _kind == 1 ? _name : default;

    public VuetifyTreeviewActiveStrategyDefinition? AsDefinition => _kind == 2 ? _definition : default;

    public VuetifyTreeviewActiveStrategyFactory? AsFactory => _kind == 3 ? _factory : default;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTreeviewActiveStrategyValue From(VuetifyTreeviewActiveStrategy value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTreeviewActiveStrategyValue From(VuetifyTreeviewActiveStrategyDefinition value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTreeviewActiveStrategyValue From(VuetifyTreeviewActiveStrategyFactory value);

    public static implicit operator VuetifyTreeviewActiveStrategyValue(VuetifyTreeviewActiveStrategy value)
        => new(value);

    public static implicit operator VuetifyTreeviewActiveStrategyValue(VuetifyTreeviewActiveStrategyDefinition value)
        => new(value);

    public static implicit operator VuetifyTreeviewActiveStrategyValue(VuetifyTreeviewActiveStrategyFactory value)
        => new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyTreeviewSelectStrategyValue
{
    private readonly byte _kind;
    private readonly VuetifyTreeviewSelectStrategy? _name;
    private readonly VuetifyTreeviewSelectStrategyDefinition? _definition;
    private readonly VuetifyTreeviewSelectStrategyFactory? _factory;

    private VuetifyTreeviewSelectStrategyValue(VuetifyTreeviewSelectStrategy value)
    {
        _kind = 1;
        _name = value;
        _definition = default;
        _factory = default;
    }

    private VuetifyTreeviewSelectStrategyValue(VuetifyTreeviewSelectStrategyDefinition value)
    {
        _kind = 2;
        _name = default;
        _definition = value;
        _factory = default;
    }

    private VuetifyTreeviewSelectStrategyValue(VuetifyTreeviewSelectStrategyFactory value)
    {
        _kind = 3;
        _name = default;
        _definition = default;
        _factory = value;
    }

    public VuetifyTreeviewSelectStrategy? AsName => _kind == 1 ? _name : default;

    public VuetifyTreeviewSelectStrategyDefinition? AsDefinition => _kind == 2 ? _definition : default;

    public VuetifyTreeviewSelectStrategyFactory? AsFactory => _kind == 3 ? _factory : default;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTreeviewSelectStrategyValue From(VuetifyTreeviewSelectStrategy value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTreeviewSelectStrategyValue From(VuetifyTreeviewSelectStrategyDefinition value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTreeviewSelectStrategyValue From(VuetifyTreeviewSelectStrategyFactory value);

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
