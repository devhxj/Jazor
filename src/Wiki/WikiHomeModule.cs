using System.Collections.Generic;
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

[ECMAScriptModule("./components/wiki-home.mjs")]
public static partial class WikiHomeModule
{
    private const string OverviewPath = "/";
    private const string GettingStartedPath = "/guides/getting-started";
    private const string ContentModelPath = "/guides/content-model";
    private const string NavigationDiscoveryPath = "/guides/navigation-discovery";
    private const string InformationArchitecturePath = "/guides/information-architecture";
    private const string HFunctionAuthoringPath = "/engineering/h-function-authoring";
    private const string CompilerBoundaryPath = "/engineering/compiler-support-boundary";
    private const string RouteCatalogContractPath = "/engineering/route-catalog-contract";
    private const string HostSemanticSeamsPath = "/engineering/host-semantic-seams";
    private const string ImportEmitContractPath = "/engineering/import-emit-contract";
    private const string RuntimeCatalogPath = "/engineering/runtime-catalog";
    private const string ContentGovernancePath = "/operations/content-governance";
    private const string DeploymentPath = "/operations/deployment";
    private const string TestingVerificationPath = "/operations/testing-verification";
    private static IVueRef<string>? CurrentPathRef;
    private static IVueRef<string>? CurrentHashRef;
    private static IVueRef<string>? CopiedSectionRef;
    private static IVueRef<string>? PermalinkReadySectionRef;
    private static IVueRef<string>? NavFilterRef;
    private static int PermalinkFeedbackResetTimerId;

    public static IVueComponent Component
        => DefineComponent(new VueComponentOptions
        {
            Name = "WikiHome",
            Setup = Setup
        });

    private static VueRenderCallback Setup()
    {
        var requestedPath = OverviewPath;
        var requestedHash = "";
        var location = ECMAScript.Global.Document.Location;
        if (location != null)
        {
            requestedPath = NormalizePath(location.Pathname);
            requestedHash = NormalizeHash(location.Hash);

            var requestedUrl = BuildUrl(requestedPath, requestedHash);
            if (requestedPath != location.Pathname || GetHashFragment(requestedHash) != location.Hash)
                ECMAScript.Global.Window.History.ReplaceState(requestedUrl, "", requestedUrl);
        }

        var currentPath = Ref(requestedPath);
        var currentHash = Ref(requestedHash);
        var copiedSection = Ref("");
        var permalinkReadySection = Ref("");
        var navFilter = Ref("");
        CurrentPathRef = currentPath;
        CurrentHashRef = currentHash;
        CopiedSectionRef = copiedSection;
        PermalinkReadySectionRef = permalinkReadySection;
        NavFilterRef = navFilter;
        SyncDocumentState(requestedPath);
        ECMAScript.Global.Window.Onpopstate = OnPopState;
        ECMAScript.Global.Window.Onhashchange = OnHashChange;

        if (requestedHash.Length > 0)
            QueueScrollToHashAnchor(requestedHash);

        return () => Render(currentPath.Value, currentHash.Value, navFilter.Value);
    }

