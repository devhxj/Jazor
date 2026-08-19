// WikiHomeModule.RouteContract.cs - Wiki 页面路由查询辅助 / Wiki page route lookup helpers
// 路由元数据数组（PagePaths/PageGroups/...）由 obj/wiki/WikiDocsContent.g.cs 从 docs/ 生成；
// 本文件只保留目录查询、分页、404 建议和 TOC 渲染逻辑。
using System.Collections.Generic;
using ECMAScript;
using static ECMAScript.Vue;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 生成数据与查询逻辑分层：本层保留原 RouteContract 的并行数组形状，
    // 所有数据只从 docs 编译出的 WikiDocsContent 读取。
    // Use static getters rather than field initializers. Jazor emits the generated class later
    // in the ESM module, so eager field aliases would read it inside its JavaScript TDZ.
    internal static string[] PagePaths => WikiDocsContent.PagePaths;
    internal static string[] PageGroups => WikiDocsContent.PageGroups;
    internal static string[] PageTitles => WikiDocsContent.PageTitles;
    internal static string[] PageSummaries => WikiDocsContent.PageSummaries;
    internal static string[] PageStatuses => WikiDocsContent.PageStatuses;
    internal static string[] PageOwners => WikiDocsContent.PageOwners;
    internal static string[] PageAudiences => WikiDocsContent.PageAudiences;
    internal static string[] PageSourceFiles => WikiDocsContent.PageSourceFiles;
    internal static string[] PageLastUpdatedDates => WikiDocsContent.PageLastUpdatedDates;
    internal static int[] PageReadingMinutes => WikiDocsContent.PageReadingMinutes;
    internal static string[] PageSearchBodies => WikiDocsContent.PageSearchBodies;
    internal static string[][] PageTagSets => WikiDocsContent.PageTagSets;
    internal static DocsBlock[][] PageBlockSets => WikiDocsContent.PageBlockSets;
    internal static string[][] PageSectionIdSets => WikiDocsContent.PageSectionIdSets;
    internal static string[][] PageSectionTitleSets => WikiDocsContent.PageSectionTitleSets;
    internal static string[][] PageRelatedPathSets => WikiDocsContent.PageRelatedPathSets;
    internal static string[] NavGroupIds => WikiDocsContent.NavGroupIds;
    internal static string[] NavGroupLabels => WikiDocsContent.NavGroupLabels;
    internal static string[] NavGroupLandingPaths => WikiDocsContent.NavGroupLandingPaths;

    private static string GetGroupLabel(string groupId)
        => WikiDocsContent.GetGroupLabel(groupId);

    // ── 目录查询与索引 / Catalog lookup and indexing ──
    private static int TotalPageCount => PagePaths.Length;

    private static bool IsKnownPage(string currentPath)
        => GetPageIndex(currentPath) >= 0;

    private static int GetPageIndex(string currentPath)
    {
        for (var pageIndex = 0; pageIndex < PagePaths.Length; pageIndex++)
        {
            if (PagePaths[pageIndex] == currentPath)
                return pageIndex;
        }

        return -1;
    }

    private static string GetPagePath(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PagePaths.Length)
            return PagePaths[pageIndex];

        return "";
    }

    // ── 导航筛选与搜索 / Nav filtering and search ──
    private static bool MatchesPageFilter(string currentPath, string filterText)
    {
        if (filterText.Length == 0)
            return true;

        if (currentPath.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageGroup(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageGroupLabel(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageTitle(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageSummary(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageStatus(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageOwner(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageAudience(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageSourceFile(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageLastUpdated(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
            GetPageSearchBody(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var tags = GetPageTags(currentPath);
        for (var tagIndex = 0; tagIndex < tags.Length; tagIndex++)
        {
            if (tags[tagIndex].Contains(filterText, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    // ── 单字段 getter / Single-field getters ──
    private static string GetPageGroup(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageGroups.Length)
            return PageGroups[pageIndex];

        return "Unregistered";
    }

    private static string GetPageGroupLabel(string currentPath)
        => GetGroupLabel(GetPageGroup(currentPath));

    private static string GetPageTitle(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageTitles.Length)
            return PageTitles[pageIndex];

        return "Unregistered page";
    }

    private static string GetPageSummary(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageSummaries.Length)
            return PageSummaries[pageIndex];

        return "The requested path is not part of the registered Wiki page catalog.";
    }

    private static string GetPageStatus(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageStatuses.Length)
            return PageStatuses[pageIndex];

        return "Not Found";
    }

    private static string GetPageOwner(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageOwners.Length)
            return PageOwners[pageIndex];

        return "Unknown";
    }

    private static string GetPageAudience(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageAudiences.Length)
            return PageAudiences[pageIndex];

        return "Unknown";
    }

    private static string GetPageSourceFile(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageSourceFiles.Length)
            return PageSourceFiles[pageIndex];

        return "";
    }

    private static string GetPageLastUpdated(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageLastUpdatedDates.Length)
            return PageLastUpdatedDates[pageIndex];

        return "";
    }

    private static int GetPageReadingMinutes(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageReadingMinutes.Length)
            return PageReadingMinutes[pageIndex];

        return 0;
    }

    private static string GetPageSearchBody(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageSearchBodies.Length)
            return PageSearchBodies[pageIndex];

        return "";
    }

    private static string[] GetPageTags(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex >= 0 && pageIndex < PageTagSets.Length)
            return PageTagSets[pageIndex];

        return [];
    }

    // ── 前后页面导航 / Previous/next page navigation ──
    private static string GetPreviousPath(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex < 0)
            return "";

        for (var previousIndex = pageIndex - 1; previousIndex >= 0; previousIndex--)
        {
            if (PagePaths[previousIndex] != SearchPath)
                return PagePaths[previousIndex];
        }

        return "";
    }

    private static string GetNextPath(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex < 0)
            return "";

        for (var nextIndex = pageIndex + 1; nextIndex < TotalPageCount; nextIndex++)
        {
            if (PagePaths[nextIndex] != SearchPath)
                return PagePaths[nextIndex];
        }

        return "";
    }

    // ── 页面章节与正文获取 / Page section and body retrieval ──
    private static string[] GetPageSectionIds(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PageSectionIdSets.Length)
            return PageSectionIdSets[pageIndex];

        return [];
    }

    private static string[] GetPageSectionTitles(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PageSectionTitleSets.Length)
            return PageSectionTitleSets[pageIndex];

        return [];
    }

    private static IVNode GetPageBody(int pageIndex)
    {
        // 搜索页正文手写在 Search.cs；docs 页正文来自生成数据渲染层
        if (pageIndex >= 0 && pageIndex < PagePaths.Length && PagePaths[pageIndex] == SearchPath)
            return SearchBody();

        if (pageIndex >= 0 && pageIndex < PageBlockSets.Length)
            return RenderDocsPage(pageIndex);

        return H("div", new VueObject { Class = "doc-body" }, []);
    }

    private static string[] GetPageRelatedPaths(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PageRelatedPathSets.Length)
            return PageRelatedPathSets[pageIndex];

        return [];
    }

    // ── 建议路径与 404 恢复 / Suggested paths and 404 recovery ──
    private static string[] GetSuggestedPaths(string currentPath)
    {
        var fragment = GetRouteFragment(currentPath);
        var suggestions = new List<string>();

        if (fragment.Length > 0)
        {
            for (var pageIndex = 0; pageIndex < PagePaths.Length; pageIndex++)
            {
                var pagePath = PagePaths[pageIndex];
                if (MatchesPageFilter(pagePath, fragment))
                    suggestions.Add(pagePath);
            }
        }

        if (suggestions.Count == 0)
        {
            var requestedGroup = GetRequestedGroup(currentPath);
            if (requestedGroup.Length > 0)
            {
                for (var pageIndex = 0; pageIndex < PagePaths.Length; pageIndex++)
                {
                    var pagePath = PagePaths[pageIndex];
                    if (GetPageGroup(pagePath) == requestedGroup)
                        suggestions.Add(pagePath);
                }
            }
        }

        if (suggestions.Count == 0)
            return [OverviewPath, SearchPath, NavGroupLandingPaths.Length > 0 ? NavGroupLandingPaths[0] : OverviewPath];

        if (suggestions.Count > 3)
            suggestions.RemoveRange(3, suggestions.Count - 3);

        return suggestions.ToArray();
    }

    private static string GetRequestedGroup(string currentPath)
    {
        // 与生成路由前缀对齐：/overview /architecture /guides /roadmap /history
        for (var groupIndex = 0; groupIndex < NavGroupIds.Length; groupIndex++)
        {
            if (currentPath.StartsWith(NavGroupLandingPaths[groupIndex] + "/", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(currentPath, NavGroupLandingPaths[groupIndex], StringComparison.OrdinalIgnoreCase))
                return NavGroupIds[groupIndex];
        }

        return "";
    }

    private static string GetRouteFragment(string currentPath)
    {
        var normalizedPath = currentPath.Trim('/');
        if (normalizedPath.Length == 0)
            return "";

        var lastSlashIndex = normalizedPath.LastIndexOf('/');
        if (lastSlashIndex >= 0 && lastSlashIndex < normalizedPath.Length - 1)
            return normalizedPath.Substring(lastSlashIndex + 1);

        return normalizedPath;
    }

    // ── 目录侧边栏渲染 / TOC rail rendering ──
    private static IVNode TocRail(string currentPath, string currentHash)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex < 0)
            return EmptyTocRail();

        var sectionIds = GetPageSectionIds(pageIndex);
        var sectionTitles = GetPageSectionTitles(pageIndex);
        var links = new List<IVNode>();
        for (var sectionIndex = 0; sectionIndex < sectionIds.Length && sectionIndex < sectionTitles.Length; sectionIndex++)
            links.Add(TocLink(currentPath, sectionIds[sectionIndex], sectionTitles[sectionIndex], currentHash));

        return TocRail("本页目录", links.ToArray());
    }
}
