using ECMAScript;
using static ECMAScript.Vue;

namespace BindingsShowcase.Host;

/// <summary>Vue framing owned by the showcase host; page behavior stays ordinary Razor.</summary>
[ECMAScriptModule("app.mjs")]
public static class Bootstrap
{
    private static readonly IVueComponent Root = DefineComponent(new VueComponentOptions
    {
        Name = "BindingsShowcaseRoot",
        Render = RenderRoot
    });

    private static readonly bool Started = Start();

    private static IVNode RenderRoot() => H(ShowcasePageModule.Default);

    private static bool Start()
    {
        CreateApp(Root).Mount("#app");
        return true;
    }
}
