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

    private static IVNode HeaderLink(string path, string label)
        => H("a", new VueObject
        {
            Class = "header-link",
            Href = path,
            Events = CreateRouteClickEvents()
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
        if (path == currentPath)
            className = "nav-link nav-link-active";

        return H("a", new VueObject
        {
            Class = className,
            Href = path,
            Events = CreateRouteClickEvents()
        },
        [
            H("span", new VueObject { Class = "nav-link-title" }, title),
            H("span", new VueObject { Class = "nav-link-summary" }, summary)
        ]);
    }

    private static IVNode TocRail(string title, IVNode[] links)
        => H("aside", new VueObject { Class = "toc-rail" },
        [
            H("div", new VueObject { Class = "rail-card toc-card" },
            [
                H("p", new VueObject { Class = "rail-kicker" }, title),
                H("p", new VueObject { Class = "rail-copy" }, "Stable anchors are part of the docs contract. Treat them as user-facing entry points."),
                H("div", new VueObject { Class = "toc-links" }, links)
            ])
        ]);

    private static IVNode EmptyTocRail()
        => H("aside", new VueObject { Class = "toc-rail toc-rail-empty" },
        [
            H("div", new VueObject { Class = "rail-card toc-card" },
            [
                H("p", new VueObject { Class = "rail-kicker" }, "Missing page"),
                H("p", new VueObject { Class = "rail-copy" }, "The requested route is not registered in the current Wiki route map.")
            ])
        ]);

    private static IVNode TocLink(string path, string id, string title, string currentHash)
    {
        var className = "toc-link";
        if (currentHash == id)
            className = "toc-link toc-link-active";

        return H("a", new VueObject
        {
            Class = className,
            Href = path + "#" + id,
            Events = CreateTocClickEvents()
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
        => H("div", new VueObject { Class = "code-frame" },
        [
            H("div", new VueObject { Class = "code-label" }, label),
            H("pre", new VueObject { Class = "code-block" }, code)
        ]);
}
