using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode ContentModelBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("page-contract", "Page contract",
            [
                H("p", "Each page owns explicit route metadata in one central catalog: path, group, title, summary, status, section anchors, related-page links, and body dispatch. That is enough to drive the shell without introducing a hidden content layer."),
                H("ul",
                [
                    H("li", "Paths are real URLs and part of the hosting contract."),
                    H("li", "Summaries are short product-facing explanations, not internal engineering notes."),
                    H("li", "Statuses communicate maturity without inventing a versioning system for every page."),
                    H("li", "Adjacent-page recommendations are curated in the same catalog instead of inferred at runtime.")
                ])
            ]),
            PageSection("navigation-contract", "Navigation contract",
            [
                H("p", "Navigation is explicit by design. The left rail is grouped by product concern, the right rail is generated from section-level anchors, and the related-pages panel is curated from the same page catalog."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Left rail", "Stable page entry points grouped by user concern."),
                    CheckCard("Article body", "Readable sections authored directly in H functions."),
                    CheckCard("Right rail", "Anchor-level TOC for fast scanning and direct linking."),
                    CheckCard("Related pages", "Curated next-step links that stay in sync with the route catalog.")
                ])
            ]),
            PageSection("editing-rules", "Editing rules",
            [
                H("p", "The site is code-first, but it should not read like arbitrary application code. Editing rules keep it readable for documentation work."),
                H("ul",
                [
                    H("li", "Keep each section short enough to scan without opening generated output."),
                    H("li", "Prefer explicit helpers like `PageSection`, `Callout`, `CodeBlock`, and route-card grids over generic DSL layers."),
                    H("li", "Treat page catalog entries, navigation metadata, and section anchors as part of the product contract."),
                    H("li", "When adding a page, update the catalog once and let nav, TOC, related links, and pager flow from that source.")
                ]),
                Callout("Do not optimize for cleverness", "If a docs page becomes hard to edit in C#, the answer is usually clearer H composition, not a new meta-language.")
            ])
        ]);
}
