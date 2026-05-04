using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode ContentGovernanceBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("ownership-model", "Ownership model",
            [
                H("p", "Wiki is code-first, but it is still a product surface. Content ownership is explicit: page prose lives in per-page H-function files, route metadata lives in the central catalog, and host or shell behavior lives in the shared module and element helpers."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Page source", "Edit page copy, section order, and examples in the dedicated page file for that route."),
                    CheckCard("Route metadata", "Edit path, title, summary, status, TOC labels, and related-page wiring in `WikiHomeModule.RouteContract.cs`."),
                    CheckCard("Shell behavior", "Edit navigation, pager, TOC, permalink, and not-found behavior in shared shell files instead of duplicating local page logic.")
                ])
            ]),
            PageSection("source-boundaries", "Source boundaries",
            [
                H("p", "The authoring boundary must stay obvious to maintainers. Source files are edited directly; generated browser artifacts are inspected as outputs, not treated as hand-maintained source."),
                H("ul",
                [
                    H("li", "Author content in `WikiHomeModule.*.cs`, `Program.cs`, `AppModule.cs`, `index.html`, and `site.css`."),
                    H("li", "Do not hand-maintain `src/Wiki/jazor/main.mjs`, `components/wiki-home.mjs`, or emitted manifest files as the primary source of behavior."),
                    H("li", "If emitted output changes because source changed, review the generated diff and keep it in sync with the source change that caused it.")
                ])
            ]),
            PageSection("generated-assets", "Generated assets",
            [
                H("p", "Generated assets are part of the shipped product, so they matter operationally. The rule is not to ignore them. The rule is to regenerate them from source and review them as downstream product artifacts."),
                CodeBlock("Authoring versus output", """
Author here:
  src/Wiki/WikiHomeModule.*.cs
  src/Wiki/AppModule.cs
  src/Wiki/Program.cs
  src/Wiki/wwwroot/index.html
  src/Wiki/wwwroot/site.css

Review output here:
  src/Wiki/jazor/main.mjs
  src/Wiki/jazor/components/wiki-home.mjs
  src/Wiki/jazor/jazor-manifest.json
"""),
                H("p", "That split keeps maintainers grounded in source while still forcing review of what the browser will actually execute.")
            ]),
            PageSection("change-flow", "Safe change flow",
            [
                H("p", "A content change is complete only when source, catalog, and emitted product outputs all agree."),
                CodeBlock("Operational flow", """
1. Edit the page body or shell source.
2. Update the central route catalog if the route contract changed.
3. Update preview URLs and smoke expectations for any new route.
4. Build the host to regenerate emitted browser assets.
5. Run `verify-smoke.ps1` before treating the page as ready.
"""),
                H("ul",
                [
                    H("li", "If a route is added, README and preview tooling move in the same slice."),
                    H("li", "If shell interaction changes, verify the emitted module still contains the expected browser markers."),
                    H("li", "If a maintainer cannot tell which file owns a change, the authoring boundary already needs correction.")
                ])
            ]),
            PageSection("release-discipline", "Release discipline",
            [
                H("p", "Production readiness is not only about whether the page reads well. A docs change is releasable only when the real host and emitted assets still satisfy the declared product contract."),
                H("ul",
                [
                    H("li", "Keep page titles and summaries product-facing; they drive navigation, hero copy, suggestions, and search filtering."),
                    H("li", "Do not merge route-catalog drift, missing section anchors, or stale preview lists just because the page body itself compiles."),
                    H("li", "Treat build and smoke verification as the minimum release gate for every new route and every shell-affecting change.")
                ]),
                Callout("Practical rule", "The docs site is production code. Content changes are not exempt from source ownership, generated-output review, or operational verification.")
            ])
        ]);
}
