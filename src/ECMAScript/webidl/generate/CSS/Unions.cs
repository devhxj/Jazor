namespace ECMAScript.CSS;

/// <summary>
/// AppendValues
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AppendValues(CSSStyleValue, string)
{

    public CSSStyleValue? AsCSSStyleValue => Value is CSSStyleValue value ? value : default(CSSStyleValue?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator AppendValues(CSSStyleValue value)
        => new(value);

    public static implicit operator AppendValues(string value)
        => new(value);
}

/// <summary>
/// CSSColorAngle
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSColorAngle(CSSNumberish, CSSKeywordish)
{

    public CSSNumberish? AsCSSNumberish => Value is CSSNumberish value ? value : default(CSSNumberish?);

    public CSSKeywordish? AsCSSKeywordish => Value is CSSKeywordish value ? value : default(CSSKeywordish?);

    public static implicit operator CSSColorAngle(CSSNumberish value)
        => new(value);

    public static implicit operator CSSColorAngle(CSSKeywordish value)
        => new(value);
}

/// <summary>
/// CSSColorNumber
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSColorNumber(CSSNumberish, CSSKeywordish)
{

    public CSSNumberish? AsCSSNumberish => Value is CSSNumberish value ? value : default(CSSNumberish?);

    public CSSKeywordish? AsCSSKeywordish => Value is CSSKeywordish value ? value : default(CSSKeywordish?);

    public static implicit operator CSSColorNumber(CSSNumberish value)
        => new(value);

    public static implicit operator CSSColorNumber(CSSKeywordish value)
        => new(value);
}

/// <summary>
/// CSSColorPercent
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSColorPercent(CSSNumberish, CSSKeywordish)
{

    public CSSNumberish? AsCSSNumberish => Value is CSSNumberish value ? value : default(CSSNumberish?);

    public CSSKeywordish? AsCSSKeywordish => Value is CSSKeywordish value ? value : default(CSSKeywordish?);

    public static implicit operator CSSColorPercent(CSSNumberish value)
        => new(value);

    public static implicit operator CSSColorPercent(CSSKeywordish value)
        => new(value);
}

/// <summary>
/// CSSColorRGBComp
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSColorRGBComp(CSSNumberish, CSSKeywordish)
{

    public CSSNumberish? AsCSSNumberish => Value is CSSNumberish value ? value : default(CSSNumberish?);

    public CSSKeywordish? AsCSSKeywordish => Value is CSSKeywordish value ? value : default(CSSKeywordish?);

    public static implicit operator CSSColorRGBComp(CSSNumberish value)
        => new(value);

    public static implicit operator CSSColorRGBComp(CSSKeywordish value)
        => new(value);
}

/// <summary>
/// CSSColorValueParseResult
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSColorValueParseResult(CSSColorValue, CSSStyleValue)
{

    public CSSColorValue? AsCSSColorValue => Value is CSSColorValue value ? value : default(CSSColorValue?);

    public CSSStyleValue? AsCSSStyleValue => Value is CSSStyleValue value ? value : default(CSSStyleValue?);

    public static implicit operator CSSColorValueParseResult(CSSColorValue value)
        => new(value);

    public static implicit operator CSSColorValueParseResult(CSSStyleValue value)
        => new(value);
}

/// <summary>
/// CSSKeywordish
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSKeywordish(string, CSSKeywordValue)
{

    public string? AsString => Value is string value ? value : default(string?);

    public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    public static implicit operator CSSKeywordish(string value)
        => new(value);

    public static implicit operator CSSKeywordish(CSSKeywordValue value)
        => new(value);
}

/// <summary>
/// CSSNumberish
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSNumberish(double, CSSNumericValue)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    public static implicit operator CSSNumberish(double value)
        => new(value);

    public static implicit operator CSSNumberish(CSSNumericValue value)
        => new(value);
}

/// <summary>
/// CSSPerspectiveValue
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSPerspectiveValue(CSSNumericValue, CSSKeywordish)
{

    public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    public CSSKeywordish? AsCSSKeywordish => Value is CSSKeywordish value ? value : default(CSSKeywordish?);

    public static implicit operator CSSPerspectiveValue(CSSNumericValue value)
        => new(value);

    public static implicit operator CSSPerspectiveValue(CSSKeywordish value)
        => new(value);
}

/// <summary>
/// CSSStringSource
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSStringSource(string, ReadableStream)
{

    public string? AsString => Value is string value ? value : default(string?);

    public ReadableStream? AsReadableStream => Value is ReadableStream value ? value : default(ReadableStream?);

    public static implicit operator CSSStringSource(string value)
        => new(value);

    public static implicit operator CSSStringSource(ReadableStream value)
        => new(value);
}

