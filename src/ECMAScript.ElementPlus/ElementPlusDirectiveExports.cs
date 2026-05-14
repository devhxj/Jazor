#nullable enable

namespace ECMAScript.ElementPlus;

/// <summary>
/// Export surface for Element Plus directives.
/// </summary>
[ECMAScript("element-plus")]
public static class ElementPlusDirectives
{
    [ECMAScriptName("ElInfiniteScroll")]
    public extern static ElementPlusDirective InfiniteScroll { get; }

    [ECMAScriptName("ElLoadingDirective")]
    public extern static VueDirective<ElementPlusDirectiveValue> Loading { get; }

}
