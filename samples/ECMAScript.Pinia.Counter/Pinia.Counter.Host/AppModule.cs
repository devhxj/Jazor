using ECMAScript;
using static ECMAScript.Pinia;
using static ECMAScript.Vue3;

namespace Pinia.Counter.Host;

[ECMAScriptModule("host/app.mjs")]
public static class AppModule
{
    public static void Boot(string selector)
    {
        var app = CreateApp(CounterAppModule.Component);
        var pinia = CreatePinia();

        app.Use(pinia);
        app.Mount(selector);
    }
}
