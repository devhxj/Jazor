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
    [Description("@#top")]
    Top,

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
    [Description("@#always")]
    Always,

    [Description("@#desktop")]
    Desktop,

    [Description("@#mobile")]
    Mobile
}

public delegate bool VuetifyValueComparator(VueValue? first, VueValue? second);

[ECMAScript]
[Description("@#")]
public readonly union VuetifyMandatoryValue(bool, VuetifyMandatoryMode)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public VuetifyMandatoryMode? AsMode
        => Value is VuetifyMandatoryMode value ? value : default(VuetifyMandatoryMode?);

    public static implicit operator VuetifyMandatoryValue(bool value)
        => new(value);

    public static implicit operator VuetifyMandatoryValue(VuetifyMandatoryMode value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifyShowArrowsValue(bool, VuetifyShowArrowsMode)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public VuetifyShowArrowsMode? AsMode
        => Value is VuetifyShowArrowsMode value ? value : default(VuetifyShowArrowsMode?);

    public static implicit operator VuetifyShowArrowsValue(bool value)
        => new(value);

    public static implicit operator VuetifyShowArrowsValue(VuetifyShowArrowsMode value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyGroupModelValuesCollectionBuilder), nameof(VuetifyGroupModelValuesCollectionBuilder.Create))]
public readonly union VuetifyGroupModelValues(VuetifyGroupModelValue[]) : IEnumerable<VuetifyGroupModelValue>
{
    public VuetifyGroupModelValue[]? AsArray => Value as VuetifyGroupModelValue[];

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
        => ((IEnumerable<VuetifyGroupModelValue>)(AsArray ?? [])).GetEnumerator();

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
[Description("@#")]
public readonly union VuetifyGroupModelValue(
    string,
    Number,
    bool,
    Symbol,
    VueProps,
    VuetifyGroupModelValues)
{
    public string? AsString => Value as string;

    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public Symbol? AsSymbol => Value as Symbol;

    public VueProps? AsObject => Value as VueProps;

    public VuetifyGroupModelValues? AsValues
        => Value is VuetifyGroupModelValues value ? value : default(VuetifyGroupModelValues?);

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
