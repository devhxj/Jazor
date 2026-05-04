using System.Collections.Generic;
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static VueEventHandlers<MouseEvent> CreateRouteClickEvents()
        => new()
        {
            ["onClick"] = OnRouteClick
        };

    private static VueEventHandlers<Event> CreateNavFilterInputEvents()
        => new()
        {
            ["onInput"] = OnNavFilterInput
        };

    private static VueEventHandlers<MouseEvent> CreateClearNavFilterEvents()
        => new()
        {
            ["onClick"] = ClearNavFilter
        };

    private static VueEventHandlers<Event> CreateSearchInputEvents()
        => new()
        {
            ["onInput"] = OnSearchInput
        };

    private static VueEventHandlers<MouseEvent> CreateClearSearchEvents()
        => new()
        {
            ["onClick"] = ClearSearch
        };

    private static VueEventHandlers<MouseEvent> CreateTocClickEvents()
        => new()
        {
            ["onClick"] = OnTocClick
        };

    private static VueEventHandlers<MouseEvent> CreateSectionPermalinkEvents()
        => new()
        {
            ["onClick"] = OnSectionPermalinkClick
        };

    private static VueEventHandlers<MouseEvent> CreatePagePermalinkEvents()
        => new()
        {
            ["onClick"] = OnPagePermalinkClick
        };

    private static VueEventHandlers<MouseEvent> CreateCodeBlockCopyEvents()
        => new()
        {
            ["onClick"] = OnCodeBlockCopyClick
        };

    private static VueEventHandlers<MouseEvent> CreateThemeToggleEvents()
        => new()
        {
            ["onClick"] = ToggleTheme
        };

    private static VueEventHandlers<MouseEvent> CreateOpenNavDrawerEvents()
        => new()
        {
            ["onClick"] = OpenNavDrawer
        };

    private static VueEventHandlers<MouseEvent> CreateOpenTocDrawerEvents()
        => new()
        {
            ["onClick"] = OpenTocDrawer
        };

    private static VueEventHandlers<MouseEvent> CreateCloseDrawersEvents()
        => new()
        {
            ["onClick"] = CloseAllDrawers
        };

    private static VueEventHandlers<MouseEvent> CreatePageFeedbackEvents()
        => new()
        {
            ["onClick"] = OnPageFeedbackClick
        };

    private static IVNode HeaderLink(string path, string label)
        => H("a", new VueObject
        {
            Class = "header-link",
            Href = path,
            Events = CreateRouteClickEvents()
        }, label);

    private static IVNode DrawerButton(string label, string className, string controlsId, bool isExpanded, bool isDisabled, VueEventHandlers<MouseEvent> events)
        => H("button", new VueObject
        {
            Class = className,
            Type = "button",
            Disabled = isDisabled,
            Events = events,
            Raw = new VueDictionary
            {
                ["aria-controls"] = controlsId,
                ["aria-expanded"] = isExpanded ? "true" : "false"
            }
        }, label);

    private static IVNode NavGroup(string title, IVNode[] links)
        => H("section", new VueObject { Class = "nav-group" },
        [
            H("p", new VueObject { Class = "nav-group-title" }, title),
            H("div", new VueObject { Class = "nav-group-links" }, links)
        ]);

    private static IVNode NavLink(string path, string title, string summary, string currentPath)
    {
        var className = "nav-link";
        VueDictionary? raw = null;
        if (path == currentPath)
        {
            className = "nav-link nav-link-active";
            raw = new VueDictionary
            {
                ["aria-current"] = "page"
            };
        }

        return H("a", new VueObject
        {
            Class = className,
            Href = path,
            Events = CreateRouteClickEvents(),
            Raw = raw
        },
        [
            H("span", new VueObject { Class = "nav-link-title" }, title),
            H("span", new VueObject { Class = "nav-link-summary" }, summary)
        ]);
    }

    private static IVNode TocRail(string title, IVNode[] links)
    {
        var className = "toc-rail";
        if (IsTocDrawerOpen())
            className += " toc-rail-open";

        return H("aside", new VueObject
        {
            Id = TocRailId,
            Class = className,
            Role = "navigation",
            Raw = new VueDictionary
            {
                ["aria-label"] = title
            }
        },
        [
            H("div", new VueObject { Class = "rail-card toc-card" },
            [
                H("div", new VueObject { Class = "toc-drawer-head" },
                [
                    H("p", new VueObject { Class = "rail-kicker" }, title),
                    H("button", new VueObject
                    {
                        Class = "drawer-close",
                        Type = "button",
                        Title = "Close table of contents",
                        Events = CreateCloseDrawersEvents()
                    }, "Close")
                ]),
                H("p", new VueObject { Class = "rail-copy" }, "Stable anchors are part of the docs contract. Treat them as user-facing entry points."),
                H("div", new VueObject { Class = "toc-links" }, links)
            ])
        ]);
    }

    private static IVNode EmptyTocRail()
    {
        var className = "toc-rail toc-rail-empty";
        if (IsTocDrawerOpen())
            className += " toc-rail-open";

        return H("aside", new VueObject
        {
            Id = TocRailId,
            Class = className
        },
        [
            H("div", new VueObject { Class = "rail-card toc-card" },
            [
                H("div", new VueObject { Class = "toc-drawer-head" },
                [
                    H("p", new VueObject { Class = "rail-kicker" }, "Missing page"),
                    H("button", new VueObject
                    {
                        Class = "drawer-close",
                        Type = "button",
                        Title = "Close table of contents",
                        Events = CreateCloseDrawersEvents()
                    }, "Close")
                ]),
                H("p", new VueObject { Class = "rail-copy" }, "The requested route is not registered in the current Wiki route map.")
            ])
        ]);
    }

    private static IVNode TocLink(string path, string id, string title, string currentHash)
    {
        var className = "toc-link";
        VueDictionary? raw = null;
        if (currentHash == id)
        {
            className = "toc-link toc-link-active";
            raw = new VueDictionary
            {
                ["aria-current"] = "location"
            };
        }

        return H("a", new VueObject
        {
            Class = className,
            Href = path + "#" + id,
            Events = CreateTocClickEvents(),
            Raw = raw
        }, title);
    }

    private static IVNode PagerLink(string direction, string path, string title)
        => H("a", new VueObject
        {
            Class = "pager-link",
            Href = path,
            Events = CreateRouteClickEvents()
        },
        [
            H("span", new VueObject { Class = "pager-direction" }, direction),
            H("span", new VueObject { Class = "pager-title" }, title)
        ]);

    private static IVNode EmptyPagerSlot()
        => H("div", new VueObject { Class = "pager-slot" }, "");

    private static IVNode RouteCardGrid(string[] paths)
    {
        var routeCards = new List<IVNode>();
        for (var index = 0; index < paths.Length; index++)
        {
            var path = paths[index];
            routeCards.Add(H("a", new VueObject
            {
                Class = "route-card",
                Href = path,
                Events = CreateRouteClickEvents()
            },
            [
                H("span", new VueObject { Class = "route-card-group" }, GetPageGroup(path)),
                H("strong", new VueObject { Class = "route-card-title" }, GetPageTitle(path)),
                H("code", new VueObject { Class = "route-card-path" }, path),
                H("span", new VueObject { Class = "route-card-summary" }, GetPageSummary(path))
            ]));
        }

        return H("div", new VueObject { Class = "route-grid" }, routeCards.ToArray());
    }

    private static IVNode TagLink(string tag)
        => H("a", new VueObject
        {
            Class = "tag-pill",
            Href = BuildSearchRoute(tag),
            Events = CreateRouteClickEvents()
        }, tag);

    private static IVNode MetaCard(string title, string value, string summary)
        => H("article", new VueObject { Class = "meta-card" },
        [
            H("p", new VueObject { Class = "meta-card-title" }, title),
            H("strong", new VueObject { Class = "meta-card-value" }, value),
            H("p", new VueObject { Class = "meta-card-summary" }, summary)
        ]);

    private static IVNode FeedbackButton(string label, string value, string currentValue)
    {
        var className = "feedback-button";
        if (currentValue == value)
            className += " feedback-button-active";

        return H("button", new VueObject
        {
            Class = className,
            Type = "button",
            Value = value,
            Events = CreatePageFeedbackEvents()
        }, label);
    }

    private static IVNode PageSection(string id, string title, IVNode[] content)
    {
        var className = "doc-section";
        if (GetCurrentHashRef()?.Value == id)
            className = "doc-section doc-section-active";

        var permalinkLabel = "Copy link";
        var permalinkClassName = "section-permalink";
        var permalinkTitle = "Copy direct link to this section";
        if (GetCopiedSectionRef()?.Value == id)
        {
            permalinkLabel = "Copied";
            permalinkClassName = "section-permalink section-permalink-copied";
            permalinkTitle = "Direct link copied to clipboard";
        }
        else if (GetPermalinkReadySectionRef()?.Value == id)
        {
            permalinkLabel = "Link ready";
            permalinkClassName = "section-permalink section-permalink-ready";
            permalinkTitle = "Direct link is ready in the address bar; clipboard copy was not available";
        }

        return H("section", new VueObject
        {
            Id = id,
            Class = className
        },
        [
            H("div", new VueObject { Class = "section-anchor" }, id),
            H("div", new VueObject { Class = "section-title-row" },
            [
                H("h2", title),
                H("button", new VueObject
                {
                    Class = permalinkClassName,
                    Type = "button",
                    Value = id,
                    Title = permalinkTitle,
                    Events = CreateSectionPermalinkEvents()
                }, permalinkLabel)
            ]),
            H("div", new VueObject { Class = "section-body" }, content)
        ]);
    }

    private static IVNode MetricCard(string value, string title, string summary)
        => H("article", new VueObject { Class = "metric-card" },
        [
            H("p", new VueObject { Class = "metric-value" }, value),
            H("h3", new VueObject { Class = "metric-title" }, title),
            H("p", new VueObject { Class = "metric-summary" }, summary)
        ]);

    private static IVNode CheckCard(string title, string summary)
        => H("article", new VueObject { Class = "check-card" },
        [
            H("h3", new VueObject { Class = "check-title" }, title),
            H("p", new VueObject { Class = "check-summary" }, summary)
        ]);

    private static IVNode Callout(string title, string summary)
        => H("div", new VueObject { Class = "callout" },
        [
            H("p", new VueObject { Class = "callout-title" }, title),
            H("p", new VueObject { Class = "callout-summary" }, summary)
        ]);

    private static IVNode CodeBlock(string label, string code)
    {
        var codeBlockId = BuildCodeBlockId(label, code);
        var copyLabel = "Copy code";
        var copyClassName = "code-copy-button";
        var copyTitle = "Copy this code block";

        if (GetCopiedCodeBlockRef()?.Value == codeBlockId)
        {
            copyLabel = "Copied";
            copyClassName = "code-copy-button code-copy-button-copied";
            copyTitle = "Code block copied to clipboard";
        }
        else if (GetUnavailableCodeBlockRef()?.Value == codeBlockId)
        {
            copyLabel = "Copy unavailable";
            copyClassName = "code-copy-button code-copy-button-unavailable";
            copyTitle = "Clipboard copy is not available in this browser";
        }

        return H("div", new VueObject { Class = "code-frame" },
        [
            H("div", new VueObject { Class = "code-label-row" },
            [
                H("div", new VueObject { Class = "code-label" }, label),
                H("button", new VueObject
                {
                    Class = copyClassName,
                    Type = "button",
                    Value = codeBlockId,
                    Title = copyTitle,
                    Events = CreateCodeBlockCopyEvents()
                }, copyLabel)
            ]),
            H("pre", new VueObject
            {
                Id = codeBlockId,
                Class = "code-block"
            }, code)
        ]);
    }
}
