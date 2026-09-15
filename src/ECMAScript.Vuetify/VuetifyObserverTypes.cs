using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify IntersectionObserver 配置选项。
/// Vuetify IntersectionObserver configuration options.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyIntersectionObserverOptions : VueProps
{
    /// <summary>
    /// IntersectionObserver 的可见性比较根元素；null 使用浏览器视口。
    /// </summary>
    [Description("@#root")]
    public VuetifyIntersectionObserverRoot? Root { get; init; }

    /// <summary>
    /// 扩展或缩小根元素比较区域的 CSS 边距字符串。
    /// </summary>
    [Description("@#rootMargin")]
    public string? RootMargin { get; init; }

    /// <summary>
    /// 触发交叉观察回调的可见面积比例，可以是单个比例或有序比例数组。
    /// </summary>
    [Description("@#threshold")]
    public VuetifyIntersectionObserverThreshold? Threshold { get; init; }
}

/// <summary>
/// C# 联合参数，允许 Element, DocumentRef。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyIntersectionObserverRoot(Element, DocumentRef)
{
    /// <summary>
    /// 读取当前值的 Element 分支；不属于该分支时返回 null。
    /// </summary>
    public Element? AsElement => Value as Element;

    /// <summary>
    /// 读取当前值的 DocumentRef 分支；不属于该分支时返回 null。
    /// </summary>
    public DocumentRef? AsDocument => Value as DocumentRef;

    /// <summary>
    /// 将 Element 值转换为 VuetifyIntersectionObserverRoot，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIntersectionObserverRoot(Element value)
        => new(value);

    /// <summary>
    /// 将 DocumentRef 值转换为 VuetifyIntersectionObserverRoot，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIntersectionObserverRoot(DocumentRef value)
        => new(value);
}

/// <summary>
/// C# 联合参数，允许 Number, Number[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyIntersectionObserverThresholdCollectionBuilder), nameof(VuetifyIntersectionObserverThresholdCollectionBuilder.Create))]
public readonly union VuetifyIntersectionObserverThreshold(Number, Number[]) : IEnumerable<Number>
{
    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 Number[] 分支；不属于该分支时返回 null。
    /// </summary>
    public Number[]? AsNumbers => Value as Number[];

    /// <summary>
    /// 将 Number 值转换为 VuetifyIntersectionObserverThreshold，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIntersectionObserverThreshold(Number value)
        => new(value);

    /// <summary>
    /// 将 Number[] 值转换为 VuetifyIntersectionObserverThreshold，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIntersectionObserverThreshold(Number[] value)
        => new(value);

    /// <summary>
    /// 将 double 值转换为 VuetifyIntersectionObserverThreshold，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIntersectionObserverThreshold(double value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyIntersectionObserverThreshold，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIntersectionObserverThreshold(int value)
        => new((Number)value);

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIntersectionObserverThreshold(double[] value)
        => new(Array.ConvertAll(value, static item => (Number)item));

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIntersectionObserverThreshold(int[] value)
        => new(Array.ConvertAll(value, static item => (Number)item));

    IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
        => ((IEnumerable<Number>)(AsNumbers ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Number>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyIntersectionObserverThresholdCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    public static VuetifyIntersectionObserverThreshold Create(ReadOnlySpan<Number> values)
        => values.ToArray();
}
