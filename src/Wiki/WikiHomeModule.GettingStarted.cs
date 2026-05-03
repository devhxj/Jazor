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
                    H("li", "`/guides/getting-started` for local workflow"),
                    H("li", "`/guides/content-model` for page authoring rules"),
                    H("li", "`/engineering/h-function-authoring` for the H-function contract"),
                    H("li", "`/operations/deployment` for build and hosting details")
                ])
            ]),
            PageSection("add-a-page", "Add a page safely",
            [
                H("p", "A new page is introduced by adding one route constant, one route-contract entry, one nav entry, one dedicated page file, and one TOC branch. The point is not to invent a mini CMS inside the codebase."),
                CodeBlock("Minimum page shape", """
private const string NewPagePath = "/guides/new-page";

private static IVNode NewPageBody()
    => H("div", "...");
"""),
                H("p", "After the route exists, add TOC entries and pager links, then rerun the smoke script.")
            ]),
            PageSection("verify-the-result", "Verify the result",
            [
                H("p", "For Wiki, verification is deliberately operational. Build output, route availability, and shell stability matter more than screenshot-only review."),
                H("ul",
                [
                    H("li", "Confirm `main.mjs` and `components/wiki-home.mjs` exist after build."),
                    H("li", "Confirm all registered docs routes return the frontend shell through route fallback."),
                    H("li", "Confirm emitted module text still contains expected route identifiers and page labels.")
                ])
            ])
        ]);
}
