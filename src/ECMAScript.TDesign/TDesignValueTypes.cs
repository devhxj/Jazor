namespace ECMAScript.TDesign;

// Defines TDesign value domains, typed payload records, and erased union authoring contracts.
// 定义 TDesign 值域、强类型载荷 record 与擦除 union；可安全表达的联合值统一使用 C# 原生 union。

[String]
public enum TSize
{
    [Description("@#small")]
    Small,

    [Description("@#medium")]
    Medium,

    [Description("@#large")]
    Large
}

[String]
public enum TButtonShape
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
public enum TButtonTheme
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
public enum TButtonType
{
    [Description("@#submit")]
    Submit,

    [Description("@#reset")]
    Reset,

    [Description("@#button")]
    Button
}

[String]
public enum TButtonVariant
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
public enum TButtonTag
{
    [Description("@#button")]
    Button,

    [Description("@#a")]
    Anchor,

    [Description("@#div")]
    Div
}

[String]
public enum TLayoutDirection
{
    [Description("@#vertical")]
    Vertical,

    [Description("@#horizontal")]
    Horizontal
}

[String]
public enum TMenuExpandType
{
    [Description("@#normal")]
    Normal,

    [Description("@#popup")]
    Popup
}

[String]
public enum TMenuTheme
{
    [Description("@#light")]
    Light,

    [Description("@#dark")]
    Dark
}

[String]
public enum TTarget
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
public enum TSpaceAlign
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
public enum TSpaceDirection
{
    [Description("@#vertical")]
    Vertical,

    [Description("@#horizontal")]
    Horizontal
}

[String]
public enum TDividerAlign
{
    [Description("@#left")]
    Left,

    [Description("@#right")]
    Right,

    [Description("@#center")]
    Center
}

[String]
public enum TDividerLayout
{
    [Description("@#horizontal")]
    Horizontal,

    [Description("@#vertical")]
    Vertical
}

[String]
public enum TCardSize
{
    [Description("@#medium")]
    Medium,

    [Description("@#small")]
    Small
}

[String]
public enum TCardTheme
{
    [Description("@#normal")]
    Normal,

    [Description("@#poster1")]
    Poster1,

    [Description("@#poster2")]
    Poster2
}

[String]
public enum TBreadcrumbTheme
{
    [Description("@#light")]
    Light
}

[String]
public enum TLinkHover
{
    [Description("@#color")]
    Color,

    [Description("@#underline")]
    Underline
}

[String]
public enum TLinkTheme
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
public enum TTabsPlacement
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
public enum TTabsScrollPosition
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
public enum TTabsSize
{
    [Description("@#medium")]
    Medium,

    [Description("@#large")]
    Large
}

[String]
public enum TTabsTheme
{
    [Description("@#normal")]
    Normal,

    [Description("@#card")]
    Card
}

[String]
public enum TAvatarShape
{
    [Description("@#circle")]
    Circle,

    [Description("@#round")]
    Round
}

[String]
public enum TAvatarGroupCascading
{
    [Description("@#left-up")]
    LeftUp,

    [Description("@#right-up")]
    RightUp
}

[String]
public enum TBadgeShape
{
    [Description("@#circle")]
    Circle,

    [Description("@#round")]
    Round
}

[String]
public enum TBadgeSize
{
    [Description("@#small")]
    Small,

    [Description("@#medium")]
    Medium
}

[ECMAScript]
[Description("@#")]
public readonly union TLinkDownloadValue(bool, string)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public string? AsString => Value as string;
}

[ECMAScript]
[Description("@#")]
public readonly union TTabValue(double, string)
{
    public double? AsNumber => Value is double value ? value : default(double?);

    public string? AsString => Value as string;
}

[ECMAScript]
[Description("@#")]
public readonly union TBadgeCountValue(double, string)
{
    public double? AsNumber => Value is double value ? value : default(double?);

    public string? AsString => Value as string;
}

