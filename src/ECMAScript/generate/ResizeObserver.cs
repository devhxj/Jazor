namespace ECMAScript;

/// <summary>
/// ResizeObserverOptions
/// </summary>
[ECMAScript]
[Description("@#ResizeObserverOptions")]
public record ResizeObserverOptions(
    [property: Description("@#box")]ResizeObserverBoxOptions Box = ResizeObserverBoxOptions.ContentBox);

/// <summary>
/// ResizeObserver
/// </summary>
[ECMAScript]
[Description("@#ResizeObserver")]
public class ResizeObserver
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="callback">callback</param>
    public extern ResizeObserver(ResizeObserverCallback callback);

    /// <summary>
    /// observe
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="options">options</param>
    [Description("@#observe")]
    public extern void Observe(Element target, ResizeObserverOptions? options = default);

    /// <summary>
    /// unobserve
    /// </summary>
    /// <param name="target">target</param>
    [Description("@#unobserve")]
    public extern void Unobserve(Element target);

    /// <summary>
    /// disconnect
    /// </summary>
    [Description("@#disconnect")]
    public extern void Disconnect();
}

/// <summary>
/// ResizeObserverEntry
/// </summary>
[ECMAScript]
[Description("@#ResizeObserverEntry")]
public class ResizeObserverEntry
{
    /// <summary>
    /// target
    /// </summary>
    [Description("@#target")]
    public extern Element Target { get; }

    /// <summary>
    /// contentRect
    /// </summary>
    [Description("@#contentRect")]
    public extern DOMRectReadOnly ContentRect { get; }

    /// <summary>
    /// borderBoxSize
    /// </summary>
    [Description("@#borderBoxSize")]
    public extern FrozenSet<ResizeObserverSize> BorderBoxSize { get; }

    /// <summary>
    /// contentBoxSize
    /// </summary>
    [Description("@#contentBoxSize")]
    public extern FrozenSet<ResizeObserverSize> ContentBoxSize { get; }

    /// <summary>
    /// devicePixelContentBoxSize
    /// </summary>
    [Description("@#devicePixelContentBoxSize")]
    public extern FrozenSet<ResizeObserverSize> DevicePixelContentBoxSize { get; }
}

/// <summary>
/// ResizeObserverSize
/// </summary>
[ECMAScript]
[Description("@#ResizeObserverSize")]
public class ResizeObserverSize
{
    /// <summary>
    /// inlineSize
    /// </summary>
    [Description("@#inlineSize")]
    public extern double InlineSize { get; }

    /// <summary>
    /// blockSize
    /// </summary>
    [Description("@#blockSize")]
    public extern double BlockSize { get; }
}