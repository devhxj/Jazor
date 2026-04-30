using ECMAScript;
using ECMAScript.Vuetify;
using static ECMAScript.Vue3;

namespace Wiki;

[ECMAScriptModule("main.mjs")]
public static class AppModule
{
    private static readonly VuetifyOptions VuetifyConfiguration = new()
    {
        Components = new VuetifyComponentRegistry
        {
            VBtn = VuetifyComponents.VBtn,
            VCard = VuetifyComponents.VCard,
            VTextField = VuetifyComponents.VTextField
        },
        Directives = new VuetifyDirectiveRegistry
        {
            Ripple = VuetifyDirectives.Ripple
        },
        Theme = new VuetifyThemeOptions
        {
            DefaultTheme = "light"
        },
        Display = new VuetifyDisplayOptions
        {
            MobileBreakpoint = "md"
        }
    };
    private static readonly bool Initialized = Initialize();

    private static bool Initialize()
    {
        Boot();
        return true;
    }

    public static void Boot()
    {
        var app = CreateApp(WikiHomeModule.Component);
        app.Use(Vuetify.CreateVuetify(VuetifyConfiguration));
        app.Mount("#app");
    }
}
