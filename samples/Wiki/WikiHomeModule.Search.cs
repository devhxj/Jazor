// WikiHomeModule.Search.cs - 搜索 / Search
using System.Collections.Generic;
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 精选搜索标签 / Featured search tags
    private static readonly string[] FeaturedSearchTags =
    [
        "compiler",
        "razor-sg",
        "razorvue",
        "vueroute",
        "runtime",
        "catalog",
        "smoke"
    ];

    // 构建搜索页面主体 / Build the search page body
    private static IVNode SearchBody()
    {
        var query = GetCurrentSearchQueryRef()?.Value ?? "";
        return H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("full-text", "全文搜索",
            [
                SearchInputCard(query),
                SearchPageResults(query)
            ]),
            PageSection("section-hits", "章节匹配",
            [
                SearchSectionResults(query)
            ]),
            PageSection("topic-entry", "主题入口",
            [
                H("p", "当你知道关注点但不知道确切页面标题时，使用标签。"),
                SearchTagRow(FeaturedSearchTags),
                RouteCardGrid([TopicIndexPath, GlossaryPath, TroubleshootingPath, CompilerOverviewPath, RazorVueLibraryModePath, VueRouteBindingsPath])
            ]),
            PageSection("query-sharing", "可分享查询",
            [
                H("p", "搜索运行在真实路由上，带有真实的 `?q=` 查询参数。这使得结果刷新安全、可链接，且易于分享给其他贡献者。"),
                CodeBlock("当前搜索 URL", BuildSearchShareUrl(query)),
                H("p", "主视觉区中的页面级复制操作将复制完整的搜索 URL，包括当前查询。")
            ])
        ]);
    }

    private static IVNode SearchInputCard(string query)
        => H("div", new VueObject { Class = "search-shell-card" },
        [
            H("div", new VueObject { Class = "search-row" },
            [
                H("input", new VueObject
                {
                    Id = SearchInputId,
                    Class = "search-input",
                    Type = "search",
                    Placeholder = "搜索编译器、运行时、Razor SG、RazorVue、验证...",
                    Autocomplete = "off",
                    Value = query,
                    Events = CreateSearchInputEvents()
                }),
                H("button", new VueObject
                {
                    Class = "search-clear",
                    Type = "button",
                    Disabled = query.Length == 0,
                    Events = CreateClearSearchEvents()
                }, "清除")
            ]),
            H("p", new VueObject { Class = "search-status" }, GetSearchStatus(query)),
            H("p", new VueObject { Class = "search-hint" }, "查询匹配页面标题、摘要、标签、状态、路由路径和策划的页面正文文本。")
        ]);

    // 获取搜索状态文本 / Get search status text
    private static string GetSearchStatus(string query)
    {
        if (query.Length == 0)
            return "通过关键词、子系统或工作流搜索完整 Wiki 语料库。";

        var pageResults = BuildPageSearchResults(query);
        var sectionResults = BuildSectionSearchResults(query);
        return pageResults.Count + " 个页面结果和 " + sectionResults.Count + " 个章节匹配 \"" + query + "\"。";
    }

    // 构建页面搜索结果 / Build page search results
    private static IVNode SearchPageResults(string query)
    {
        if (query.Length == 0)
        {
            return H("div", new VueObject { Class = "search-empty-state" },
            [
                H("p", new VueObject { Class = "search-empty-title" }, "从路由或子系统名称开始。"),
                H("p", new VueObject { Class = "search-empty-summary" }, "有用的起点包括 `compiler`、`razor-sg`、`razorvue`、`vueroute`、`runtime`、`catalog` 和 `smoke`。"),
                RouteCardGrid([GettingStartedPath, ProjectLinesPath, CompilerOverviewPath, RuntimeCatalogPath])
            ]);
        }

        var pageResults = BuildPageSearchResults(query);
        if (pageResults.Count == 0)
        {
            return H("div", new VueObject { Class = "search-empty-state" },
            [
                H("p", new VueObject { Class = "search-empty-title" }, "没有匹配的页面级结果。"),
                H("p", new VueObject { Class = "search-empty-summary" }, "尝试子系统名称、路由片段或下方标签。")
            ]);
        }

        return H("div", new VueObject { Class = "search-result-list" }, pageResults.ToArray());
    }

    // 构建章节搜索结果 / Build section search results
    private static IVNode SearchSectionResults(string query)
    {
        if (query.Length == 0)
        {
            return H("p", new VueObject { Class = "search-section-summary" }, "输入查询后显示章节匹配。");
        }

        var sectionResults = BuildSectionSearchResults(query);
        if (sectionResults.Count == 0)
        {
            return H("div", new VueObject { Class = "search-empty-state" },
            [
                H("p", new VueObject { Class = "search-empty-title" }, "没有找到章节级匹配。"),
                H("p", new VueObject { Class = "search-empty-summary" }, "尝试更宽泛的关键词，或打开术语表和主题索引按概念切换。")
            ]);
        }

        return H("div", new VueObject { Class = "search-result-list search-result-list-sections" }, sectionResults.ToArray());
    }

    private static List<IVNode> BuildPageSearchResults(string query)
    {
        var results = new List<IVNode>();
        for (var pageIndex = 0; pageIndex < PagePaths.Length; pageIndex++)
        {
            var path = PagePaths[pageIndex];
            if (path == SearchPath || !PageMatchesSearch(path, query))
                continue;

            var snippet = ExtractSearchSnippet(GetPageSearchBody(path), query, GetPageSummary(path));
            results.Add(H("a", new VueObject
            {
                Class = "search-result-card",
                Href = BuildBrowserUrl(path, "", ""),
                Events = CreateRouteClickEvents()
            },
            [
                H("div", new VueObject { Class = "search-result-meta" },
                [
                    H("span", new VueObject { Class = "search-result-group" }, GetPageGroup(path)),
                    H("span", new VueObject { Class = "search-result-kind" }, "页面")
                ]),
                H("h3", new VueObject { Class = "search-result-title" }, HighlightText(GetPageTitle(path), query)),
                H("p", new VueObject { Class = "search-result-snippet" }, HighlightText(snippet, query)),
                H("div", new VueObject { Class = "search-result-footer" },
                [
                    H("code", new VueObject { Class = "search-result-path" }, path),
                    H("span", new VueObject { Class = "search-result-status" }, GetPageStatus(path))
                ])
            ]));
        }

        return results;
    }

    private static List<IVNode> BuildSectionSearchResults(string query)
    {
        var results = new List<IVNode>();
        for (var pageIndex = 0; pageIndex < PagePaths.Length; pageIndex++)
        {
            var path = PagePaths[pageIndex];
            if (path == SearchPath)
                continue;

            var sectionIds = GetPageSectionIds(pageIndex);
            var sectionTitles = GetPageSectionTitles(pageIndex);
            for (var sectionIndex = 0; sectionIndex < sectionIds.Length && sectionIndex < sectionTitles.Length; sectionIndex++)
            {
                var sectionId = sectionIds[sectionIndex];
                var sectionTitle = sectionTitles[sectionIndex];
                if (!MatchesSearchQuery(sectionId, query) && !MatchesSearchQuery(sectionTitle, query))
                    continue;

                results.Add(H("a", new VueObject
                {
                    Class = "search-result-card search-result-card-section",
                    Href = BuildBrowserUrl(path, sectionId, ""),
                    Events = CreateTocClickEvents()
                },
                [
                    H("div", new VueObject { Class = "search-result-meta" },
                    [
                        H("span", new VueObject { Class = "search-result-group" }, GetPageTitle(path)),
                        H("span", new VueObject { Class = "search-result-kind" }, "章节")
                    ]),
                    H("h3", new VueObject { Class = "search-result-title" }, HighlightText(sectionTitle, query)),
                    H("p", new VueObject { Class = "search-result-snippet" }, HighlightText(GetPageSummary(path), query)),
                    H("div", new VueObject { Class = "search-result-footer" },
                    [
                        H("code", new VueObject { Class = "search-result-path" }, path + "#" + sectionId),
                        H("span", new VueObject { Class = "search-result-status" }, GetPageGroup(path))
                    ])
                ]));
            }
        }

        return results;
    }

    private static bool PageMatchesSearch(string path, string query)
    {
        if (MatchesSearchQuery(path, query) ||
            MatchesSearchQuery(GetPageGroup(path), query) ||
            MatchesSearchQuery(GetPageTitle(path), query) ||
            MatchesSearchQuery(GetPageSummary(path), query) ||
            MatchesSearchQuery(GetPageStatus(path), query) ||
            MatchesSearchQuery(GetPageOwner(path), query) ||
            MatchesSearchQuery(GetPageAudience(path), query) ||
            MatchesSearchQuery(GetPageSourceFile(path), query) ||
            MatchesSearchQuery(GetPageLastUpdated(path), query) ||
            MatchesSearchQuery(GetPageSearchBody(path), query))
        {
            return true;
        }

        var tags = GetPageTags(path);
        for (var tagIndex = 0; tagIndex < tags.Length; tagIndex++)
        {
            if (MatchesSearchQuery(tags[tagIndex], query))
                return true;
        }

        return false;
    }

    private static bool MatchesSearchQuery(string text, string query)
    {
        if (query.Length == 0 || text.Length == 0)
            return false;

        return text.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string ExtractSearchSnippet(string text, string query, string fallback)
    {
        if (text.Length == 0)
            return fallback;

        var matchIndex = text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (matchIndex < 0)
            return fallback;

        var start = matchIndex - 42;
        if (start < 0)
            start = 0;

        var length = 120;
        if (start + length > text.Length)
            length = text.Length - start;

        var snippet = text.Substring(start, length).Trim();
        if (start > 0)
            snippet = "..." + snippet;
        if (start + length < text.Length)
            snippet += "...";

        return snippet;
    }

    private static string BuildSearchShareUrl(string query)
    {
        var relativeUrl = BuildSearchRoute(query);
        var location = ECMAScript.Global.Document.Location;
        if (location == null)
            return relativeUrl;

        return location.Origin + relativeUrl;
    }

    private static IVNode SearchTagRow(string[] tags)
    {
        var links = new List<IVNode>();
        for (var tagIndex = 0; tagIndex < tags.Length; tagIndex++)
            links.Add(TagLink(tags[tagIndex]));

        return H("div", new VueObject { Class = "tag-row" }, links.ToArray());
    }

    private static IVNode[] HighlightText(string text, string query)
    {
        if (query.Length == 0 || text.Length == 0)
            return [H("span", text)];

        var results = new List<IVNode>();
        var remainingIndex = 0;
        while (remainingIndex < text.Length)
        {
            var matchIndex = text.IndexOf(query, remainingIndex, StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0)
            {
                results.Add(H("span", text.Substring(remainingIndex)));
                break;
            }

            if (matchIndex > remainingIndex)
                results.Add(H("span", text.Substring(remainingIndex, matchIndex - remainingIndex)));

            results.Add(H("mark", new VueObject { Class = "search-mark" }, text.Substring(matchIndex, query.Length)));
            remainingIndex = matchIndex + query.Length;
        }

        return results.ToArray();
    }
}
