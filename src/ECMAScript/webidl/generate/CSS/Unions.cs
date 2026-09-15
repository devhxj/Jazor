namespace ECMAScript.CSS;

/// <summary>WebIDL 联合值：AnimationEffect、AnimationEffect[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheCollectionBuilder), nameof(StructuralCacheCollectionBuilder.Create))]
public readonly union StructuralCache(AnimationEffect, AnimationEffect[]) : IEnumerable<AnimationEffect>
{

    /// <summary>读取 AnimationEffect 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AnimationEffect? AsAnimationEffect => Value is AnimationEffect value ? value : default(AnimationEffect?);

    /// <summary>读取 AnimationEffect[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AnimationEffect[]? AsAnimationEffectArray => Value is AnimationEffect[] value ? value : default(AnimationEffect[]?);

    /// <summary>将 AnimationEffect 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCache(AnimationEffect value)
        => new(value);

    /// <summary>将 AnimationEffect[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCache(AnimationEffect[] value)
        => new(value);

    IEnumerator<AnimationEffect> IEnumerable<AnimationEffect>.GetEnumerator()
        => ((IEnumerable<AnimationEffect>)(AsAnimationEffectArray ?? Array.Empty<AnimationEffect>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<AnimationEffect>)this).GetEnumerator();
}

/// <summary>为 StructuralCache 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static StructuralCache Create(ReadOnlySpan<AnimationEffect> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：AnimationEffect、AnimationEffect[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WorkletAnimationEffectsCollectionBuilder), nameof(WorkletAnimationEffectsCollectionBuilder.Create))]
public readonly union WorkletAnimationEffects(AnimationEffect, AnimationEffect[]) : IEnumerable<AnimationEffect>
{

    /// <summary>读取 AnimationEffect 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AnimationEffect? AsAnimationEffect => Value is AnimationEffect value ? value : default(AnimationEffect?);

    /// <summary>读取 AnimationEffect[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AnimationEffect[]? AsAnimationEffectArray => Value is AnimationEffect[] value ? value : default(AnimationEffect[]?);

    /// <summary>将 AnimationEffect 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WorkletAnimationEffects(AnimationEffect value)
        => new(value);

    /// <summary>将 AnimationEffect[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WorkletAnimationEffects(AnimationEffect[] value)
        => new(value);

    IEnumerator<AnimationEffect> IEnumerable<AnimationEffect>.GetEnumerator()
        => ((IEnumerable<AnimationEffect>)(AsAnimationEffectArray ?? Array.Empty<AnimationEffect>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<AnimationEffect>)this).GetEnumerator();
}

/// <summary>为 WorkletAnimationEffects 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WorkletAnimationEffectsCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WorkletAnimationEffects Create(ReadOnlySpan<AnimationEffect> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：CSSColorValue、CSSStyleValue。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct CSSColorValueParseResult : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly CSSColorValue? _value1;
    private readonly CSSStyleValue? _value2;

    /// <summary>将 CSSColorValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
