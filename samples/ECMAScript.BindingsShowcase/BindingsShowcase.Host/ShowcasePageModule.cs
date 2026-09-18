#pragma warning disable CS0626 // The ECMAScript module binding supplies this generated module export at runtime.

namespace BindingsShowcase.Host;

/// <summary>
/// Imports the generated root page module so the host can mount it without a handwritten JS
/// boot wrapper. 导入生成的根页面模块，使宿主无需手写 JS 启动层即可挂载。
/// </summary>
[ECMAScript("components/showcase-landing.mjs")]
[Description("@#")]
internal static class ShowcasePageModule
{
    [ECMAScriptName("default")]
    public extern static IVueComponent Default { get; }
}
