using System.Collections.Generic;
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

[ECMAScriptModule("./components/wiki-home.mjs")]
public static partial class WikiHomeModule
{
    private const string NavSearchInputId = "wiki-nav-search-input";
    private const string SearchInputId = "wiki-search-input";
    private const string NavRailId = "wiki-nav-rail";
    private const string TocRailId = "wiki-toc-rail";
    private const string MainContentId = "wiki-main-content";
    private const double SectionActivationLine = 148;
    private const string ThemeStorageKey = "jazor.wiki.theme";
    private const string FeedbackStoragePrefix = "jazor.wiki.feedback:";
    private const string RepositoryRootUrl = "https://github.com/devhxj/Jazor";
    private const string RepositoryBlobBaseUrl = RepositoryRootUrl + "/blob/main/";
    private const string RepositoryIssueBaseUrl = RepositoryRootUrl + "/issues/new?title=";

    private const string OverviewPath = "/";
    private const string SearchPath = "/search";
    private const string GettingStartedPath = "/guides/getting-started";
    private const string ProjectLinesPath = "/guides/project-lines";
    private const string ContentModelPath = "/guides/content-model";
    private const string NavigationDiscoveryPath = "/guides/navigation-discovery";
    private const string InformationArchitecturePath = "/guides/information-architecture";
    private const string TopicIndexPath = "/guides/topic-index";
    private const string GlossaryPath = "/guides/glossary";
    private const string FaqPath = "/guides/faq";
    private const string TroubleshootingPath = "/guides/troubleshooting";
    private const string HFunctionAuthoringPath = "/engineering/h-function-authoring";
    private const string CompilerOverviewPath = "/engineering/compiler-overview";
    private const string CompilerBoundaryPath = "/engineering/compiler-support-boundary";
    private const string RouteCatalogContractPath = "/engineering/route-catalog-contract";
    private const string HostSemanticSeamsPath = "/engineering/host-semantic-seams";
    private const string ImportEmitContractPath = "/engineering/import-emit-contract";
    private const string RuntimeCatalogPath = "/engineering/runtime-catalog";
    private const string JoltHostPath = "/engineering/jolt-host";
    private const string RazorVueLibraryModePath = "/engineering/razorvue-library-mode";
    private const string VueRouteBindingsPath = "/engineering/vueroute-bindings";
    private const string ContentGovernancePath = "/operations/content-governance";
    private const string DeploymentPath = "/operations/deployment";
    private const string TestingVerificationPath = "/operations/testing-verification";

    private static IVueRef<string>? CurrentPathRef;
    private static IVueRef<string>? CurrentHashRef;
    private static IVueRef<string>? CurrentSearchQueryRef;
    private static IVueRef<string>? CurrentThemeRef;
    private static IVueRef<string>? CopiedSectionRef;
    private static IVueRef<string>? PermalinkReadySectionRef;
    private static IVueRef<string>? CopiedPageRef;
    private static IVueRef<string>? PageLinkReadyRef;
    private static IVueRef<string>? CopiedCodeBlockRef;
    private static IVueRef<string>? UnavailableCodeBlockRef;
    private static IVueRef<string>? NavFilterRef;
    private static IVueRef<string>? CurrentPageFeedbackRef;
    private static IVueRef<string>? LiveStatusRef;
    private static IVueRef<int>? ReadingProgressPercentRef;
    private static IVueRef<bool>? NavDrawerOpenRef;
    private static IVueRef<bool>? TocDrawerOpenRef;

    private static readonly List<string> StoredScrollRouteKeys = [];
    private static readonly List<double> StoredScrollOffsets = [];

    private static int PermalinkFeedbackResetTimerId;
    private static int PageLinkFeedbackResetTimerId;
    private static int CodeBlockFeedbackResetTimerId;
    private static bool ActiveSectionSyncQueued;

    public static ECMAScript.Vue3.IVueComponent Component
        => DefineComponent(new VueComponentOptions
        {
            Name = "WikiHome",
            Setup = Setup
        });

    private static VueRenderCallback Setup()
    {
        var requestedPath = OverviewPath;
        var requestedHash = "";
        var requestedSearchQuery = "";
        var location = ECMAScript.Global.Document.Location;
        if (location != null)
        {
            requestedPath = NormalizePath(location.Pathname);
            requestedHash = NormalizeHash(location.Hash);
            requestedSearchQuery = GetSearchQueryFromLocation(location, requestedPath);

            var requestedUrl = BuildUrl(requestedPath, requestedHash, requestedSearchQuery);
            if (requestedPath != location.Pathname ||
                GetHashFragment(requestedHash) != location.Hash ||
                GetSearchFragment(requestedPath, requestedSearchQuery) != location.Search)
            {
                ECMAScript.Global.Window.History.ReplaceState(requestedUrl, "", requestedUrl);
            }
        }

        var currentPath = Ref(requestedPath);
        var currentHash = Ref(requestedHash);
        var currentSearchQuery = Ref(requestedSearchQuery);
        var currentTheme = Ref(ReadStoredPreference(ThemeStorageKey, "dark"));
        var copiedSection = Ref("");
        var permalinkReadySection = Ref("");
        var copiedPage = Ref("");
        var pageLinkReady = Ref("");
        var copiedCodeBlock = Ref("");
        var unavailableCodeBlock = Ref("");
        var navFilter = Ref("");
        var currentPageFeedback = Ref(ReadStoredPageFeedback(requestedPath));
        var liveStatus = Ref("");
        var readingProgressPercent = Ref(0);
        var navDrawerOpen = Ref(false);
        var tocDrawerOpen = Ref(false);

        CurrentPathRef = currentPath;
        CurrentHashRef = currentHash;
        CurrentSearchQueryRef = currentSearchQuery;
        CurrentThemeRef = currentTheme;
        CopiedSectionRef = copiedSection;
        PermalinkReadySectionRef = permalinkReadySection;
        CopiedPageRef = copiedPage;
        PageLinkReadyRef = pageLinkReady;
        CopiedCodeBlockRef = copiedCodeBlock;
        UnavailableCodeBlockRef = unavailableCodeBlock;
        NavFilterRef = navFilter;
        CurrentPageFeedbackRef = currentPageFeedback;
        LiveStatusRef = liveStatus;
        ReadingProgressPercentRef = readingProgressPercent;
        NavDrawerOpenRef = navDrawerOpen;
        TocDrawerOpenRef = tocDrawerOpen;

        ApplyTheme(currentTheme.Value);
        SyncDocumentState(requestedPath, requestedSearchQuery);
        ECMAScript.Global.Window.Onpopstate = OnPopState;
        ECMAScript.Global.Window.Onhashchange = OnHashChange;
        ECMAScript.Global.Window.Onkeydown = OnGlobalKeyDown;
        ECMAScript.Global.Window.Onscroll = OnScroll;

        if (requestedHash.Length > 0)
            QueueScrollToHashAnchor(requestedHash);
        else
            QueueActiveSectionSync();

        return () => Render(currentPath.Value, currentHash.Value, navFilter.Value, currentSearchQuery.Value);
    }

