using System;
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private const int TotalPageCount = 5;

    private static bool IsKnownPage(string currentPath)
        => GetPageIndex(currentPath) >= 0;

    private static int GetPageIndex(string currentPath)
    {
        if (currentPath == OverviewPath)
            return 0;

        if (currentPath == GettingStartedPath)
            return 1;

        if (currentPath == ContentModelPath)
            return 2;

        if (currentPath == HFunctionAuthoringPath)
            return 3;

        if (currentPath == DeploymentPath)
            return 4;

        return -1;
    }

    private static string GetPagePath(int pageIndex)
    {
        if (pageIndex == 0)
            return OverviewPath;

        if (pageIndex == 1)
            return GettingStartedPath;

        if (pageIndex == 2)
            return ContentModelPath;

        if (pageIndex == 3)
            return HFunctionAuthoringPath;

        if (pageIndex == 4)
            return DeploymentPath;

        return "";
    }

    private static bool MatchesPageFilter(string currentPath, string filterText)
    {
        if (filterText.Length == 0)
            return true;

        return currentPath.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
               GetPageGroup(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
               GetPageTitle(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
               GetPageSummary(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
               GetPageStatus(currentPath).Contains(filterText, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetPageGroup(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex <= 2)
            return "Foundation";

        if (pageIndex == 3)
            return "Engineering";

        return "Operations";
    }

    private static string GetPageTitle(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex == 0)
            return "Overview";

        if (pageIndex == 1)
            return "Getting Started";

        if (pageIndex == 2)
            return "Content Model";

        if (pageIndex == 3)
            return "H-Function Authoring";

        return "Deployment";
    }

    private static string GetPageSummary(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex == 0)
            return "A production-oriented docs shell for Jazor, authored entirely with ECMAScript.Vue3 H functions.";

        if (pageIndex == 1)
            return "Run the site locally, understand the route model, and validate the emitted Wiki host end to end.";

        if (pageIndex == 2)
            return "Code-first page metadata, explicit sections, and a navigation contract that stays readable in C#.";

        if (pageIndex == 3)
            return "Why H functions are the production authoring surface for this Wiki, and the conventions that keep it maintainable.";

        return "Build outputs, fallback routing, smoke verification, and the static delivery contract for Wiki.";
    }

    private static string GetPageStatus(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex == 0)
            return "Real Project MVP";

        if (pageIndex == 1)
            return "Foundation";

        if (pageIndex == 2)
            return "Authoring";

        if (pageIndex == 3)
            return "Engineering";

        return "Operations";
    }

    private static string GetPreviousPath(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex <= 0)
            return "";

        return GetPagePath(pageIndex - 1);
    }

    private static string GetNextPath(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex < 0 || pageIndex >= TotalPageCount - 1)
            return "";

        return GetPagePath(pageIndex + 1);
    }

    private static IVNode TocRail(string currentPath, string currentHash)
    {
        var pageIndex = GetPageIndex(currentPath);
        if (pageIndex == 0)
        {
            return TocRail("On this page",
            [
                TocLink(OverviewPath, "what-ships-now", "What ships now", currentHash),
                TocLink(OverviewPath, "why-this-exists", "Why this exists", currentHash),
                TocLink(OverviewPath, "mvp-boundary", "MVP boundary", currentHash),
                TocLink(OverviewPath, "site-structure", "Site structure", currentHash)
            ]);
        }

        if (pageIndex == 1)
        {
            return TocRail("On this page",
            [
                TocLink(GettingStartedPath, "boot-the-site", "Boot the site", currentHash),
                TocLink(GettingStartedPath, "route-model", "Route model", currentHash),
                TocLink(GettingStartedPath, "add-a-page", "Add a page", currentHash),
                TocLink(GettingStartedPath, "verify-the-result", "Verify the result", currentHash)
            ]);
        }

        if (pageIndex == 2)
        {
            return TocRail("On this page",
            [
                TocLink(ContentModelPath, "page-contract", "Page contract", currentHash),
                TocLink(ContentModelPath, "navigation-contract", "Navigation contract", currentHash),
                TocLink(ContentModelPath, "editing-rules", "Editing rules", currentHash)
            ]);
        }

        if (pageIndex == 3)
        {
            return TocRail("On this page",
            [
                TocLink(HFunctionAuthoringPath, "layout-composition", "Layout composition", currentHash),
                TocLink(HFunctionAuthoringPath, "production-rules", "Production rules", currentHash),
                TocLink(HFunctionAuthoringPath, "why-this-works", "Why this works", currentHash)
            ]);
        }

        return TocRail("On this page",
        [
            TocLink(DeploymentPath, "build-output", "Build output", currentHash),
            TocLink(DeploymentPath, "route-fallback", "Route fallback", currentHash),
            TocLink(DeploymentPath, "operational-checks", "Operational checks", currentHash)
        ]);
    }
}
