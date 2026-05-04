using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode InformationArchitectureBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("concern-groups", "Concern groups",
            [
                H("p", "Wiki route structure is grouped by reader concern first. The goal is to keep discovery obvious even as the docs surface grows, instead of flattening everything into one long route list."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Foundation", "Guides that explain how to use, navigate, and maintain the docs shell itself."),
                    CheckCard("Engineering", "Contracts, seams, compiler boundaries, and runtime or emit behavior that contributors need to reason about."),
                    CheckCard("Operations", "Build, governance, verification, and release-facing rules that keep the site shippable.")
                ])
            ]),
            PageSection("route-shape", "Route shape",
            [
                H("p", "URL shape is part of the product contract. The route should tell readers what kind of page they are opening before they read any body copy."),
                CodeBlock("Current route families", """
/                         overview
/guides/*                 reader and maintainer guides
/engineering/*            compiler and host contracts
/operations/*             build, governance, and verification
"""),
                H("ul",
                [
                    H("li", "Use lowercase route segments."),
                    H("li", "Use hyphenated English words instead of camelCase or opaque abbreviations."),
                    H("li", "Keep group prefixes stable so recovery and suggestion logic stay predictable.")
                ])
            ]),
            PageSection("naming-rules", "Naming rules",
            [
                H("p", "Route names, titles, summaries, and section anchors should read well together. The shell exposes all of them directly through navigation, filtering, TOC links, route cards, and not-found suggestions."),
                H("ul",
                [
                    H("li", "Titles should be product-facing and scannable, not internal task labels."),
                    H("li", "Summaries should explain the page outcome in one short sentence."),
                    H("li", "Section ids should be stable and link-friendly because they become shareable anchors."),
                    H("li", "Section titles should read naturally both in the body and in the right-rail TOC.")
                ])
            ]),
            PageSection("ordering-rules", "Ordering and reading flow",
            [
                H("p", "Catalog order is not cosmetic. It controls previous or next flow, influences how related concepts are discovered, and sets the reading rhythm for the whole site."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Pager continuity", "Routes should be ordered so previous and next navigation feels intentional instead of arbitrary."),
                    CheckCard("Related pages", "Curated related links should reinforce the same mental model as the route order, not fight it."),
                    CheckCard("Overview catalog", "The overview page doubles as the route map, so route order is also part of the visible site structure.")
                ])
            ]),
            PageSection("growth-without-drift", "Growth without drift",
            [
                H("p", "Adding pages should make the site richer without eroding the route model. The test is whether a new page can be placed cleanly into one concern group and one reading path."),
                CodeBlock("Safe growth checklist", """
1. Choose the correct concern group first.
2. Pick a route that matches the existing family shape.
3. Add product-facing title, summary, and status metadata.
4. Add related pages that help the next reading step.
5. Rebuild and rerun smoke so route and section markers stay protected.
"""),
                Callout("Practical rule", "If a new page does not have an obvious home in Foundation, Engineering, or Operations, the information architecture probably needs revision before the page is added.")
            ])
        ]);
}