    private static IVNode Render(string currentPath, string currentHash, string navFilter, string currentSearchQuery)
    {
        var article = NotFoundArticle(currentPath);
        var toc = EmptyTocRail();

        if (IsKnownPage(currentPath))
        {
            article = DocumentColumn(currentPath);
            toc = TocRail(currentPath, currentHash);
        }

        return H("main", new VueObject
        {
            Class = GetShellClassName(),
            Id = "top"
        },
        [
            H("a", new VueObject
            {
                Class = "skip-link",
                Href = "#" + MainContentId
            }, "Skip to content"),
            H("p", new VueObject
            {
                Class = "sr-only",
                Attrs = new VueDictionary
                {
                    ["aria-live"] = "polite",
                    ["aria-atomic"] = "true"
                }
            }, GetLiveStatusRef()?.Value ?? ""),
            SiteHeader(currentPath),
            MobileUtilityBar(currentPath),
            DrawerBackdrop(),
            H("div", new VueObject { Class = "wiki-layout" },
            [
                NavigationRail(currentPath, navFilter),
                article,
                toc
            ]),
            SiteFooter(currentSearchQuery)
        ]);
    }

    private static string GetShellClassName()
    {
        var className = "wiki-shell";
        if (IsNavDrawerOpen())
            className += " wiki-shell-nav-open";
        if (IsTocDrawerOpen())
            className += " wiki-shell-toc-open";

        return className;
    }

    private static bool IsNavDrawerOpen()
        => GetNavDrawerOpenRef()?.Value == true;

    private static bool IsTocDrawerOpen()
        => GetTocDrawerOpenRef()?.Value == true;

    private static IVNode DrawerBackdrop()
    {
        var className = "drawer-backdrop";
        if (IsNavDrawerOpen() || IsTocDrawerOpen())
            className += " drawer-backdrop-open";

        return H("button", new VueObject
        {
            Class = className,
            Type = "button",
            Title = "Close navigation panels",
            Events = CreateCloseDrawersEvents(),
            Raw = new VueDictionary
            {
                ["aria-hidden"] = (IsNavDrawerOpen() || IsTocDrawerOpen()) ? "false" : "true"
            }
        }, "");
    }

    private static IVNode SiteHeader(string currentPath)
    {
        var theme = GetCurrentThemeRef()?.Value ?? "dark";
        var themeLabel = theme == "light" ? "Theme: Light" : "Theme: Dark";
        var themeTitle = theme == "light" ? "Switch to dark theme" : "Switch to light theme";

        return H("header", new VueObject { Class = "site-header" },
        [
            H("div", new VueObject { Class = "site-header-inner" },
            [
                H("div", new VueObject { Class = "site-brand" },
                [
                    H("p", new VueObject { Class = "brand-kicker" }, "jazor.wiki"),
                    H("h1", new VueObject { Class = "brand-title" }, "Production Docs Built with Vue 3 H Functions"),
                    H("p", new VueObject { Class = "brand-summary" }, "A real documentation shell for Jazor, now with route-driven search, metadata-rich pages, and production-grade reading flow.")
                ]),
                H("div", new VueObject { Class = "brand-actions-panel" },
                [
                    H("div", new VueObject { Class = "brand-actions" },
                    [
                        HeaderLink(SearchPath, "Search"),
                        HeaderLink(GettingStartedPath, "Get Started"),
                        HeaderLink(TopicIndexPath, "Topic Index")
                    ]),
                    H("div", new VueObject { Class = "brand-toggles" },
                    [
                        H("button", new VueObject
                        {
                            Class = "header-toggle",
                            Type = "button",
                            Title = themeTitle,
                            Events = CreateThemeToggleEvents()
                        }, themeLabel)
                    ])
                ])
            ]),
            ReadingProgressStrip(currentPath)
        ]);
    }

    private static IVNode ReadingProgressStrip(string currentPath)
    {
        var progressPercent = GetReadingProgressPercent();
        return H("div", new VueObject { Class = "reading-progress-shell" },
        [
            H("div", new VueObject { Class = "reading-progress-row" },
            [
                H("span", new VueObject { Class = "reading-progress-title" }, GetReadingProgressTitle(currentPath)),
                H("span", new VueObject { Class = "reading-progress-value" }, progressPercent + "%")
            ]),
            H("div", new VueObject
            {
                Class = "reading-progress-track",
                Role = "progressbar",
                Raw = new VueDictionary
                {
                    ["aria-label"] = "Reading progress",
                    ["aria-valuemin"] = "0",
                    ["aria-valuemax"] = "100",
                    ["aria-valuenow"] = progressPercent + ""
                }
            },
            [
                H("span", new VueObject
                {
                    Class = "reading-progress-bar",
                    Style = new VueDictionary
                    {
                        ["width"] = progressPercent + "%"
                    }
                }, "")
            ])
        ]);
    }

    private static string GetReadingProgressTitle(string currentPath)
    {
        if (currentPath == SearchPath)
        {
            var currentSearchQuery = GetCurrentSearchQueryRef()?.Value ?? "";
            return currentSearchQuery.Length == 0 ? "Search view" : "Search results";
        }

        if (IsKnownPage(currentPath))
            return GetPageTitle(currentPath);

        return "Page shell";
    }

    private static IVNode MobileUtilityBar(string currentPath)
        => H("div", new VueObject { Class = "mobile-utility-bar" },
        [
            DrawerButton("Browse", "utility-button", NavRailId, IsNavDrawerOpen(), false, CreateOpenNavDrawerEvents()),
            DrawerButton("On this page", "utility-button", TocRailId, IsTocDrawerOpen(), !IsKnownPage(currentPath), CreateOpenTocDrawerEvents()),
            H("a", new VueObject
            {
                Class = "utility-link",
                Href = SearchPath,
                Events = CreateRouteClickEvents()
            }, "Search")
        ]);

