namespace ECMAScript.Vuetify;

[ECMAScriptModule("vuetify/directives")]
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

[ECMAScript]
public abstract class VuetifyDirective : VueDirective
{
    protected VuetifyDirective()
    {
    }
}
