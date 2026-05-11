using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyIntersectionObserverRoot : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly Element? _element;
    private readonly Document? _document;

    private VuetifyIntersectionObserverRoot(Element value)
    {
        _kind = 1;
        _element = value;
        _document = default;
    }

    private VuetifyIntersectionObserverRoot(Document value)
    {
        _kind = 2;
        _element = default;
        _document = value;
    }

    public Element? AsElement => _kind == 1 ? _element : default;

    public Document? AsDocument => _kind == 2 ? _document : default;

    public object? Value => _kind switch
    {
        1 => AsElement,
        2 => AsDocument,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyIntersectionObserverRoot From(Element value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyIntersectionObserverRoot From(Document value);

    public static implicit operator VuetifyIntersectionObserverRoot(Element value)
        => new(value);

    public static implicit operator VuetifyIntersectionObserverRoot(Document value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyIntersectionObserverThresholdCollectionBuilder), nameof(VuetifyIntersectionObserverThresholdCollectionBuilder.Create))]
public readonly struct VuetifyIntersectionObserverThreshold : System.Runtime.CompilerServices.IUnion, IEnumerable<Number>
{
    private readonly byte _kind;
    private readonly Number? _number;
    private readonly Number[]? _numbers;

    private VuetifyIntersectionObserverThreshold(Number value)
    {
        _kind = 1;
        _number = value;
        _numbers = default;
    }

    private VuetifyIntersectionObserverThreshold(Number[] value)
    {
        _kind = 2;
        _number = default;
        _numbers = value;
    }

    public Number? AsNumber => _kind == 1 ? _number : default;

    public Number[]? AsNumbers => _kind == 2 ? _numbers : default;

    public object? Value => _kind switch
    {
        1 => AsNumber,
        2 => AsNumbers,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyIntersectionObserverThreshold From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyIntersectionObserverThreshold From(Number[] value);

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
        => ((IEnumerable<Number>)(_numbers ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Number>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyIntersectionObserverThresholdCollectionBuilder
{
    public static VuetifyIntersectionObserverThreshold Create(ReadOnlySpan<Number> values)
        => values.ToArray();
}
