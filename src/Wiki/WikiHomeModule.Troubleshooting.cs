using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode TroubleshootingBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("route-and-host", "Route and host issues",
            [
                H("p", "If a direct route refresh fails or the docs shell loads without content, check the host contract before touching page code."),
                H("ul",
                [
                    H("li", "Confirm `Program.cs` still serves static files and falls back to `index.html`."),
                    H("li", "Confirm `/jazor/main.mjs` resolves from the local emit directory in development."),
                    H("li", "Confirm the requested route is registered in `WikiHomeModule.RouteContract.cs`.")
                ])
            ]),
            PageSection("runtime-imports", "Runtime import failures",
            [
                H("p", "If browser-served `System/*` helpers fail to load, the problem is usually emit output or import-map wiring, not page prose."),
                CodeBlock("Check these paths", """
src/Wiki/wwwroot/index.html
src/Wiki/jazor/main.mjs
src/Wiki/jazor/System/
src/Jazor.Compiler.Generator/Program.cs
src/ECMAScript/Jazor.Generated.ClrRuntimeCatalog.g.cs
"""),
                H("p", "Rebuild the project and verify the import-map prefix still points to `/jazor/System/`.")
            ]),
            PageSection("compiler-diagnostics", "Compiler and analyzer diagnostics",
            [
                H("p", "When diagnostics mention unsupported types or members, identify whether the issue is authoring-time analysis, whitelist mapping, or runtime-sensitive lowering."),
                H("ul",
                [
                    H("li", "Use the compiler overview and support-boundary pages to decide whether the use site is supposed to work."),
                    H("li", "Check `Jazor.CLR` declarations when the member should be host-mapped but is not."),
                    H("li", "Keep analyzer and compiler expectations aligned with the documented boundary instead of weakening the failure mode.")
                ])
            ]),
            PageSection("workflow-fixes", "Workflow fixes",
            [
                H("p", "The quickest repair loop for Wiki still stays operational and explicit."),
                CodeBlock("Focused repair loop", """
dotnet build .\src\Wiki\Wiki.csproj -v minimal
.\src\Wiki\verify-smoke.ps1 -Build
.\src\Wiki\serve.ps1 -Build
"""),
                Callout("Practical rule", "If a page renders but smoke fails, treat the host or emitted asset contract as broken. Do not mark the page done because the prose looks correct in one tab.")
            ])
        ]);
}