[ECMAScript]
[Description("@#")]
public readonly union TBadgeOffsetValue(double, string)
{
    public double? AsNumber => Value is double value ? value : default(double?);

    public string? AsString => Value as string;
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(TBadgeOffsetCollectionBuilder), nameof(TBadgeOffsetCollectionBuilder.Create))]
public readonly union TBadgeOffset(TBadgeOffsetValue[]) : IEnumerable<TBadgeOffsetValue>
{
    public TBadgeOffsetValue[]? AsValues => Value as TBadgeOffsetValue[];

    IEnumerator<TBadgeOffsetValue> IEnumerable<TBadgeOffsetValue>.GetEnumerator()
        => ((IEnumerable<TBadgeOffsetValue>)(AsValues ?? Array.Empty<TBadgeOffsetValue>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<TBadgeOffsetValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class TBadgeOffsetCollectionBuilder
{
    public static TBadgeOffset Create(ReadOnlySpan<TBadgeOffsetValue> values)
        => values.ToArray();
}

[ECMAScript]
[Description("@#TabAddContext")]
public sealed record TTabAddContext : VueProps
{
    [Description("@#e")]
    public required MouseEvent Event { get; init; }
}

[ECMAScript]
[Description("@#TabRemoveContext")]
public sealed record TTabRemoveContext : VueProps
{
    [Description("@#value")]
    public required TTabValue Value { get; init; }

    [Description("@#index")]
    public required int Index { get; init; }

    [Description("@#e")]
    public required MouseEvent Event { get; init; }
}

[ECMAScript]
[Description("@#TabPanelRemoveContext")]
public sealed record TTabPanelRemoveContext : VueProps
{
    [Description("@#value")]
    public required TTabValue Value { get; init; }

    [Description("@#e")]
    public required MouseEvent Event { get; init; }
}

[ECMAScript]
[Description("@#TabsDragSortContext")]
public sealed record TTabsDragSortContext : VueProps
{
    [Description("@#currentIndex")]
    public required int CurrentIndex { get; init; }

    [Description("@#current")]
    public required TTabValue Current { get; init; }

    [Description("@#targetIndex")]
    public required int TargetIndex { get; init; }

    [Description("@#target")]
    public required TTabValue Target { get; init; }
}

[ECMAScript]
[Description("@#AvatarErrorContext")]
public sealed record TAvatarErrorContext : VueProps
{
    [Description("@#e")]
    public required Event Event { get; init; }
}

[ECMAScript]
[Description("@#Styles")]
public sealed record TStyles : VueDictionary<VueStringNumberValue>
{
}

[ECMAScript]
[Description("@#")]
public readonly union TDimensionValue(Number, string)
{
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    public string? AsString
        => Value is string value ? value : default(string?);

    public static implicit operator TDimensionValue(Number value)
        => new(value);

    public static implicit operator TDimensionValue(string value)
        => new(value);

    public static implicit operator TDimensionValue(byte value)
        => new((Number)value);

    public static implicit operator TDimensionValue(sbyte value)
        => new((Number)value);

    public static implicit operator TDimensionValue(short value)
        => new((Number)value);

    public static implicit operator TDimensionValue(ushort value)
        => new((Number)value);

    public static implicit operator TDimensionValue(int value)
        => new((Number)value);

    public static implicit operator TDimensionValue(uint value)
        => new((Number)value);

    public static implicit operator TDimensionValue(float value)
        => new((Number)value);

    public static implicit operator TDimensionValue(double value)
        => new((Number)value);

    public static implicit operator TDimensionValue(decimal value)
        => new((Number)value);
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(TDimensionValuesCollectionBuilder), nameof(TDimensionValuesCollectionBuilder.Create))]
public readonly union TDimensionValues(TDimensionValue[]) : IEnumerable<TDimensionValue>
{
    public TDimensionValue[]? AsArray
        => Value is TDimensionValue[] value ? value : default(TDimensionValue[]?);

    public static implicit operator TDimensionValues(TDimensionValue[] values)
        => new(values);

    IEnumerator<TDimensionValue> IEnumerable<TDimensionValue>.GetEnumerator()
        => ((IEnumerable<TDimensionValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<TDimensionValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class TDimensionValuesCollectionBuilder
{
    public static TDimensionValues Create(ReadOnlySpan<TDimensionValue> values)
        => values.ToArray();
}

[ECMAScript]
[Description("@#")]
public readonly union TMenuWidthValue(TDimensionValue, TDimensionValues)
{
    public TDimensionValue? AsValue
        => Value is TDimensionValue value ? value : default(TDimensionValue?);

    public TDimensionValues? AsValues
        => Value is TDimensionValues value ? value : default(TDimensionValues?);

    public static implicit operator TMenuWidthValue(TDimensionValue value)
        => new(value);

    public static implicit operator TMenuWidthValue(TDimensionValues value)
        => new(value);

    public static implicit operator TMenuWidthValue(string value)
        => new((TDimensionValue)value);

    public static implicit operator TMenuWidthValue(Number value)
        => new((TDimensionValue)value);

    public static implicit operator TMenuWidthValue(byte value)
        => new((TDimensionValue)value);

    public static implicit operator TMenuWidthValue(sbyte value)
        => new((TDimensionValue)value);

    public static implicit operator TMenuWidthValue(short value)
        => new((TDimensionValue)value);

    public static implicit operator TMenuWidthValue(ushort value)
        => new((TDimensionValue)value);

    public static implicit operator TMenuWidthValue(int value)
        => new((TDimensionValue)value);

    public static implicit operator TMenuWidthValue(uint value)
        => new((TDimensionValue)value);

    public static implicit operator TMenuWidthValue(float value)
        => new((TDimensionValue)value);

    public static implicit operator TMenuWidthValue(double value)
        => new((TDimensionValue)value);

    public static implicit operator TMenuWidthValue(decimal value)
        => new((TDimensionValue)value);

    public static implicit operator TMenuWidthValue(TDimensionValue[] values)
        => new((TDimensionValues)values);
}

[ECMAScript]
[Description("@#")]
public readonly union TSpaceSizeValue(
    Number,
    string,
    TSize)
{
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    public string? AsString
        => Value is string value ? value : default(string?);

    public TSize? AsSize
        => Value is TSize value ? value : default(TSize?);

    public static implicit operator TSpaceSizeValue(Number value)
        => new(value);

    public static implicit operator TSpaceSizeValue(string value)
        => new(value);

    public static implicit operator TSpaceSizeValue(TSize value)
        => new(value);

    public static implicit operator TSpaceSizeValue(byte value)
        => new((Number)value);

    public static implicit operator TSpaceSizeValue(sbyte value)
        => new((Number)value);

    public static implicit operator TSpaceSizeValue(short value)
        => new((Number)value);

    public static implicit operator TSpaceSizeValue(ushort value)
        => new((Number)value);

    public static implicit operator TSpaceSizeValue(int value)
        => new((Number)value);

    public static implicit operator TSpaceSizeValue(uint value)
        => new((Number)value);

    public static implicit operator TSpaceSizeValue(float value)
        => new((Number)value);

    public static implicit operator TSpaceSizeValue(double value)
        => new((Number)value);

    public static implicit operator TSpaceSizeValue(decimal value)
        => new((Number)value);
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(TSpaceSizeValuesCollectionBuilder), nameof(TSpaceSizeValuesCollectionBuilder.Create))]
public readonly union TSpaceSizeValues(TSpaceSizeValue[]) : IEnumerable<TSpaceSizeValue>
{
    public TSpaceSizeValue[]? AsArray
        => Value is TSpaceSizeValue[] value ? value : default(TSpaceSizeValue[]?);

    public static implicit operator TSpaceSizeValues(TSpaceSizeValue[] values)
        => new(values);

    IEnumerator<TSpaceSizeValue> IEnumerable<TSpaceSizeValue>.GetEnumerator()
        => ((IEnumerable<TSpaceSizeValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<TSpaceSizeValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class TSpaceSizeValuesCollectionBuilder
{
    public static TSpaceSizeValues Create(ReadOnlySpan<TSpaceSizeValue> values)
        => values.ToArray();
}

[ECMAScript]
[Description("@#")]
public readonly union TSpaceSize(TSpaceSizeValue, TSpaceSizeValues)
{
    public TSpaceSizeValue? AsValue
        => Value is TSpaceSizeValue value ? value : default(TSpaceSizeValue?);

    public TSpaceSizeValues? AsValues
        => Value is TSpaceSizeValues value ? value : default(TSpaceSizeValues?);

    public static implicit operator TSpaceSize(TSpaceSizeValue value)
        => new(value);

    public static implicit operator TSpaceSize(TSpaceSizeValues value)
        => new(value);

    public static implicit operator TSpaceSize(Number value)
        => new((TSpaceSizeValue)value);

    public static implicit operator TSpaceSize(string value)
        => new((TSpaceSizeValue)value);

    public static implicit operator TSpaceSize(TSize value)
        => new((TSpaceSizeValue)value);

    public static implicit operator TSpaceSize(byte value)
        => new((TSpaceSizeValue)value);

    public static implicit operator TSpaceSize(sbyte value)
        => new((TSpaceSizeValue)value);

    public static implicit operator TSpaceSize(short value)
        => new((TSpaceSizeValue)value);

    public static implicit operator TSpaceSize(ushort value)
        => new((TSpaceSizeValue)value);

    public static implicit operator TSpaceSize(int value)
        => new((TSpaceSizeValue)value);

    public static implicit operator TSpaceSize(uint value)
        => new((TSpaceSizeValue)value);

    public static implicit operator TSpaceSize(float value)
        => new((TSpaceSizeValue)value);

    public static implicit operator TSpaceSize(double value)
        => new((TSpaceSizeValue)value);

    public static implicit operator TSpaceSize(decimal value)
        => new((TSpaceSizeValue)value);

    public static implicit operator TSpaceSize(TSpaceSizeValue[] values)
        => new((TSpaceSizeValues)values);
}

[ECMAScript]
[Description("@#")]
public readonly union TMenuValue(Number, string)
{
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    public string? AsString
        => Value is string value ? value : default(string?);

    public static implicit operator TMenuValue(Number value)
        => new(value);

    public static implicit operator TMenuValue(string value)
        => new(value);

    public static implicit operator TMenuValue(byte value)
        => new((Number)value);

    public static implicit operator TMenuValue(sbyte value)
        => new((Number)value);

    public static implicit operator TMenuValue(short value)
        => new((Number)value);

    public static implicit operator TMenuValue(ushort value)
        => new((Number)value);

    public static implicit operator TMenuValue(int value)
        => new((Number)value);

    public static implicit operator TMenuValue(uint value)
        => new((Number)value);

    public static implicit operator TMenuValue(float value)
        => new((Number)value);

    public static implicit operator TMenuValue(double value)
        => new((Number)value);

    public static implicit operator TMenuValue(decimal value)
        => new((Number)value);
}

[ECMAScript]
[Description("@#")]
public readonly union TMenuQueryValue(string, string[])
{
    public string? AsString
        => Value is string value ? value : default(string?);

    public string[]? AsStrings
        => Value is string[] value ? value : default(string[]?);

    public static implicit operator TMenuQueryValue(string value)
        => new(value);

    public static implicit operator TMenuQueryValue(string[] value)
        => new(value);
}

[ECMAScript]
[Description("@#MenuQueryData")]
public sealed record TMenuQueryData : VueDictionary<TMenuQueryValue>
{
}

[ECMAScript]
[Description("@#MenuRoute")]
public sealed record TMenuRoute : VueProps
{
    [Description("@#path")]
    public string? Path { get; init; }

    [Description("@#name")]
    public string? Name { get; init; }

    [Description("@#hash")]
    public string? Hash { get; init; }

    [Description("@#query")]
    public TMenuQueryData? Query { get; init; }

    [Description("@#params")]
    public TMenuQueryData? Params { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union TMenuRouteTarget(string, TMenuRoute)
{
    public string? AsString
        => Value is string value ? value : default(string?);

    public TMenuRoute? AsRoute
        => Value is TMenuRoute value ? value : default(TMenuRoute?);

    public static implicit operator TMenuRouteTarget(string value)
        => new(value);

    public static implicit operator TMenuRouteTarget(TMenuRoute value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public sealed record TMenuItemClickContext : VueProps
{
    [Description("@#e")]
    public required MouseEvent Event { get; init; }

    [Description("@#value")]
    public required TMenuValue Value { get; init; }
}
