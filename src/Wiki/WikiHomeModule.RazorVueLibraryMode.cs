using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode RazorVueLibraryModeBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("why-razorvue", "Why RazorVue exists",
            [
                H("p", "Not every project needs a full development host. RazorVue exists for build-time Razor-to-JavaScript compilation where the output is a library artifact rather than a live app shell."),
                H("ul",
                [
                    H("li", "Compile Razor components during `dotnet build`."),
                    H("li", "Ship library artifacts without requiring Jolt in the consumer project."),
                    H("li", "Share compiler, analyzer, emit, and source-origin foundations with the rest of the repository.")
                ])
            ]),
            PageSection("physical-split", "Physical split",
            [
                H("p", "The external RazorVue naming remains stable, but the physical source is intentionally split by concern."),
                CodeBlock("Current physical ownership", """
src/Jazor.Common/RazorVue/
src/Jazor.Analyzer/RazorVue/
src/ECMAScript.Vuetify/
src/ECMAScript.Contract/
"""),
                H("p", "That split keeps shared semantics, Roslyn host behavior, and library-component bindings from drifting into one project.")
            ]),
            PageSection("build-time-flow", "Build-time flow",
            [
                H("p", "RazorVue's contract is build-time artifact generation, not a long-running app host."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Razor semantics", "Extract component meaning from Razor authoring."),
                    CheckCard("Artifact generation", "Produce catalogs, compiled JS modules, and source origins."),
                    CheckCard("Emit materialization", "Pass stable artifacts downstream so emit can write browser-ready outputs.")
                ])
            ]),
            PageSection("when-to-choose-library-mode", "When to choose library mode",
            [
                H("p", "Choose RazorVue when the user story is package creation, reusable components, or build-time integration without a full workspace host."),
                RouteCardGrid([ProjectLinesPath, JoltHostPath, HFunctionAuthoringPath])
            ])
        ]);
}
