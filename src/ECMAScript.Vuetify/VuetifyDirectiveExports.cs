namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 指令的导出入口。
/// Export surface for Vuetify directives.
/// </summary>
[ECMAScript("vuetify/directives")]
public static class VuetifyDirectives
{
    [ECMAScriptName("ClickOutside")]
    public extern static VuetifyDirective ClickOutside { get; }

    [ECMAScriptName("Intersect")]
    public extern static VuetifyDirective Intersect { get; }

    [ECMAScriptName("Mutate")]
    public extern static VuetifyDirective Mutate { get; }

    [ECMAScriptName("Resize")]
    public extern static VuetifyDirective Resize { get; }

    [ECMAScriptName("Ripple")]
    public extern static VuetifyDirective Ripple { get; }

    [ECMAScriptName("Scroll")]
    public extern static VuetifyDirective Scroll { get; }

    [ECMAScriptName("Touch")]
    public extern static VuetifyDirective Touch { get; }

    [ECMAScriptName("Tooltip")]
    public extern static VuetifyDirective Tooltip { get; }
}

/// <summary>
/// Vuetify 指令的基类型。
/// Base type for Vuetify directives.
/// </summary>
[ECMAScript]
public abstract record VuetifyDirective : VueDirective
{
}
