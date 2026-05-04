using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode JoltHostBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("why-jolt", "Why Jolt exists",
            [
                H("p", "Library-mode emission is not enough for full application development. Jolt exists to provide the development-time host around `.jazor` authoring, workspace context, preview, build, and debug loops."),
                H("ul",
                [
                    H("li", "`.jazor` stays the first-class authoring surface."),
                    H("li", "The workspace is treated as a graph, not a pile of isolated files."),
                    H("li", "Jazor, Roslyn, and Volar each keep their own semantic lane.")
                ])
            ]),
            PageSection("subsystems", "Subsystems",
            [
                H("p", "Jolt is not one monolith. It coordinates several focused subsystems."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Jazor core", "Parses `.jazor`, builds projections, and derives frontend context."),
                    CheckCard("LSP", "Routes requests across Jazor, Roslyn, and Volar lanes."),
                    CheckCard("DevServer and Build", "Handles preview, HMR, production build, CSS, assets, and import maps."),
                    CheckCard("Volar / Deno", "Provides Vue, TypeScript, CSS, and HTML semantic workers.")
                ])
            ]),
            PageSection("run-modes", "Run modes",
            [
                H("p", "Jolt exposes distinct modes for the workflows it needs to own, instead of one catch-all startup path."),
                CodeBlock("Representative modes", """
--stdio
--lsp
--dev
--build
--analysis-stdio
""")
            ]),
            PageSection("when-to-choose-jolt", "When to choose Jolt",
            [
                H("p", "Choose Jolt when the task is primarily about application development ergonomics instead of library artifact generation."),
                RouteCardGrid([ProjectLinesPath, RazorVueLibraryModePath, GettingStartedPath])
            ])
        ]);
}
