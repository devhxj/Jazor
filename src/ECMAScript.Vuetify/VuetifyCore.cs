namespace ECMAScript.Vuetify;

[ECMAScript("npm:vuetify")]
[Description("@#")]
public static class Vuetify
{
    [Description("@#createVuetify")]
    public extern static VuetifyPlugin CreateVuetify();

    [Description("@#createVuetify")]
    public extern static VuetifyPlugin CreateVuetify(VuetifyOptions options);
}

[ECMAScript]
public interface IVuetifyComponent : IVueComponent { }

[ECMAScript]
public sealed record VuetifyPlugin : VuePlugin
{
    private VuetifyPlugin()
    {
    }
}

[ECMAScript]
[Description("@#VuetifyOptions")]
public sealed record VuetifyOptions : VuePluginOptions
{
    [Description("@#components")]
    public VueComponentRegistry? Components { get; init; }

    [Description("@#directives")]
    public VueDirectiveRegistry? Directives { get; init; }

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

[ECMAScript]
[Description("@#VuetifyThemeOptions")]
public sealed record VuetifyThemeOptions
{
    [Description("@#defaultTheme")]
    public string? DefaultTheme { get; init; }

    [Description("@#variations")]
    public VuetifyThemeVariationOptions? Variations { get; init; }
}

[ECMAScript]
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

[ECMAScript]
[Description("@#VuetifyDisplayOptions")]
public sealed record VuetifyDisplayOptions
{
    [Description("@#mobileBreakpoint")]
    public VuetifyDisplayBreakpoint? MobileBreakpoint { get; init; }

    [Description("@#thresholds")]
    public VuetifyDisplayThresholds? Thresholds { get; init; }
}

[ECMAScript]
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

[ECMAScript]
[Description("@#VuetifyIconOptions")]
public sealed record VuetifyIconOptions
{
    [Description("@#defaultSet")]
    public string? DefaultSet { get; init; }
}

[ECMAScript]
[Description("@#VuetifyLocaleOptions")]
public sealed record VuetifyLocaleOptions
{
    [Description("@#locale")]
    public string? Locale { get; init; }

    [Description("@#fallback")]
    public string? Fallback { get; init; }
}

[ECMAScript]
[Description("@#VuetifyDateOptions")]
public sealed record VuetifyDateOptions
{
    [Description("@#locale")]
    public string? Locale { get; init; }
}
