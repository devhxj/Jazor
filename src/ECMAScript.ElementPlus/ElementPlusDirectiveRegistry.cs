#nullable enable

namespace ECMAScript.ElementPlus;

/// <summary>
/// Registry of Element Plus directives.
/// </summary>
[ECMAScript]
[Description("@#ElDirectiveRegistry")]
public sealed record ElDirectiveRegistry : VueDirectiveRegistry
{
    [Description("@#InfiniteScroll")]
    public ElDirective? InfiniteScroll { get; init; }

    [Description("@#Loading")]
    public VueDirective<ElDirectiveValue>? Loading { get; init; }

}
