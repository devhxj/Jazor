using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode RuntimeCatalogBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("why-catalog-exists", "Why the catalog exists",
            [
                H("p", "Wiki now consumes the same CLR-backed runtime surface that production Jazor output needs in the browser. The catalog exists so `Jazor.CLR` import helpers become explicit `System/*` ESM modules instead of hidden assumptions inside the compiler."),
                H("ul",
                [
                    H("li", "Browser entry points can import only the runtime helpers they actually use."),
                    H("li", "Generated `System/*` modules stay inspectable under the local emit directory `src/Wiki/jazor/System/`."),
                    H("li", "The docs site proves this path with real browser-served assets, not just compiler unit tests.")
                ])
            ]),
            PageSection("generation-pipeline", "Generation pipeline",
            [
                H("p", "The current pipeline scans CLR whitelist declarations, emits compiler whitelist artifacts, refreshes the in-process whitelist view, and then materializes browser-ready runtime modules into the ECMAScript catalog."),
                CodeBlock("Current pipeline touchpoints", """
src/Jazor.Compiler.Generator/Program.cs
src/Jazor.Compiler.Generator/ClrRuntimeCatalogEmitter.cs
src/Jazor.Compiler.Generator/ClrRuntimeSelection.cs
src/Jazor.Compiler/WhiteList.cs.Generate.cs
src/ECMAScript/Jazor.Generated.ClrRuntimeCatalog.g.cs
src/Wiki/jazor/System/
"""),
                H("p", "That single-run refresh matters. New CLR mappings should be visible to runtime-catalog emission in the same generator invocation, not only after a second pass.")
            ]),
            PageSection("runtime-contract", "Runtime contract",
            [
                H("p", "The contract is intentionally explicit: browser entry HTML declares the `System/` import-map prefix, emitted Jazor modules import named runtime helpers, and catalog output provides stable ESM exports for those helpers."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Import map", "The browser resolves `System/*` through `/jazor/System/` from `wwwroot/index.html`."),
                    CheckCard("Named exports", "Generated runtime modules expose callable helper exports plus module namespace objects for import stability."),
                    CheckCard("Local assets", "Wiki serves runtime helpers from the project-local emit directory in development and from `wwwroot/jazor/System/` after publish.")
                ])
            ]),
            PageSection("operational-guardrails", "Operational guardrails",
            [
                H("p", "Production safety comes from build and smoke discipline, not from assuming the catalog stayed correct after refactors."),
                H("ul",
                [
                    H("li", "Run the CLR catalog generator and keep the generated whitelist and catalog in sync with source mappings."),
                    H("li", "Keep focused emit tests around runtime export shape and wrapper import behavior."),
                    H("li", "Verify Wiki still serves `/jazor/System/*` modules and the import-map contract after changes.")
                ]),
                Callout("Practical rule", "If a new CLR import helper cannot be emitted into the runtime catalog in one generator run, the developer experience is not production-ready yet.")
            ])
        ]);
}
