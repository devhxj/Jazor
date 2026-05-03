using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

[ECMAScriptModule("./components/wiki-home.mjs")]
public static partial class WikiHomeModule
{
    private const string OverviewPath = "/";
    private const string GettingStartedPath = "/guides/getting-started";
    private const string ContentModelPath = "/guides/content-model";
    private const string HFunctionAuthoringPath = "/engineering/h-function-authoring";
    private const string DeploymentPath = "/operations/deployment";

    public static readonly IVueComponent Component = DefineComponent(new VueComponentOptions
    {
        Name = "WikiHome",
        Setup = Setup
    });

    private static VueRenderCallback Setup()
    {
        var requestedPath = OverviewPath;
        var location = ECMAScript.Global.Document.Location;
        if (location != null)
            requestedPath = NormalizePath(location.Pathname);

        if (IsKnownPage(requestedPath))
            ECMAScript.Global.Document.Title = GetPageTitle(requestedPath) + " | jazor.wiki";
        else
            ECMAScript.Global.Document.Title = "Page Not Found | jazor.wiki";

        return () => Render(requestedPath);
    }

    private static IVNode Render(string currentPath)
    {
        var article = NotFoundArticle(currentPath);
        var toc = EmptyTocRail();

        if (IsKnownPage(currentPath))
        {
            article = DocumentColumn(currentPath);
            toc = TocRail(currentPath);
        }

        return H("main", new VueObject { Class = "wiki-shell" },
        [
            SiteHeader(),
            H("div", new VueObject { Class = "wiki-layout" },
            [
                NavigationRail(currentPath),
                article,
                toc
            ]),
            SiteFooter()
        ]);
    }

    private static IVNode SiteHeader()
        => H("header", new VueObject { Class = "site-header" },
        [
            H("div", new VueObject { Class = "site-header-inner" },
            [
                H("div", new VueObject { Class = "site-brand" },
                [
                    H("p", new VueObject { Class = "brand-kicker" }, "jazor.wiki"),
                    H("h1", new VueObject { Class = "brand-title" }, "Production Docs Built with Vue 3 H Functions"),
                    H("p", new VueObject { Class = "brand-summary" }, "A real documentation shell for Jazor, with H-function-authored layout and product-facing routes.")
                ]),
                H("div", new VueObject { Class = "brand-actions" },
                [
                    HeaderLink(GettingStartedPath, "Get Started"),
                    HeaderLink(DeploymentPath, "Deploy It")
                ])
            ])
        ]);

    private static IVNode NavigationRail(string currentPath)
        => H("aside", new VueObject { Class = "nav-rail" },
        [
            H("div", new VueObject { Class = "rail-card" },
            [
                H("p", new VueObject { Class = "rail-kicker" }, "Product map"),
                H("p", new VueObject { Class = "rail-copy" }, "Wiki is now the product-facing docs shell for Jazor. Routes, structure, and deployment flow are treated as user-facing contracts.")
            ]),
            NavGroup("Foundation",
            [
                NavLink(OverviewPath, "Overview", "What ships now, why the site exists, and the MVP boundary.", currentPath),
                NavLink(GettingStartedPath, "Getting Started", "Run the site locally, understand routes, and verify the emitted host.", currentPath),
                NavLink(ContentModelPath, "Content Model", "How pages, anchors, and navigation stay maintainable in a code-first docs site.", currentPath)
            ]),
            NavGroup("Engineering",
            [
                NavLink(HFunctionAuthoringPath, "H-Function Authoring", "Why H functions own the shell and what rules keep the authoring path stable.", currentPath)
            ]),
            NavGroup("Operations",
            [
                NavLink(DeploymentPath, "Deployment", "Build outputs, route fallback, and the smoke-verification contract.", currentPath)
            ])
        ]);

    private static IVNode DocumentColumn(string currentPath)
        => H("article", new VueObject { Class = "doc-column" },
        [
            DocumentHero(currentPath),
            DocumentBody(currentPath),
            PagePager(currentPath)
        ]);

