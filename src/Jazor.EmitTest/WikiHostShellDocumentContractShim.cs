using System;

namespace Wiki;

// Jazor.EmitTest compiles the real WikiHostShell source directly but only needs
// the document-routing contract, not the full Vue-authored WikiHomeModule surface.
public static partial class WikiHomeModule
{
    private const string OverviewPath = "/";
    private const string SearchPath = "/search";
    private const string GettingStartedPath = "/guides/getting-started";

    private static readonly string[] PagePaths =
    [
        OverviewPath,
        GettingStartedPath
    ];

    private static readonly string[] PageTitles =
    [
        "Overview",
        "Getting Started"
    ];

    private static readonly string[] PageSummaries =
    [
        "A production-oriented docs shell for Jazor, authored entirely with ECMAScript.Vue H functions.",
        "Run the site locally, understand the route model, and validate the emitted Wiki host end to end."
    ];

    internal static string NormalizeRequestPath(string pathname)
        => NormalizePath(pathname);

    internal static string NormalizeRequestSearchQuery(string query)
        => NormalizeSearchQuery(query);

    internal static bool IsSearchRoute(string currentPath)
        => currentPath == SearchPath;

    internal static bool IsRegisteredDocumentPath(string currentPath)
        => currentPath == SearchPath || IsKnownPage(currentPath);

    internal static bool IsIndexableDocumentPath(string currentPath)
        => currentPath != SearchPath && IsKnownPage(currentPath);

    internal static string GetDocumentRobotsDirective(string currentPath)
        => IsIndexableDocumentPath(currentPath)
            ? "index, follow"
            : "noindex, nofollow";

    internal static string GetDocumentPageTitle(string currentPath, string currentSearchQuery)
    {
        if (currentPath == SearchPath)
            return currentSearchQuery.Length == 0 ? "Search" : "Search: " + currentSearchQuery;

        if (IsKnownPage(currentPath))
            return GetPageTitle(currentPath);

        return "Page Not Found";
    }

    internal static string GetDocumentPageSummary(string currentPath, string currentSearchQuery)
    {
        if (currentPath == SearchPath)
        {
            return currentSearchQuery.Length == 0
                ? "Search the full Wiki corpus by subsystem, route fragment, workflow, or tag."
                : "Search results for \"" + currentSearchQuery + "\" across route metadata, tags, curated page body text, and section titles.";
        }

        if (IsKnownPage(currentPath))
            return GetPageSummary(currentPath);

        return "The current path is not registered in the Wiki page catalog.";
    }

    private static string NormalizePath(string pathname)
    {
        if (pathname.Length == 0)
            return OverviewPath;

        var normalized = pathname;
        if (normalized == "/index.html")
            normalized = OverviewPath;
        else if (normalized.EndsWith("/index.html", StringComparison.Ordinal))
            normalized = normalized[..^"/index.html".Length];

        if (normalized.Length > 1 && normalized.EndsWith("/", StringComparison.Ordinal))
            normalized = normalized[..^1];

        return normalized.Length == 0 ? OverviewPath : normalized;
    }

    private static string NormalizeSearchQuery(string query)
        => query.Trim();

    private static bool IsKnownPage(string currentPath)
        => GetPageIndex(currentPath) >= 0;

    private static int GetPageIndex(string currentPath)
        => Array.IndexOf(PagePaths, currentPath);

    private static string GetPageTitle(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        return pageIndex >= 0 && pageIndex < PageTitles.Length
            ? PageTitles[pageIndex]
            : "Unregistered page";
    }

    private static string GetPageSummary(string currentPath)
    {
        var pageIndex = GetPageIndex(currentPath);
        return pageIndex >= 0 && pageIndex < PageSummaries.Length
            ? PageSummaries[pageIndex]
            : "The requested path is not part of the registered Wiki page catalog.";
    }
}
