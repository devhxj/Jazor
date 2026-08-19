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

    private static VueEventHandlers<MouseEvent> CreateSearchActionEvents()
        => new()
        {
            ["onClick"] = OpenSearchRoute
        };

    private static VueEventHandlers<MouseEvent> CreateQuickStartEvents()
        => new()
        {
            ["onClick"] = OpenQuickStart
        };

    private static VueEventHandlers<MouseEvent> CreatePageFeedbackEvents()
        => new()
        {
            ["onClick"] = OnPageFeedbackClick
        };

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

    // 目录侧边栏（右侧 TOC 导航，置于 s-drawer 的 end 槽位） / TOC rail in the s-drawer end slot
    private static IVNode TocRail(string title, IVNode[] links)
        => H("aside", new VueObject
        {
            Id = TocRailId,
            Class = "wiki-toc",
            Role = "navigation",
            Raw = new VueDictionary
            {
                ["slot"] = "end",
                ["aria-label"] = title
            }
        },
        [
            H("div", new VueObject { Class = "toc-card" },
            [
                H("div", new VueObject { Class = "toc-head" },
                [
                    H("p", new VueObject { Class = "rail-kicker" }, title),
                    H("s-icon-button", new VueObject
                    {
                        Class = "wiki-toc-close",
                        Title = "关闭目录",
                        Raw = new VueDictionary { ["aria-label"] = "关闭目录" },
                        Events = CreateCloseDrawersEvents()
                    },
                    [
                        H("s-icon", new VueObject { Raw = new VueDictionary { ["name"] = "close" } }, "")
                    ])
                ]),
                H("p", new VueObject { Class = "rail-copy" }, "稳定的锚点是文档契约的一部分，应视为面向用户的入口点。"),
                H("div", new VueObject { Class = "toc-links" }, links)
            ])
        ]);

    // 空目录侧边栏（页面未注册时显示） / Empty TOC rail (shown when page is not registered)
    private static IVNode EmptyTocRail()
        => H("aside", new VueObject
        {
            Id = TocRailId,
            Class = "wiki-toc wiki-toc-empty",
            Raw = new VueDictionary
            {
                ["slot"] = "end"
            }
        },
        [
            H("div", new VueObject { Class = "toc-card" },
            [
                H("p", new VueObject { Class = "rail-kicker" }, "缺失页面"),
                H("p", new VueObject { Class = "rail-copy" }, "请求的路由未在当前 Wiki 路由映射中注册。")
            ])
        ]);

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

    // 反馈按钮（有帮助/需改进，MD3 s-button） / Feedback button (helpful/needs-work)
    private static IVNode FeedbackButton(string label, string value, string currentValue)
    {
        var className = "feedback-button";
        if (currentValue == value)
            className += " feedback-button-active";

        return H("s-button", new VueObject
        {
            Class = className,
            Type = value == "helpful" ? "filled-tonal" : "outlined",
            Raw = new VueDictionary
            {
                ["value"] = value,
                ["selected"] = currentValue == value ? "true" : "false"
            },
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
                H("s-button", new VueObject
                {
                    Class = permalinkClassName,
                    Type = "text",
                    Raw = new VueDictionary { ["value"] = id },
                    Title = permalinkTitle,
                    Events = CreateSectionPermalinkEvents()
                }, permalinkLabel)
            ]),
            H("div", new VueObject { Class = "section-body" }, content)
        ]);
    }

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
                H("s-button", new VueObject
                {
                    Class = copyClassName,
                    Type = "text",
                    Raw = new VueDictionary { ["value"] = codeBlockId },
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
