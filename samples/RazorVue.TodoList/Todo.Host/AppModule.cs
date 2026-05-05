using ECMAScript;

namespace Todo.Host;

[ECMAScriptModule("host/app.mjs")]
public static class AppModule
{
    public static string Boot() => "RazorVue Todo host ready";
}
