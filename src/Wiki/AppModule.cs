using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

[ECMAScriptModule("main.mjs")]
public static class AppModule
{
    private static readonly bool Initialized = Initialize();

    private static bool Initialize()
    {
        Boot();
        return true;
    }

    public static void Boot()
    {
        var app = CreateApp(WikiHomeModule.Component);
        app.Mount("#app");
    }
}
