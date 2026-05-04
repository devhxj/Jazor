using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode TestingVerificationBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("verification-layers", "Verification layers",
            [
                H("p", "The production contract is protected by multiple test layers, not by one oversized suite. Each layer answers a different failure mode."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Compiler regressions", "`Jazor.CompilerTest` locks semantic lowering, import/header stability, naming, and source-map or catalog determinism."),
                    CheckCard("Emit regressions", "`Jazor.EmitTest` checks bundle and file-materialization behavior instead of trusting compiler output alone."),
                    CheckCard("Operational smoke", "`src/Wiki/verify-smoke.ps1` proves that emitted assets, route fallback, browser entry wiring, and static hosting still behave as a real site."),
                    CheckCard("Browser runtime", "`src/Wiki/verify-browser.ps1` drives a headless Edge session through mount, SPA navigation, search, not-found recovery, persisted shell state, hash routing, and mobile drawer behavior.")
                ])
            ]),
            PageSection("focused-commands", "Focused commands",
            [
                H("p", "Verification should stay focused. Run the smallest command that proves the changed contract, then expand only when the risk surface grows."),
                CodeBlock("Typical command set", """
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter SemanticWalkerReferenceTest
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
pwsh ./scripts/test-dotnet.ps1
pwsh ./scripts/test-dotnet.ps1 -Project wiki
pwsh ./scripts/test-dotnet.ps1 -Project wiki-publish
pwsh ./scripts/test-dotnet.ps1 -Project wiki-browser
pwsh ./scripts/test-dotnet.ps1 -Project wiki-browser-publish
.\src\Wiki\verify-smoke.ps1 -BuildLocal
.\src\Wiki\verify-browser.ps1 -BuildLocal
"""),
                H("ul",
                [
                    H("li", "Use focused `--filter` runs while iterating on one lowering route or one contract family."),
                    H("li", "Use the repo test script when a change crosses compiler, emit, CLR, or host boundaries."),
                    H("li", "Use Wiki smoke when a change touches generated browser assets, route registration, or hosting behavior."),
                    H("li", "Use Wiki browser verification when a change touches history routing, hash navigation, clipboard flows, `localStorage`, focus/live-region behavior, or mobile drawers.")
                ])
            ]),
            PageSection("coverage-and-determinism", "Coverage and determinism",
            [
                H("p", "The active test discipline is not only about line coverage. It also protects deterministic output surfaces that downstream tooling depends on."),
                H("ul",
                [
                    H("li", "Behavior contracts come first: lock observable lowering behavior before asserting text shape."),
                    H("li", "If a change affects source origin, source map, catalog, or output text, add the matching `SourceOrigin`, `SourceMap`, or `ESGenerator` regression."),
                    H("li", "If a change adds helpers, overload dispatchers, or synthetic temps, prove the names stay stable and do not drift with traversal order."),
                    H("li", "Coverage settings remain part of the suite through `coverlet.runsettings`, but deterministic output is treated as a product contract, not only a test convenience.")
                ])
            ]),
            PageSection("wiki-release-gate", "Wiki release gate",
            [
                H("p", "For `jazor.wiki`, release readiness is operational. The browser-facing shell has to build, mount, route, and serve the expected assets exactly as declared."),
                H("ul",
                [
                    H("li", "Build output must include `src/Wiki/jazor/main.mjs`, `components/wiki-home.mjs`, and `jazor-manifest.json`."),
                    H("li", "Registered docs routes must return the shell with `#app`, `/jazor/main.mjs`, and the `System/` import-map prefix."),
                    H("li", "Browser assets such as `/jazor/System/StringModule.js`, `/site.css`, and `/favicon.svg` must resolve successfully."),
                    H("li", "Headless browser verification must prove real mount, SPA route transitions, search/not-found recovery, persisted shell state, copy affordances, hash routing, and mobile drawer behavior without console or runtime errors."),
                    H("li", "Unknown docs routes must still fall back to `index.html` so the frontend shell can recover instead of failing at the host boundary."),
                    H("li", "Publish verification must prove `wwwroot/jazor` serves production assets and that no root shadow `jazor/` directory survives to override that contract.")
                ]),
                Callout("Practical rule", "A compiler or emit change is not ready for product use if unit tests pass but the Wiki smoke contract or the headless browser contract regresses.")
            ])
        ]);
}
