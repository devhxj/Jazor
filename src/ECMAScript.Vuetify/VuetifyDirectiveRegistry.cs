namespace ECMAScript.Vuetify;

[ECMAScriptModule]
[Description("@#VuetifyDirectiveRegistry")]
public sealed record VuetifyDirectiveRegistry : VueDirectiveRegistry
{
    [Description("@#ClickOutside")]
    public VuetifyDirective? ClickOutside { get; init; }

    [Description("@#Intersect")]
    public VuetifyDirective? Intersect { get; init; }

    [Description("@#Mutate")]
    public VuetifyDirective? Mutate { get; init; }

    [Description("@#Resize")]
    public VuetifyDirective? Resize { get; init; }

    [Description("@#Ripple")]
    public VuetifyDirective? Ripple { get; init; }

    [Description("@#Scroll")]
    public VuetifyDirective? Scroll { get; init; }

    [Description("@#Tooltip")]
    public VuetifyDirective? Tooltip { get; init; }

    [Description("@#Touch")]
    public VuetifyDirective? Touch { get; init; }
}

