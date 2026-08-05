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
    [Description("@#root")]
    public VuetifyIntersectionObserverRoot? Root { get; init; }

    [Description("@#rootMargin")]
    public string? RootMargin { get; init; }

    [Description("@#threshold")]
    public VuetifyIntersectionObserverThreshold? Threshold { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifyIntersectionObserverRoot(Element, Document)
{
    public Element? AsElement => Value as Element;

    public Document? AsDocument => Value as Document;

    public static implicit operator VuetifyIntersectionObserverRoot(Element value)
        => new(value);

    public static implicit operator VuetifyIntersectionObserverRoot(Document value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyIntersectionObserverThresholdCollectionBuilder), nameof(VuetifyIntersectionObserverThresholdCollectionBuilder.Create))]
public readonly union VuetifyIntersectionObserverThreshold(Number, Number[]) : IEnumerable<Number>
{
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public Number[]? AsNumbers => Value as Number[];

    public static implicit operator VuetifyIntersectionObserverThreshold(Number value)
        => new(value);

    public static implicit operator VuetifyIntersectionObserverThreshold(Number[] value)
        => new(value);

    public static implicit operator VuetifyIntersectionObserverThreshold(double value)
        => new((Number)value);

    public static implicit operator VuetifyIntersectionObserverThreshold(int value)
        => new((Number)value);

    public static implicit operator VuetifyIntersectionObserverThreshold(double[] value)
        => new(Array.ConvertAll(value, static item => (Number)item));

    public static implicit operator VuetifyIntersectionObserverThreshold(int[] value)
        => new(Array.ConvertAll(value, static item => (Number)item));

    IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
        => ((IEnumerable<Number>)(AsNumbers ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Number>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyIntersectionObserverThresholdCollectionBuilder
{
    public static VuetifyIntersectionObserverThreshold Create(ReadOnlySpan<Number> values)
        => values.ToArray();
}
