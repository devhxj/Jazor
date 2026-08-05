#nullable enable

namespace ECMAScript.ElementPlus;

/// <summary>
/// Export surface for Element Plus directives.
/// </summary>
[ECMAScript("element-plus")]
public static class ElDirectives
{
    [ECMAScriptName("ElInfiniteScroll")]
    public extern static ElDirective InfiniteScroll { get; }

    [ECMAScriptName("ElLoadingDirective")]
    public extern static VueDirective<ElDirectiveValue> Loading { get; }

}
