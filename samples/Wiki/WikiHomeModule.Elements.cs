// WikiHomeModule.Elements.cs - 共享 UI 元素（按钮、链接、卡片、代码块等） / Shared UI elements (buttons, links, cards, code blocks, etc.)
using System.Collections.Generic;
using ECMAScript;
using static ECMAScript.Vue;

namespace Wiki;

public static partial class WikiHomeModule
{
    // ── 事件处理器工厂 / Event handler factories ──
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

    // 头部导航链接 / Header navigation link
    private static IVNode HeaderLink(string path, string label)
        => H("a", new VueObject
        {
            Class = "header-link",
            Href = BuildBrowserUrl(path, "", ""),
            Events = CreateRouteClickEvents()
        }, label);

    // 抽屉按钮（移动端导航/目录切换） / Drawer button for mobile nav/TOC toggle
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

    // 导航分组（按主题归类链接） / Navigation group (links grouped by topic)
    private static IVNode NavGroup(string title, IVNode[] links)
        => H("section", new VueObject { Class = "nav-group" },
        [
            H("p", new VueObject { Class = "nav-group-title" }, title),
            H("div", new VueObject { Class = "nav-group-links" }, links)
        ]);

    // 导航链接（带高亮当前页面状态） / Navigation link with active page highlight
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
            Href = BuildBrowserUrl(path, "", ""),
            Events = CreateRouteClickEvents(),
            Raw = raw
        },
        [
            H("span", new VueObject { Class = "nav-link-title" }, title),
            H("span", new VueObject { Class = "nav-link-summary" }, summary)
        ]);
    }

    // 目录侧边栏（右侧 TOC 导航） / Table of contents rail (right-side TOC navigation)
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
                        Title = "关闭目录",
                        Events = CreateCloseDrawersEvents()
                    }, "关闭")
                ]),
                H("p", new VueObject { Class = "rail-copy" }, "稳定的锚点是文档契约的一部分，应视为面向用户的入口点。"),
                H("div", new VueObject { Class = "toc-links" }, links)
            ])
        ]);
    }

    // 空目录侧边栏（页面未注册时显示） / Empty TOC rail (shown when page is not registered)
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
                    H("p", new VueObject { Class = "rail-kicker" }, "缺失页面"),
                    H("button", new VueObject
                    {
                        Class = "drawer-close",
                        Type = "button",
                        Title = "关闭目录",
                        Events = CreateCloseDrawersEvents()
                    }, "关闭")
                ]),
                H("p", new VueObject { Class = "rail-copy" }, "请求的路由未在当前 Wiki 路由映射中注册。")
            ])
        ]);
    }

    // 目录链接（带当前锚点高亮） / TOC link with active anchor highlight
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
            Href = BuildBrowserUrl(path, id, ""),
            Events = CreateTocClickEvents(),
            Raw = raw
        }, title);
    }

    // 分页链接（上一页/下一页） / Pager link (previous/next)
    private static IVNode PagerLink(string direction, string path, string title)
        => H("a", new VueObject
        {
            Class = "pager-link",
            Href = BuildBrowserUrl(path, "", ""),
            Events = CreateRouteClickEvents()
        },
        [
            H("span", new VueObject { Class = "pager-direction" }, direction),
            H("span", new VueObject { Class = "pager-title" }, title)
        ]);

    // 空分页槽位（占位） / Empty pager slot placeholder
    private static IVNode EmptyPagerSlot()
        => H("div", new VueObject { Class = "pager-slot" }, "");

    // 路由卡片网格（相关页面展示） / Route card grid for related pages
    private static IVNode RouteCardGrid(string[] paths)
    {
        var routeCards = new List<IVNode>();
        for (var index = 0; index < paths.Length; index++)
        {
            var path = paths[index];
            routeCards.Add(H("a", new VueObject
            {
                Class = "route-card",
                Href = BuildBrowserUrl(path, "", ""),
                Events = CreateRouteClickEvents()
            },
            [
                H("span", new VueObject { Class = "route-card-group" }, GetPageGroupLabel(path)),
                H("strong", new VueObject { Class = "route-card-title" }, GetPageTitle(path)),
                H("code", new VueObject { Class = "route-card-path" }, path),
                H("span", new VueObject { Class = "route-card-summary" }, GetPageSummary(path))
            ]));
        }

        return H("div", new VueObject { Class = "route-grid" }, routeCards.ToArray());
    }

    // 标签链接（点击跳转搜索） / Tag link (click to search)
    private static IVNode TagLink(string tag)
        => H("a", new VueObject
        {
            Class = "tag-pill",
            Href = BuildSearchRoute(tag),
            Events = CreateRouteClickEvents()
        }, tag);

    // 元数据卡片（Owner/Audience/Updated 等信息展示） / Metadata card for Owner/Audience/Updated etc.
    private static IVNode MetaCard(string title, string value, string summary)
        => H("article", new VueObject { Class = "meta-card" },
        [
            H("p", new VueObject { Class = "meta-card-title" }, title),
            H("strong", new VueObject { Class = "meta-card-value" }, value),
            H("p", new VueObject { Class = "meta-card-summary" }, summary)
        ]);

    // 反馈按钮（有帮助/需改进） / Feedback button (helpful/needs-work)
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

    // 文档段落（带锚点和复制链接功能） / Document section with anchor and copy-link feature
    private static IVNode PageSection(string id, string title, IVNode[] content)
    {
        var className = "doc-section";
        if (GetCurrentHashRef()?.Value == id)
            className = "doc-section doc-section-active";

        var permalinkLabel = "复制链接";
        var permalinkClassName = "section-permalink";
        var permalinkTitle = "复制此段落的直接链接";
        if (GetCopiedSectionRef()?.Value == id)
        {
            permalinkLabel = "已复制";
            permalinkClassName = "section-permalink section-permalink-copied";
            permalinkTitle = "直接链接已复制到剪贴板";
        }
        else if (GetPermalinkReadySectionRef()?.Value == id)
        {
            permalinkLabel = "链接已就绪";
            permalinkClassName = "section-permalink section-permalink-ready";
            permalinkTitle = "直接链接已在地址栏就绪；剪贴板复制不可用";
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

    // 指标卡片（数值+标题+描述） / Metric card (value + title + summary)
    private static IVNode MetricCard(string value, string title, string summary)
        => H("article", new VueObject { Class = "metric-card" },
        [
            H("p", new VueObject { Class = "metric-value" }, value),
            H("h3", new VueObject { Class = "metric-title" }, title),
            H("p", new VueObject { Class = "metric-summary" }, summary)
        ]);

    // 检查卡片（标题+描述） / Check card (title + summary)
    private static IVNode CheckCard(string title, string summary)
        => H("article", new VueObject { Class = "check-card" },
        [
            H("h3", new VueObject { Class = "check-title" }, title),
            H("p", new VueObject { Class = "check-summary" }, summary)
        ]);

    // 提示框（标题+描述） / Callout box (title + summary)
    private static IVNode Callout(string title, string summary)
        => H("div", new VueObject { Class = "callout" },
        [
            H("p", new VueObject { Class = "callout-title" }, title),
            H("p", new VueObject { Class = "callout-summary" }, summary)
        ]);

    // 代码块（带复制按钮） / Code block with copy button
    private static IVNode CodeBlock(string label, string code)
    {
        var codeBlockId = BuildCodeBlockId(label, code);
        var copyLabel = "复制代码";
        var copyClassName = "code-copy-button";
        var copyTitle = "复制此代码块";

        if (GetCopiedCodeBlockRef()?.Value == codeBlockId)
        {
            copyLabel = "已复制";
            copyClassName = "code-copy-button code-copy-button-copied";
            copyTitle = "代码块已复制到剪贴板";
        }
        else if (GetUnavailableCodeBlockRef()?.Value == codeBlockId)
        {
            copyLabel = "复制不可用";
            copyClassName = "code-copy-button code-copy-button-unavailable";
            copyTitle = "当前浏览器不支持剪贴板复制";
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
