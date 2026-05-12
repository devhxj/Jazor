using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 选择器项目列表的擦除值联合类型。
/// Erased value union for Vuetify select item collections.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifySelectItemsCollectionBuilder), nameof(VuetifySelectItemsCollectionBuilder.Create))]
public readonly struct VuetifySelectItems : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifySelectItemValue>
{
    private readonly VuetifySelectItemValue[]? _items;

    private VuetifySelectItems(VuetifySelectItemValue[] items)
    {
        _items = items;
    }

    public VuetifySelectItemValue[]? AsArray => _items;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItems From(VuetifySelectItemValue[] items);

    public static implicit operator VuetifySelectItems(VuetifySelectItemValue[] items)
        => new(items);

    public static implicit operator VuetifySelectItems(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySelectItemValue)item));

    IEnumerator<VuetifySelectItemValue> IEnumerable<VuetifySelectItemValue>.GetEnumerator()
        => ((IEnumerable<VuetifySelectItemValue>)(_items ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifySelectItemValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifySelectItemsCollectionBuilder
{
    public static VuetifySelectItems Create(ReadOnlySpan<VuetifySelectItemValue> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifySelectModelValuesCollectionBuilder), nameof(VuetifySelectModelValuesCollectionBuilder.Create))]
public readonly struct VuetifySelectModelValues : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifySelectModelValue>
{
    private readonly VuetifySelectModelValue[]? _values;

    private VuetifySelectModelValues(VuetifySelectModelValue[] values)
    {
        _values = values;
    }

    public VuetifySelectModelValue[]? AsArray => _values;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectModelValues From(VuetifySelectModelValue[] values);

    public static implicit operator VuetifySelectModelValues(VuetifySelectModelValue[] values)
        => new(values);

    public static implicit operator VuetifySelectModelValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    public static implicit operator VuetifySelectModelValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    public static implicit operator VuetifySelectModelValues(bool[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    public static implicit operator VuetifySelectModelValues(Symbol[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    public static implicit operator VuetifySelectModelValues(VueProps[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    public static implicit operator VuetifySelectModelValues(VueDictionary[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    public static implicit operator VuetifySelectModelValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    public static implicit operator VuetifySelectModelValues(double[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    IEnumerator<VuetifySelectModelValue> IEnumerable<VuetifySelectModelValue>.GetEnumerator()
        => ((IEnumerable<VuetifySelectModelValue>)(_values ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifySelectModelValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifySelectModelValuesCollectionBuilder
{
    public static VuetifySelectModelValues Create(ReadOnlySpan<VuetifySelectModelValue> values)
        => values.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifySelectModelValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Number? _number;
    private readonly bool? _boolean;
    private readonly Symbol? _symbol;
    private readonly VueProps? _object;
    private readonly VuetifySelectModelValues? _values;

    private VuetifySelectModelValue(string value)
    {
        _kind = 1;
        _string = value;
        _number = default;
        _boolean = default;
        _symbol = default;
        _object = default;
        _values = default;
    }

    private VuetifySelectModelValue(Number value)
    {
        _kind = 2;
        _string = default;
        _number = value;
        _boolean = default;
        _symbol = default;
        _object = default;
        _values = default;
    }

    private VuetifySelectModelValue(bool value)
    {
        _kind = 3;
        _string = default;
        _number = default;
        _boolean = value;
        _symbol = default;
        _object = default;
        _values = default;
    }

    private VuetifySelectModelValue(Symbol value)
    {
        _kind = 4;
        _string = default;
        _number = default;
        _boolean = default;
        _symbol = value;
        _object = default;
        _values = default;
    }

    private VuetifySelectModelValue(VueProps value)
    {
        _kind = 5;
        _string = default;
        _number = default;
        _boolean = default;
        _symbol = default;
        _object = value;
        _values = default;
    }

    private VuetifySelectModelValue(VuetifySelectModelValues value)
    {
        _kind = 6;
        _string = default;
        _number = default;
        _boolean = default;
        _symbol = default;
        _object = default;
        _values = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public bool? AsBool => _kind == 3 ? _boolean : default;

    public Symbol? AsSymbol => _kind == 4 ? _symbol : default;

    public VueProps? AsObject => _kind == 5 ? _object : default;

    public VuetifySelectModelValues? AsValues => _kind == 6 ? _values : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsNumber,
        3 => AsBool,
        4 => AsSymbol,
        5 => AsObject,
        6 => AsValues,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectModelValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectModelValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectModelValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectModelValue From(Symbol value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectModelValue From(VueProps value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectModelValue From(VuetifySelectModelValues value);

    public static implicit operator VuetifySelectModelValue(string value)
        => new(value);

    public static implicit operator VuetifySelectModelValue(Number value)
        => new(value);

    public static implicit operator VuetifySelectModelValue(bool value)
        => new(value);

    public static implicit operator VuetifySelectModelValue(Symbol value)
        => new(value);

    public static implicit operator VuetifySelectModelValue(VueProps value)
        => new(value);

    public static implicit operator VuetifySelectModelValue(VueDictionary value)
        => new(value);

    public static implicit operator VuetifySelectModelValue(VuetifySelectModelValues value)
        => new(value);

    public static implicit operator VuetifySelectModelValue(VuetifySelectModelValue[] value)
        => new((VuetifySelectModelValues)value);

    public static implicit operator VuetifySelectModelValue(string[] value)
        => new((VuetifySelectModelValues)value);

    public static implicit operator VuetifySelectModelValue(Number[] value)
        => new((VuetifySelectModelValues)value);

    public static implicit operator VuetifySelectModelValue(bool[] value)
        => new((VuetifySelectModelValues)value);

    public static implicit operator VuetifySelectModelValue(Symbol[] value)
        => new((VuetifySelectModelValues)value);

    public static implicit operator VuetifySelectModelValue(VueProps[] value)
        => new((VuetifySelectModelValues)value);

    public static implicit operator VuetifySelectModelValue(VueDictionary[] value)
        => new((VuetifySelectModelValues)value);

    public static implicit operator VuetifySelectModelValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(short value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(int value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(float value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(double value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(decimal value)
        => new((Number)value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifySelectItemValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly VuetifySelectItem? _item;
    private readonly Number? _number;
    private readonly bool? _boolean;
    private readonly VueProps? _object;

    private VuetifySelectItemValue(string value)
    {
        _kind = 1;
        _string = value;
        _item = default;
        _number = default;
        _boolean = default;
        _object = default;
    }

    private VuetifySelectItemValue(VuetifySelectItem value)
    {
        _kind = 2;
        _string = default;
        _item = value;
        _number = default;
        _boolean = default;
        _object = default;
    }

    private VuetifySelectItemValue(Number value)
    {
        _kind = 3;
        _string = default;
        _item = default;
        _number = value;
        _boolean = default;
        _object = default;
    }

    private VuetifySelectItemValue(bool value)
    {
        _kind = 4;
        _string = default;
        _item = default;
        _number = default;
        _boolean = value;
        _object = default;
    }

    private VuetifySelectItemValue(VueProps value)
    {
        _kind = 5;
        _string = default;
        _item = default;
        _number = default;
        _boolean = default;
        _object = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public VuetifySelectItem? AsItem => _kind == 2 ? _item : default;

    public Number? AsNumber => _kind == 3 ? _number : default;

    public bool? AsBool => _kind == 4 ? _boolean : default;

    public VueProps? AsObject => _kind == 5 ? _object : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsItem,
        3 => AsNumber,
        4 => AsBool,
        5 => AsObject,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemValue From(VuetifySelectItem value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemValue From(VueProps value);

    public static implicit operator VuetifySelectItemValue(string value)
        => new(value);

    public static implicit operator VuetifySelectItemValue(VuetifySelectItem value)
        => new(value);

    public static implicit operator VuetifySelectItemValue(Number value)
        => new(value);

    public static implicit operator VuetifySelectItemValue(bool value)
        => new(value);

    public static implicit operator VuetifySelectItemValue(VueProps value)
        => new(value);

    public static implicit operator VuetifySelectItemValue(VueDictionary value)
        => new(value);

    public static implicit operator VuetifySelectItemValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(short value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(int value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(float value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(double value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(decimal value)
        => new((Number)value);
}

[ECMAScript]
[Description("@#")]
public sealed class VuetifySelectItem
{
    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#value")]
    public VueValue? Value { get; init; }

    [Description("@#props")]
    public VuetifySelectItemPropsValue? Props { get; init; }

    [Description("@#children")]
    public VuetifySelectItemValue[]? Children { get; init; }

    [Description("@#raw")]
    public VueValue? Raw { get; init; }
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifySelectItemKey : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly string[]? _path;
    private readonly VuetifySelectItemKeySelector? _selector;
    private readonly bool? _boolean;

    private VuetifySelectItemKey(string value)
    {
        _kind = 1;
        _string = value;
        _path = default;
        _selector = default;
        _boolean = default;
    }

    private VuetifySelectItemKey(string[] value)
    {
        _kind = 2;
        _string = default;
        _path = value;
        _selector = default;
        _boolean = default;
    }

    private VuetifySelectItemKey(VuetifySelectItemKeySelector value)
    {
        _kind = 3;
        _string = default;
        _path = default;
        _selector = value;
        _boolean = default;
    }

    private VuetifySelectItemKey(bool value)
    {
        _kind = 4;
        _string = default;
        _path = default;
        _selector = default;
        _boolean = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public string[]? AsPath => _kind == 2 ? _path : default;

    public VuetifySelectItemKeySelector? AsSelector => _kind == 3 ? _selector : default;

    public bool? AsBool => _kind == 4 ? _boolean : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsPath,
        3 => AsSelector,
        4 => AsBool,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemKey From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemKey From(string[] value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemKey From(VuetifySelectItemKeySelector value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemKey From(bool value);

    public static implicit operator VuetifySelectItemKey(string value)
        => new(value);

    public static implicit operator VuetifySelectItemKey(string[] value)
        => new(value);

    public static implicit operator VuetifySelectItemKey(VuetifySelectItemKeySelector value)
        => new(value);

    public static implicit operator VuetifySelectItemKey(bool value)
        => new(value);
}

public delegate VueValue? VuetifySelectItemKeySelector(VueValue item, string fallback);

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifySelectItemPropsSelector : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly string[]? _path;
    private readonly VuetifySelectItemPropsCallback? _callback;
    private readonly bool? _boolean;

    private VuetifySelectItemPropsSelector(string value)
    {
        _kind = 1;
        _string = value;
        _path = default;
        _callback = default;
        _boolean = default;
    }

    private VuetifySelectItemPropsSelector(string[] value)
    {
        _kind = 2;
        _string = default;
        _path = value;
        _callback = default;
        _boolean = default;
    }

    private VuetifySelectItemPropsSelector(VuetifySelectItemPropsCallback value)
    {
        _kind = 3;
        _string = default;
        _path = default;
        _callback = value;
        _boolean = default;
    }

    private VuetifySelectItemPropsSelector(bool value)
    {
        _kind = 4;
        _string = default;
        _path = default;
        _callback = default;
        _boolean = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public string[]? AsPath => _kind == 2 ? _path : default;

    public VuetifySelectItemPropsCallback? AsCallback => _kind == 3 ? _callback : default;

    public bool? AsBool => _kind == 4 ? _boolean : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsPath,
        3 => AsCallback,
        4 => AsBool,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemPropsSelector From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemPropsSelector From(string[] value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemPropsSelector From(VuetifySelectItemPropsCallback value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemPropsSelector From(bool value);

    public static implicit operator VuetifySelectItemPropsSelector(string value)
        => new(value);

    public static implicit operator VuetifySelectItemPropsSelector(string[] value)
        => new(value);

    public static implicit operator VuetifySelectItemPropsSelector(VuetifySelectItemPropsCallback value)
        => new(value);

    public static implicit operator VuetifySelectItemPropsSelector(bool value)
        => new(value);
}

public delegate VuetifyItemProps? VuetifySelectItemPropsCallback(VueValue item);

public delegate bool VuetifySelectValueComparator(VueValue? first, VueValue? second);

public delegate VuetifyFilterMatch VuetifyFilterFunction(string? value, string? query, VuetifyListItem? item = default);

[ECMAScript]
[Description("@#")]
public sealed record VuetifyFilterKeyFunctions : VueDictionary<VuetifyFilterFunction>;

[String]
public enum VuetifyFilterMode
{
    [Description("@#some")]
    Some,

    [Description("@#every")]
    Every,

    [Description("@#union")]
    Union,

    [Description("@#intersection")]
    Intersection
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyFilterKeys : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly string[]? _strings;

    private VuetifyFilterKeys(string value)
    {
        _kind = 1;
        _string = value;
        _strings = default;
    }

    private VuetifyFilterKeys(string[] value)
    {
        _kind = 2;
        _string = default;
        _strings = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public string[]? AsStrings => _kind == 2 ? _strings : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsStrings,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyFilterKeys From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyFilterKeys From(string[] value);

    public static implicit operator VuetifyFilterKeys(string value)
        => new(value);

    public static implicit operator VuetifyFilterKeys(string[] value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyFilterMatch : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _boolean;
    private readonly Number? _number;
    private readonly Number[]? _range;
    private readonly Number[][]? _ranges;

    private VuetifyFilterMatch(bool value)
    {
        _kind = 1;
        _boolean = value;
        _number = default;
        _range = default;
        _ranges = default;
    }

    private VuetifyFilterMatch(Number value)
    {
        _kind = 2;
        _boolean = default;
        _number = value;
        _range = default;
        _ranges = default;
    }

    private VuetifyFilterMatch(Number[] value)
    {
        _kind = 3;
        _boolean = default;
        _number = default;
        _range = value;
        _ranges = default;
    }

    private VuetifyFilterMatch(Number[][] value)
    {
        _kind = 4;
        _boolean = default;
        _number = default;
        _range = default;
        _ranges = value;
    }

    public bool? AsBool => _kind == 1 ? _boolean : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public Number[]? AsRange => _kind == 3 ? _range : default;

    public Number[][]? AsRanges => _kind == 4 ? _ranges : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsNumber,
        3 => AsRange,
        4 => AsRanges,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyFilterMatch From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyFilterMatch From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyFilterMatch From(Number[] value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyFilterMatch From(Number[][] value);

    [ECMAScriptInline("[__arg1, __arg2]")]
    public extern static VuetifyFilterMatch Range(Number start, Number end);

    public static implicit operator VuetifyFilterMatch(bool value)
        => new(value);

    public static implicit operator VuetifyFilterMatch(Number value)
        => new(value);

    public static implicit operator VuetifyFilterMatch(Number[] value)
        => new(value);

    public static implicit operator VuetifyFilterMatch(Number[][] value)
        => new(value);

    public static implicit operator VuetifyFilterMatch(byte value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(short value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(int value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(uint value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(float value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(double value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(decimal value)
        => new((Number)value);
}

[ECMAScript]
[Description("@#")]
public sealed record VuetifyListItem
{
    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#value")]
    public VueValue? Value { get; init; }

    [Description("@#props")]
    public VueProps? Props { get; init; }

    [Description("@#children")]
    public VuetifyListItem[]? Children { get; init; }

    [Description("@#raw")]
    public VueValue? Raw { get; init; }
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifySelectItemPropsValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly VuetifyItemProps? _props;
    private readonly bool? _boolean;

    private VuetifySelectItemPropsValue(VuetifyItemProps value)
    {
        _kind = 1;
        _props = value;
        _boolean = default;
    }

    private VuetifySelectItemPropsValue(bool value)
    {
        _kind = 2;
        _props = default;
        _boolean = value;
    }

    public VuetifyItemProps? AsProps => _kind == 1 ? _props : default;

    public bool? AsBool => _kind == 2 ? _boolean : default;

    public object? Value => _kind switch
    {
        1 => AsProps,
        2 => AsBool,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemPropsValue From(VuetifyItemProps value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySelectItemPropsValue From(bool value);

    public static implicit operator VuetifySelectItemPropsValue(VuetifyItemProps value)
        => new(value);

    public static implicit operator VuetifySelectItemPropsValue(bool value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public sealed class VuetifyItemProps : IEnumerable
{
    public extern VueValue? this[string key] { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public extern void Add(string key, VueValue value);

    extern IEnumerator IEnumerable.GetEnumerator();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyBreadcrumbItemsCollectionBuilder), nameof(VuetifyBreadcrumbItemsCollectionBuilder.Create))]
public readonly struct VuetifyBreadcrumbItems : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyBreadcrumbItemValue>
{
    private readonly VuetifyBreadcrumbItemValue[]? _items;

    private VuetifyBreadcrumbItems(VuetifyBreadcrumbItemValue[] items)
    {
        _items = items;
    }

    public VuetifyBreadcrumbItemValue[]? AsArray => _items;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyBreadcrumbItems From(VuetifyBreadcrumbItemValue[] items);

    public static implicit operator VuetifyBreadcrumbItems(VuetifyBreadcrumbItemValue[] items)
        => new(items);

    public static implicit operator VuetifyBreadcrumbItems(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifyBreadcrumbItemValue)item));

    IEnumerator<VuetifyBreadcrumbItemValue> IEnumerable<VuetifyBreadcrumbItemValue>.GetEnumerator()
        => ((IEnumerable<VuetifyBreadcrumbItemValue>)(_items ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyBreadcrumbItemValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyBreadcrumbItemsCollectionBuilder
{
    public static VuetifyBreadcrumbItems Create(ReadOnlySpan<VuetifyBreadcrumbItemValue> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyBreadcrumbItemValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly VuetifyBreadcrumbItem? _item;
    private readonly Number? _number;

    private VuetifyBreadcrumbItemValue(string value)
    {
        _kind = 1;
        _string = value;
        _item = default;
        _number = default;
    }

    private VuetifyBreadcrumbItemValue(VuetifyBreadcrumbItem value)
    {
        _kind = 2;
        _string = default;
        _item = value;
        _number = default;
    }

    private VuetifyBreadcrumbItemValue(Number value)
    {
        _kind = 3;
        _string = default;
        _item = default;
        _number = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public VuetifyBreadcrumbItem? AsItem => _kind == 2 ? _item : default;

    public Number? AsNumber => _kind == 3 ? _number : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsItem,
        3 => AsNumber,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyBreadcrumbItemValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyBreadcrumbItemValue From(VuetifyBreadcrumbItem value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyBreadcrumbItemValue From(Number value);

    public static implicit operator VuetifyBreadcrumbItemValue(string value)
        => new(value);

    public static implicit operator VuetifyBreadcrumbItemValue(VuetifyBreadcrumbItem value)
        => new(value);

    public static implicit operator VuetifyBreadcrumbItemValue(Number value)
        => new(value);

    public static implicit operator VuetifyBreadcrumbItemValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(decimal value)
        => new((Number)value);
}

[ECMAScript]
[Description("@#")]
public sealed class VuetifyBreadcrumbItem
{
    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }

    [Description("@#href")]
    public string? Href { get; init; }

    [Description("@#to")]
    public string? To { get; init; }

    [Description("@#replace")]
    public bool? Replace { get; init; }

    [Description("@#exact")]
    public bool? Exact { get; init; }
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableHeadersCollectionBuilder), nameof(VuetifyDataTableHeadersCollectionBuilder.Create))]
public readonly struct VuetifyDataTableHeaders : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyDataTableHeader>
{
    private readonly VuetifyDataTableHeader[]? _headers;

    private VuetifyDataTableHeaders(VuetifyDataTableHeader[] headers)
    {
        _headers = headers;
    }

    public VuetifyDataTableHeader[]? AsArray => _headers;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataTableHeaders From(VuetifyDataTableHeader[] headers);

    public static implicit operator VuetifyDataTableHeaders(VuetifyDataTableHeader[] headers)
        => new(headers);

    IEnumerator<VuetifyDataTableHeader> IEnumerable<VuetifyDataTableHeader>.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableHeader>)(_headers ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableHeader>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataTableHeadersCollectionBuilder
{
    public static VuetifyDataTableHeaders Create(ReadOnlySpan<VuetifyDataTableHeader> headers)
        => headers.ToArray();
}

[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableHeader
{
    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#key")]
    public string? Key { get; init; }

    [Description("@#value")]
    public VuetifySelectItemKey? Value { get; init; }

    [Description("@#sortable")]
    public bool? Sortable { get; init; }

    [Description("@#align")]
    public VuetifyDataTableHeaderAlign? Align { get; init; }

    [Description("@#width")]
    public VueStringNumberValue? Width { get; init; }

    [Description("@#minWidth")]
    public VueStringNumberValue? MinWidth { get; init; }

    [Description("@#maxWidth")]
    public VueStringNumberValue? MaxWidth { get; init; }

    [Description("@#nowrap")]
    public bool? Nowrap { get; init; }

    [Description("@#fixed")]
    public bool? Fixed { get; init; }

    [Description("@#children")]
    public VuetifyDataTableHeader[]? Children { get; init; }
}

[String]
public enum VuetifyDataTableHeaderAlign
{
    [Description("@#start")]
    Start,

    [Description("@#end")]
    End,

    [Description("@#center")]
    Center
}

[String]
public enum VuetifyDataTableSortOrder
{
    [Description("@#asc")]
    Asc,

    [Description("@#desc")]
    Desc
}

[String]
public enum VuetifyDataTableSelectStrategy
{
    [Description("@#single")]
    Single,

    [Description("@#page")]
    Page,

    [Description("@#all")]
    All
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableItemsCollectionBuilder), nameof(VuetifyDataTableItemsCollectionBuilder.Create))]
public readonly struct VuetifyDataTableItems : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyDataTableItem>
{
    private readonly VuetifyDataTableItem[]? _items;

    private VuetifyDataTableItems(VuetifyDataTableItem[] items)
    {
        _items = items;
    }

    public VuetifyDataTableItem[]? AsArray => _items;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataTableItems From(VuetifyDataTableItem[] items);

    public static implicit operator VuetifyDataTableItems(VuetifyDataTableItem[] items)
        => new(items);

    IEnumerator<VuetifyDataTableItem> IEnumerable<VuetifyDataTableItem>.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableItem>)(_items ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableItem>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataTableItemsCollectionBuilder
{
    public static VuetifyDataTableItems Create(ReadOnlySpan<VuetifyDataTableItem> items)
        => items.ToArray();
}

[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableItem : IEnumerable
{
    public extern VueValue? this[string key] { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public extern void Add(string key, VueValue value);

    extern IEnumerator IEnumerable.GetEnumerator();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableSelectedValuesCollectionBuilder), nameof(VuetifyDataTableSelectedValuesCollectionBuilder.Create))]
public readonly struct VuetifyDataTableSelectedValues : System.Runtime.CompilerServices.IUnion, IEnumerable<VueValue>
{
    private readonly VueValue[]? _values;

    private VuetifyDataTableSelectedValues(VueValue[] values)
    {
        _values = values;
    }

    public VueValue[]? AsArray => _values;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataTableSelectedValues From(VueValue[] values);

    public static implicit operator VuetifyDataTableSelectedValues(VueValue[] values)
        => new(values);

    public static implicit operator VuetifyDataTableSelectedValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDataTableSelectedValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDataTableSelectedValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDataTableSelectedValues(double[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    IEnumerator<VueValue> IEnumerable<VueValue>.GetEnumerator()
        => ((IEnumerable<VueValue>)(_values ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataTableSelectedValuesCollectionBuilder
{
    public static VuetifyDataTableSelectedValues Create(ReadOnlySpan<VueValue> values)
        => values.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableSortItemsCollectionBuilder), nameof(VuetifyDataTableSortItemsCollectionBuilder.Create))]
public readonly struct VuetifyDataTableSortItems : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyDataTableSortItem>
{
    private readonly VuetifyDataTableSortItem[]? _items;

    private VuetifyDataTableSortItems(VuetifyDataTableSortItem[] items)
    {
        _items = items;
    }

    public VuetifyDataTableSortItem[]? AsArray => _items;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataTableSortItems From(VuetifyDataTableSortItem[] items);

    public static implicit operator VuetifyDataTableSortItems(VuetifyDataTableSortItem[] items)
        => new(items);

    IEnumerator<VuetifyDataTableSortItem> IEnumerable<VuetifyDataTableSortItem>.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableSortItem>)(_items ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableSortItem>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataTableSortItemsCollectionBuilder
{
    public static VuetifyDataTableSortItems Create(ReadOnlySpan<VuetifyDataTableSortItem> items)
        => items.ToArray();
}

[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableSortItem
{
    [Description("@#key")]
    public string? Key { get; init; }

    [Description("@#order")]
    public VuetifyDataTableSortOrder? Order { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableOptions
{
    [Description("@#page")]
    public int? Page { get; init; }

    [Description("@#itemsPerPage")]
    public int? ItemsPerPage { get; init; }

    [Description("@#sortBy")]
    public VuetifyDataTableSortItem[]? SortBy { get; init; }

    [Description("@#groupBy")]
    public VuetifyDataTableSortItem[]? GroupBy { get; init; }

    [Description("@#search")]
    public string? Search { get; init; }
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableItemsPerPageOptionsCollectionBuilder), nameof(VuetifyDataTableItemsPerPageOptionsCollectionBuilder.Create))]
public readonly struct VuetifyDataTableItemsPerPageOptions : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyDataTableItemsPerPageOption>
{
    private readonly VuetifyDataTableItemsPerPageOption[]? _options;

    private VuetifyDataTableItemsPerPageOptions(VuetifyDataTableItemsPerPageOption[] options)
    {
        _options = options;
    }

    public VuetifyDataTableItemsPerPageOption[]? AsArray => _options;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataTableItemsPerPageOptions From(VuetifyDataTableItemsPerPageOption[] options);

    public static implicit operator VuetifyDataTableItemsPerPageOptions(VuetifyDataTableItemsPerPageOption[] options)
        => new(options);

    public static implicit operator VuetifyDataTableItemsPerPageOptions(Number[] options)
        => new(Array.ConvertAll(options, static value => (VuetifyDataTableItemsPerPageOption)value));

    public static implicit operator VuetifyDataTableItemsPerPageOptions(int[] options)
        => new(Array.ConvertAll(options, static value => (VuetifyDataTableItemsPerPageOption)value));

    IEnumerator<VuetifyDataTableItemsPerPageOption> IEnumerable<VuetifyDataTableItemsPerPageOption>.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableItemsPerPageOption>)(_options ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableItemsPerPageOption>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataTableItemsPerPageOptionsCollectionBuilder
{
    public static VuetifyDataTableItemsPerPageOptions Create(ReadOnlySpan<VuetifyDataTableItemsPerPageOption> options)
        => options.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyDataTableItemsPerPageOption : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly Number? _number;
    private readonly VuetifyDataTableItemsPerPageOptionItem? _item;

    private VuetifyDataTableItemsPerPageOption(Number value)
    {
        _kind = 1;
        _number = value;
        _item = default;
    }

    private VuetifyDataTableItemsPerPageOption(VuetifyDataTableItemsPerPageOptionItem value)
    {
        _kind = 2;
        _number = default;
        _item = value;
    }

    public Number? AsNumber => _kind == 1 ? _number : default;

    public VuetifyDataTableItemsPerPageOptionItem? AsItem => _kind == 2 ? _item : default;

    public object? Value => _kind switch
    {
        1 => AsNumber,
        2 => AsItem,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataTableItemsPerPageOption From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataTableItemsPerPageOption From(VuetifyDataTableItemsPerPageOptionItem value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(Number value)
        => new(value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(VuetifyDataTableItemsPerPageOptionItem value)
        => new(value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(byte value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(short value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(int value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(uint value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(float value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(double value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(decimal value)
        => new((Number)value);
}

[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableItemsPerPageOptionItem
{
    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#value")]
    public int? Value { get; init; }
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyDataTableRowProps : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly VueProps? _props;
    private readonly VuetifyDataTableRowPropsCallback? _callback;

    private VuetifyDataTableRowProps(VueProps value)
    {
        _kind = 1;
        _props = value;
        _callback = default;
    }

    private VuetifyDataTableRowProps(VuetifyDataTableRowPropsCallback value)
    {
        _kind = 2;
        _props = default;
        _callback = value;
    }

    public VueProps? AsProps => _kind == 1 ? _props : default;

    public VuetifyDataTableRowPropsCallback? AsCallback => _kind == 2 ? _callback : default;

    public object? Value => _kind switch
    {
        1 => AsProps,
        2 => AsCallback,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataTableRowProps From(VueProps value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataTableRowProps From(VuetifyDataTableRowPropsCallback value);

    public static implicit operator VuetifyDataTableRowProps(VueProps value)
        => new(value);

    public static implicit operator VuetifyDataTableRowProps(VueDictionary value)
        => new(value);

    public static implicit operator VuetifyDataTableRowProps(VuetifyDataTableRowPropsCallback value)
        => new(value);
}

public delegate VueProps? VuetifyDataTableRowPropsCallback(VuetifyDataTableRowPropsContext context);

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyDataTableCellProps : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly VueProps? _props;
    private readonly VuetifyDataTableCellPropsCallback? _callback;

    private VuetifyDataTableCellProps(VueProps value)
    {
        _kind = 1;
        _props = value;
        _callback = default;
    }

    private VuetifyDataTableCellProps(VuetifyDataTableCellPropsCallback value)
    {
        _kind = 2;
        _props = default;
        _callback = value;
    }

    public VueProps? AsProps => _kind == 1 ? _props : default;

    public VuetifyDataTableCellPropsCallback? AsCallback => _kind == 2 ? _callback : default;

    public object? Value => _kind switch
    {
        1 => AsProps,
        2 => AsCallback,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataTableCellProps From(VueProps value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataTableCellProps From(VuetifyDataTableCellPropsCallback value);

    public static implicit operator VuetifyDataTableCellProps(VueProps value)
        => new(value);

    public static implicit operator VuetifyDataTableCellProps(VueDictionary value)
        => new(value);

    public static implicit operator VuetifyDataTableCellProps(VuetifyDataTableCellPropsCallback value)
        => new(value);
}

public delegate VueProps? VuetifyDataTableCellPropsCallback(VuetifyDataTableCellPropsContext context);
