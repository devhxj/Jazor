using ECMAScript;
using Microsoft.AspNetCore.Components;

namespace BindingsShowcase.Host;

[ECMAScriptModule("./components/showcase-landing")]
public partial class ShowcaseLanding : ComponentBase, IVueComponent
{
    // The ISO date is a fixed input so the browser smoke can assert one deterministic value.
    private static readonly Date SampleDate = DateFns.ParseISO("2026-09-17T08:30:00.000Z");

    private static readonly VueI18nComposer composer = VueI18n.CreateI18n(new VueI18nCreateOptions
    {
        Locale = "en-US",
        FallbackLocale = "en-US",
        Legacy = false,
        Messages = new VueDictionary
        {
            ["en-US"] = new VueDictionary { ["greeting"] = "Hello from vue-i18n" },
            ["zh-CN"] = new VueDictionary { ["greeting"] = "来自 vue-i18n 的问候" }
        }
    }).Global;

    private static readonly VueComputedRef<bool> wideViewport = VueUse.UseMediaQuery("(min-width: 1px)");

    private static readonly VueComputedRef<bool> prefersDark = VueUse.UsePreferredDark();

    private static readonly FloatingMiddleware offsetMiddleware = FloatingUi.Offset(8);

    private string IsoDate => DateFns.FormatISO(SampleDate);

    private string FormattedDate => DateFns.Format(SampleDate, "yyyy-MM-dd");

    private string CurrentLocale => composer.Locale.Value;

    private string Greeting => composer.T("greeting");

    private bool MediaQueryValue => wideViewport.Value;

    private bool PrefersDarkValue => prefersDark.Value;

    private bool FloatingLoaded => offsetMiddleware is not null;

    private bool VeeValidateLoaded => VeeValidateLive;

    private bool VueQueryLoaded => VueQueryProbe is not null;

    private static bool VeeValidateLive
    {
        get
        {
            VeeValidate.Configure(new VeeValidateConfig());
            return true;
        }
    }

    private static VuePlugin? VueQueryProbe => VueQuery.VueQueryPlugin;

    private void SwitchLocale()
        => composer.Locale.Value = CurrentLocale == "en-US" ? "zh-CN" : "en-US";
}
