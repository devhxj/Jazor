namespace ECMAScript.TDesign;

[String]
public enum TDesignSize
{
    [Description("@#small")]
    Small,

    [Description("@#medium")]
    Medium,

    [Description("@#large")]
    Large
}

[String]
public enum TDesignButtonShape
{
    [Description("@#rectangle")]
    Rectangle,

    [Description("@#square")]
    Square,

    [Description("@#round")]
    Round,

    [Description("@#circle")]
    Circle
}

[String]
public enum TDesignButtonTheme
{
    [Description("@#default")]
    Default,

    [Description("@#primary")]
    Primary,

    [Description("@#danger")]
    Danger,

    [Description("@#warning")]
    Warning,

    [Description("@#success")]
    Success
}

[String]
public enum TDesignButtonType
{
    [Description("@#submit")]
    Submit,

    [Description("@#reset")]
    Reset,

    [Description("@#button")]
    Button
}

[String]
public enum TDesignButtonVariant
{
    [Description("@#base")]
    Base,

    [Description("@#outline")]
    Outline,

    [Description("@#dashed")]
    Dashed,

    [Description("@#text")]
    Text
}

[String]
public enum TDesignButtonTag
{
    [Description("@#button")]
    Button,

    [Description("@#a")]
    Anchor,

    [Description("@#div")]
    Div
}

[String]
public enum TDesignLayoutDirection
{
    [Description("@#vertical")]
    Vertical,

    [Description("@#horizontal")]
    Horizontal
}

[String]
public enum TDesignMenuExpandType
{
    [Description("@#normal")]
    Normal,

    [Description("@#popup")]
    Popup
}

[String]
public enum TDesignMenuTheme
{
    [Description("@#light")]
    Light,

    [Description("@#dark")]
    Dark
}

[String]
public enum TDesignTarget
{
    [Description("@#_blank")]
    Blank,

    [Description("@#_self")]
    Self,

    [Description("@#_parent")]
    Parent,

    [Description("@#_top")]
    Top
}

[String]
public enum TDesignSpaceAlign
{
    [Description("@#start")]
    Start,

    [Description("@#end")]
    End,

    [Description("@#center")]
    Center,

    [Description("@#baseline")]
    Baseline
}

[String]
public enum TDesignSpaceDirection
{
    [Description("@#vertical")]
    Vertical,

    [Description("@#horizontal")]
    Horizontal
}

[String]
public enum TDesignDividerAlign
{
    [Description("@#left")]
    Left,

    [Description("@#right")]
    Right,

    [Description("@#center")]
    Center
}

[String]
public enum TDesignDividerLayout
{
    [Description("@#horizontal")]
    Horizontal,

    [Description("@#vertical")]
    Vertical
}

[String]
public enum TDesignCardSize
{
    [Description("@#medium")]
    Medium,

    [Description("@#small")]
    Small
}

[String]
public enum TDesignCardTheme
{
    [Description("@#normal")]
    Normal,

    [Description("@#poster1")]
    Poster1,

    [Description("@#poster2")]
    Poster2
}

[String]
public enum TDesignBreadcrumbTheme
{
    [Description("@#light")]
    Light
}

[String]
public enum TDesignLinkHover
{
    [Description("@#color")]
    Color,

    [Description("@#underline")]
    Underline
}

[String]
public enum TDesignLinkTheme
{
    [Description("@#default")]
    Default,

    [Description("@#primary")]
    Primary,

    [Description("@#danger")]
    Danger,

    [Description("@#warning")]
    Warning,

    [Description("@#success")]
    Success
}

[String]
public enum TDesignTabsPlacement
{
    [Description("@#left")]
    Left,

    [Description("@#top")]
    Top,

    [Description("@#bottom")]
    Bottom,

    [Description("@#right")]
    Right
}

[String]
public enum TDesignTabsScrollPosition
{
    [Description("@#auto")]
    Auto,

    [Description("@#start")]
    Start,

    [Description("@#center")]
    Center,

