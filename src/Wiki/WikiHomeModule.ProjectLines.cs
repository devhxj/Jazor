using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode ProjectLinesBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("two-lines", "Two active lines",
            [
                H("p", "Jazor now has two active technical lines, and they solve different product problems. Readers should choose based on authoring mode and runtime expectations, not on historical naming."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("RazorVue library mode", "Compile Razor components into JavaScript modules during `dotnet build` and ship them as normal library artifacts."),
                    CheckCard("Jolt full host", "Use `.jazor` authoring with LSP, preview, HMR, build, and debug support when the project needs a full development-time host.")
                ])
            ]),
            PageSection("choose-a-path", "Choose the right path",
            [
                H("p", "The fastest way to get unstuck is to decide whether your problem is build-time artifact generation or full application development flow."),
                H("ul",
                [
                    H("li", "Choose RazorVue when the deliverable is a library or component package and the authoring surface can stay inside Razor components."),
                    H("li", "Choose Jolt when the deliverable needs a live app host, workspace graph, and browser-first development loop."),
                    H("li", "Treat both lines as consumers of the same compiler, emit, and source-origin foundations.")
                ])
            ]),
            PageSection("shared-core", "Shared core",
            [
                H("p", "The lines diverge at authoring and host behavior, but they intentionally share the compiler and emit substrate so semantics do not drift."),
                CodeBlock("Shared modules", """
src/Jazor.Compiler/
src/Jazor.Emit/
src/Jazor.Common/
src/Jazor.Name/
src/Jazor.Analyzer/
"""),
                H("p", "That shared core is why compiler boundary pages, runtime-catalog pages, and emit-contract pages matter to both lines.")
            ]),
            PageSection("where-to-read-next", "Where to read next",
            [
                H("p", "Use the line-specific pages below to go deeper instead of flattening all architecture questions into one overview."),
                RouteCardGrid([RazorVueLibraryModePath, JoltHostPath, CompilerOverviewPath])
            ])
        ]);
}
