namespace Wiki;

public static partial class WikiHomeModule
{
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
}
