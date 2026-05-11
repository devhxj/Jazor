using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

[String]
public enum VuetifyMandatoryMode
{
    [Description("@#force")]
    Force
}

[String]
public enum VuetifyItemLabelPosition
{
    [Description("@#top")]
    Top,

    [Description("@#bottom")]
    Bottom
}

[String]
public enum VuetifyShowArrowsMode
{
    [Description("@#always")]
    Always,

    [Description("@#desktop")]
    Desktop,

    [Description("@#mobile")]
    Mobile
}

public delegate bool VuetifyValueComparator(VueValue? first, VueValue? second);

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyMandatoryValue
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyMandatoryMode? _mode;

    private VuetifyMandatoryValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _mode = default;
    }

    private VuetifyMandatoryValue(VuetifyMandatoryMode value)
    {
        _kind = 2;
        _bool = default;
        _mode = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyMandatoryMode? AsMode => _kind == 2 ? _mode : default;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyMandatoryValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyMandatoryValue From(VuetifyMandatoryMode value);

    public static implicit operator VuetifyMandatoryValue(bool value)
        => new(value);

    public static implicit operator VuetifyMandatoryValue(VuetifyMandatoryMode value)
        => new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyShowArrowsValue
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyShowArrowsMode? _mode;

    private VuetifyShowArrowsValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _mode = default;
    }

    private VuetifyShowArrowsValue(VuetifyShowArrowsMode value)
    {
        _kind = 2;
        _bool = default;
        _mode = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyShowArrowsMode? AsMode => _kind == 2 ? _mode : default;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyShowArrowsValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyShowArrowsValue From(VuetifyShowArrowsMode value);

    public static implicit operator VuetifyShowArrowsValue(bool value)
        => new(value);

    public static implicit operator VuetifyShowArrowsValue(VuetifyShowArrowsMode value)
        => new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyGroupModelValuesCollectionBuilder), nameof(VuetifyGroupModelValuesCollectionBuilder.Create))]
public readonly struct VuetifyGroupModelValues : IEnumerable<VuetifyGroupModelValue>
{
    private readonly VuetifyGroupModelValue[]? _values;

    private VuetifyGroupModelValues(VuetifyGroupModelValue[] values)
    {
        _values = values;
    }

    public VuetifyGroupModelValue[]? AsArray => _values;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyGroupModelValues From(VuetifyGroupModelValue[] values);

    public static implicit operator VuetifyGroupModelValues(VuetifyGroupModelValue[] values)
        => new(values);

    public static implicit operator VuetifyGroupModelValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyGroupModelValue)value));

    public static implicit operator VuetifyGroupModelValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyGroupModelValue)value));

    public static implicit operator VuetifyGroupModelValues(bool[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyGroupModelValue)value));

    public static implicit operator VuetifyGroupModelValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyGroupModelValue)value));

    public static implicit operator VuetifyGroupModelValues(double[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyGroupModelValue)value));

    IEnumerator<VuetifyGroupModelValue> IEnumerable<VuetifyGroupModelValue>.GetEnumerator()
        => ((IEnumerable<VuetifyGroupModelValue>)(_values ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyGroupModelValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyGroupModelValuesCollectionBuilder
{
    public static VuetifyGroupModelValues Create(ReadOnlySpan<VuetifyGroupModelValue> values)
        => values.ToArray();
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyGroupModelValue
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Number? _number;
    private readonly bool? _bool;
    private readonly Symbol? _symbol;
    private readonly VueProps? _object;
    private readonly VuetifyGroupModelValues? _values;

    private VuetifyGroupModelValue(string value)
    {
        _kind = 1;
        _string = value;
        _number = default;
        _bool = default;
        _symbol = default;
        _object = default;
        _values = default;
    }

    private VuetifyGroupModelValue(Number value)
    {
        _kind = 2;
        _string = default;
        _number = value;
        _bool = default;
        _symbol = default;
        _object = default;
        _values = default;
    }

    private VuetifyGroupModelValue(bool value)
    {
        _kind = 3;
        _string = default;
        _number = default;
        _bool = value;
        _symbol = default;
        _object = default;
        _values = default;
    }

    private VuetifyGroupModelValue(Symbol value)
    {
        _kind = 4;
        _string = default;
        _number = default;
        _bool = default;
        _symbol = value;
        _object = default;
        _values = default;
    }

    private VuetifyGroupModelValue(VueProps value)
    {
        _kind = 5;
        _string = default;
        _number = default;
        _bool = default;
        _symbol = default;
        _object = value;
        _values = default;
    }

    private VuetifyGroupModelValue(VuetifyGroupModelValues value)
    {
        _kind = 6;
        _string = default;
        _number = default;
        _bool = default;
        _symbol = default;
        _object = default;
        _values = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public bool? AsBool => _kind == 3 ? _bool : default;

    public Symbol? AsSymbol => _kind == 4 ? _symbol : default;

    public VueProps? AsObject => _kind == 5 ? _object : default;

    public VuetifyGroupModelValues? AsValues => _kind == 6 ? _values : default;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyGroupModelValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyGroupModelValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyGroupModelValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyGroupModelValue From(Symbol value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyGroupModelValue From(VueProps value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyGroupModelValue From(VuetifyGroupModelValues value);

    public static implicit operator VuetifyGroupModelValue(string value)
        => new(value);

    public static implicit operator VuetifyGroupModelValue(Number value)
        => new(value);

    public static implicit operator VuetifyGroupModelValue(bool value)
        => new(value);

    public static implicit operator VuetifyGroupModelValue(Symbol value)
        => new(value);

    public static implicit operator VuetifyGroupModelValue(VueProps value)
        => new(value);

    public static implicit operator VuetifyGroupModelValue(VueDictionary value)
        => new(value);

    public static implicit operator VuetifyGroupModelValue(VuetifyGroupModelValues value)
        => new(value);

    public static implicit operator VuetifyGroupModelValue(VuetifyGroupModelValue[] value)
        => new((VuetifyGroupModelValues)value);

    public static implicit operator VuetifyGroupModelValue(string[] value)
        => new((VuetifyGroupModelValues)value);

    public static implicit operator VuetifyGroupModelValue(Number[] value)
        => new((VuetifyGroupModelValues)value);

    public static implicit operator VuetifyGroupModelValue(bool[] value)
        => new((VuetifyGroupModelValues)value);

    public static implicit operator VuetifyGroupModelValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyGroupModelValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyGroupModelValue(int[] value)
        => new((VuetifyGroupModelValues)value);

    public static implicit operator VuetifyGroupModelValue(double[] value)
        => new((VuetifyGroupModelValues)value);
}
