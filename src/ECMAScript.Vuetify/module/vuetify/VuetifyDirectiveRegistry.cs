namespace ECMAScript.Vue.Vuetify;

[ECMAScriptModule]
[Description("@#VuetifyDirectiveRegistry")]
public sealed record VuetifyDirectiveRegistry : VueDirectiveRegistry
{
    [Description("@#ClickOutside")]
    public ClickOutsideDirective? ClickOutside { get; init; }

    [Description("@#Intersect")]
    public IntersectDirective? Intersect { get; init; }

    [Description("@#Mutate")]
    public MutateDirective? Mutate { get; init; }

    [Description("@#Resize")]
    public ResizeDirective? Resize { get; init; }

    [Description("@#Ripple")]
    public RippleDirective? Ripple { get; init; }

    [Description("@#Scroll")]
    public ScrollDirective? Scroll { get; init; }

    [Description("@#Tooltip")]
    public TooltipDirective? Tooltip { get; init; }

    [Description("@#Touch")]
    public TouchDirective? Touch { get; init; }
}

