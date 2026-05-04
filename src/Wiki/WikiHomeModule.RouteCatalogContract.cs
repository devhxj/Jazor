using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode RouteCatalogContractBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("single-source", "Single source of truth",
            [
                H("p", "Wiki route registration is intentionally centralized. `WikiHomeModule.RouteContract.cs` is the single source of truth for route metadata, section anchors, related pages, and body dispatch."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Catalog arrays", "`PagePaths`, groups, titles, summaries, statuses, section ids, section titles, and related paths stay aligned as one contract surface."),
                    CheckCard("Shell consumers", "Navigation rail, hero copy, right-rail TOC, related-page panel, pager flow, and not-found suggestions all read from that same catalog."),
                    CheckCard("Why centralize", "The goal is not abstraction for its own sake. The goal is to avoid route drift across multiple files and hidden registration points.")
                ])
            ]),
            PageSection("what-the-catalog-owns", "What the catalog owns",
            [
                H("p", "The catalog is responsible for more than route existence. It defines the product-facing metadata that the shell presents to users and maintainers."),
                H("ul",
                [
                    H("li", "Real route path and product group."),
                    H("li", "Page title, summary, and status badge."),
                    H("li", "Body dispatch function."),
                    H("li", "Section anchors and TOC labels."),
                    H("li", "Related-page suggestions and previous or next continuity.")
                ])
            ]),
            PageSection("safe-change-flow", "Safe change flow",
            [
                H("p", "A page change is considered safe only when the catalog, the page body, and the operational checks all move together."),
                CodeBlock("Minimum route addition workflow", """
1. Add one route constant.
2. Add one page body file and body method.
3. Register path, title, summary, status, sections, and related paths in the central catalog.
4. Update preview and smoke route expectations.
5. Rebuild and rerun verify-smoke.
"""),
                H("ul",
                [
                    H("li", "Do not add hidden route registries or inferred discovery rules in parallel with the central catalog."),
                    H("li", "Do not let nav or TOC read from a second metadata source that can drift from page body registration."),
                    H("li", "Treat array-length alignment in the catalog as a correctness rule, not just a style preference.")
                ])
            ]),
            PageSection("failure-modes", "Failure modes to avoid",
            [
                H("p", "Most maintainability regressions come from splitting metadata ownership or from assuming the shell can infer structure later."),
                H("ul",
                [
                    H("li", "Route exists but page title or summary was not registered."),
                    H("li", "Page body exists but section anchors were not added, so TOC and direct linking drift."),
                    H("li", "Related pages or pager order no longer match the intended reading flow."),
                    H("li", "Preview URLs, smoke route lists, or emitted marker checks were not updated after catalog changes.")
                ]),
                Callout("Practical rule", "If a maintainer has to edit two unrelated metadata systems to add one page, the design already regressed.")
            ]),
            PageSection("verification-contract", "Verification contract",
            [
                H("p", "The route catalog is operationally protected. The docs shell is not considered valid unless the catalog is reflected in emitted modules and served routes."),
                H("ul",
                [
                    H("li", "Host startup now validates catalog alignment, duplicate paths, duplicate section ids, and related-page integrity before serving requests."),
                    H("li", "Smoke checks route markers, page-title markers, and section-anchor markers inside emitted `wiki-home.mjs`."),
                    H("li", "Smoke checks all registered docs routes through the real host with fallback routing enabled."),
                    H("li", "Not-found recovery depends on the same catalog through suggested pages and group-based fallback."),
                    H("li", "The overview page doubles as a live catalog surface through `RouteCardGrid(PagePaths)`.")
                ])
            ])
        ]);
}
