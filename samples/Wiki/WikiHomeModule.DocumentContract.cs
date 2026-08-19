// WikiHomeModule.DocumentContract.cs - 文档契约接口 / Document contract interface
// 暴露给服务端（WikiHostShell）的文档元数据查询方法
// Exposes document metadata query methods to the server-side (WikiHostShell)

namespace Wiki;

public static partial class WikiHomeModule
{
    // 规范化请求路径 / Normalize request path
    internal static string NormalizeRequestPath(string pathname)
        => NormalizePath(pathname);

    // 规范化搜索查询 / Normalize search query
    internal static string NormalizeRequestSearchQuery(string query)
        => NormalizeSearchQuery(query);

    // 判断是否为搜索路由 / Check if current path is search route
    internal static bool IsSearchRoute(string currentPath)
        => currentPath == SearchPath;

    // 判断是否为已注册文档路径 / Check if current path is a registered document path
    internal static bool IsRegisteredDocumentPath(string currentPath)
        => currentPath == SearchPath || IsKnownPage(currentPath);

    // 判断是否可被搜索引擎索引 / Check if current path is indexable by search engines
    internal static bool IsIndexableDocumentPath(string currentPath)
        => currentPath != SearchPath && IsKnownPage(currentPath);

    // 获取 robots 指令 / Get robots directive
    internal static string GetDocumentRobotsDirective(string currentPath)
        => IsIndexableDocumentPath(currentPath)
            ? "index, follow"
            : "noindex, nofollow";

    // 获取页面标题（用于 HTML title） / Get page title (for HTML title)
    internal static string GetDocumentPageTitle(string currentPath, string currentSearchQuery)
    {
        if (currentPath == SearchPath)
            return currentSearchQuery.Length == 0 ? "搜索" : "搜索: " + currentSearchQuery;

        if (IsKnownPage(currentPath))
            return GetPageTitle(currentPath);

        return "页面未找到";
    }

    // 获取页面摘要（用于 meta description） / Get page summary (for meta description)
    internal static string GetDocumentPageSummary(string currentPath, string currentSearchQuery)
    {
        if (currentPath == SearchPath)
        {
            // 空查询时与目录摘要保持一致 / Empty query delegates to the catalog summary
            return currentSearchQuery.Length == 0
                ? GetPageSummary(SearchPath)
                : "搜索结果：\"" + currentSearchQuery + "\"，覆盖路由元数据、标签、页面正文和章节标题。";
        }

        if (IsKnownPage(currentPath))
            return GetPageSummary(currentPath);

        return "当前路径未在 Wiki 页面目录中注册。";
    }
}
