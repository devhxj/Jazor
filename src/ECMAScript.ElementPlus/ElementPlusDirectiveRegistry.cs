namespace ECMAScript.ElementPlus;

/// <summary>
/// Registry of Element Plus directives.
/// </summary>
[ECMAScript]
[Description("@#ElementPlusDirectiveRegistry")]
public sealed record ElementPlusDirectiveRegistry : VueDirectiveRegistry
{
    [Description("@#InfiniteScroll")]
    public ElementPlusDirective? InfiniteScroll { get; init; }

    [Description("@#Loading")]
    public VueDirective<ElementPlusDirectiveValue>? Loading { get; init; }

}
