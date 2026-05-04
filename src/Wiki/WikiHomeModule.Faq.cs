using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode FaqBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("using-jazor", "Using Jazor",
            [
                H("p", "Q: Should a new project start with RazorVue or Jolt?"),
                H("p", "A: Start with RazorVue if the work is library-mode component emission during `dotnet build`. Start with Jolt if the project needs `.jazor` authoring, preview, HMR, or multi-language workspace tooling."),
                H("p", "Q: Is the Wiki itself proof that H-function authoring is production-safe?"),
                H("p", "A: Yes. The current shell, navigation, route fallback, and runtime-module imports all run on the same H-function authoring surface that production code uses.")
            ]),
            PageSection("compiler-boundaries", "Compiler boundaries",
            [
                H("p", "Q: Why does the analyzer sometimes complain earlier than the compiler fails?"),
                H("p", "A: That asymmetry is intentional. The analyzer is allowed to be stricter in erased positions so unsupported concrete external types surface earlier, while the compiler still decides final acceptance at the runtime-sensitive lowering site."),
                H("p", "Q: Why not silently fall back to raw JavaScript?"),
                H("p", "A: Because unsupported runtime-sensitive behavior must fail explicitly. Silent raw-JS fallback erodes determinism and makes the supported boundary impossible to reason about.")
            ]),
            PageSection("runtime-and-host", "Runtime and host behavior",
            [
                H("p", "Q: Why are `System/*` helpers explicit browser modules instead of hidden runtime glue?"),
                H("p", "A: Because production output must be inspectable, importable, and smoke-verifiable. Explicit modules keep the browser contract visible."),
                H("p", "Q: Why does the docs host still serve `index.html` on unknown routes?"),
                H("p", "A: So direct refreshes and typed URLs still boot the SPA shell, which can then recover into a not-found page with route suggestions.")
            ]),
            PageSection("wiki-workflow", "Wiki workflow",
            [
                H("p", "Q: Is Wiki a CMS?"),
                H("p", "A: No. It is a code-first documentation product with explicit ownership, generated browser artifacts, and operational verification."),
                H("p", "Q: What makes a docs change complete?"),
                H("p", "A: The source page, central route catalog, emitted browser output, and smoke verification all need to agree before the page is treated as ready.")
            ])
        ]);
}
