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
                H("p", "Each page owns explicit route metadata in code: path, group, title, summary, and status. That is enough to drive the shell, page hero, and previous/next navigation without introducing a hidden content layer."),
                H("ul",
                [
                    H("li", "Paths are real URLs and part of the hosting contract."),
                    H("li", "Summaries are short product-facing explanations, not internal engineering notes."),
                    H("li", "Statuses communicate maturity without inventing a versioning system for every page.")
                ])
            ]),
            PageSection("navigation-contract", "Navigation contract",
            [
                H("p", "Navigation is explicit by design. The left rail is grouped by product concern, and the right rail is generated from section-level anchors that live beside the content they describe."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Left rail", "Stable page entry points grouped by user concern."),
                    CheckCard("Article body", "Readable sections authored directly in H functions."),
                    CheckCard("Right rail", "Anchor-level TOC for fast scanning and direct linking.")
                ])
            ]),
            PageSection("editing-rules", "Editing rules",
            [
                H("p", "The site is code-first, but it should not read like arbitrary application code. Editing rules keep it readable for documentation work."),
                H("ul",
                [
                    H("li", "Keep each section short enough to scan without opening generated output."),
                    H("li", "Prefer explicit helpers like `PageSection`, `Callout`, and `CodeBlock` over generic DSL layers."),
                    H("li", "Treat navigation metadata and section anchors as part of the product contract.")
                ]),
                Callout("Do not optimize for cleverness", "If a docs page becomes hard to edit in C#, the answer is usually clearer H composition, not a new meta-language.")
            ])
        ]);
}
