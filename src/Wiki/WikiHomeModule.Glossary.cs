using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode GlossaryBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("compiler-terms", "Compiler terms",
            [
                H("ul",
                [
                    H("li", [H("strong", "SemanticWalker"), H("span", " lowers Roslyn `IOperation` trees into ESTree while preserving usage-site observable behavior.")]),
                    H("li", [H("strong", "AstConverter"), H("span", " owns module-level AST assembly, import declaration materialization, and final structure planning.")]),
                    H("li", [H("strong", "Source origin"), H("span", " anchors generated JavaScript back to authored C# so source maps and debug tools stay trustworthy.")]),
                    H("li", [H("strong", "WhiteList"), H("span", " is the compiler's supported external runtime capability surface, generated from declaration sources.")])
                ])
            ]),
            PageSection("runtime-terms", "Runtime terms",
            [
                H("ul",
                [
                    H("li", [H("strong", "Import map"), H("span", " tells the browser how module specifiers such as `System/*` resolve at runtime.")]),
                    H("li", [H("strong", "CLR catalog"), H("span", " is the emitted browser-facing module set that backs CLR helper imports with explicit ESM files.")]),
                    H("li", [H("strong", "Alias / Inline / Import / Compile"), H("span", " are the ordered host-semantic seams used when mapping external members.")])
                ])
            ]),
            PageSection("host-terms", "Host and workflow terms",
            [
                H("ul",
                [
                    H("li", [H("strong", "RazorVue"), H("span", " is the build-time library mode for compiling Razor components into JS artifacts.")]),
                    H("li", [H("strong", "Jolt"), H("span", " is the development-time host that provides `.jazor` editing, preview, build, and debug flows.")]),
                    H("li", [H("strong", "Route catalog"), H("span", " is the single registration surface that drives docs metadata, navigation, TOC anchors, and related-page flow.")]),
                    H("li", [H("strong", "Smoke verification"), H("span", " is the fast operational check that proves the emitted Wiki shell and host routes still work end to end.")])
                ]),
                Callout("Practical rule", "If a term is overloaded between compiler, emit, and host concerns, document the owning boundary instead of relying on tribal knowledge.")
            ])
        ]);
}