public CSSColorValueParseResult(CSSColorValue value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    /// <summary>将 CSSStyleValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
public CSSColorValueParseResult(CSSStyleValue value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    /// <summary>读取 CSSColorValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSColorValue? AsCSSColorValue => _kind == 1 ? _value1 : default;

    /// <summary>读取 CSSStyleValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSStyleValue? AsCSSStyleValue => _kind == 2 ? _value2 : default;

    /// <summary>读取当前分支保存的原始值；未初始化的联合值返回 null。此属性不进行分支转换。</summary>
public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    /// <summary>将 CSSColorValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSColorValueParseResult(CSSColorValue value)
        => new(value);

    /// <summary>将 CSSStyleValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSColorValueParseResult(CSSStyleValue value)
        => new(value);
}

/// <summary>WebIDL 联合值：CSSColorValue、CSSStyleValue。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct StructuralCacheValue2 : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly CSSColorValue? _value1;
    private readonly CSSStyleValue? _value2;

    /// <summary>将 CSSColorValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
public StructuralCacheValue2(CSSColorValue value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    /// <summary>将 CSSStyleValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
public StructuralCacheValue2(CSSStyleValue value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    /// <summary>读取 CSSColorValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSColorValue? AsCSSColorValue => _kind == 1 ? _value1 : default;

    /// <summary>读取 CSSStyleValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSStyleValue? AsCSSStyleValue => _kind == 2 ? _value2 : default;

    /// <summary>读取当前分支保存的原始值；未初始化的联合值返回 null。此属性不进行分支转换。</summary>
public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    /// <summary>将 CSSColorValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue2(CSSColorValue value)
        => new(value);

    /// <summary>将 CSSStyleValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue2(CSSStyleValue value)
        => new(value);
}

/// <summary>WebIDL 联合值：CSSStyleValue、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AppendValues(CSSStyleValue, string)
{

    /// <summary>读取 CSSStyleValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSStyleValue? AsCSSStyleValue => Value is CSSStyleValue value ? value : default(CSSStyleValue?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 CSSStyleValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AppendValues(CSSStyleValue value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AppendValues(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：CSSNumberish、CSSKeywordish。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSColorAngle(CSSNumberish, CSSKeywordish)
{

    /// <summary>读取 CSSNumberish 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSNumberish? AsCSSNumberish => Value is CSSNumberish value ? value : default(CSSNumberish?);

    /// <summary>读取 CSSKeywordish 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSKeywordish? AsCSSKeywordish => Value is CSSKeywordish value ? value : default(CSSKeywordish?);

    /// <summary>将 CSSNumberish 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSColorAngle(CSSNumberish value)
        => new(value);

    /// <summary>将 CSSKeywordish 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSColorAngle(CSSKeywordish value)
        => new(value);
}

/// <summary>WebIDL 联合值：CSSNumberish、CSSKeywordish。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSColorNumber(CSSNumberish, CSSKeywordish)
{

    /// <summary>读取 CSSNumberish 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSNumberish? AsCSSNumberish => Value is CSSNumberish value ? value : default(CSSNumberish?);

    /// <summary>读取 CSSKeywordish 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSKeywordish? AsCSSKeywordish => Value is CSSKeywordish value ? value : default(CSSKeywordish?);

    /// <summary>将 CSSNumberish 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSColorNumber(CSSNumberish value)
        => new(value);

    /// <summary>将 CSSKeywordish 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSColorNumber(CSSKeywordish value)
        => new(value);
}

/// <summary>WebIDL 联合值：CSSNumberish、CSSKeywordish。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSColorPercent(CSSNumberish, CSSKeywordish)
{

    /// <summary>读取 CSSNumberish 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSNumberish? AsCSSNumberish => Value is CSSNumberish value ? value : default(CSSNumberish?);

    /// <summary>读取 CSSKeywordish 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSKeywordish? AsCSSKeywordish => Value is CSSKeywordish value ? value : default(CSSKeywordish?);

    /// <summary>将 CSSNumberish 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSColorPercent(CSSNumberish value)
        => new(value);

    /// <summary>将 CSSKeywordish 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSColorPercent(CSSKeywordish value)
        => new(value);
}

/// <summary>WebIDL 联合值：CSSNumberish、CSSKeywordish。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSColorRGBComp(CSSNumberish, CSSKeywordish)
{

    /// <summary>读取 CSSNumberish 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSNumberish? AsCSSNumberish => Value is CSSNumberish value ? value : default(CSSNumberish?);

    /// <summary>读取 CSSKeywordish 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSKeywordish? AsCSSKeywordish => Value is CSSKeywordish value ? value : default(CSSKeywordish?);

    /// <summary>将 CSSNumberish 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSColorRGBComp(CSSNumberish value)
        => new(value);

    /// <summary>将 CSSKeywordish 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSColorRGBComp(CSSKeywordish value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、CSSKeywordValue。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSKeywordish(string, CSSKeywordValue)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 CSSKeywordValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSKeywordish(string value)
        => new(value);

    /// <summary>将 CSSKeywordValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSKeywordish(CSSKeywordValue value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、CSSNumericValue。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSNumberish(double, CSSNumericValue)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 CSSNumericValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSNumberish(double value)
        => new(value);

    /// <summary>将 CSSNumericValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSNumberish(CSSNumericValue value)
        => new(value);
}

/// <summary>WebIDL 联合值：CSSNumericValue、CSSKeywordish。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSPerspectiveValue(CSSNumericValue, CSSKeywordish)
{

    /// <summary>读取 CSSNumericValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    /// <summary>读取 CSSKeywordish 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSKeywordish? AsCSSKeywordish => Value is CSSKeywordish value ? value : default(CSSKeywordish?);

    /// <summary>将 CSSNumericValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSPerspectiveValue(CSSNumericValue value)
        => new(value);

    /// <summary>将 CSSKeywordish 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSPerspectiveValue(CSSKeywordish value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、ReadableStream。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSStringSource(string, ReadableStream)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 ReadableStream 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ReadableStream? AsReadableStream => Value is ReadableStream value ? value : default(ReadableStream?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSStringSource(string value)
        => new(value);

    /// <summary>将 ReadableStream 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSStringSource(ReadableStream value)
        => new(value);
}

/// <summary>WebIDL 联合值：MediaList、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSStyleSheetInitMedia(MediaList, string)
{

    /// <summary>读取 MediaList 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaList? AsMediaList => Value is MediaList value ? value : default(MediaList?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 MediaList 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSStyleSheetInitMedia(MediaList value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSStyleSheetInitMedia(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、CSSStyleValue、CSSParserValue。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSToken(string, CSSStyleValue, CSSParserValue)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 CSSStyleValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSStyleValue? AsCSSStyleValue => Value is CSSStyleValue value ? value : default(CSSStyleValue?);

    /// <summary>读取 CSSParserValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSParserValue? AsCSSParserValue => Value is CSSParserValue value ? value : default(CSSParserValue?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSToken(string value)
        => new(value);

    /// <summary>将 CSSStyleValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSToken(CSSStyleValue value)
        => new(value);

    /// <summary>将 CSSParserValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSToken(CSSParserValue value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、CSSVariableReferenceValue。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSUnparsedSegment(string, CSSVariableReferenceValue)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 CSSVariableReferenceValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSVariableReferenceValue? AsCSSVariableReferenceValue => Value is CSSVariableReferenceValue value ? value : default(CSSVariableReferenceValue?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSUnparsedSegment(string value)
        => new(value);

    /// <summary>将 CSSVariableReferenceValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSUnparsedSegment(CSSVariableReferenceValue value)
        => new(value);
}

/// <summary>WebIDL 联合值：CSSStyleValue、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetValues(CSSStyleValue, string)
{

    /// <summary>读取 CSSStyleValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSStyleValue? AsCSSStyleValue => Value is CSSStyleValue value ? value : default(CSSStyleValue?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 CSSStyleValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetValues(CSSStyleValue value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetValues(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：CSSStyleValue、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue(CSSStyleValue, string)
{

    /// <summary>读取 CSSStyleValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSStyleValue? AsCSSStyleValue => Value is CSSStyleValue value ? value : default(CSSStyleValue?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 CSSStyleValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue(CSSStyleValue value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Element、ProcessingInstruction。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue3(Element, ProcessingInstruction)
{

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>读取 ProcessingInstruction 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ProcessingInstruction? AsProcessingInstruction => Value is ProcessingInstruction value ? value : default(ProcessingInstruction?);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue3(Element value)
        => new(value);

    /// <summary>将 ProcessingInstruction 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue3(ProcessingInstruction value)
        => new(value);
}

/// <summary>WebIDL 联合值：CSSStyleValue、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StylePropertyMapAppendValues(CSSStyleValue, string)
{

    /// <summary>读取 CSSStyleValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSStyleValue? AsCSSStyleValue => Value is CSSStyleValue value ? value : default(CSSStyleValue?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 CSSStyleValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StylePropertyMapAppendValues(CSSStyleValue value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StylePropertyMapAppendValues(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：CSSStyleValue、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StylePropertyMapSetValues(CSSStyleValue, string)
{

    /// <summary>读取 CSSStyleValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSStyleValue? AsCSSStyleValue => Value is CSSStyleValue value ? value : default(CSSStyleValue?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 CSSStyleValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StylePropertyMapSetValues(CSSStyleValue value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StylePropertyMapSetValues(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Element、ProcessingInstruction。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StyleSheetOwnerNode(Element, ProcessingInstruction)
{

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>读取 ProcessingInstruction 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ProcessingInstruction? AsProcessingInstruction => Value is ProcessingInstruction value ? value : default(ProcessingInstruction?);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StyleSheetOwnerNode(Element value)
        => new(value);

    /// <summary>将 ProcessingInstruction 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StyleSheetOwnerNode(ProcessingInstruction value)
        => new(value);
}