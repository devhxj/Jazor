namespace ECMAScript.CSS;

/// <summary>
/// AppendValues
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct AppendValues
{
    private readonly byte _kind;
    private readonly CSSStyleValue? _value1;
    private readonly string? _value2;

    private AppendValues(CSSStyleValue value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AppendValues(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CSSStyleValue? AsCSSStyleValue => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator AppendValues(CSSStyleValue value)
        => new(value);

    public static implicit operator AppendValues(string value)
        => new(value);
}

/// <summary>
/// CSSColorAngle
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CSSColorAngle
{
    private readonly byte _kind;
    private readonly CSSNumberish? _value1;
    private readonly CSSKeywordish? _value2;

    private CSSColorAngle(CSSNumberish value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CSSColorAngle(CSSKeywordish value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CSSNumberish? AsCSSNumberish => _kind == 1 ? _value1 : default;

    public CSSKeywordish? AsCSSKeywordish => _kind == 2 ? _value2 : default;

    public static implicit operator CSSColorAngle(CSSNumberish value)
        => new(value);

    public static implicit operator CSSColorAngle(CSSKeywordish value)
        => new(value);
}

/// <summary>
/// CSSColorNumber
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CSSColorNumber
{
    private readonly byte _kind;
    private readonly CSSNumberish? _value1;
    private readonly CSSKeywordish? _value2;

    private CSSColorNumber(CSSNumberish value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CSSColorNumber(CSSKeywordish value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CSSNumberish? AsCSSNumberish => _kind == 1 ? _value1 : default;

    public CSSKeywordish? AsCSSKeywordish => _kind == 2 ? _value2 : default;

    public static implicit operator CSSColorNumber(CSSNumberish value)
        => new(value);

    public static implicit operator CSSColorNumber(CSSKeywordish value)
        => new(value);
}

/// <summary>
/// CSSColorPercent
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CSSColorPercent
{
    private readonly byte _kind;
    private readonly CSSNumberish? _value1;
    private readonly CSSKeywordish? _value2;

    private CSSColorPercent(CSSNumberish value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CSSColorPercent(CSSKeywordish value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CSSNumberish? AsCSSNumberish => _kind == 1 ? _value1 : default;

    public CSSKeywordish? AsCSSKeywordish => _kind == 2 ? _value2 : default;

    public static implicit operator CSSColorPercent(CSSNumberish value)
        => new(value);

    public static implicit operator CSSColorPercent(CSSKeywordish value)
        => new(value);
}

/// <summary>
/// CSSColorRGBComp
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CSSColorRGBComp
{
    private readonly byte _kind;
    private readonly CSSNumberish? _value1;
    private readonly CSSKeywordish? _value2;

    private CSSColorRGBComp(CSSNumberish value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CSSColorRGBComp(CSSKeywordish value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CSSNumberish? AsCSSNumberish => _kind == 1 ? _value1 : default;

    public CSSKeywordish? AsCSSKeywordish => _kind == 2 ? _value2 : default;

    public static implicit operator CSSColorRGBComp(CSSNumberish value)
        => new(value);

    public static implicit operator CSSColorRGBComp(CSSKeywordish value)
        => new(value);
}

/// <summary>
/// CSSColorValueParseResult
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CSSColorValueParseResult
{
    private readonly byte _kind;
    private readonly CSSColorValue? _value1;
    private readonly CSSStyleValue? _value2;

    private CSSColorValueParseResult(CSSColorValue value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CSSColorValueParseResult(CSSStyleValue value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CSSColorValue? AsCSSColorValue => _kind == 1 ? _value1 : default;

    public CSSStyleValue? AsCSSStyleValue => _kind == 2 ? _value2 : default;

    public static implicit operator CSSColorValueParseResult(CSSColorValue value)
        => new(value);

    public static implicit operator CSSColorValueParseResult(CSSStyleValue value)
        => new(value);
}

/// <summary>
/// CSSKeywordish
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CSSKeywordish
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly CSSKeywordValue? _value2;

    private CSSKeywordish(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CSSKeywordish(CSSKeywordValue value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public CSSKeywordValue? AsCSSKeywordValue => _kind == 2 ? _value2 : default;

    public static implicit operator CSSKeywordish(string value)
        => new(value);

    public static implicit operator CSSKeywordish(CSSKeywordValue value)
        => new(value);
}

/// <summary>
/// CSSNumberish
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CSSNumberish
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly CSSNumericValue? _value2;

    private CSSNumberish(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CSSNumberish(CSSNumericValue value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public CSSNumericValue? AsCSSNumericValue => _kind == 2 ? _value2 : default;

    public static implicit operator CSSNumberish(double value)
        => new(value);

    public static implicit operator CSSNumberish(CSSNumericValue value)
        => new(value);
}

/// <summary>
/// CSSPerspectiveValue
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CSSPerspectiveValue
{
    private readonly byte _kind;
    private readonly CSSNumericValue? _value1;
    private readonly CSSKeywordish? _value2;

    private CSSPerspectiveValue(CSSNumericValue value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CSSPerspectiveValue(CSSKeywordish value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CSSNumericValue? AsCSSNumericValue => _kind == 1 ? _value1 : default;

    public CSSKeywordish? AsCSSKeywordish => _kind == 2 ? _value2 : default;

    public static implicit operator CSSPerspectiveValue(CSSNumericValue value)
        => new(value);

    public static implicit operator CSSPerspectiveValue(CSSKeywordish value)
        => new(value);
}

/// <summary>
/// CSSStringSource
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CSSStringSource
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly ReadableStream? _value2;

    private CSSStringSource(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CSSStringSource(ReadableStream value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public ReadableStream? AsReadableStream => _kind == 2 ? _value2 : default;

    public static implicit operator CSSStringSource(string value)
        => new(value);

    public static implicit operator CSSStringSource(ReadableStream value)
        => new(value);
}

/// <summary>
/// CSSStyleSheetInitMedia
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CSSStyleSheetInitMedia
{
    private readonly byte _kind;
    private readonly MediaList? _value1;
    private readonly string? _value2;

    private CSSStyleSheetInitMedia(MediaList value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CSSStyleSheetInitMedia(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public MediaList? AsMediaList => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator CSSStyleSheetInitMedia(MediaList value)
        => new(value);

    public static implicit operator CSSStyleSheetInitMedia(string value)
        => new(value);
}

/// <summary>
/// CSSToken
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CSSToken
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly CSSStyleValue? _value2;
    private readonly CSSParserValue? _value3;

    private CSSToken(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private CSSToken(CSSStyleValue value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private CSSToken(CSSParserValue value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public CSSStyleValue? AsCSSStyleValue => _kind == 2 ? _value2 : default;

    public CSSParserValue? AsCSSParserValue => _kind == 3 ? _value3 : default;

    public static implicit operator CSSToken(string value)
        => new(value);

    public static implicit operator CSSToken(CSSStyleValue value)
        => new(value);

    public static implicit operator CSSToken(CSSParserValue value)
        => new(value);
}

/// <summary>
/// CSSUnparsedSegment
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CSSUnparsedSegment
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly CSSVariableReferenceValue? _value2;

    private CSSUnparsedSegment(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CSSUnparsedSegment(CSSVariableReferenceValue value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public CSSVariableReferenceValue? AsCSSVariableReferenceValue => _kind == 2 ? _value2 : default;

    public static implicit operator CSSUnparsedSegment(string value)
        => new(value);

    public static implicit operator CSSUnparsedSegment(CSSVariableReferenceValue value)
        => new(value);
}

/// <summary>
/// SetValues
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct SetValues
{
    private readonly byte _kind;
    private readonly CSSStyleValue? _value1;
    private readonly string? _value2;

    private SetValues(CSSStyleValue value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private SetValues(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CSSStyleValue? AsCSSStyleValue => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator SetValues(CSSStyleValue value)
        => new(value);

    public static implicit operator SetValues(string value)
        => new(value);
}

/// <summary>
/// StructuralCache
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheCollectionBuilder), nameof(StructuralCacheCollectionBuilder.Create))]
public readonly struct StructuralCache : IEnumerable<AnimationEffect>
{
    private readonly byte _kind;
    private readonly AnimationEffect? _value1;
    private readonly AnimationEffect[]? _value2;

    private StructuralCache(AnimationEffect value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCache(AnimationEffect[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public AnimationEffect? AsAnimationEffect => _kind == 1 ? _value1 : default;

    public AnimationEffect[]? AsAnimationEffectArray => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCache(AnimationEffect value)
        => new(value);

    public static implicit operator StructuralCache(AnimationEffect[] value)
        => new(value);

    IEnumerator<AnimationEffect> IEnumerable<AnimationEffect>.GetEnumerator()
        => ((IEnumerable<AnimationEffect>)(AsAnimationEffectArray ?? Array.Empty<AnimationEffect>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<AnimationEffect>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheCollectionBuilder
{
    public static StructuralCache Create(ReadOnlySpan<AnimationEffect> items)
        => items.ToArray();
}

/// <summary>
/// StructuralCacheValue
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue
{
    private readonly byte _kind;
    private readonly CSSStyleValue? _value1;
    private readonly string? _value2;

    private StructuralCacheValue(CSSStyleValue value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CSSStyleValue? AsCSSStyleValue => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue(CSSStyleValue value)
        => new(value);

    public static implicit operator StructuralCacheValue(string value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue2
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue2
{
    private readonly byte _kind;
    private readonly CSSColorValue? _value1;
    private readonly CSSStyleValue? _value2;

    private StructuralCacheValue2(CSSColorValue value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue2(CSSStyleValue value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CSSColorValue? AsCSSColorValue => _kind == 1 ? _value1 : default;

    public CSSStyleValue? AsCSSStyleValue => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue2(CSSColorValue value)
        => new(value);

    public static implicit operator StructuralCacheValue2(CSSStyleValue value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue3
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue3
{
    private readonly byte _kind;
    private readonly Element? _value1;
    private readonly ProcessingInstruction? _value2;

    private StructuralCacheValue3(Element value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue3(ProcessingInstruction value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Element? AsElement => _kind == 1 ? _value1 : default;

    public ProcessingInstruction? AsProcessingInstruction => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue3(Element value)
        => new(value);

    public static implicit operator StructuralCacheValue3(ProcessingInstruction value)
        => new(value);
}

/// <summary>
/// StylePropertyMapAppendValues
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StylePropertyMapAppendValues
{
    private readonly byte _kind;
    private readonly CSSStyleValue? _value1;
    private readonly string? _value2;

    private StylePropertyMapAppendValues(CSSStyleValue value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StylePropertyMapAppendValues(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CSSStyleValue? AsCSSStyleValue => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator StylePropertyMapAppendValues(CSSStyleValue value)
        => new(value);

    public static implicit operator StylePropertyMapAppendValues(string value)
        => new(value);
}

/// <summary>
/// StylePropertyMapSetValues
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StylePropertyMapSetValues
{
    private readonly byte _kind;
    private readonly CSSStyleValue? _value1;
    private readonly string? _value2;

    private StylePropertyMapSetValues(CSSStyleValue value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StylePropertyMapSetValues(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CSSStyleValue? AsCSSStyleValue => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator StylePropertyMapSetValues(CSSStyleValue value)
        => new(value);

    public static implicit operator StylePropertyMapSetValues(string value)
        => new(value);
}

/// <summary>
/// StyleSheetOwnerNode
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StyleSheetOwnerNode
{
    private readonly byte _kind;
    private readonly Element? _value1;
    private readonly ProcessingInstruction? _value2;

    private StyleSheetOwnerNode(Element value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StyleSheetOwnerNode(ProcessingInstruction value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Element? AsElement => _kind == 1 ? _value1 : default;

    public ProcessingInstruction? AsProcessingInstruction => _kind == 2 ? _value2 : default;

    public static implicit operator StyleSheetOwnerNode(Element value)
        => new(value);

    public static implicit operator StyleSheetOwnerNode(ProcessingInstruction value)
        => new(value);
}

/// <summary>
/// WorkletAnimationEffects
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WorkletAnimationEffectsCollectionBuilder), nameof(WorkletAnimationEffectsCollectionBuilder.Create))]
public readonly struct WorkletAnimationEffects : IEnumerable<AnimationEffect>
{
    private readonly byte _kind;
    private readonly AnimationEffect? _value1;
    private readonly AnimationEffect[]? _value2;

    private WorkletAnimationEffects(AnimationEffect value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WorkletAnimationEffects(AnimationEffect[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public AnimationEffect? AsAnimationEffect => _kind == 1 ? _value1 : default;

    public AnimationEffect[]? AsAnimationEffectArray => _kind == 2 ? _value2 : default;

    public static implicit operator WorkletAnimationEffects(AnimationEffect value)
        => new(value);

    public static implicit operator WorkletAnimationEffects(AnimationEffect[] value)
        => new(value);

    IEnumerator<AnimationEffect> IEnumerable<AnimationEffect>.GetEnumerator()
        => ((IEnumerable<AnimationEffect>)(AsAnimationEffectArray ?? Array.Empty<AnimationEffect>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<AnimationEffect>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WorkletAnimationEffectsCollectionBuilder
{
    public static WorkletAnimationEffects Create(ReadOnlySpan<AnimationEffect> items)
        => items.ToArray();
}
