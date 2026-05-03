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
                    H("li", "Navigation, page discovery, route entry, and deployment guidance are treated as product contracts."),
                    H("li", "The content model stays explicit so maintainers can evolve it without a hidden pipeline.")
                ])
            ]),
            PageSection("mvp-boundary", "MVP boundary",
            [
                Callout("Included now", "Real routes, multi-page docs, left navigation with local page filtering, right-side table of contents, and previous/next page flow."),
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
  WikiHomeModule.Elements.cs
  WikiHomeModule.Overview.cs
  WikiHomeModule.GettingStarted.cs
  WikiHomeModule.ContentModel.cs
  WikiHomeModule.HFunctionAuthoring.cs
  WikiHomeModule.Deployment.cs
  wwwroot/index.html
  wwwroot/site.css
  verify-smoke.ps1
""")
            ])
        ]);
}