    private static IVNode NavigationRail(string currentPath, string navFilter)
    {
        var foundationLinks = BuildNavLinksForGroup("Foundation", currentPath, navFilter);
        var engineeringLinks = BuildNavLinksForGroup("Engineering", currentPath, navFilter);
        var operationsLinks = BuildNavLinksForGroup("Operations", currentPath, navFilter);

        var visibleCount = foundationLinks.Count + engineeringLinks.Count + operationsLinks.Count;
        var railClassName = "nav-rail";
        if (IsNavDrawerOpen())
            railClassName += " nav-rail-open";

        var railChildren = new List<IVNode>
        {
            H("div", new VueObject { Class = "rail-card nav-drawer-head" },
            [
                H("p", new VueObject { Class = "rail-kicker" }, "Browse docs"),
                H("button", new VueObject
                {
                    Class = "drawer-close",
                    Type = "button",
                    Title = "Close page map",
                    Events = CreateCloseDrawersEvents()
                }, "Close")
            ]),
            H("div", new VueObject { Class = "rail-card nav-search-card" },
            [
                H("p", new VueObject { Class = "rail-kicker" }, "Find a page"),
                H("p", new VueObject { Class = "rail-copy" }, "Filter routes, titles, summaries, tags, and status labels without leaving the current page."),
                H("div", new VueObject { Class = "nav-search-row" },
                [
                    H("input", new VueObject
                    {
                        Id = NavSearchInputId,
                        Class = "nav-search-input",
                        Type = "search",
                        Placeholder = "Search docs pages",
                        Autocomplete = "off",
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
                H("p", new VueObject { Class = "nav-search-hint" }, "Press / or Ctrl+K to focus search. Press Escape to clear or exit."),
                H("p", new VueObject { Class = "nav-search-status" }, GetNavFilterStatus(navFilter, visibleCount))
            ]),
            H("div", new VueObject { Class = "rail-card" },
            [
                H("p", new VueObject { Class = "rail-kicker" }, "Full-text search"),
                H("p", new VueObject { Class = "rail-copy" }, "Open the dedicated search route when you want query URLs, body-text matches, and section-level hits."),
                H("a", new VueObject
                {
                    Class = "route-card route-card-inline",
                    Href = BuildSearchRoute(navFilter),
                    Events = CreateRouteClickEvents()
                },
                [
                    H("span", new VueObject { Class = "route-card-group" }, "Foundation"),
                    H("strong", new VueObject { Class = "route-card-title" }, navFilter.Length == 0 ? "Open Search" : "Search all content for \"" + navFilter + "\""),
                    H("code", new VueObject { Class = "route-card-path" }, BuildSearchRoute(navFilter)),
                    H("span", new VueObject { Class = "route-card-summary" }, "Use the `/search` route to match page body text, tags, and section titles.")
                ])
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
                H("p", new VueObject { Class = "nav-search-empty-summary" }, "Try a broader term or open the dedicated search route for full-text results.")
            ]));
        }

        return H("aside", new VueObject
        {
            Id = NavRailId,
            Class = railClassName,
            Role = "navigation",
            Raw = new VueDictionary
            {
                ["aria-label"] = "Page map"
            }
        }, railChildren.ToArray());
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
        => H("article", new VueObject
        {
            Class = "doc-column",
            Id = MainContentId,
            Tabindex = -1
        },
        [
            DocumentHero(currentPath),
            PageMetaPanel(currentPath),
            DocumentBody(currentPath),
            RelatedPagesPanel(currentPath),
            PageFeedbackPanel(currentPath),
            PagePager(currentPath)
        ]);

    private static IVNode DocumentHero(string currentPath)
    {
        var pageButtonLabel = "Copy page link";
        var pageButtonClassName = "page-permalink";
        var pageButtonTitle = "Copy direct link to this page";
        if (GetCopiedPageRef()?.Value == currentPath)
        {
            pageButtonLabel = "Copied";
            pageButtonClassName = "page-permalink page-permalink-copied";
            pageButtonTitle = "Page link copied to clipboard";
        }
        else if (GetPageLinkReadyRef()?.Value == currentPath)
        {
            pageButtonLabel = "Link ready";
            pageButtonClassName = "page-permalink page-permalink-ready";
            pageButtonTitle = "Page link is ready in the address bar; clipboard copy was not available";
        }

        return H("header", new VueObject { Class = "doc-hero" },
        [
            Breadcrumbs(currentPath),
            H("div", new VueObject { Class = "hero-meta-row" },
            [
                H("span", new VueObject { Class = "hero-group" }, GetPageGroup(currentPath)),
                H("span", new VueObject { Class = "hero-status" }, GetPageStatus(currentPath)),
                H("code", new VueObject { Class = "hero-route" }, currentPath)
            ]),
            H("h1", new VueObject { Class = "doc-title" }, GetPageTitle(currentPath)),
            H("p", new VueObject { Class = "doc-summary" }, GetPageSummary(currentPath)),
            H("div", new VueObject { Class = "hero-tags-row" }, BuildTagLinks(currentPath)),
            H("div", new VueObject { Class = "hero-actions-row" },
            [
                H("button", new VueObject
                {
                    Class = pageButtonClassName,
                    Type = "button",
                    Title = pageButtonTitle,
                    Value = currentPath,
                    Events = CreatePagePermalinkEvents()
                }, pageButtonLabel),
                H("a", new VueObject
                {
                    Class = "hero-action-link",
                    Href = BuildSourceUrl(currentPath),
                    Target = "_blank",
                    Rel = "noreferrer"
                }, "View source"),
                H("a", new VueObject
                {
                    Class = "hero-action-link",
                    Href = BuildIssueUrl(currentPath),
                    Target = "_blank",
                    Rel = "noreferrer"
                }, "Report issue")
            ])
        ]);
    }

    private static IVNode[] BuildTagLinks(string currentPath)
    {
        var tags = GetPageTags(currentPath);
        var nodes = new IVNode[tags.Length];
        for (var tagIndex = 0; tagIndex < tags.Length; tagIndex++)
            nodes[tagIndex] = TagLink(tags[tagIndex]);

        return nodes;
    }

    private static IVNode Breadcrumbs(string currentPath)
    {
        var group = GetPageGroup(currentPath);
        var children = new List<IVNode>
        {
            H("a", new VueObject
            {
                Class = "breadcrumb-link",
                Href = OverviewPath,
                Events = CreateRouteClickEvents()
            }, "Home")
        };

        if (currentPath != OverviewPath)
        {
            children.Add(H("span", new VueObject { Class = "breadcrumb-separator" }, "/"));

            if (group != "Unregistered")
            {
                children.Add(H("a", new VueObject
                {
                    Class = "breadcrumb-link",
                    Href = GetGroupLandingPath(group),
                    Events = CreateRouteClickEvents()
                }, group));
                children.Add(H("span", new VueObject { Class = "breadcrumb-separator" }, "/"));
            }

            children.Add(H("span", new VueObject { Class = "breadcrumb-current" }, GetPageTitle(currentPath)));
        }

        return H("nav", new VueObject
        {
            Class = "breadcrumbs",
            Role = "navigation",
            Raw = new VueDictionary
            {
                ["aria-label"] = "Breadcrumbs"
            }
        }, children.ToArray());
    }

    private static string GetGroupLandingPath(string group)
    {
        if (group == "Foundation")
            return TopicIndexPath;

        if (group == "Engineering")
            return CompilerOverviewPath;

        if (group == "Operations")
            return DeploymentPath;

        return OverviewPath;
    }

    private static IVNode PageMetaPanel(string currentPath)
        => H("section", new VueObject { Class = "meta-grid" },
        [
            MetaCard("Owner", GetPageOwner(currentPath), "Who owns the accuracy and maintenance of this page."),
            MetaCard("Audience", GetPageAudience(currentPath), "Who should read this page first when choosing an entry point."),
            MetaCard("Updated", GetPageLastUpdated(currentPath), "Exact date of the latest catalog-backed edit on this route."),
            MetaCard("Reading time", GetReadingTimeLabel(GetPageReadingMinutes(currentPath)), "Estimated scan time based on the current page contract."),
            MetaCard("Source file", GetPageSourceFile(currentPath), "Primary source file that owns the page body content."),
            MetaCard("Status", GetPageStatus(currentPath), "Current maturity marker exposed in navigation and search.")
        ]);

    private static string GetReadingTimeLabel(int minutes)
    {
        if (minutes == 1)
            return "1 min read";

        return minutes + " min read";
    }

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

    private static IVNode PageFeedbackPanel(string currentPath)
    {
        var currentFeedback = GetCurrentPageFeedbackRef()?.Value ?? "";
        var summary = "Use this quick signal to mark the page as helpful or needing work. For concrete gaps, open a GitHub issue from the hero action row.";
        if (currentFeedback == "helpful")
            summary = "Thanks. This page is currently marked as helpful in your local browser state.";
        else if (currentFeedback == "needs-work")
            summary = "This page is currently marked as needing work in your local browser state. The report-issue link can capture the concrete gap.";

        return H("section", new VueObject { Class = "doc-section feedback-panel" },
        [
            H("div", new VueObject { Class = "section-title-row" },
            [
                H("h2", "Page feedback")
            ]),
            H("div", new VueObject { Class = "section-body" },
            [
                H("p", summary),
                H("div", new VueObject { Class = "feedback-row" },
                [
                    FeedbackButton("Helpful", "helpful", currentFeedback),
                    FeedbackButton("Needs work", "needs-work", currentFeedback)
                ])
            ])
        ]);
    }

    private static IVNode NotFoundArticle(string currentPath)
    {
        var suggestedPaths = GetSuggestedPaths(currentPath);
        return H("article", new VueObject
        {
            Class = "doc-column",
            Id = MainContentId,
            Tabindex = -1
        },
        [
            H("header", new VueObject { Class = "doc-hero" },
            [
                H("nav", new VueObject { Class = "breadcrumbs" },
                [
                    H("a", new VueObject
                    {
                        Class = "breadcrumb-link",
                        Href = OverviewPath,
                        Events = CreateRouteClickEvents()
                    }, "Home"),
                    H("span", new VueObject { Class = "breadcrumb-separator" }, "/"),
                    H("span", new VueObject { Class = "breadcrumb-current" }, "Not Found")
                ]),
                H("div", new VueObject { Class = "hero-meta-row" },
                [
                    H("span", new VueObject { Class = "hero-group" }, "Routing"),
                    H("span", new VueObject { Class = "hero-status" }, "Not Found"),
                    H("code", new VueObject { Class = "hero-route" }, currentPath)
                ]),
                H("h1", new VueObject { Class = "doc-title" }, "Page Not Found"),
                H("p", new VueObject { Class = "doc-summary" }, "The current path is not registered in the Wiki page catalog. Route fallback is working, but this URL is outside the current docs map."),
                H("div", new VueObject { Class = "hero-actions-row" },
                [
                    H("a", new VueObject
                    {
                        Class = "hero-action-link",
                        Href = SearchPath,
                        Events = CreateRouteClickEvents()
                    }, "Open Search"),
                    H("a", new VueObject
                    {
                        Class = "hero-action-link",
                        Href = TopicIndexPath,
                        Events = CreateRouteClickEvents()
                    }, "Open Topic Index")
                ])
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
                        H("li", "Use `/search` if you only remember a subsystem or route fragment."),
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
        if (currentPath == SearchPath)
            return H("div", "");

        var previousNode = EmptyPagerSlot();
        var nextNode = EmptyPagerSlot();
        var previousPath = GetPreviousPath(currentPath);
        var nextPath = GetNextPath(currentPath);

        if (previousPath.Length > 0)
            previousNode = PagerLink("Previous", previousPath, GetPageTitle(previousPath));

        if (nextPath.Length > 0)
            nextNode = PagerLink("Next", nextPath, GetPageTitle(nextPath));

        return H("nav", new VueObject
        {
            Class = "pager",
            Role = "navigation",
            Raw = new VueDictionary
            {
                ["aria-label"] = "Page pager"
            }
        },
        [
            previousNode,
            nextNode
        ]);
    }

    private static IVNode SiteFooter(string currentSearchQuery)
    {
        var footerSummary = "jazor.wiki now runs as a real docs shell: H-function authored, statically emitted, route-fallback ready, and backed by a central page catalog.";
        if (currentSearchQuery.Length > 0)
            footerSummary = "Current search query: \"" + currentSearchQuery + "\" | " + footerSummary;

        return H("footer", new VueObject { Class = "site-footer" },
        [
            H("p", footerSummary),
            H("p", "Health endpoint: /health | Registered docs pages: " + TotalPageCount + " | Latest catalog refresh: " + GetLatestCatalogRefreshDate())
        ]);
    }

    private static string GetLatestCatalogRefreshDate()
    {
        var latest = "";
        for (var index = 0; index < PageLastUpdatedDates.Length; index++)
        {
            var current = PageLastUpdatedDates[index];
            if (string.CompareOrdinal(current, latest) > 0)
                latest = current;
        }

        return latest;
    }

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

    private static string NormalizeSearchQuery(string query)
        => query.Trim();

    private static string GetHashFragment(string hash)
    {
        if (hash.Length == 0)
            return "";

        return "#" + hash;
    }

    private static string GetSearchFragment(string path, string searchQuery)
    {
        if (path != SearchPath)
            return "";

        var normalizedQuery = NormalizeSearchQuery(searchQuery);
        if (normalizedQuery.Length == 0)
            return "";

        return "?q=" + ECMAScript.Global.EncodeURIComponent(normalizedQuery);
    }

    private static string BuildUrl(string path, string hash, string searchQuery)
        => path + GetSearchFragment(path, searchQuery) + GetHashFragment(hash);

    private static string BuildSearchRoute(string query)
        => BuildUrl(SearchPath, "", query);

    private static string GetSearchQueryFromLocation(Location location, string normalizedPath)
    {
        if (normalizedPath != SearchPath || location.Search.Length == 0)
            return "";

        return GetSearchQueryFromSearchString(location.Search, normalizedPath);
    }

    private static string GetSearchQueryFromSearchString(string search, string normalizedPath)
    {
        if (normalizedPath != SearchPath || search.Length == 0)
            return "";

        try
        {
            var query = new URLSearchParams(search).Get("q") ?? "";
            return NormalizeSearchQuery(query);
        }
        catch
        {
            return "";
        }
    }

    private static bool IsBrowserLocationSynchronized(string path, string hash, string searchQuery)
    {
        var location = ECMAScript.Global.Document.Location;
        if (location == null)
            return false;

        var normalizedBrowserPath = NormalizePath(location.Pathname);
        var normalizedBrowserHash = NormalizeHash(location.Hash);
        var normalizedBrowserSearchQuery = normalizedBrowserPath == SearchPath
            ? GetSearchQueryFromLocation(location, normalizedBrowserPath)
            : "";

        return normalizedBrowserPath == path &&
            normalizedBrowserHash == hash &&
            normalizedBrowserSearchQuery == searchQuery;
    }

    private static void SyncDocumentState(string currentPath, string currentSearchQuery)
    {
        var pageTitle = GetDocumentPageTitle(currentPath, currentSearchQuery);
        var pageSummary = GetDocumentPageSummary(currentPath, currentSearchQuery);
        var robotsDirective = GetDocumentRobotsDirective(currentPath);
        ECMAScript.Global.Document.Title = pageTitle + " | jazor.wiki";
        UpdateDocumentMeta(pageTitle, pageSummary, robotsDirective, BuildUrl(currentPath, "", currentSearchQuery));
    }

    private static void UpdateDocumentMeta(string pageTitle, string pageSummary, string robotsDirective, string relativeUrl)
    {
        var location = ECMAScript.Global.Document.Location;
        var absoluteUrl = relativeUrl;
        if (location != null)
            absoluteUrl = location.Origin + relativeUrl;

        SetMetaContent("meta[name=\"description\"]", pageSummary);
        SetMetaContent("meta[property=\"og:title\"]", pageTitle + " | jazor.wiki");
        SetMetaContent("meta[property=\"og:description\"]", pageSummary);
        SetMetaContent("meta[property=\"og:url\"]", absoluteUrl);
        SetMetaContent("meta[name=\"twitter:title\"]", pageTitle + " | jazor.wiki");
        SetMetaContent("meta[name=\"twitter:description\"]", pageSummary);
        SetMetaContent("meta[name=\"robots\"]", robotsDirective);
        SetLinkHref("link[rel=\"canonical\"]", absoluteUrl);
    }

    private static void SetMetaContent(string selector, string value)
    {
        if (ECMAScript.Global.Document.QuerySelector(selector) is not Element metaElement)
            return;

        metaElement.SetAttribute("content", value);
    }

    private static void SetLinkHref(string selector, string value)
    {
        if (ECMAScript.Global.Document.QuerySelector(selector) is not Element linkElement)
            return;

        linkElement.SetAttribute("href", value);
    }

    private static IVueRef<string>? GetCurrentPathRef()
        => CurrentPathRef;

    private static IVueRef<string>? GetCurrentHashRef()
        => CurrentHashRef;

    private static IVueRef<string>? GetCurrentSearchQueryRef()
        => CurrentSearchQueryRef;

    private static IVueRef<string>? GetCurrentThemeRef()
        => CurrentThemeRef;

    private static IVueRef<string>? GetCopiedSectionRef()
        => CopiedSectionRef;

    private static IVueRef<string>? GetPermalinkReadySectionRef()
        => PermalinkReadySectionRef;

    private static IVueRef<string>? GetCopiedPageRef()
        => CopiedPageRef;

    private static IVueRef<string>? GetPageLinkReadyRef()
        => PageLinkReadyRef;

    private static IVueRef<string>? GetNavFilterRef()
        => NavFilterRef;

    private static IVueRef<string>? GetCopiedCodeBlockRef()
        => CopiedCodeBlockRef;

    private static IVueRef<string>? GetUnavailableCodeBlockRef()
        => UnavailableCodeBlockRef;

    private static IVueRef<string>? GetCurrentPageFeedbackRef()
        => CurrentPageFeedbackRef;

    private static IVueRef<string>? GetLiveStatusRef()
        => LiveStatusRef;

    private static IVueRef<int>? GetReadingProgressPercentRef()
        => ReadingProgressPercentRef;

    private static IVueRef<bool>? GetNavDrawerOpenRef()
        => NavDrawerOpenRef;

    private static IVueRef<bool>? GetTocDrawerOpenRef()
        => TocDrawerOpenRef;

    private static void SetNavFilter(string value)
    {
        var navFilter = GetNavFilterRef();
        if (navFilter == null)
            return;

        navFilter.Value = value.Trim();
    }

    private static void SetSearchQuery(string value, bool updateLocation)
    {
        var searchQuery = GetCurrentSearchQueryRef();
        var currentPath = GetCurrentPathRef();
        var currentHash = GetCurrentHashRef();
        if (searchQuery == null || currentPath == null || currentHash == null)
            return;

        var normalizedQuery = NormalizeSearchQuery(value);
        searchQuery.Value = normalizedQuery;

        if (updateLocation && currentPath.Value == SearchPath)
        {
            var url = BuildUrl(currentPath.Value, currentHash.Value, normalizedQuery);
            ECMAScript.Global.Window.History.ReplaceState(url, "", url);
            SyncDocumentState(currentPath.Value, normalizedQuery);
        }
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

    private static void SetCopiedPage(string value)
    {
        var copiedPage = GetCopiedPageRef();
        if (copiedPage == null)
            return;

        copiedPage.Value = value;
    }

    private static void SetPageLinkReady(string value)
    {
        var pageLinkReady = GetPageLinkReadyRef();
        if (pageLinkReady == null)
            return;

        pageLinkReady.Value = value;
    }

    private static void SetCopiedCodeBlock(string value)
    {
        var copiedCodeBlock = GetCopiedCodeBlockRef();
        if (copiedCodeBlock == null)
            return;

        copiedCodeBlock.Value = value;
    }

    private static void SetUnavailableCodeBlock(string value)
    {
        var unavailableCodeBlock = GetUnavailableCodeBlockRef();
        if (unavailableCodeBlock == null)
            return;

        unavailableCodeBlock.Value = value;
    }

    private static void SetCurrentPageFeedback(string value)
    {
        var currentPageFeedback = GetCurrentPageFeedbackRef();
        if (currentPageFeedback == null)
            return;

        currentPageFeedback.Value = value;
    }

    private static void SetLiveStatus(string value)
    {
        var liveStatus = GetLiveStatusRef();
        if (liveStatus == null)
            return;

        liveStatus.Value = value;
    }

    private static int GetReadingProgressPercent()
        => GetReadingProgressPercentRef()?.Value ?? 0;

    private static void SetReadingProgressPercent(int value)
    {
        if (value < 0)
            value = 0;
        else if (value > 100)
            value = 100;

        var readingProgressPercent = GetReadingProgressPercentRef();
        if (readingProgressPercent == null)
            return;

        readingProgressPercent.Value = value;
    }

    private static void SetNavDrawerOpen(bool value)
    {
        var navDrawer = GetNavDrawerOpenRef();
        if (navDrawer == null)
            return;

        navDrawer.Value = value;
    }

    private static void SetTocDrawerOpen(bool value)
    {
        var tocDrawer = GetTocDrawerOpenRef();
        if (tocDrawer == null)
            return;

        tocDrawer.Value = value;
    }

    private static void CloseDrawers()
    {
        SetNavDrawerOpen(false);
        SetTocDrawerOpen(false);
    }

    private static void ShowCopiedSection(string sectionId)
    {
        SetPermalinkReadySection("");
        SetCopiedSection(sectionId);
        SetLiveStatus("Section link copied.");
        QueuePermalinkFeedbackReset();
    }

    private static void ShowPermalinkReady(string sectionId)
    {
        SetCopiedSection("");
        SetPermalinkReadySection(sectionId);
        SetLiveStatus("Section link ready in the address bar.");
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

    private static void ShowCopiedPage(string path)
    {
        SetPageLinkReady("");
        SetCopiedPage(path);
        SetLiveStatus("Page link copied.");
        QueuePageLinkFeedbackReset();
    }

    private static void ShowPageLinkReady(string path)
    {
        SetCopiedPage("");
        SetPageLinkReady(path);
        SetLiveStatus("Page link ready in the address bar.");
        QueuePageLinkFeedbackReset();
    }

    private static void QueuePageLinkFeedbackReset()
    {
        if (PageLinkFeedbackResetTimerId != 0)
            ECMAScript.Global.Window.ClearTimeout(PageLinkFeedbackResetTimerId);

        PageLinkFeedbackResetTimerId = ECMAScript.Global.Window.SetTimeout((Delegate)(Action)ResetPageLinkFeedback, 1800);
    }

    private static void ResetPageLinkFeedback()
    {
        SetCopiedPage("");
        SetPageLinkReady("");
        PageLinkFeedbackResetTimerId = 0;
    }

    private static void ShowCopiedCodeBlock(string codeBlockId)
    {
        SetUnavailableCodeBlock("");
        SetCopiedCodeBlock(codeBlockId);
        SetLiveStatus("Code block copied.");
        QueueCodeBlockFeedbackReset();
    }

    private static void ShowUnavailableCodeBlock(string codeBlockId)
    {
        SetCopiedCodeBlock("");
        SetUnavailableCodeBlock(codeBlockId);
        SetLiveStatus("Code block copy is unavailable in this browser.");
        QueueCodeBlockFeedbackReset();
    }

    private static void QueueCodeBlockFeedbackReset()
    {
        if (CodeBlockFeedbackResetTimerId != 0)
            ECMAScript.Global.Window.ClearTimeout(CodeBlockFeedbackResetTimerId);

        CodeBlockFeedbackResetTimerId = ECMAScript.Global.Window.SetTimeout((Delegate)(Action)ResetCodeBlockFeedback, 1800);
    }

    private static void ResetCodeBlockFeedback()
    {
        SetCopiedCodeBlock("");
        SetUnavailableCodeBlock("");
        CodeBlockFeedbackResetTimerId = 0;
    }

    private static string ReadStoredPreference(string key, string fallback)
    {
        try
        {
            var storage = ECMAScript.Global.Window.LocalStorage;
            if (storage == null)
                return fallback;

            return storage.GetItem(key) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private static void WriteStoredPreference(string key, string value)
    {
        try
        {
            var storage = ECMAScript.Global.Window.LocalStorage;
            if (storage == null)
                return;

            storage.SetItem(key, value);
        }
        catch
        {
        }
    }

    private static string ReadStoredPageFeedback(string currentPath)
    {
        if (!IsKnownPage(currentPath))
            return "";

        return ReadStoredPreference(FeedbackStoragePrefix + currentPath, "");
    }

    private static void PersistCurrentPageFeedback(string currentPath, string value)
    {
        if (!IsKnownPage(currentPath))
            return;

        WriteStoredPreference(FeedbackStoragePrefix + currentPath, value);
    }

    private static void ApplyTheme(string theme)
    {
        if (ECMAScript.Global.Document.QuerySelector("html") is not Element htmlElement)
            return;

        htmlElement.SetAttribute("data-theme", theme);
    }

    private static string BuildCodeBlockId(string label, string code)
    {
        var seed = label + "\n" + code;
        var hash = 17;
        for (var index = 0; index < seed.Length; index++)
            hash = (hash * 31) + seed[index];

        if (hash == int.MinValue)
            return "code-block-2147483648";

        if (hash < 0)
            hash = 0 - hash;

        return "code-block-" + hash;
    }

    private static string BuildSourceUrl(string currentPath)
        => RepositoryBlobBaseUrl + GetPageSourceFile(currentPath);

    private static string BuildIssueUrl(string currentPath)
    {
        var issueTitle = "Wiki: " + GetPageTitle(currentPath) + " (" + currentPath + ")";
        return RepositoryIssueBaseUrl + ECMAScript.Global.EncodeURIComponent(issueTitle);
    }

    private static void QueueScrollToHashAnchor(string hash)
    {
        if (hash.Length == 0)
            return;

        Vue3.NextTick(() => ScrollToHashAnchor(hash));
    }

    private static void QueueRouteChangeFocus(string path, string searchQuery)
    {
        var routeKey = BuildScrollRouteKey(path, searchQuery);
        Vue3.NextTick(() => FocusMainContentForRoute(routeKey));
    }

    private static void QueueStoredScrollRestore(string path, string searchQuery, bool fallbackToTop)
        => Vue3.NextTick(() => RestoreStoredScrollPosition(path, searchQuery, fallbackToTop));

    private static void ScrollToHashAnchor(string hash)
    {
        if (ECMAScript.Global.Document.GetElementById(hash) is not Element sectionElement)
            return;

        sectionElement.ScrollIntoView(true);
        RememberCurrentScrollPosition();
        QueueActiveSectionSync();
    }

    private static string BuildScrollRouteKey(string path, string searchQuery)
        => path + GetSearchFragment(path, searchQuery);

    private static void RememberCurrentScrollPosition()
    {
        var currentPath = GetCurrentPathRef();
        var currentSearchQuery = GetCurrentSearchQueryRef();
        if (currentPath == null || currentSearchQuery == null)
            return;

        RememberScrollPosition(currentPath.Value, currentSearchQuery.Value, ECMAScript.Global.Window.PageYOffset);
    }

    private static void RememberScrollPosition(string path, string searchQuery, double offset)
    {
        var routeKey = BuildScrollRouteKey(path, searchQuery);
        for (var index = 0; index < StoredScrollRouteKeys.Count; index++)
        {
            if (StoredScrollRouteKeys[index] != routeKey)
                continue;

            StoredScrollOffsets[index] = offset;
            return;
        }

        StoredScrollRouteKeys.Add(routeKey);
        StoredScrollOffsets.Add(offset);
    }

    private static bool TryGetStoredScrollPosition(string path, string searchQuery, out double offset)
    {
        var routeKey = BuildScrollRouteKey(path, searchQuery);
        for (var index = 0; index < StoredScrollRouteKeys.Count; index++)
        {
            if (StoredScrollRouteKeys[index] != routeKey)
                continue;

            offset = StoredScrollOffsets[index];
            return true;
        }

        offset = 0;
        return false;
    }

    private static void RestoreStoredScrollPosition(string path, string searchQuery, bool fallbackToTop)
    {
        if (TryGetStoredScrollPosition(path, searchQuery, out var offset))
        {
            ECMAScript.Global.Window.ScrollTo(0, offset);
            QueueActiveSectionSync();
            return;
        }

        if (!fallbackToTop)
        {
            QueueActiveSectionSync();
            return;
        }

        ECMAScript.Global.Window.ScrollTo(0, 0);
        QueueActiveSectionSync();
    }

    private static void QueueActiveSectionSync()
    {
        if (ActiveSectionSyncQueued)
            return;

        ActiveSectionSyncQueued = true;
        ECMAScript.Global.Window.RequestAnimationFrame(SyncActiveSectionOnFrame);
    }

    private static void SyncActiveSectionOnFrame(double time)
    {
        ActiveSectionSyncQueued = false;
        SyncActiveSectionFromScrollPosition();
    }

    private static void SyncActiveSectionFromScrollPosition()
    {
        SyncReadingProgressFromScrollPosition();

        var currentPath = GetCurrentPathRef();
        var currentHash = GetCurrentHashRef();
        if (currentPath == null || currentHash == null || !IsKnownPage(currentPath.Value))
            return;

        var pageIndex = GetPageIndex(currentPath.Value);
        var sectionIds = GetPageSectionIds(pageIndex);
        if (sectionIds.Length == 0)
            return;

        var activeSectionId = "";
        var nearestDistance = double.MaxValue;

        for (var sectionIndex = 0; sectionIndex < sectionIds.Length; sectionIndex++)
        {
            var sectionId = sectionIds[sectionIndex];
            if (ECMAScript.Global.Document.GetElementById(sectionId) is not Element sectionElement)
                continue;

            var bounds = sectionElement.GetBoundingClientRect();
            if (bounds.Top <= SectionActivationLine && bounds.Bottom > SectionActivationLine)
            {
                activeSectionId = sectionId;
                break;
            }

            var distance = bounds.Top > SectionActivationLine
                ? bounds.Top - SectionActivationLine
                : SectionActivationLine - bounds.Bottom;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                activeSectionId = sectionId;
            }
        }

        if (activeSectionId.Length == 0 || currentHash.Value == activeSectionId)
            return;

        currentHash.Value = activeSectionId;
    }

    private static void SyncReadingProgressFromScrollPosition()
    {
        if (ECMAScript.Global.Document.GetElementById(MainContentId) is not Element mainContentElement)
        {
            SetReadingProgressPercent(0);
            return;
        }

        var bounds = mainContentElement.GetBoundingClientRect();
        if (bounds.Height <= 0)
        {
            SetReadingProgressPercent(0);
            return;
        }

        var rawProgress = (SectionActivationLine - bounds.Top) / bounds.Height;
        if (rawProgress <= 0)
        {
            SetReadingProgressPercent(0);
            return;
        }

        if (rawProgress >= 1)
        {
            SetReadingProgressPercent(100);
            return;
        }

        SetReadingProgressPercent((int)(rawProgress * 100));
    }

    private static void FocusMainContentForRoute(string routeKey)
    {
        var currentPath = GetCurrentPathRef();
        var currentSearchQuery = GetCurrentSearchQueryRef();
        if (currentPath == null || currentSearchQuery == null)
            return;

        if (BuildScrollRouteKey(currentPath.Value, currentSearchQuery.Value) != routeKey)
            return;

        if (ECMAScript.Global.Document.GetElementById(MainContentId) is not HTMLElement mainContentElement)
            return;

        mainContentElement.Focus(new FocusOptions(PreventScroll: true, FocusVisible: true));
        SetLiveStatus(BuildRouteChangeAnnouncement(currentPath.Value, currentSearchQuery.Value));
    }

    private static string BuildRouteChangeAnnouncement(string currentPath, string currentSearchQuery)
    {
        if (currentPath == SearchPath)
        {
            if (currentSearchQuery.Length == 0)
                return "Opened search.";

            return "Opened search results for \"" + currentSearchQuery + "\".";
        }

        if (IsKnownPage(currentPath))
            return "Opened " + GetPageTitle(currentPath) + ".";

        return "Opened page not found for " + currentPath + ".";
    }

    private static void NavigateTo(string path, string hash, string searchQuery, bool updateHistory, bool resetScroll)
    {
        var currentPath = GetCurrentPathRef();
        var currentHash = GetCurrentHashRef();
        var currentSearchQuery = GetCurrentSearchQueryRef();
        if (currentPath == null || currentHash == null || currentSearchQuery == null)
            return;

        RememberScrollPosition(currentPath.Value, currentSearchQuery.Value, ECMAScript.Global.Window.PageYOffset);

        var normalizedPath = NormalizePath(path);
        var normalizedHash = NormalizeHash(hash);
        var normalizedSearchQuery = normalizedPath == SearchPath
            ? NormalizeSearchQuery(searchQuery)
            : "";
        var primaryRouteChanged = currentPath.Value != normalizedPath ||
            currentSearchQuery.Value != normalizedSearchQuery;
        var browserLocationSynchronized = IsBrowserLocationSynchronized(normalizedPath, normalizedHash, normalizedSearchQuery);

        if (currentPath.Value == normalizedPath &&
            currentHash.Value == normalizedHash &&
            currentSearchQuery.Value == normalizedSearchQuery)
        {
            if (updateHistory && !browserLocationSynchronized)
            {
                var url = BuildUrl(normalizedPath, normalizedHash, normalizedSearchQuery);
                ECMAScript.Global.Window.History.PushState(url, "", url);
            }

            if (normalizedHash.Length > 0)
                QueueScrollToHashAnchor(normalizedHash);
            else if (resetScroll)
            {
                ECMAScript.Global.Window.ScrollTo(0, 0);
                RememberScrollPosition(normalizedPath, normalizedSearchQuery, 0);
                QueueActiveSectionSync();
            }

            return;
        }

        currentPath.Value = normalizedPath;
        currentHash.Value = normalizedHash;
        currentSearchQuery.Value = normalizedSearchQuery;
        CloseDrawers();
        SetCurrentPageFeedback(ReadStoredPageFeedback(normalizedPath));

        if (updateHistory)
        {
            var url = BuildUrl(normalizedPath, normalizedHash, normalizedSearchQuery);
            ECMAScript.Global.Window.History.PushState(url, "", url);
        }

        SyncDocumentState(normalizedPath, normalizedSearchQuery);
        if (normalizedHash.Length > 0)
            QueueScrollToHashAnchor(normalizedHash);
        else if (resetScroll)
        {
            ECMAScript.Global.Window.ScrollTo(0, 0);
            RememberScrollPosition(normalizedPath, normalizedSearchQuery, 0);
            QueueActiveSectionSync();
        }
        else
        {
            QueueStoredScrollRestore(normalizedPath, normalizedSearchQuery, fallbackToTop: false);
        }

        if (primaryRouteChanged)
            QueueRouteChangeFocus(normalizedPath, normalizedSearchQuery);
    }

    private static void SyncLocationStateFromBrowser()
    {
        var location = ECMAScript.Global.Document.Location;
        var currentPath = GetCurrentPathRef();
        var currentHash = GetCurrentHashRef();
        var currentSearchQuery = GetCurrentSearchQueryRef();
        if (location == null || currentPath == null || currentHash == null || currentSearchQuery == null)
            return;

        RememberScrollPosition(currentPath.Value, currentSearchQuery.Value, ECMAScript.Global.Window.PageYOffset);

        var normalizedPath = NormalizePath(location.Pathname);
        var normalizedHash = NormalizeHash(location.Hash);
        var normalizedSearchQuery = GetSearchQueryFromLocation(location, normalizedPath);
        var primaryRouteChanged = currentPath.Value != normalizedPath ||
            currentSearchQuery.Value != normalizedSearchQuery;

        currentPath.Value = normalizedPath;
        currentHash.Value = normalizedHash;
        currentSearchQuery.Value = normalizedSearchQuery;
        CloseDrawers();
        SetCurrentPageFeedback(ReadStoredPageFeedback(normalizedPath));
        SyncDocumentState(normalizedPath, normalizedSearchQuery);

        if (normalizedHash.Length > 0)
            QueueScrollToHashAnchor(normalizedHash);
        else
            QueueStoredScrollRestore(normalizedPath, normalizedSearchQuery, fallbackToTop: false);

        if (primaryRouteChanged)
            QueueRouteChangeFocus(normalizedPath, normalizedSearchQuery);
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
        NavigateTo(anchor.Pathname, "", GetSearchQueryFromSearchString(anchor.Search, NormalizePath(anchor.Pathname)), updateHistory: true, resetScroll: true);
    }

    private static void OnTocClick(MouseEvent mouseEvent)
    {
        if (ShouldAllowBrowserDefault(mouseEvent))
            return;

        if (mouseEvent.CurrentTarget is not HTMLAnchorElement anchor)
            return;

        mouseEvent.PreventDefault();
        CloseDrawers();
        NavigateTo(anchor.Pathname, anchor.Hash, GetSearchQueryFromSearchString(anchor.Search, NormalizePath(anchor.Pathname)), updateHistory: true, resetScroll: true);
    }

    private static void OnSectionPermalinkClick(MouseEvent mouseEvent)
    {
        if (mouseEvent.CurrentTarget is not HTMLButtonElement buttonElement)
            return;

        mouseEvent.PreventDefault();

        var currentPath = GetCurrentPathRef();
        var currentSearchQuery = GetCurrentSearchQueryRef();
        if (currentPath == null || currentSearchQuery == null)
            return;

        var sectionId = NormalizeHash(buttonElement.Value);
        if (sectionId.Length == 0)
            return;

        ResetPermalinkFeedback();
        NavigateTo(currentPath.Value, sectionId, currentSearchQuery.Value, updateHistory: true, resetScroll: true);

        var location = ECMAScript.Global.Document.Location;
        var sectionUrl = BuildUrl(currentPath.Value, sectionId, currentSearchQuery.Value);
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

    private static void OnPagePermalinkClick(MouseEvent mouseEvent)
    {
        if (mouseEvent.CurrentTarget is not HTMLButtonElement buttonElement)
            return;

        mouseEvent.PreventDefault();

        var currentPath = GetCurrentPathRef();
        var currentSearchQuery = GetCurrentSearchQueryRef();
        if (currentPath == null || currentSearchQuery == null)
            return;

        var targetPath = NormalizePath(buttonElement.Value);
        ResetPageLinkFeedback();

        var location = ECMAScript.Global.Document.Location;
        var pageUrl = BuildUrl(targetPath, "", currentSearchQuery.Value);
        var shareUrl = pageUrl;
        if (location != null)
            shareUrl = location.Origin + pageUrl;

        try
        {
            var clipboard = ECMAScript.Global.Window.Navigator.Clipboard;
            if (clipboard == null)
            {
                ShowPageLinkReady(targetPath);
                return;
            }

            Promise.Resolve(clipboard.WriteText(shareUrl)).Then(
                () => ShowCopiedPage(targetPath),
                () => ShowPageLinkReady(targetPath));
        }
        catch
        {
            ShowPageLinkReady(targetPath);
        }
    }

    private static void OnCodeBlockCopyClick(MouseEvent mouseEvent)
    {
        if (mouseEvent.CurrentTarget is not HTMLButtonElement buttonElement)
            return;

        mouseEvent.PreventDefault();

        var codeBlockId = buttonElement.Value;
        if (codeBlockId.Length == 0)
            return;

        if (ECMAScript.Global.Document.GetElementById(codeBlockId) is not Element codeBlockElement)
            return;

        var code = codeBlockElement.TextContent ?? "";
        if (code.Length == 0)
            return;

        ResetCodeBlockFeedback();

        try
        {
            var clipboard = ECMAScript.Global.Window.Navigator.Clipboard;
            if (clipboard == null)
            {
                ShowUnavailableCodeBlock(codeBlockId);
                return;
            }

            Promise.Resolve(clipboard.WriteText(code)).Then(
                () => ShowCopiedCodeBlock(codeBlockId),
                () => ShowUnavailableCodeBlock(codeBlockId));
        }
        catch
        {
            ShowUnavailableCodeBlock(codeBlockId);
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

    private static void OnSearchInput(Event @event)
    {
        if (@event.CurrentTarget is not HTMLInputElement inputElement)
            return;

        SetSearchQuery(inputElement.Value, updateLocation: true);
    }

    private static void ClearSearch(MouseEvent mouseEvent)
    {
        mouseEvent.PreventDefault();
        SetSearchQuery("", updateLocation: true);
    }

    private static void ToggleTheme(MouseEvent mouseEvent)
    {
        mouseEvent.PreventDefault();

        var theme = GetCurrentThemeRef();
        if (theme == null)
            return;

        theme.Value = theme.Value == "light" ? "dark" : "light";
        ApplyTheme(theme.Value);
        WriteStoredPreference(ThemeStorageKey, theme.Value);
        SetLiveStatus("Theme switched to " + theme.Value + ".");
    }

    private static void OpenNavDrawer(MouseEvent mouseEvent)
    {
        mouseEvent.PreventDefault();
        SetNavDrawerOpen(true);
        SetTocDrawerOpen(false);
    }

    private static void OpenTocDrawer(MouseEvent mouseEvent)
    {
        mouseEvent.PreventDefault();
        SetTocDrawerOpen(true);
        SetNavDrawerOpen(false);
    }

    private static void CloseAllDrawers(MouseEvent mouseEvent)
    {
        mouseEvent.PreventDefault();
        CloseDrawers();
    }

    private static void OnPageFeedbackClick(MouseEvent mouseEvent)
    {
        if (mouseEvent.CurrentTarget is not HTMLButtonElement buttonElement)
            return;

        mouseEvent.PreventDefault();

        var currentPath = GetCurrentPathRef();
        if (currentPath == null)
            return;

        var feedbackValue = buttonElement.Value;
        SetCurrentPageFeedback(feedbackValue);
        PersistCurrentPageFeedback(currentPath.Value, feedbackValue);

        if (feedbackValue == "helpful")
            SetLiveStatus("Page marked as helpful.");
        else
            SetLiveStatus("Page marked as needing work.");
    }

    private static void FocusNavSearch()
    {
        if (ECMAScript.Global.Document.GetElementById(NavSearchInputId) is not HTMLInputElement inputElement)
            return;

        inputElement.Focus();
        inputElement.Select();
    }

    private static void FocusPageSearch()
    {
        if (ECMAScript.Global.Document.GetElementById(SearchInputId) is not HTMLInputElement inputElement)
            return;

        inputElement.Focus();
        inputElement.Select();
    }

    private static void FocusPrimarySearch()
    {
        if (GetCurrentPathRef()?.Value == SearchPath)
        {
            FocusPageSearch();
            return;
        }

        FocusNavSearch();
    }

    private static bool IsEditableActiveElement()
    {
        if (ECMAScript.Global.Document.ActiveElement is HTMLInputElement)
            return true;

        if (ECMAScript.Global.Document.ActiveElement is HTMLTextAreaElement)
            return true;

        return false;
    }

    private static bool IsNavSearchFocused()
        => ECMAScript.Global.Document.ActiveElement is HTMLInputElement activeInput &&
           activeInput.Id == NavSearchInputId;

    private static bool IsPageSearchFocused()
        => ECMAScript.Global.Document.ActiveElement is HTMLInputElement activeInput &&
           activeInput.Id == SearchInputId;

    private static object OnGlobalKeyDown(Event @event)
    {
        if (@event is not KeyboardEvent keyboardEvent)
            return 0;

        var key = keyboardEvent.Key;
        var hasPrimaryModifier = keyboardEvent.CtrlKey || keyboardEvent.MetaKey;

        if (!keyboardEvent.AltKey && !keyboardEvent.ShiftKey && !hasPrimaryModifier && key == "/")
        {
            if (!IsEditableActiveElement())
            {
                keyboardEvent.PreventDefault();
                FocusPrimarySearch();
            }

            return 0;
        }

        if (!keyboardEvent.AltKey && !keyboardEvent.ShiftKey && hasPrimaryModifier && (key == "k" || key == "K"))
        {
            keyboardEvent.PreventDefault();
            FocusPrimarySearch();
            return 0;
        }

        if (key == "Escape")
        {
            if (IsPageSearchFocused())
            {
                keyboardEvent.PreventDefault();
                if ((GetCurrentSearchQueryRef()?.Value ?? "").Length > 0)
                    SetSearchQuery("", updateLocation: true);
                else if (ECMAScript.Global.Document.ActiveElement is HTMLInputElement searchInput)
                    searchInput.Blur();

                return 0;
            }

            if (IsNavSearchFocused())
            {
                keyboardEvent.PreventDefault();
                if ((GetNavFilterRef()?.Value ?? "").Length > 0)
                    SetNavFilter("");
                else if (ECMAScript.Global.Document.ActiveElement is HTMLInputElement activeInput)
                    activeInput.Blur();

                return 0;
            }

            if (IsNavDrawerOpen() || IsTocDrawerOpen())
            {
                keyboardEvent.PreventDefault();
                CloseDrawers();
                return 0;
            }
        }

        return 0;
    }

    private static object OnHashChange(Event @event)
    {
        SyncLocationStateFromBrowser();
        return 0;
    }

    private static object OnScroll(Event @event)
    {
        RememberCurrentScrollPosition();
        QueueActiveSectionSync();
        return 0;
    }

    private static object OnPopState(Event @event)
    {
        SyncLocationStateFromBrowser();
        return 0;
    }
}