/// <summary>
/// CSSStyleSheetInitMedia
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSStyleSheetInitMedia(MediaList, string)
{

    public MediaList? AsMediaList => Value is MediaList value ? value : default(MediaList?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator CSSStyleSheetInitMedia(MediaList value)
        => new(value);

    public static implicit operator CSSStyleSheetInitMedia(string value)
        => new(value);
}

/// <summary>
/// CSSToken
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSToken(string, CSSStyleValue, CSSParserValue)
{

    public string? AsString => Value is string value ? value : default(string?);

    public CSSStyleValue? AsCSSStyleValue => Value is CSSStyleValue value ? value : default(CSSStyleValue?);

    public CSSParserValue? AsCSSParserValue => Value is CSSParserValue value ? value : default(CSSParserValue?);

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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSUnparsedSegment(string, CSSVariableReferenceValue)
{

    public string? AsString => Value is string value ? value : default(string?);

    public CSSVariableReferenceValue? AsCSSVariableReferenceValue => Value is CSSVariableReferenceValue value ? value : default(CSSVariableReferenceValue?);

    public static implicit operator CSSUnparsedSegment(string value)
        => new(value);

    public static implicit operator CSSUnparsedSegment(CSSVariableReferenceValue value)
        => new(value);
}

/// <summary>
/// SetValues
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetValues(CSSStyleValue, string)
{

    public CSSStyleValue? AsCSSStyleValue => Value is CSSStyleValue value ? value : default(CSSStyleValue?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator SetValues(CSSStyleValue value)
        => new(value);

    public static implicit operator SetValues(string value)
        => new(value);
}

/// <summary>
/// StructuralCache
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheCollectionBuilder), nameof(StructuralCacheCollectionBuilder.Create))]
public readonly union StructuralCache(AnimationEffect, AnimationEffect[]) : IEnumerable<AnimationEffect>
{

    public AnimationEffect? AsAnimationEffect => Value is AnimationEffect value ? value : default(AnimationEffect?);

    public AnimationEffect[]? AsAnimationEffectArray => Value is AnimationEffect[] value ? value : default(AnimationEffect[]?);

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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue(CSSStyleValue, string)
{

    public CSSStyleValue? AsCSSStyleValue => Value is CSSStyleValue value ? value : default(CSSStyleValue?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCacheValue(CSSStyleValue value)
        => new(value);

    public static implicit operator StructuralCacheValue(string value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue2
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue2(CSSColorValue, CSSStyleValue)
{

    public CSSColorValue? AsCSSColorValue => Value is CSSColorValue value ? value : default(CSSColorValue?);

    public CSSStyleValue? AsCSSStyleValue => Value is CSSStyleValue value ? value : default(CSSStyleValue?);

    public static implicit operator StructuralCacheValue2(CSSColorValue value)
        => new(value);

    public static implicit operator StructuralCacheValue2(CSSStyleValue value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue3
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue3(Element, ProcessingInstruction)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public ProcessingInstruction? AsProcessingInstruction => Value is ProcessingInstruction value ? value : default(ProcessingInstruction?);

    public static implicit operator StructuralCacheValue3(Element value)
        => new(value);

    public static implicit operator StructuralCacheValue3(ProcessingInstruction value)
        => new(value);
}

/// <summary>
/// StylePropertyMapAppendValues
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StylePropertyMapAppendValues(CSSStyleValue, string)
{

    public CSSStyleValue? AsCSSStyleValue => Value is CSSStyleValue value ? value : default(CSSStyleValue?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StylePropertyMapAppendValues(CSSStyleValue value)
        => new(value);

    public static implicit operator StylePropertyMapAppendValues(string value)
        => new(value);
}

/// <summary>
/// StylePropertyMapSetValues
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StylePropertyMapSetValues(CSSStyleValue, string)
{

    public CSSStyleValue? AsCSSStyleValue => Value is CSSStyleValue value ? value : default(CSSStyleValue?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StylePropertyMapSetValues(CSSStyleValue value)
        => new(value);

    public static implicit operator StylePropertyMapSetValues(string value)
        => new(value);
}

/// <summary>
/// StyleSheetOwnerNode
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StyleSheetOwnerNode(Element, ProcessingInstruction)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public ProcessingInstruction? AsProcessingInstruction => Value is ProcessingInstruction value ? value : default(ProcessingInstruction?);

    public static implicit operator StyleSheetOwnerNode(Element value)
        => new(value);

    public static implicit operator StyleSheetOwnerNode(ProcessingInstruction value)
        => new(value);
}

/// <summary>
/// WorkletAnimationEffects
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WorkletAnimationEffectsCollectionBuilder), nameof(WorkletAnimationEffectsCollectionBuilder.Create))]
public readonly union WorkletAnimationEffects(AnimationEffect, AnimationEffect[]) : IEnumerable<AnimationEffect>
{

    public AnimationEffect? AsAnimationEffect => Value is AnimationEffect value ? value : default(AnimationEffect?);

    public AnimationEffect[]? AsAnimationEffectArray => Value is AnimationEffect[] value ? value : default(AnimationEffect[]?);

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
