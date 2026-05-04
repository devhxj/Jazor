using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode ImportEmitContractBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("boundary-split", "Boundary split",
            [
                H("p", "The compiler and the emitter are intentionally different products in one pipeline. `Jazor.Compiler` owns semantic lowering and module-shape output; `Jazor.Emit` owns host-facing file materialization and bundling."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Compiler side", "Build ESTree, module text, import structure, source-origin anchors, and catalog or source-map carriers."),
                    CheckCard("Emit side", "Load catalogs from assemblies, write `.mjs` and manifest files, and run bundle orchestration."),
                    CheckCard("Why it matters", "A feature is not well-shaped if it needs compiler logic to secretly write browser files or emit logic to invent lowering semantics.")
                ])
            ]),
            PageSection("import-mainline", "Import mainline",
            [
                H("p", "The import path is already a closed mainline, not a loose collection of helper calls. Contributors should preserve that flow instead of bypassing it."),
                CodeBlock("Import flow", """
SemanticWalker
  -> host mapping chooses Alias / Inline / Import / Compile
SenseArgument
  -> collects and flushes import specifiers
AstConverter
  -> merges, dedupes, orders, and emits ImportDeclaration headers
ESGenerator
  -> serializes the final module text and carriers
"""),
                H("ul",
                [
                    H("li", "`Op.Import` is discovered at the semantic lowering site, not synthesized later by string rewriting."),
                    H("li", "Import alias stability is part of the contract, so the same module symbol should not drift to different local names inside one module."),
                    H("li", "Module-header ordering and dedupe belong to `AstConverter`, not to later file-writing stages.")
                ])
            ]),
            PageSection("layered-output", "Layered output contract",
            [
                H("p", "Output is layered on purpose. Each stage owns one boundary, and mixing them makes the pipeline harder to reason about and harder to test."),
                CodeBlock("Current ownership", """
src/Jazor.Compiler/core/SemanticWalker.cs
src/Jazor.Compiler/SenseArgument.cs
src/Jazor.Compiler/AstConverter.cs
src/Jazor.Compiler/ESGenerator.cs
src/Jazor.Emit/ModuleCollector.cs
src/Jazor.Emit/ModuleWriter.cs
src/Jazor.Emit/ModuleBundler.cs
"""),
                H("ul",
                [
                    H("li", "`AstConverter` owns module AST shape, not filesystem output."),
                    H("li", "`ESGenerator` owns JavaScript text plus catalog or source-map carriers, not browser hosting policy."),
                    H("li", "`Jazor.Emit` owns `.mjs`, `.mjs.map`, manifest, and bundle materialization, not language lowering.")
                ])
            ]),
            PageSection("host-materialization", "Host materialization",
            [
                H("p", "`Jazor.Emit` works from compiled assemblies and generated catalogs. That keeps host output reproducible and keeps browser delivery decisions out of the lowering core."),
                H("ul",
                [
                    H("li", "Collect root assembly and referenced assemblies."),
                    H("li", "Read ECMAScript module catalogs and optional RazorVue catalogs."),
                    H("li", "Write module files, manifests, and source maps."),
                    H("li", "Optionally rewrite imports and bundle through `DenoHost`.")
                ]),
                Callout("Practical rule", "If a change needs to shortcut catalogs and write browser files directly from compiler lowering, it is probably crossing the wrong boundary.")
            ]),
            PageSection("verification-signals", "Verification signals",
            [
                H("p", "This boundary is only useful if regressions are caught at the correct layer."),
                H("ul",
                [
                    H("li", "Use compiler tests when import collection, alias stability, source-origin, or carrier shape changes."),
                    H("li", "Use emit tests when manifest output, bundle rewriting, or file materialization changes."),
                    H("li", "Use Wiki smoke when browser-served modules, route assets, or import-map expectations change."),
                    H("li", "Treat source-map and catalog determinism as production contracts, not optional debug extras.")
                ])
            ])
        ]);
}
