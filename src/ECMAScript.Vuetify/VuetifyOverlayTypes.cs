using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyOverlayOffsetValuesCollectionBuilder), nameof(VuetifyOverlayOffsetValuesCollectionBuilder.Create))]
public readonly struct VuetifyOverlayOffsetValues : System.Runtime.CompilerServices.IUnion, IEnumerable<Number>
{
    private readonly Number[]? _values;

    private VuetifyOverlayOffsetValues(Number[] values)
    {
        _values = values;
    }

    public Number[]? AsArray => _values;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyOverlayOffsetValues From(Number[] values);

    public static implicit operator VuetifyOverlayOffsetValues(Number[] values)
        => new(values);

    public static implicit operator VuetifyOverlayOffsetValues(int[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    public static implicit operator VuetifyOverlayOffsetValues(double[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
        => ((IEnumerable<Number>)(_values ?? [])).GetEnumerator();

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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyOverlayOffsetValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Number? _number;
    private readonly VuetifyOverlayOffsetValues? _values;

    private VuetifyOverlayOffsetValue(string value)
    {
        _kind = 1;
        _string = value;
        _number = default;
        _values = default;
    }

    private VuetifyOverlayOffsetValue(Number value)
    {
        _kind = 2;
        _string = default;
        _number = value;
        _values = default;
    }

    private VuetifyOverlayOffsetValue(VuetifyOverlayOffsetValues value)
    {
        _kind = 3;
        _string = default;
        _number = default;
        _values = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public VuetifyOverlayOffsetValues? AsValues => _kind == 3 ? _values : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsNumber,
        3 => AsValues,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyOverlayOffsetValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyOverlayOffsetValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyOverlayOffsetValue From(VuetifyOverlayOffsetValues value);

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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyOriginValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly VuetifyLocation? _location;
    private readonly VuetifyOriginMode? _mode;
    private readonly string? _custom;

    private VuetifyOriginValue(VuetifyLocation value)
    {
        _kind = 1;
        _location = value;
        _mode = default;
        _custom = default;
    }

    private VuetifyOriginValue(VuetifyOriginMode value)
    {
        _kind = 2;
        _location = default;
        _mode = value;
        _custom = default;
    }

    private VuetifyOriginValue(string value)
    {
        _kind = 3;
        _location = default;
        _mode = default;
        _custom = value;
    }

    public VuetifyLocation? AsLocation => _kind == 1 ? _location : default;

    public VuetifyOriginMode? AsMode => _kind == 2 ? _mode : default;

    public string? AsCustom => _kind == 3 ? _custom : default;

    public object? Value => _kind switch
    {
        1 => AsLocation,
        2 => AsMode,
        3 => AsCustom,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyOriginValue From(VuetifyLocation value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyOriginValue From(VuetifyOriginMode value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyOriginValue From(string value);

    public static implicit operator VuetifyOriginValue(VuetifyLocation value)
        => new(value);

    public static implicit operator VuetifyOriginValue(VuetifyOriginMode value)
        => new(value);

    public static implicit operator VuetifyOriginValue(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyOverlayCoordinateTarget : System.Runtime.CompilerServices.IUnion
{
    private readonly Number[]? _values;

    private VuetifyOverlayCoordinateTarget(Number[] values)
    {
        _values = values;
    }

    public Number[]? AsArray => _values;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyOverlayCoordinateTarget From(Number[] values);

    [ECMAScriptInline("[__arg1, __arg2]")]
    public extern static VuetifyOverlayCoordinateTarget From(Number x, Number y);

    public static implicit operator VuetifyOverlayCoordinateTarget(Number[] values)
        => new(values);

    public static implicit operator VuetifyOverlayCoordinateTarget(int[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    public static implicit operator VuetifyOverlayCoordinateTarget(double[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyDialogTarget : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly Element? _element;
    private readonly VueComponentPublicInstance? _component;
    private readonly string? _string;
    private readonly VuetifyOverlayCoordinateTarget? _coordinates;

    private VuetifyDialogTarget(Element value)
    {
        _kind = 1;
        _element = value;
        _component = default;
        _string = default;
        _coordinates = default;
    }

    private VuetifyDialogTarget(VueComponentPublicInstance value)
    {
        _kind = 2;
        _element = default;
        _component = value;
        _string = default;
        _coordinates = default;
    }

    private VuetifyDialogTarget(string value)
    {
        _kind = 3;
        _element = default;
        _component = default;
        _string = value;
        _coordinates = default;
    }

    private VuetifyDialogTarget(VuetifyOverlayCoordinateTarget value)
    {
        _kind = 4;
        _element = default;
        _component = default;
        _string = default;
        _coordinates = value;
    }

    public Element? AsElement => _kind == 1 ? _element : default;

    public VueComponentPublicInstance? AsComponent => _kind == 2 ? _component : default;

    public string? AsString => _kind == 3 ? _string : default;

    public VuetifyOverlayCoordinateTarget? AsCoordinates => _kind == 4 ? _coordinates : default;

    public object? Value => _kind switch
    {
        1 => AsElement,
        2 => AsComponent,
        3 => AsString,
        4 => AsCoordinates,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDialogTarget From(Element value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDialogTarget From(VueComponentPublicInstance value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDialogTarget From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDialogTarget From(VuetifyOverlayCoordinateTarget value);

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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyDialogActivatorTarget : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly Element? _element;
    private readonly VueComponentPublicInstance? _component;
    private readonly string? _string;

    private VuetifyDialogActivatorTarget(Element value)
    {
        _kind = 1;
        _element = value;
        _component = default;
        _string = default;
    }

    private VuetifyDialogActivatorTarget(VueComponentPublicInstance value)
    {
        _kind = 2;
        _element = default;
        _component = value;
        _string = default;
    }

    private VuetifyDialogActivatorTarget(string value)
    {
        _kind = 3;
        _element = default;
        _component = default;
        _string = value;
    }

    public Element? AsElement => _kind == 1 ? _element : default;

    public VueComponentPublicInstance? AsComponent => _kind == 2 ? _component : default;

    public string? AsString => _kind == 3 ? _string : default;

    public object? Value => _kind switch
    {
        1 => AsElement,
        2 => AsComponent,
        3 => AsString,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDialogActivatorTarget From(Element value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDialogActivatorTarget From(VueComponentPublicInstance value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDialogActivatorTarget From(string value);

    [ECMAScriptInline("'parent'")]
    public extern static VuetifyDialogActivatorTarget Parent();

    public static implicit operator VuetifyDialogActivatorTarget(Element value)
        => new(value);

    public static implicit operator VuetifyDialogActivatorTarget(VueComponentPublicInstance value)
        => new(value);

    public static implicit operator VuetifyDialogActivatorTarget(string value)
        => new(value);
}
