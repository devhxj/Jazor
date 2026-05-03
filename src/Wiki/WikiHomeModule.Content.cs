using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode OverviewBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("what-ships-now", "What ships now",
            [
                H("p", "Wiki now runs as a real docs shell instead of a single-page playground. The primary contract is a stable static host, explicit document routes, and H-function-authored layout."),
                H("div", new VueObject { Class = "metric-grid" },
                [
                    MetricCard("5", "Core routes", "Overview, guides, engineering, and operations pages ship as first-class entry points."),
                    MetricCard("1", "Static host", "ASP.NET Core serves assets, health checks, and fallback routing with one small host."),
                    MetricCard("100%", "Shell in H", "Navigation, hero, article sections, TOC, and pager all live on the H-function authoring surface.")
                ])
            ]),
            PageSection("why-this-exists", "Why this exists",
            [
                H("p", "The old Wiki proved that Jazor could emit Vue modules. The new Wiki proves that the H-function path can carry a production-facing information architecture, not just a demo panel."),
                H("ul",
                [
                    H("li", "The site itself is now a product surface, not just a compiler sample."),
                    H("li", "Navigation, route entry, and deployment guidance are treated as product contracts."),
                    H("li", "The content model stays explicit so maintainers can evolve it without a hidden pipeline.")
                ])
            ]),
            PageSection("mvp-boundary", "MVP boundary",
            [
                Callout("Included now", "Real routes, multi-page docs, left navigation, right-side table of contents, and previous/next page flow."),
                H("ul",
                [
                    H("li", "Included: a production-oriented docs shell, code-first pages, and smoke-verifiable routes."),
                    H("li", "Deferred: markdown ingestion, editable content management, comments, and user-specific state."),
                    H("li", "Deferred: external search service and non-CDN asset packaging.")
                ])
            ]),
            PageSection("site-structure", "Site structure",
            [
                H("p", "The site is intentionally small and explicit. The production proof point is not an abstraction layer; it is that a maintainable docs site can live directly on the H-function authoring surface."),
                CodeBlock("Current production surface", """
src/Wiki/
  Program.cs
  AppModule.cs
  WikiHomeModule.cs
  WikiHomeModule.RouteContract.cs
  WikiHomeModule.Content.cs
  WikiHomeModule.Elements.cs
  wwwroot/index.html
  wwwroot/site.css
  verify-smoke.ps1
""")
            ])
        ]);

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
                H("p", "A new page is introduced by adding one route constant, one route-contract entry, one nav entry, one body method, and one TOC branch. The point is not to invent a mini CMS inside the codebase."),
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

    private static IVNode HFunctionAuthoringBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("layout-composition", "Layout composition",
            [
                H("p", "H functions are the production surface here because they keep the rendered structure explicit while staying inside the same typed ecosystem as the rest of the project."),
                CodeBlock("Section composition", """
private static IVNode PageSection(string id, string title, IVNode[] content)
    => H("section", new VueObject { Id = id, Class = "doc-section" },
    [
        H("div", new VueObject { Class = "section-anchor" }, id),
        H("h2", title),
        H("div", new VueObject { Class = "section-body" }, content)
    ]);
""")
            ]),
            PageSection("production-rules", "Production rules for H authoring",
            [
                H("ul",
                [
                    H("li", "Route and metadata shape come first; visual polish sits on top of a stable shell."),
                    H("li", "Prefer semantic HTML nodes and typed props over stringly-typed DOM manipulation."),
                    H("li", "Keep helper methods focused on one visual concept so the page source stays readable."),
                    H("li", "If a page needs richer interaction later, add it intentionally rather than hiding it inside layout helpers.")
                ])
            ]),
            PageSection("why-this-works", "Why this works for a real project",
            [
                H("p", "The shell is where H functions deliver the most value: route-aware layout, reusable structure, consistent page chrome, and type-checked authoring inside the same codebase as the rest of the product."),
                Callout("Service over purity", "The site optimizes for usability first: H owns the shell because that is the part users and maintainers need to stay consistent.")
            ])
        ]);

    private static IVNode DeploymentBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("build-output", "Build output",
            [
                H("p", "Wiki still emits static ESM modules into `wwwroot/jazor`, but those modules now back a product-facing documentation shell instead of a sample-only landing page."),
                CodeBlock("Key artifacts", """
src/Wiki/wwwroot/jazor/main.mjs
src/Wiki/wwwroot/jazor/components/wiki-home.mjs
src/Wiki/wwwroot/jazor/jazor-manifest.json
""")
            ]),
            PageSection("route-fallback", "Route fallback",
            [
                H("p", "The host maps unknown document paths back to `index.html`. That makes routes like `/guides/getting-started` refresh-safe while keeping the hosting model static and simple."),
                H("ul",
                [
                    H("li", "Static assets resolve normally through `UseStaticFiles()`."),
                    H("li", "Unknown docs paths fall through to the frontend entry page."),
                    H("li", "Health remains a real backend endpoint at `/health`.")
                ])
            ]),
            PageSection("operational-checks", "Operational checks",
            [
                H("p", "The minimum release discipline for Wiki is build, route, and entry verification. This is what keeps the site from silently drifting back into sample-only quality."),
                CodeBlock("Recommended verification", """
.\src\Wiki\verify-smoke.ps1 -BuildLocal
"""),
                Callout("Dependency note", "CDN-backed Vue and Vuetify remain acceptable for MVP, but productization should decide whether to lock, mirror, or localize those assets.")
            ])
        ]);
}
