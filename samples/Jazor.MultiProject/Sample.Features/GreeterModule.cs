using ECMAScript;
using Sample.Contracts;

namespace Sample.Features;

[ECMAScriptModule("features/greeter.mjs")]
public static class GreeterModule
{
    public static string Greet(string name)
    {
        return GreetingSharedModule.Compose(name);
    }
}
