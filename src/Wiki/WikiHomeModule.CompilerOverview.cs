using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode CompilerOverviewBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("what-it-is", "What it is",
            [
                H("p", "`Jazor.Compiler` is the core C# to JavaScript compiler in this repository. It does not aim to re-create arbitrary CLR runtime identity in JavaScript. It aims to preserve usage-site observable behavior inside a controlled domain."),
                H("ul",
                [
                    H("li", "Primary semantic input: Roslyn `IOperation`."),
                    H("li", "Primary intermediate representation: Acornima ESTree."),
                    H("li", "Primary output contract: stable AST, imports, names, source origins, and downstream emit carriers.")
                ])
            ]),
            PageSection("core-pipeline", "Core pipeline",
            [
                H("p", "The active pipeline is intentionally layered so host mapping, lowering, and file materialization do not collapse into one ambiguous stage."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("SemanticWalker", "Performs expression and statement lowering from `IOperation` into ESTree fragments."),
                    CheckCard("AstConverter", "Builds module-level declarations, class members, imports, and top-level shape."),
                    CheckCard("Jazor.Emit", "Materializes `.mjs`, `.mjs.map`, manifest files, and bundle-facing outputs.")
                ])
            ]),
            PageSection("hard-contracts", "Hard contracts",
            [
                H("p", "Several once-exploratory routes are now fixed enough that contributors should treat them as engineering contracts."),
                H("ul",
                [
                    H("li", "Tuple and record routes preserve usage-site behavior instead of CLR runtime identity."),
                    H("li", "Interfaces stay contract-only; they do not emit runtime declarations."),
                    H("li", "Import discovery and module-header generation are now stable mainline behavior, not optional follow-up work."),
                    H("li", "Unsupported runtime-sensitive behavior should fail explicitly instead of degrading to raw JavaScript.")
                ])
            ]),
            PageSection("read-this-next", "Read this next",
            [
                H("p", "Use the boundary and seam pages when you need a narrower rule than this overview provides."),
                RouteCardGrid([CompilerBoundaryPath, HostSemanticSeamsPath, ImportEmitContractPath, RuntimeCatalogPath])
            ])
        ]);
}
