using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode NavigationDiscoveryBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("left-rail", "Left rail discovery",
            [
                H("p", "The left rail is the primary discovery surface. It groups pages by concern, keeps the current page visible, and exposes client-side filtering without leaving the current route."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Grouped entry points", "Foundation, Engineering, and Operations stay visible as stable product groups instead of one flat page list."),
                    CheckCard("Local filtering", "The search box filters by route fragment, group label, title, summary, and status in the live shell."),
                    CheckCard("Current-page context", "Active route styling keeps readers oriented while they move across related topics.")
                ])
            ]),
            PageSection("right-rail", "Right rail navigation",
            [
                H("p", "Section-level navigation is part of the product contract, not an afterthought. The right rail is generated from registered section ids and titles, so every document has direct in-page entry points."),
                H("ul",
                [
                    H("li", "Hash links remain shareable and refresh-safe."),
                    H("li", "The active section state follows the current hash so readers can see where they are."),
                    H("li", "Permalink actions expose direct links to each section without inventing a second routing system.")
                ])
            ]),
            PageSection("related-pages", "Related pages and reading flow",
            [
                H("p", "The docs shell intentionally helps readers continue instead of stopping at one page. Related pages and previous or next flow are curated in the central catalog so the reading path stays purposeful."),
                H("ul",
                [
                    H("li", "Related pages are explicit catalog entries, not keyword guesses."),
                    H("li", "Previous and next flow comes from route order, which keeps long-form reading predictable."),
                    H("li", "Overview route cards reuse the same metadata surface, so the site map and page chrome stay aligned.")
                ])
            ]),
            PageSection("not-found-recovery", "Not-found recovery",
            [
                H("p", "Unknown URLs do not drop readers into a dead-end shell. The host still serves the app, and the docs surface offers recovery through group-aware and fragment-aware suggestions."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Route fallback", "ASP.NET Core still returns `index.html` for unknown docs paths so the frontend can recover."),
                    CheckCard("Requested-path context", "The not-found document shows the requested route so maintainers can diagnose whether a page is missing or mistyped."),
                    CheckCard("Suggested pages", "Recovery links are derived from the same route catalog used by normal navigation.")
                ])
            ]),
            PageSection("authoring-implications", "Authoring implications",
            [
                H("p", "Navigation quality depends on page metadata quality. A page is not done when its body exists; it is done when its title, summary, sections, related links, and route placement all support discovery."),
                CodeBlock("Discovery-ready page checklist", """
1. Register the route in the central catalog.
2. Add product-facing title, summary, and status.
3. Add section ids and TOC labels that read well as direct links.
4. Curate related pages that help the next reading step.
5. Rebuild and rerun smoke so emitted navigation markers stay valid.
"""),
                Callout("Practical rule", "If a reader cannot reliably find, scan, and continue from a page, the content is still incomplete even if the prose itself is finished.")
            ])
        ]);
}
