using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

[String]
public enum VIconBtnSizeName
{
    [Description("@#x-small")]
    XSmall,

    [Description("@#small")]
    Small,

    [Description("@#default")]
    Default,

    [Description("@#large")]
    Large,

    [Description("@#x-large")]
    XLarge
}

[ECMAScript]
[Description("@#")]
public sealed record VIconBtnSizeEntry
{
    [Description("@#name")]
    public VIconBtnSizeName Name { get; init; }

    [Description("@#value")]
    public Number Value { get; init; }
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VIconBtnSizeMapCollectionBuilder), nameof(VIconBtnSizeMapCollectionBuilder.Create))]
public readonly struct VIconBtnSizeMap : System.Runtime.CompilerServices.IUnion, IEnumerable<VIconBtnSizeEntry>
{
    private readonly VIconBtnSizeEntry[]? _entries;

    private VIconBtnSizeMap(VIconBtnSizeEntry[] entries)
    {
        _entries = entries;
    }

    public VIconBtnSizeEntry[]? AsArray => _entries;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VIconBtnSizeMap From(VIconBtnSizeEntry[] entries);

    public static implicit operator VIconBtnSizeMap(VIconBtnSizeEntry[] entries)
        => new(entries);

    IEnumerator<VIconBtnSizeEntry> IEnumerable<VIconBtnSizeEntry>.GetEnumerator()
        => ((IEnumerable<VIconBtnSizeEntry>)(_entries ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VIconBtnSizeEntry>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VIconBtnSizeMapCollectionBuilder
{
    public static VIconBtnSizeMap Create(ReadOnlySpan<VIconBtnSizeEntry> entries)
        => entries.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VIconBtnTextValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly Number? _number;
    private readonly string? _string;

    private VIconBtnTextValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _number = default;
        _string = default;
    }

    private VIconBtnTextValue(Number value)
    {
        _kind = 2;
        _bool = default;
        _number = value;
        _string = default;
    }

    private VIconBtnTextValue(string value)
    {
        _kind = 3;
        _bool = default;
        _number = default;
        _string = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public string? AsString => _kind == 3 ? _string : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsNumber,
        3 => AsString,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VIconBtnTextValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VIconBtnTextValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VIconBtnTextValue From(string value);

    public static implicit operator VIconBtnTextValue(bool value)
        => new(value);

    public static implicit operator VIconBtnTextValue(Number value)
        => new(value);

    public static implicit operator VIconBtnTextValue(string value)
        => new(value);

    public static implicit operator VIconBtnTextValue(byte value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(sbyte value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(short value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(ushort value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(int value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(uint value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(float value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(double value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(decimal value)
        => new((Number)value);
}
