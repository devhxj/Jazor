using ECMAScript;

namespace Sample.Contracts;

[ECMAScriptModule("shared/greetings.mjs")]
public static class GreetingSharedModule
{
    public static string Prefix()
    {
        return "Hello";
    }

    public static string Compose(string name)
    {
        return $"{Prefix()}, {name}";
    }
}
