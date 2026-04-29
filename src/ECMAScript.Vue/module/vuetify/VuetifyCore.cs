namespace ECMAScript.Vue.Vuetify;

[ECMAScriptModule("vuetify")]
public static class Vuetify
{
    [ECMAScriptName("createVuetify")]
    public extern static VuetifyPlugin CreateVuetify();

    [ECMAScriptName("createVuetify")]
    public extern static VuetifyPlugin CreateVuetify(VuetifyOptions options);
}

[ECMAScriptModule("vuetify")]
public sealed class VuetifyPlugin : VuePlugin
{
    private VuetifyPlugin()
    {
    }
}

[ECMAScriptModule]
[Description("@#VuetifyOptions")]
public sealed record VuetifyOptions : VuePluginOptions
{
    [Description("@#components")]
    public VuetifyComponentRegistry? Components { get; init; }

    [Description("@#directives")]
    public VuetifyDirectiveRegistry? Directives { get; init; }

    [Description("@#display")]
    public VuetifyDisplayOptions? Display { get; init; }

    [Description("@#theme")]
    public VuetifyThemeOptions? Theme { get; init; }

    [Description("@#icons")]
    public VuetifyIconOptions? Icons { get; init; }

    [Description("@#locale")]
    public VuetifyLocaleOptions? Locale { get; init; }

    [Description("@#date")]
    public VuetifyDateOptions? Date { get; init; }

    [Description("@#ssr")]
    public bool? Ssr { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyThemeOptions")]
public sealed record VuetifyThemeOptions
{
    [Description("@#defaultTheme")]
    public string? DefaultTheme { get; init; }

    [Description("@#variations")]
    public VuetifyThemeVariationOptions? Variations { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyThemeVariationOptions")]
public sealed record VuetifyThemeVariationOptions
{
    [Description("@#colors")]
    public string[]? Colors { get; init; }

    [Description("@#lighten")]
    public int? Lighten { get; init; }

    [Description("@#darken")]
    public int? Darken { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyDisplayOptions")]
public sealed record VuetifyDisplayOptions
{
    [Description("@#mobileBreakpoint")]
    public Either<string, Number>? MobileBreakpoint { get; init; }

    [Description("@#thresholds")]
    public VuetifyDisplayThresholds? Thresholds { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyDisplayThresholds")]
public sealed record VuetifyDisplayThresholds
{
    [Description("@#xs")]
    public int? Xs { get; init; }

    [Description("@#sm")]
    public int? Sm { get; init; }

    [Description("@#md")]
    public int? Md { get; init; }

    [Description("@#lg")]
    public int? Lg { get; init; }

    [Description("@#xl")]
    public int? Xl { get; init; }

    [Description("@#xxl")]
    public int? Xxl { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyIconOptions")]
public sealed record VuetifyIconOptions
{
    [Description("@#defaultSet")]
    public string? DefaultSet { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyLocaleOptions")]
public sealed record VuetifyLocaleOptions
{
    [Description("@#locale")]
    public string? Locale { get; init; }

    [Description("@#fallback")]
    public string? Fallback { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyDateOptions")]
public sealed record VuetifyDateOptions
{
    [Description("@#locale")]
    public string? Locale { get; init; }
}

[ECMAScriptModule("vuetify/components")]
public abstract class VuetifyComponent : VueComponent
{
    protected VuetifyComponent()
    {
    }
}

[ECMAScriptModule("vuetify/directives")]
public abstract class VuetifyDirective : VueDirective
{
    protected VuetifyDirective()
    {
    }
}