    private static IVNode Render(string currentPath, string currentHash, string navFilter)
    {
        var article = NotFoundArticle(currentPath);
        var toc = EmptyTocRail();

        if (IsKnownPage(currentPath))
        {
            article = DocumentColumn(currentPath);
            toc = TocRail(currentPath, currentHash);
        }

        return H("main", new VueObject { Class = "wiki-shell" },
        [
            SiteHeader(),
            H("div", new VueObject { Class = "wiki-layout" },
            [
                NavigationRail(currentPath, navFilter),
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

    private static IVNode NavigationRail(string currentPath, string navFilter)
    {
        var foundationLinks = BuildNavLinksForGroup("Foundation", currentPath, navFilter);
        var engineeringLinks = BuildNavLinksForGroup("Engineering", currentPath, navFilter);
        var operationsLinks = BuildNavLinksForGroup("Operations", currentPath, navFilter);

        var visibleCount = foundationLinks.Count + engineeringLinks.Count + operationsLinks.Count;
        var railChildren = new List<IVNode>
        {
            H("div", new VueObject { Class = "rail-card nav-search-card" },
            [
                H("p", new VueObject { Class = "rail-kicker" }, "Find a page"),
                H("p", new VueObject { Class = "rail-copy" }, "Filter routes, titles, summaries, status, and group labels without leaving the current page."),
                H("div", new VueObject { Class = "nav-search-row" },
                [
                    H("input", new VueObject
                    {
                        Class = "nav-search-input",
                        Type = "search",
                        Placeholder = "Search docs pages",
                        AutoComplete = "off",
                        Value = navFilter,
                        Events = CreateNavFilterInputEvents()
                    }),
                    H("button", new VueObject
                    {
                        Class = "nav-search-clear",
                        Type = "button",
                        Disabled = navFilter.Length == 0,
                        Events = CreateClearNavFilterEvents()
                    }, "Clear")
                ]),
                H("p", new VueObject { Class = "nav-search-status" }, GetNavFilterStatus(navFilter, visibleCount))
            ]),
            H("div", new VueObject { Class = "rail-card" },
            [
                H("p", new VueObject { Class = "rail-kicker" }, "Product map"),
                H("p", new VueObject { Class = "rail-copy" }, "Wiki is now the product-facing docs shell for Jazor. Routes, structure, deployment flow, and page discovery are treated as user-facing contracts.")
            ])
        };

        if (foundationLinks.Count > 0)
            railChildren.Add(NavGroup("Foundation", foundationLinks.ToArray()));

        if (engineeringLinks.Count > 0)
            railChildren.Add(NavGroup("Engineering", engineeringLinks.ToArray()));

        if (operationsLinks.Count > 0)
            railChildren.Add(NavGroup("Operations", operationsLinks.ToArray()));

        if (visibleCount == 0)
        {
            railChildren.Add(H("div", new VueObject { Class = "rail-card nav-search-empty" },
            [
                H("p", new VueObject { Class = "nav-search-empty-title" }, "No pages match the current filter."),
                H("p", new VueObject { Class = "nav-search-empty-summary" }, "Search by route fragment, product group, page title, status, or summary copy.")
            ]));
        }

        return H("aside", new VueObject { Class = "nav-rail" }, railChildren.ToArray());
    }

    private static List<IVNode> BuildNavLinksForGroup(string group, string currentPath, string navFilter)
    {
        var links = new List<IVNode>();
        for (var pageIndex = 0; pageIndex < TotalPageCount; pageIndex++)
        {
            var path = GetPagePath(pageIndex);
            if (GetPageGroup(path) != group || !MatchesPageFilter(path, navFilter))
                continue;

            links.Add(NavLink(path, GetPageTitle(path), GetPageSummary(path), currentPath));
        }

        return links;
    }

    private static string GetNavFilterStatus(string navFilter, int visibleCount)
    {
        if (navFilter.Length == 0)
            return "Showing all " + TotalPageCount + " registered docs pages.";

        if (visibleCount == 1)
            return "1 page matches \"" + navFilter + "\".";

        return visibleCount + " pages match \"" + navFilter + "\".";
    }

    private static IVNode DocumentColumn(string currentPath)
        => H("article", new VueObject { Class = "doc-column" },
        [
            DocumentHero(currentPath),
            DocumentBody(currentPath),
            RelatedPagesPanel(currentPath),
            PagePager(currentPath)
        ]);

    private static IVNode DocumentHero(string currentPath)
        => H("header", new VueObject { Class = "doc-hero" },
        [
            H("div", new VueObject { Class = "hero-meta-row" },
            [
                H("span", new VueObject { Class = "hero-group" }, GetPageGroup(currentPath)),
                H("span", new VueObject { Class = "hero-status" }, GetPageStatus(currentPath)),
                H("code", new VueObject { Class = "hero-route" }, currentPath)
            ]),
            H("h1", new VueObject { Class = "doc-title" }, GetPageTitle(currentPath)),
            H("p", new VueObject { Class = "doc-summary" }, GetPageSummary(currentPath))
        ]);

    private static IVNode DocumentBody(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        return GetPageBody(pageIndex);
    }

    private static IVNode RelatedPagesPanel(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex < 0)
            return H("div", "");

        var relatedPaths = GetPageRelatedPaths(pageIndex);
        return H("section", new VueObject { Class = "doc-section related-pages-panel" },
        [
            H("div", new VueObject { Class = "section-title-row" },
            [
                H("h2", "Related pages")
            ]),
            H("div", new VueObject { Class = "section-body" },
            [
                H("p", "Use the central page catalog to keep adjacent concepts connected. These links are curated alongside route metadata, not discovered through brittle heuristics."),
                RouteCardGrid(relatedPaths)
            ])
        ]);
    }

    private static IVNode NotFoundArticle(string currentPath)
    {
        var suggestedPaths = GetSuggestedPaths(currentPath);
        return H("article", new VueObject { Class = "doc-column" },
        [
            H("header", new VueObject { Class = "doc-hero" },
            [
                H("div", new VueObject { Class = "hero-meta-row" },
                [
                    H("span", new VueObject { Class = "hero-group" }, "Routing"),
                    H("span", new VueObject { Class = "hero-status" }, "Not Found"),
                    H("code", new VueObject { Class = "hero-route" }, currentPath)
                ]),
                H("h1", new VueObject { Class = "doc-title" }, "Page Not Found"),
                H("p", new VueObject { Class = "doc-summary" }, "The current path is not registered in the Wiki page catalog. Route fallback is working, but this URL is outside the current docs map.")
            ]),
            H("div", new VueObject { Class = "doc-body" },
            [
                PageSection("requested-route", "Requested route",
                [
                    H("p", "The frontend shell loaded successfully. What is missing is a registered page contract for this path."),
                    CodeBlock("Requested path", currentPath)
                ]),
                PageSection("suggested-routes", "Suggested routes",
                [
                    H("p", "Start from one of the closest registered pages below, or return to the overview and navigate from the catalog."),
                    RouteCardGrid(suggestedPaths)
                ]),
                PageSection("recover", "Recover",
                [
                    H("ul",
                    [
                        H("li", "Return to the overview page and re-enter from the left navigation."),
                        H("li", "If this route should exist, add it to the central page catalog and body branch map."),
                        H("li", "Rerun `verify-smoke.ps1` after registering the route.")
                    ]),
                    H("p",
                    [
                        H("a", new VueObject
                        {
                            Class = "pager-link pager-link-single",
                            Href = OverviewPath,
                            Events = CreateRouteClickEvents()
                        }, "Open the overview page")
                    ])
                ])
            ])
        ]);
    }

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
            H("p", "jazor.wiki now runs as a real docs shell: H-function authored, statically emitted, route-fallback ready, and backed by a central page catalog."),
            H("p", "Health endpoint: /health | Registered docs pages: " + TotalPageCount)
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

    private static string NormalizeHash(string hash)
    {
        if (hash.Length == 0)
            return "";

        if (hash.StartsWith("#"))
            return hash.Substring(1);

        return hash;
    }

    private static string GetHashFragment(string hash)
    {
        if (hash.Length == 0)
            return "";

        return "#" + hash;
    }

    private static string BuildUrl(string path, string hash)
        => path + GetHashFragment(hash);

    private static void SyncDocumentState(string currentPath)
    {
        if (IsKnownPage(currentPath))
            ECMAScript.Global.Document.Title = GetPageTitle(currentPath) + " | jazor.wiki";
        else
            ECMAScript.Global.Document.Title = "Page Not Found | jazor.wiki";
    }

    private static IVueRef<string>? GetCurrentPathRef()
        => CurrentPathRef;

    private static IVueRef<string>? GetCurrentHashRef()
        => CurrentHashRef;

    private static IVueRef<string>? GetCopiedSectionRef()
        => CopiedSectionRef;

    private static IVueRef<string>? GetPermalinkReadySectionRef()
        => PermalinkReadySectionRef;

    private static IVueRef<string>? GetNavFilterRef()
        => NavFilterRef;

    private static void SetNavFilter(string value)
    {
        var navFilter = GetNavFilterRef();
        if (navFilter == null)
            return;

        navFilter.Value = value.Trim();
    }

    private static void SetCopiedSection(string value)
    {
        var copiedSection = GetCopiedSectionRef();
        if (copiedSection == null)
            return;

        copiedSection.Value = value;
    }

    private static void SetPermalinkReadySection(string value)
    {
        var permalinkReadySection = GetPermalinkReadySectionRef();
        if (permalinkReadySection == null)
            return;

        permalinkReadySection.Value = value;
    }

    private static void ShowCopiedSection(string sectionId)
    {
        SetPermalinkReadySection("");
        SetCopiedSection(sectionId);
        QueuePermalinkFeedbackReset();
    }

    private static void ShowPermalinkReady(string sectionId)
    {
        SetCopiedSection("");
        SetPermalinkReadySection(sectionId);
        QueuePermalinkFeedbackReset();
    }

    private static void QueuePermalinkFeedbackReset()
    {
        if (PermalinkFeedbackResetTimerId != 0)
            ECMAScript.Global.Window.ClearTimeout(PermalinkFeedbackResetTimerId);

        PermalinkFeedbackResetTimerId = ECMAScript.Global.Window.SetTimeout((Delegate)(Action)ResetPermalinkFeedback, 1800);
    }

    private static void ResetPermalinkFeedback()
    {
        SetCopiedSection("");
        SetPermalinkReadySection("");
        PermalinkFeedbackResetTimerId = 0;
    }

    private static void QueueScrollToHashAnchor(string hash)
    {
        if (hash.Length == 0)
            return;

        Vue3.NextTick(() => ScrollToHashAnchor(hash));
    }

    private static void ScrollToHashAnchor(string hash)
    {
        if (ECMAScript.Global.Document.GetElementById(hash) is not Element sectionElement)
            return;

        sectionElement.ScrollIntoView(true);
    }

    private static void NavigateTo(string path, string hash, bool updateHistory, bool resetScroll)
    {
        var currentPath = GetCurrentPathRef();
        var currentHash = GetCurrentHashRef();
        if (currentPath == null || currentHash == null)
            return;

        var normalizedPath = NormalizePath(path);
        var normalizedHash = NormalizeHash(hash);

        if (currentPath.Value == normalizedPath && currentHash.Value == normalizedHash)
        {
            if (normalizedHash.Length > 0)
                QueueScrollToHashAnchor(normalizedHash);
            else if (resetScroll)
                ECMAScript.Global.Window.ScrollTo(0, 0);

            return;
        }

        currentPath.Value = normalizedPath;
        currentHash.Value = normalizedHash;
        if (updateHistory)
        {
            var url = BuildUrl(normalizedPath, normalizedHash);
            ECMAScript.Global.Window.History.PushState(url, "", url);
        }

        SyncDocumentState(normalizedPath);
        if (normalizedHash.Length > 0)
            QueueScrollToHashAnchor(normalizedHash);
        else if (resetScroll)
            ECMAScript.Global.Window.ScrollTo(0, 0);
    }

    private static void SyncLocationStateFromBrowser()
    {
        var location = ECMAScript.Global.Document.Location;
        var currentPath = GetCurrentPathRef();
        var currentHash = GetCurrentHashRef();
        if (location == null || currentPath == null || currentHash == null)
            return;

        var normalizedPath = NormalizePath(location.Pathname);
        var normalizedHash = NormalizeHash(location.Hash);
        currentPath.Value = normalizedPath;
        currentHash.Value = normalizedHash;
        SyncDocumentState(normalizedPath);

        if (normalizedHash.Length > 0)
            QueueScrollToHashAnchor(normalizedHash);
    }

    private static bool ShouldAllowBrowserDefault(MouseEvent mouseEvent)
        => mouseEvent.Button != 0 ||
           mouseEvent.CtrlKey ||
           mouseEvent.MetaKey ||
           mouseEvent.ShiftKey ||
           mouseEvent.AltKey;

    private static void OnRouteClick(MouseEvent mouseEvent)
    {
        if (ShouldAllowBrowserDefault(mouseEvent))
            return;

        if (mouseEvent.CurrentTarget is not HTMLAnchorElement anchor)
            return;

        mouseEvent.PreventDefault();
        NavigateTo(anchor.Pathname, "", updateHistory: true, resetScroll: true);
    }

    private static void OnTocClick(MouseEvent mouseEvent)
    {
        if (ShouldAllowBrowserDefault(mouseEvent))
            return;

        if (mouseEvent.CurrentTarget is not HTMLAnchorElement anchor)
            return;

        mouseEvent.PreventDefault();
        NavigateTo(anchor.Pathname, anchor.Hash, updateHistory: true, resetScroll: true);
    }

    private static void OnSectionPermalinkClick(MouseEvent mouseEvent)
    {
        if (mouseEvent.CurrentTarget is not HTMLButtonElement buttonElement)
            return;

        mouseEvent.PreventDefault();

        var currentPath = GetCurrentPathRef();
        if (currentPath == null)
            return;

        var sectionId = NormalizeHash(buttonElement.Value);
        if (sectionId.Length == 0)
            return;

        ResetPermalinkFeedback();
        NavigateTo(currentPath.Value, sectionId, updateHistory: true, resetScroll: true);

        var location = ECMAScript.Global.Document.Location;
        var sectionUrl = BuildUrl(currentPath.Value, sectionId);
        var sectionShareUrl = sectionUrl;
        if (location != null)
            sectionShareUrl = location.Origin + sectionUrl;

        try
        {
            var clipboard = ECMAScript.Global.Window.Navigator.Clipboard;
            if (clipboard == null)
            {
                ShowPermalinkReady(sectionId);
                return;
            }

            Promise.Resolve(clipboard.WriteText(sectionShareUrl)).Then(
                () => ShowCopiedSection(sectionId),
                () => ShowPermalinkReady(sectionId));
        }
        catch
        {
            ShowPermalinkReady(sectionId);
        }
    }

    private static void OnNavFilterInput(Event @event)
    {
        if (@event.CurrentTarget is not HTMLInputElement inputElement)
            return;

        SetNavFilter(inputElement.Value);
    }

    private static void ClearNavFilter(MouseEvent mouseEvent)
    {
        mouseEvent.PreventDefault();
        SetNavFilter("");
    }

    private static object OnHashChange(Event @event)
    {
        SyncLocationStateFromBrowser();
        return 0;
    }

    private static object OnPopState(Event @event)
    {
        SyncLocationStateFromBrowser();
        return 0;
    }
}