    [Description("@#end")]
    End
}

[String]
public enum TDesignTabsSize
{
    [Description("@#medium")]
    Medium,

    [Description("@#large")]
    Large
}

[String]
public enum TDesignTabsTheme
{
    [Description("@#normal")]
    Normal,

    [Description("@#card")]
    Card
}

[String]
public enum TDesignAvatarShape
{
    [Description("@#circle")]
    Circle,

    [Description("@#round")]
    Round
}

[String]
public enum TDesignAvatarGroupCascading
{
    [Description("@#left-up")]
    LeftUp,

    [Description("@#right-up")]
    RightUp
}

[String]
public enum TDesignBadgeShape
{
    [Description("@#circle")]
    Circle,

    [Description("@#round")]
    Round
}

[String]
public enum TDesignBadgeSize
{
    [Description("@#small")]
    Small,

    [Description("@#medium")]
    Medium
}

[ECMAScript]
[Description("@#")]
public readonly union TDesignLinkDownloadValue(bool, string)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public string? AsString => Value as string;
}

[ECMAScript]
[Description("@#")]
public readonly union TDesignTabValue(double, string)
{
    public double? AsNumber => Value is double value ? value : default(double?);

    public string? AsString => Value as string;
}

[ECMAScript]
[Description("@#")]
public readonly union TDesignBadgeCountValue(double, string)
{
    public double? AsNumber => Value is double value ? value : default(double?);

    public string? AsString => Value as string;
}

