using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 覆盖层偏移值集合。
/// Vuetify overlay offset value collection.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyOverlayOffsetValuesCollectionBuilder), nameof(VuetifyOverlayOffsetValuesCollectionBuilder.Create))]
public readonly union VuetifyOverlayOffsetValues(Number[]) : IEnumerable<Number>
{
    public Number[]? AsArray => Value as Number[];

    public static implicit operator VuetifyOverlayOffsetValues(Number[] values)
        => new(values);

    public static implicit operator VuetifyOverlayOffsetValues(int[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    public static implicit operator VuetifyOverlayOffsetValues(double[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
        => ((IEnumerable<Number>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Number>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyOverlayOffsetValuesCollectionBuilder
{
    public static VuetifyOverlayOffsetValues Create(ReadOnlySpan<Number> values)
        => values.ToArray();
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifyOverlayActivatorTarget(Element, VueComponentPublicInstance, string)
{
    public Element? AsElement => Value as Element;

    public VueComponentPublicInstance? AsComponent => Value as VueComponentPublicInstance;

    public string? AsString => Value as string;

    [ECMAScriptInline("'parent'")]
    public extern static VuetifyOverlayActivatorTarget Parent();

    public static implicit operator VuetifyOverlayActivatorTarget(Element value)
        => new(value);

    public static implicit operator VuetifyOverlayActivatorTarget(VueComponentPublicInstance value)
        => new(value);

    public static implicit operator VuetifyOverlayActivatorTarget(string value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifyOverlayOffsetValue(string, Number, VuetifyOverlayOffsetValues)
{
    public string? AsString => Value as string;

    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public VuetifyOverlayOffsetValues? AsValues
        => Value is VuetifyOverlayOffsetValues value ? value : default(VuetifyOverlayOffsetValues?);

    public static implicit operator VuetifyOverlayOffsetValue(string value)
        => new(value);

    public static implicit operator VuetifyOverlayOffsetValue(Number value)
        => new(value);

    public static implicit operator VuetifyOverlayOffsetValue(VuetifyOverlayOffsetValues value)
        => new(value);

    public static implicit operator VuetifyOverlayOffsetValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyOverlayOffsetValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyOverlayOffsetValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyOverlayOffsetValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyOverlayOffsetValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyOverlayOffsetValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyOverlayOffsetValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyOverlayOffsetValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyOverlayOffsetValue(decimal value)
        => new((Number)value);
}

[String]
public enum VuetifyOriginMode
{
    [Description("@#auto")]
    Auto,

    [Description("@#overlap")]
    Overlap
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifyOriginValue(VuetifyLocation, VuetifyOriginMode, string)
{
    public VuetifyLocation? AsLocation
        => Value is VuetifyLocation value ? value : default(VuetifyLocation?);

    public VuetifyOriginMode? AsMode
        => Value is VuetifyOriginMode value ? value : default(VuetifyOriginMode?);

    public string? AsCustom => Value as string;

    public static implicit operator VuetifyOriginValue(VuetifyLocation value)
        => new(value);

    public static implicit operator VuetifyOriginValue(VuetifyOriginMode value)
        => new(value);

    public static implicit operator VuetifyOriginValue(string value)
        => new(value);
}

[ECMAScript]
[Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyOverlayCoordinateTargetCollectionBuilder), nameof(VuetifyOverlayCoordinateTargetCollectionBuilder.Create))]
public readonly struct VuetifyOverlayCoordinateTarget : IUnion, IEnumerable<Number>
{
    private readonly Number[]? _values;

    public VuetifyOverlayCoordinateTarget(Number[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length != 2)
            throw new ArgumentException("Vuetify overlay coordinate targets require exactly two items.", nameof(values));

        _values = values;
    }

    public Number[]? AsArray => _values;

    public Number? X => _values is { Length: > 0 } values ? values[0] : default(Number?);

    public Number? Y => _values is { Length: > 1 } values ? values[1] : default(Number?);

    public object? Value => _values;

    public static implicit operator VuetifyOverlayCoordinateTarget(Number[] values)
        => new(values);

    public static implicit operator VuetifyOverlayCoordinateTarget(int[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    public static implicit operator VuetifyOverlayCoordinateTarget(double[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
        => ((IEnumerable<Number>)(_values ?? Array.Empty<Number>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Number>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyOverlayCoordinateTargetCollectionBuilder
{
    public static VuetifyOverlayCoordinateTarget Create(ReadOnlySpan<Number> values)
        => values.ToArray();
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifyOverlayTarget(
    Element,
    VueComponentPublicInstance,
    string,
    VuetifyOverlayCoordinateTarget)
{
    public Element? AsElement => Value as Element;

    public VueComponentPublicInstance? AsComponent => Value as VueComponentPublicInstance;

    public string? AsString => Value as string;

    public VuetifyOverlayCoordinateTarget? AsCoordinates
        => Value is VuetifyOverlayCoordinateTarget value ? value : default(VuetifyOverlayCoordinateTarget?);

    [ECMAScriptInline("'cursor'")]
    public extern static VuetifyOverlayTarget Cursor();

    [ECMAScriptInline("'parent'")]
    public extern static VuetifyOverlayTarget Parent();

    public static implicit operator VuetifyOverlayTarget(Element value)
        => new(value);

    public static implicit operator VuetifyOverlayTarget(VueComponentPublicInstance value)
        => new(value);

    public static implicit operator VuetifyOverlayTarget(string value)
        => new(value);

    public static implicit operator VuetifyOverlayTarget(VuetifyOverlayCoordinateTarget value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifyDialogTarget(
    Element,
    VueComponentPublicInstance,
    string,
    VuetifyOverlayCoordinateTarget)
{
    public Element? AsElement => Value as Element;

    public VueComponentPublicInstance? AsComponent => Value as VueComponentPublicInstance;

    public string? AsString => Value as string;

    public VuetifyOverlayCoordinateTarget? AsCoordinates
        => Value is VuetifyOverlayCoordinateTarget value ? value : default(VuetifyOverlayCoordinateTarget?);

    [ECMAScriptInline("'cursor'")]
    public extern static VuetifyDialogTarget Cursor();

    [ECMAScriptInline("'parent'")]
    public extern static VuetifyDialogTarget Parent();

    public static implicit operator VuetifyDialogTarget(Element value)
        => new(value);

    public static implicit operator VuetifyDialogTarget(VueComponentPublicInstance value)
        => new(value);

    public static implicit operator VuetifyDialogTarget(string value)
        => new(value);

    public static implicit operator VuetifyDialogTarget(VuetifyOverlayCoordinateTarget value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifyDialogActivatorTarget(Element, VueComponentPublicInstance, string)
{
    public Element? AsElement => Value as Element;

    public VueComponentPublicInstance? AsComponent => Value as VueComponentPublicInstance;

    public string? AsString => Value as string;

    [ECMAScriptInline("'parent'")]
    public extern static VuetifyDialogActivatorTarget Parent();

    public static implicit operator VuetifyDialogActivatorTarget(Element value)
        => new(value);

    public static implicit operator VuetifyDialogActivatorTarget(VueComponentPublicInstance value)
        => new(value);

    public static implicit operator VuetifyDialogActivatorTarget(string value)
        => new(value);
}
