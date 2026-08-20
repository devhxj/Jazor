using System.ComponentModel;
using ECMAScript;
using static ECMAScript.Vue;

namespace JazorAdmin.DemoClient;

[ECMAScript("components/portal-page.mjs")]
[Description("@#")]
internal static class PortalPageModule
{
#pragma warning disable CS0626
    [ECMAScriptName("default")]
    public extern static IVueComponent Default { get; }
#pragma warning restore CS0626
}

[ECMAScriptModule("app.mjs")]
public static class Bootstrap
{
    private static readonly bool started = Start();

    private static bool Start()
    {
        DemoStyles.EnsureLoaded();
        CreateApp(PortalPageModule.Default).Mount("#app");
        return true;
    }
}
