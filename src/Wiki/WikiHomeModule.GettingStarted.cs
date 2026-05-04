using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode GettingStartedBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("boot-the-site", "Boot the site locally",
            [
                H("p", "The local loop is intentionally short. Build, emit, and run the static host from the repository root."),
                CodeBlock("Local commands", """
dotnet build .\src\Wiki\Wiki.csproj
.\src\Wiki\serve.ps1 -Build
.\src\Wiki\verify-smoke.ps1 -BuildLocal
"""),
                H("p", "The smoke script is part of the contract now. A real route is not considered valid until build output and route fallback are both verified.")
            ]),
            PageSection("route-model", "Understand the route model",
            [
                H("p", "Wiki now uses real URL paths with server fallback, so routes can be refreshed or opened directly in the browser."),
                H("ul",
                [
                    H("li", "`/` for the overview page"),
                    H("li", "`/search?q=compiler` for a shareable query-driven search entry"),
                    H("li", "`/guides/getting-started` for local workflow"),
                    H("li", "`/guides/project-lines` for the active product lines"),
                    H("li", "`/guides/content-model` for page authoring rules"),
                    H("li", "`/guides/navigation-discovery` for grouped navigation, TOC behavior, related pages, and not-found recovery"),
                    H("li", "`/guides/information-architecture` for route families, naming rules, and page-order discipline"),
                    H("li", "`/guides/topic-index`, `/guides/glossary`, `/guides/faq`, and `/guides/troubleshooting` for discovery and support"),
                    H("li", "`/engineering/h-function-authoring` for the H-function contract"),
                    H("li", "`/engineering/compiler-overview` for the compiler pipeline entry point"),
                    H("li", "`/engineering/compiler-support-boundary` for active compiler semantics and failure rules"),
                    H("li", "`/engineering/route-catalog-contract` for the single-source route registration contract"),
                    H("li", "`/engineering/host-semantic-seams` for Alias / Inline / Import / Compile responsibility boundaries"),
                    H("li", "`/engineering/import-emit-contract` for module import flow and file materialization boundaries"),
                    H("li", "`/engineering/runtime-catalog` for CLR runtime helper generation and browser delivery"),
                    H("li", "`/engineering/jolt-host` and `/engineering/razorvue-library-mode` for the two active delivery lines"),
                    H("li", "`/operations/content-governance` for content ownership, generated-output review, and release discipline"),
                    H("li", "`/operations/deployment` for build and hosting details"),
                    H("li", "`/operations/testing-verification` for focused test and smoke workflow")
                ])
            ]),
            PageSection("add-a-page", "Add a page safely",
            [
                H("p", "A new page is introduced by adding one route constant, one catalog entry, one dedicated page file, and one body method. Navigation groups, TOC wiring, related links, and pager continuity should all flow from that central page catalog."),
                CodeBlock("Minimum page shape", """
private const string NewPagePath = "/guides/new-page";

private static IVNode NewPageBody()
    => H("div", "...");
"""),
                H("p", "After the route exists, register its summary, status, section anchors, and related pages in the catalog, then rerun the smoke script.")
            ]),
            PageSection("verify-the-result", "Verify the result",
            [
                H("p", "For Wiki, verification is deliberately operational. Build output, route availability, and shell stability matter more than screenshot-only review."),
                H("ul",
                [
                    H("li", "Confirm `main.mjs` and `components/wiki-home.mjs` exist after build."),
                    H("li", "Confirm all registered docs routes return the frontend shell through route fallback."),
                    H("li", "Confirm emitted module text still contains expected route identifiers, search shell markers, and page labels.")
                ])
            ])
        ]);
}