[ECMAScript]
[Description("@#")]
public readonly union TDesignBadgeOffsetValue(double, string)
{
    public double? AsNumber => Value is double value ? value : default(double?);

    public string? AsString => Value as string;
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(TDesignBadgeOffsetCollectionBuilder), nameof(TDesignBadgeOffsetCollectionBuilder.Create))]
public readonly union TDesignBadgeOffset(TDesignBadgeOffsetValue[]) : IEnumerable<TDesignBadgeOffsetValue>
{
    public TDesignBadgeOffsetValue[]? AsValues => Value as TDesignBadgeOffsetValue[];

    IEnumerator<TDesignBadgeOffsetValue> IEnumerable<TDesignBadgeOffsetValue>.GetEnumerator()
        => ((IEnumerable<TDesignBadgeOffsetValue>)(AsValues ?? Array.Empty<TDesignBadgeOffsetValue>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<TDesignBadgeOffsetValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class TDesignBadgeOffsetCollectionBuilder
{
    public static TDesignBadgeOffset Create(ReadOnlySpan<TDesignBadgeOffsetValue> values)
        => values.ToArray();
}

[ECMAScript]
[Description("@#TabAddContext")]
public sealed record TDesignTabAddContext : VueProps
{
    [Description("@#e")]
    public required MouseEvent Event { get; init; }
}

[ECMAScript]
[Description("@#TabRemoveContext")]
public sealed record TDesignTabRemoveContext : VueProps
{
    [Description("@#value")]
    public required TDesignTabValue Value { get; init; }

    [Description("@#index")]
    public required int Index { get; init; }

    [Description("@#e")]
    public required MouseEvent Event { get; init; }
}

[ECMAScript]
[Description("@#TabPanelRemoveContext")]
public sealed record TDesignTabPanelRemoveContext : VueProps
{
    [Description("@#value")]
    public required TDesignTabValue Value { get; init; }

    [Description("@#e")]
    public required MouseEvent Event { get; init; }
}

[ECMAScript]
[Description("@#TabsDragSortContext")]
public sealed record TDesignTabsDragSortContext : VueProps
{
    [Description("@#currentIndex")]
    public required int CurrentIndex { get; init; }

    [Description("@#current")]
    public required TDesignTabValue Current { get; init; }

    [Description("@#targetIndex")]
    public required int TargetIndex { get; init; }

    [Description("@#target")]
    public required TDesignTabValue Target { get; init; }
}

[ECMAScript]
[Description("@#AvatarErrorContext")]
public sealed record TDesignAvatarErrorContext : VueProps
{
    [Description("@#e")]
    public required Event Event { get; init; }
}

[ECMAScript]
[Description("@#Styles")]
public sealed record TDesignStyles : VueDictionary<VueStringNumberValue>
{
}

[ECMAScript]
[Union]
[Description("@#")]
public readonly struct TDesignDimensionValue : IUnion
{
    private readonly byte _kind;
    private readonly Number? _number;
    private readonly string? _string;

    public TDesignDimensionValue(Number value)
    {
        _kind = 1;
        _number = value;
        _string = default;
    }

    public TDesignDimensionValue(string value)
    {
        _kind = 2;
        _number = default;
        _string = value;
    }

    public Number? AsNumber => _kind == 1 ? _number : default;

    public string? AsString => _kind == 2 ? _string : default;

    public object? Value => _kind switch
    {
        1 => AsNumber,
        2 => AsString,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static TDesignDimensionValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static TDesignDimensionValue From(string value);

    public static implicit operator TDesignDimensionValue(Number value)
        => new(value);

    public static implicit operator TDesignDimensionValue(string value)
        => new(value);

    public static implicit operator TDesignDimensionValue(byte value)
        => new((Number)value);

    public static implicit operator TDesignDimensionValue(sbyte value)
        => new((Number)value);

    public static implicit operator TDesignDimensionValue(short value)
        => new((Number)value);

    public static implicit operator TDesignDimensionValue(ushort value)
        => new((Number)value);

    public static implicit operator TDesignDimensionValue(int value)
        => new((Number)value);

    public static implicit operator TDesignDimensionValue(uint value)
        => new((Number)value);

    public static implicit operator TDesignDimensionValue(float value)
        => new((Number)value);

    public static implicit operator TDesignDimensionValue(double value)
        => new((Number)value);

    public static implicit operator TDesignDimensionValue(decimal value)
        => new((Number)value);
}

[ECMAScript]
[Union]
[Description("@#")]
[CollectionBuilder(typeof(TDesignDimensionValuesCollectionBuilder), nameof(TDesignDimensionValuesCollectionBuilder.Create))]
public readonly struct TDesignDimensionValues : IUnion, IEnumerable<TDesignDimensionValue>
{
    private readonly TDesignDimensionValue[]? _values;

    public TDesignDimensionValues(TDesignDimensionValue[] values)
    {
        _values = values;
    }

    public TDesignDimensionValue[]? AsArray => _values;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static TDesignDimensionValues From(TDesignDimensionValue[] values);

    public static implicit operator TDesignDimensionValues(TDesignDimensionValue[] values)
        => new(values);

    IEnumerator<TDesignDimensionValue> IEnumerable<TDesignDimensionValue>.GetEnumerator()
        => ((IEnumerable<TDesignDimensionValue>)(_values ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<TDesignDimensionValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class TDesignDimensionValuesCollectionBuilder
{
    public static TDesignDimensionValues Create(ReadOnlySpan<TDesignDimensionValue> values)
        => values.ToArray();
}

[ECMAScript]
[Union]
[Description("@#")]
public readonly struct TDesignMenuWidthValue : IUnion
{
    private readonly byte _kind;
    private readonly TDesignDimensionValue? _value;
    private readonly TDesignDimensionValues? _values;

    public TDesignMenuWidthValue(TDesignDimensionValue value)
    {
        _kind = 1;
        _value = value;
        _values = default;
    }

    public TDesignMenuWidthValue(TDesignDimensionValues value)
    {
        _kind = 2;
        _value = default;
        _values = value;
    }

    public TDesignDimensionValue? AsValue => _kind == 1 ? _value : default;

    public TDesignDimensionValues? AsValues => _kind == 2 ? _values : default;

    public object? Value => _kind switch
    {
        1 => AsValue,
        2 => AsValues,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static TDesignMenuWidthValue From(TDesignDimensionValue value);

    [ECMAScriptInline("__arg1")]
    public extern static TDesignMenuWidthValue From(TDesignDimensionValues value);

    public static implicit operator TDesignMenuWidthValue(TDesignDimensionValue value)
        => new(value);

    public static implicit operator TDesignMenuWidthValue(TDesignDimensionValues value)
        => new(value);

    public static implicit operator TDesignMenuWidthValue(string value)
        => new((TDesignDimensionValue)value);

    public static implicit operator TDesignMenuWidthValue(Number value)
        => new((TDesignDimensionValue)value);

    public static implicit operator TDesignMenuWidthValue(byte value)
        => new((TDesignDimensionValue)value);

    public static implicit operator TDesignMenuWidthValue(sbyte value)
        => new((TDesignDimensionValue)value);

    public static implicit operator TDesignMenuWidthValue(short value)
        => new((TDesignDimensionValue)value);

    public static implicit operator TDesignMenuWidthValue(ushort value)
        => new((TDesignDimensionValue)value);

    public static implicit operator TDesignMenuWidthValue(int value)
        => new((TDesignDimensionValue)value);

    public static implicit operator TDesignMenuWidthValue(uint value)
        => new((TDesignDimensionValue)value);

    public static implicit operator TDesignMenuWidthValue(float value)
        => new((TDesignDimensionValue)value);

    public static implicit operator TDesignMenuWidthValue(double value)
        => new((TDesignDimensionValue)value);

    public static implicit operator TDesignMenuWidthValue(decimal value)
        => new((TDesignDimensionValue)value);

    public static implicit operator TDesignMenuWidthValue(TDesignDimensionValue[] values)
        => new((TDesignDimensionValues)values);
}

[ECMAScript]
[Union]
[Description("@#")]
public readonly struct TDesignSpaceSizeValue : IUnion
{
    private readonly byte _kind;
    private readonly Number? _number;
    private readonly string? _string;
    private readonly TDesignSize? _size;

    public TDesignSpaceSizeValue(Number value)
    {
        _kind = 1;
        _number = value;
        _string = default;
        _size = default;
    }

    public TDesignSpaceSizeValue(string value)
    {
        _kind = 2;
        _number = default;
        _string = value;
        _size = default;
    }

    public TDesignSpaceSizeValue(TDesignSize value)
    {
        _kind = 3;
        _number = default;
        _string = default;
        _size = value;
    }

    public Number? AsNumber => _kind == 1 ? _number : default;

    public string? AsString => _kind == 2 ? _string : default;

    public TDesignSize? AsSize => _kind == 3 ? _size : default;

    public object? Value => _kind switch
    {
        1 => AsNumber,
        2 => AsString,
        3 => AsSize,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static TDesignSpaceSizeValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static TDesignSpaceSizeValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static TDesignSpaceSizeValue From(TDesignSize value);

    public static implicit operator TDesignSpaceSizeValue(Number value)
        => new(value);

    public static implicit operator TDesignSpaceSizeValue(string value)
        => new(value);

    public static implicit operator TDesignSpaceSizeValue(TDesignSize value)
        => new(value);

    public static implicit operator TDesignSpaceSizeValue(byte value)
        => new((Number)value);

    public static implicit operator TDesignSpaceSizeValue(sbyte value)
        => new((Number)value);

    public static implicit operator TDesignSpaceSizeValue(short value)
        => new((Number)value);

    public static implicit operator TDesignSpaceSizeValue(ushort value)
        => new((Number)value);

    public static implicit operator TDesignSpaceSizeValue(int value)
        => new((Number)value);

    public static implicit operator TDesignSpaceSizeValue(uint value)
        => new((Number)value);

    public static implicit operator TDesignSpaceSizeValue(float value)
        => new((Number)value);

    public static implicit operator TDesignSpaceSizeValue(double value)
        => new((Number)value);

    public static implicit operator TDesignSpaceSizeValue(decimal value)
        => new((Number)value);
}

[ECMAScript]
[Union]
[Description("@#")]
[CollectionBuilder(typeof(TDesignSpaceSizeValuesCollectionBuilder), nameof(TDesignSpaceSizeValuesCollectionBuilder.Create))]
public readonly struct TDesignSpaceSizeValues : IUnion, IEnumerable<TDesignSpaceSizeValue>
{
    private readonly TDesignSpaceSizeValue[]? _values;

    public TDesignSpaceSizeValues(TDesignSpaceSizeValue[] values)
    {
        _values = values;
    }

    public TDesignSpaceSizeValue[]? AsArray => _values;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static TDesignSpaceSizeValues From(TDesignSpaceSizeValue[] values);

    public static implicit operator TDesignSpaceSizeValues(TDesignSpaceSizeValue[] values)
        => new(values);

    IEnumerator<TDesignSpaceSizeValue> IEnumerable<TDesignSpaceSizeValue>.GetEnumerator()
        => ((IEnumerable<TDesignSpaceSizeValue>)(_values ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<TDesignSpaceSizeValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class TDesignSpaceSizeValuesCollectionBuilder
{
    public static TDesignSpaceSizeValues Create(ReadOnlySpan<TDesignSpaceSizeValue> values)
        => values.ToArray();
}

[ECMAScript]
[Union]
[Description("@#")]
public readonly struct TDesignSpaceSize : IUnion
{
    private readonly byte _kind;
    private readonly TDesignSpaceSizeValue? _value;
    private readonly TDesignSpaceSizeValues? _values;

    public TDesignSpaceSize(TDesignSpaceSizeValue value)
    {
        _kind = 1;
        _value = value;
        _values = default;
    }

    public TDesignSpaceSize(TDesignSpaceSizeValues value)
    {
        _kind = 2;
        _value = default;
        _values = value;
    }

    public TDesignSpaceSizeValue? AsValue => _kind == 1 ? _value : default;

    public TDesignSpaceSizeValues? AsValues => _kind == 2 ? _values : default;

    public object? Value => _kind switch
    {
        1 => AsValue,
        2 => AsValues,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static TDesignSpaceSize From(TDesignSpaceSizeValue value);

    [ECMAScriptInline("__arg1")]
    public extern static TDesignSpaceSize From(TDesignSpaceSizeValues value);

    public static implicit operator TDesignSpaceSize(TDesignSpaceSizeValue value)
        => new(value);

    public static implicit operator TDesignSpaceSize(TDesignSpaceSizeValues value)
        => new(value);

    public static implicit operator TDesignSpaceSize(Number value)
        => new((TDesignSpaceSizeValue)value);

    public static implicit operator TDesignSpaceSize(string value)
        => new((TDesignSpaceSizeValue)value);

    public static implicit operator TDesignSpaceSize(TDesignSize value)
        => new((TDesignSpaceSizeValue)value);

    public static implicit operator TDesignSpaceSize(byte value)
        => new((TDesignSpaceSizeValue)value);

    public static implicit operator TDesignSpaceSize(sbyte value)
        => new((TDesignSpaceSizeValue)value);

    public static implicit operator TDesignSpaceSize(short value)
        => new((TDesignSpaceSizeValue)value);

    public static implicit operator TDesignSpaceSize(ushort value)
        => new((TDesignSpaceSizeValue)value);

    public static implicit operator TDesignSpaceSize(int value)
        => new((TDesignSpaceSizeValue)value);

    public static implicit operator TDesignSpaceSize(uint value)
        => new((TDesignSpaceSizeValue)value);

    public static implicit operator TDesignSpaceSize(float value)
        => new((TDesignSpaceSizeValue)value);

    public static implicit operator TDesignSpaceSize(double value)
        => new((TDesignSpaceSizeValue)value);

    public static implicit operator TDesignSpaceSize(decimal value)
        => new((TDesignSpaceSizeValue)value);

    public static implicit operator TDesignSpaceSize(TDesignSpaceSizeValue[] values)
        => new((TDesignSpaceSizeValues)values);
}

[ECMAScript]
[Union]
[Description("@#")]
public readonly struct TDesignMenuValue : IUnion
{
    private readonly byte _kind;
    private readonly Number? _number;
    private readonly string? _string;

    public TDesignMenuValue(Number value)
    {
        _kind = 1;
        _number = value;
        _string = default;
    }

    public TDesignMenuValue(string value)
    {
        _kind = 2;
        _number = default;
        _string = value;
    }

    public Number? AsNumber => _kind == 1 ? _number : default;

    public string? AsString => _kind == 2 ? _string : default;

    public object? Value => _kind switch
    {
        1 => AsNumber,
        2 => AsString,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static TDesignMenuValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static TDesignMenuValue From(string value);

    public static implicit operator TDesignMenuValue(Number value)
        => new(value);

    public static implicit operator TDesignMenuValue(string value)
        => new(value);

    public static implicit operator TDesignMenuValue(byte value)
        => new((Number)value);

    public static implicit operator TDesignMenuValue(sbyte value)
        => new((Number)value);

    public static implicit operator TDesignMenuValue(short value)
        => new((Number)value);

    public static implicit operator TDesignMenuValue(ushort value)
        => new((Number)value);

    public static implicit operator TDesignMenuValue(int value)
        => new((Number)value);

    public static implicit operator TDesignMenuValue(uint value)
        => new((Number)value);

    public static implicit operator TDesignMenuValue(float value)
        => new((Number)value);

    public static implicit operator TDesignMenuValue(double value)
        => new((Number)value);

    public static implicit operator TDesignMenuValue(decimal value)
        => new((Number)value);
}

[ECMAScript]
[Union]
[Description("@#")]
public readonly struct TDesignMenuQueryValue : IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly string[]? _strings;

    public TDesignMenuQueryValue(string value)
    {
        _kind = 1;
        _string = value;
        _strings = default;
    }

    public TDesignMenuQueryValue(string[] value)
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
    public extern static TDesignMenuQueryValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static TDesignMenuQueryValue From(string[] value);

    public static implicit operator TDesignMenuQueryValue(string value)
        => new(value);

    public static implicit operator TDesignMenuQueryValue(string[] value)
        => new(value);
}

[ECMAScript]
[Description("@#MenuQueryData")]
public sealed record TDesignMenuQueryData : VueDictionary<TDesignMenuQueryValue>
{
}

[ECMAScript]
[Description("@#MenuRoute")]
public sealed record TDesignMenuRoute : VueProps
{
    [Description("@#path")]
    public string? Path { get; init; }

    [Description("@#name")]
    public string? Name { get; init; }

    [Description("@#hash")]
    public string? Hash { get; init; }

    [Description("@#query")]
    public TDesignMenuQueryData? Query { get; init; }

    [Description("@#params")]
    public TDesignMenuQueryData? Params { get; init; }
}

[ECMAScript]
[Union]
[Description("@#")]
public readonly struct TDesignMenuRouteTarget : IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly TDesignMenuRoute? _route;

    public TDesignMenuRouteTarget(string value)
    {
        _kind = 1;
        _string = value;
        _route = default;
    }

    public TDesignMenuRouteTarget(TDesignMenuRoute value)
    {
        _kind = 2;
        _string = default;
        _route = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public TDesignMenuRoute? AsRoute => _kind == 2 ? _route : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsRoute,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static TDesignMenuRouteTarget From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static TDesignMenuRouteTarget From(TDesignMenuRoute value);

    public static implicit operator TDesignMenuRouteTarget(string value)
        => new(value);

    public static implicit operator TDesignMenuRouteTarget(TDesignMenuRoute value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public sealed record TDesignMenuItemClickContext : VueProps
{
    [Description("@#e")]
    public required MouseEvent Event { get; init; }

    [Description("@#value")]
    public required TDesignMenuValue Value { get; init; }
}
