// WikiHostDiscoveryDocuments.cs - SEO 发现文档处理器 / SEO discovery document handler
// 动态生成 robots.txt 和 sitemap.xml / Dynamically generates robots.txt and sitemap.xml

using System.Text;
using Microsoft.Extensions.Configuration;

namespace Wiki;

internal static class WikiHostDiscoveryDocuments
{
    // 路由分发：根据请求路径返回 robots.txt 或 sitemap.xml / Route dispatch based on request path
    internal static async Task<bool> TryHandleAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            return false;

        if (context.Request.Path.Equals("/robots.txt", StringComparison.OrdinalIgnoreCase))
        {
            await WriteRobotsAsync(context, cancellationToken);
            return true;
        }

        if (context.Request.Path.Equals("/sitemap.xml", StringComparison.OrdinalIgnoreCase))
        {
            await WriteSitemapAsync(context, cancellationToken);
            return true;
        }

        return false;
    }

    // 生成 robots.txt / Generate robots.txt
    private static async Task WriteRobotsAsync(HttpContext context, CancellationToken cancellationToken)
    {
        var siteOrigin = BuildSiteOrigin(context.Request, context.RequestServices.GetRequiredService<IConfiguration>());
        var body = "User-agent: *\nAllow: /\nSitemap: " + siteOrigin + "/sitemap.xml\n";

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "text/plain; charset=utf-8";
        WikiHostShell.ApplyDiscoveryDocumentHeaders(context.Response.Headers);

        if (HttpMethods.IsHead(context.Request.Method))
            return;

        await context.Response.WriteAsync(body, cancellationToken);
    }

    // 生成 sitemap.xml / Generate sitemap.xml
    private static async Task WriteSitemapAsync(HttpContext context, CancellationToken cancellationToken)
    {
        var siteOrigin = BuildSiteOrigin(context.Request, context.RequestServices.GetRequiredService<IConfiguration>());
        var builder = new StringBuilder();
        builder.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""");
        builder.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""");

        // 遍历所有可索引页面 / Iterate all indexable pages
        for (var pageIndex = 0; pageIndex < WikiHomeModule.PagePaths.Length; pageIndex++)
        {
            if (!WikiHomeModule.IsIndexableDocumentPath(WikiHomeModule.PagePaths[pageIndex]))
                continue;

            builder.Append("  <url><loc>");
            builder.Append(System.Security.SecurityElement.Escape(siteOrigin + WikiHomeModule.PagePaths[pageIndex]));
            builder.Append("</loc><lastmod>");
            builder.Append(WikiHomeModule.PageLastUpdatedDates[pageIndex]);
            builder.AppendLine("</lastmod></url>");
        }

        builder.AppendLine("</urlset>");

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/xml; charset=utf-8";
        WikiHostShell.ApplyDiscoveryDocumentHeaders(context.Response.Headers);

        if (HttpMethods.IsHead(context.Request.Method))
            return;

        await context.Response.WriteAsync(builder.ToString(), cancellationToken);
    }

    // 构建站点 Origin URL / Build site origin URL
    private static string BuildSiteOrigin(HttpRequest request, IConfiguration configuration)
        => WikiHostShell.BuildSiteOrigin(request, configuration);
}
