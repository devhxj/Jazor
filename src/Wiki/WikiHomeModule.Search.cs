using System.Collections.Generic;
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static readonly string[] FeaturedSearchTags =
    [
        "compiler",
        "jolt",
        "razorvue",
        "vueroute",
        "runtime",
        "catalog",
        "smoke"
    ];

    private static IVNode SearchBody()
    {
        var query = GetCurrentSearchQueryRef()?.Value ?? "";
        return H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("full-text", "Full-text search",
            [
                SearchInputCard(query),
                SearchPageResults(query)
            ]),
            PageSection("section-hits", "Section matches",
            [
                SearchSectionResults(query)
            ]),
            PageSection("topic-entry", "Topic entry points",
            [
                H("p", "Use tags when you know the concern but not the exact page title."),
                SearchTagRow(FeaturedSearchTags),
                RouteCardGrid([TopicIndexPath, GlossaryPath, TroubleshootingPath, CompilerOverviewPath, JoltHostPath, RazorVueLibraryModePath, VueRouteBindingsPath])
            ]),
            PageSection("query-sharing", "Shareable queries",
            [
                H("p", "Search lives on a real route with a real `?q=` query parameter. That makes results refresh-safe, linkable, and easy to hand to another contributor."),
                CodeBlock("Current search URL", BuildSearchShareUrl(query)),
                H("p", "The page-level copy action in the hero will copy the full search URL, including the current query.")
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
                    Placeholder = "Search compiler, runtime, Jolt, RazorVue, verification...",
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
                }, "Clear")
            ]),
            H("p", new VueObject { Class = "search-status" }, GetSearchStatus(query)),
            H("p", new VueObject { Class = "search-hint" }, "Query matches page titles, summaries, tags, statuses, route paths, and curated page body text.")
        ]);

    private static string GetSearchStatus(string query)
    {
        if (query.Length == 0)
            return "Search the full Wiki corpus by keyword, subsystem, or workflow.";

        var pageResults = BuildPageSearchResults(query);
        var sectionResults = BuildSectionSearchResults(query);
        return pageResults.Count + " page results and " + sectionResults.Count + " section matches for \"" + query + "\".";
    }

    private static IVNode SearchPageResults(string query)
    {
        if (query.Length == 0)
        {
            return H("div", new VueObject { Class = "search-empty-state" },
            [
                H("p", new VueObject { Class = "search-empty-title" }, "Start with a route or subsystem name."),
                H("p", new VueObject { Class = "search-empty-summary" }, "Useful starting points are `compiler`, `jolt`, `razorvue`, `vueroute`, `runtime`, `catalog`, and `smoke`."),
                RouteCardGrid([GettingStartedPath, ProjectLinesPath, CompilerOverviewPath, RuntimeCatalogPath])
            ]);
        }

        var pageResults = BuildPageSearchResults(query);
        if (pageResults.Count == 0)
        {
            return H("div", new VueObject { Class = "search-empty-state" },
            [
                H("p", new VueObject { Class = "search-empty-title" }, "No page-level results matched."),
                H("p", new VueObject { Class = "search-empty-summary" }, "Try a subsystem name, a route fragment, or one of the tags below.")
            ]);
        }

        return H("div", new VueObject { Class = "search-result-list" }, pageResults.ToArray());
    }

    private static IVNode SearchSectionResults(string query)
    {
        if (query.Length == 0)
        {
            return H("p", new VueObject { Class = "search-section-summary" }, "Section matches appear after you enter a query.");
        }

        var sectionResults = BuildSectionSearchResults(query);
        if (sectionResults.Count == 0)
        {
            return H("div", new VueObject { Class = "search-empty-state" },
            [
                H("p", new VueObject { Class = "search-empty-title" }, "No section-level matches found."),
                H("p", new VueObject { Class = "search-empty-summary" }, "Try a broader keyword or open the glossary and topic index to pivot by concept.")
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
                    H("span", new VueObject { Class = "search-result-kind" }, "Page")
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
                        H("span", new VueObject { Class = "search-result-kind" }, "Section")
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