    private static IVNode DocumentHero(string currentPath)
        => H("header", new VueObject { Class = "doc-hero" },
        [
            H("div", new VueObject { Class = "hero-meta-row" },
            [
                H("span", new VueObject { Class = "hero-group" }, GetPageGroup(currentPath)),
                H("span", new VueObject { Class = "hero-status" }, GetPageStatus(currentPath))
            ]),
            H("h1", new VueObject { Class = "doc-title" }, GetPageTitle(currentPath)),
            H("p", new VueObject { Class = "doc-summary" }, GetPageSummary(currentPath))
        ]);

    private static IVNode DocumentBody(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex == 0)
            return OverviewBody();

        if (pageIndex == 1)
            return GettingStartedBody();

        if (pageIndex == 2)
            return ContentModelBody();

        if (pageIndex == 3)
            return HFunctionAuthoringBody();

        return DeploymentBody();
    }

    private static IVNode NotFoundArticle(string currentPath)
        => H("article", new VueObject { Class = "doc-column" },
        [
            H("header", new VueObject { Class = "doc-hero" },
            [
                H("div", new VueObject { Class = "hero-meta-row" },
                [
                    H("span", new VueObject { Class = "hero-group" }, "Routing"),
                    H("span", new VueObject { Class = "hero-status" }, "Not Found")
                ]),
                H("h1", new VueObject { Class = "doc-title" }, "Page Not Found"),
                H("p", new VueObject { Class = "doc-summary" }, "The current path is not registered in the Wiki route map. Use the navigation rail or return to the overview.")
            ]),
            H("div", new VueObject { Class = "doc-body" },
            [
                PageSection("requested-route", "Requested route",
                [
                    H("p", "Wiki route fallback is working, but this specific path does not map to a registered page."),
                    CodeBlock("Requested path", currentPath)
                ]),
                PageSection("recover", "Recover",
                [
                    H("ul",
                    [
                        H("li", "Return to the overview page and re-enter from the left navigation."),
                        H("li", "If this route should exist, add it to the route constants and body branch map."),
                        H("li", "Rerun `verify-smoke.ps1` after registering the route.")
                    ]),
                    H("p",
                    [
                        H("a", new VueObject
                        {
                            Class = "pager-link pager-link-single",
                            Href = OverviewPath
                        }, "Open the overview page")
                    ])
                ])
            ])
        ]);

    private static IVNode PagePager(string currentPath)
    {
        var previousNode = EmptyPagerSlot();
        var nextNode = EmptyPagerSlot();
        var previousPath = GetPreviousPath(currentPath);
        var nextPath = GetNextPath(currentPath);

        if (previousPath.Length > 0)
            previousNode = PagerLink("Previous", previousPath, GetPageTitle(previousPath));

        if (nextPath.Length > 0)
            nextNode = PagerLink("Next", nextPath, GetPageTitle(nextPath));

        return H("nav", new VueObject { Class = "pager" },
        [
            previousNode,
            nextNode
        ]);
    }

    private static IVNode SiteFooter()
        => H("footer", new VueObject { Class = "site-footer" },
        [
            H("p", "jazor.wiki now runs as a real docs shell: H-function authored, statically emitted, and route-fallback ready."),
            H("p", "Health endpoint: /health | Primary routes: /, /guides/getting-started, /engineering/h-function-authoring, /operations/deployment")
        ]);

    private static string NormalizePath(string pathname)
    {
        if (pathname.Length == 0)
            return OverviewPath;

        var normalized = pathname;
        if (normalized == "/index.html")
            normalized = OverviewPath;
        else if (normalized.EndsWith("/index.html"))
            normalized = normalized.Substring(0, normalized.Length - "/index.html".Length);

        if (normalized.Length > 1 && normalized.EndsWith("/"))
            normalized = normalized.Substring(0, normalized.Length - 1);

        if (normalized.Length == 0)
            return OverviewPath;

        return normalized;
    }
}
