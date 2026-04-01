using ECMAScript;
using Sample.Features;

namespace Sample.Host;

[ECMAScriptModule("host/app.mjs")]
public static class AppModule
{
    public static string Boot()
    {
        return GreeterModule.Greet("Jazor");
    }
}
