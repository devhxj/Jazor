// WikiHostShell.cs - 服务端 HTML 外壳渲染器 / Server-side HTML shell renderer
// 负责 HTML 模板渲染、安全头注入、CSP 策略、元数据 token 替换
// Handles HTML template rendering, security headers, CSP policy, metadata token replacement

using System.Security.Cryptography;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;
namespace Wiki;

internal static class WikiHostShell
{
    internal const string HtmlTemplateRelativePath = "host/index.template.html";

    // 模板 token 前缀 / Template token prefix
    private const string MetadataTokenPrefix = "__WIKI_";

    // 文档元数据 token / Document metadata tokens
    private const string TitleToken = "__WIKI_DOCUMENT_TITLE__";
    private const string DescriptionToken = "__WIKI_DOCUMENT_DESCRIPTION__";
    private const string CanonicalUrlToken = "__WIKI_DOCUMENT_CANONICAL_URL__";
    private const string OpenGraphTitleToken = "__WIKI_OPEN_GRAPH_TITLE__";
    private const string OpenGraphDescriptionToken = "__WIKI_OPEN_GRAPH_DESCRIPTION__";
    private const string OpenGraphUrlToken = "__WIKI_OPEN_GRAPH_URL__";
    private const string TwitterTitleToken = "__WIKI_TWITTER_TITLE__";
    private const string TwitterDescriptionToken = "__WIKI_TWITTER_DESCRIPTION__";
    private const string RobotsDirectiveToken = "__WIKI_DOCUMENT_ROBOTS__";
    private const string ContentSecurityPolicyToken = "__WIKI_CONTENT_SECURITY_POLICY__";
    private const string ScriptNonceToken = "__WIKI_SCRIPT_NONCE__";
    private const string PathBaseToken = "__WIKI_PATH_BASE__";
    private const string FaviconUrlToken = "__WIKI_FAVICON_URL__";
    private const string SiteCssUrlToken = "__WIKI_SITE_CSS_URL__";
    private const string MainModuleUrlToken = "__WIKI_MAIN_MODULE_URL__";
    private const string StyleImportUrlToken = "__WIKI_STYLE_IMPORT_URL__";
    private const string ComponentsImportBaseToken = "__WIKI_COMPONENTS_IMPORT_BASE__";
    private const string SystemImportBaseToken = "__WIKI_SYSTEM_IMPORT_BASE__";
    private const string VendorVueUrlToken = "__WIKI_VENDOR_VUE_URL__";
    private const string SoberUrlToken = "__WIKI_SOBER_URL__";
    // 缓存策略常量 / Cache policy constants
    private const string HtmlCacheControl = "no-cache, must-revalidate";
    private const string DiscoveryCacheControl = "public, max-age=300, must-revalidate";
    private const string MutableAssetCacheControl = "no-cache, must-revalidate";

    internal const string WikiPathBaseAttributeName = "data-wiki-path-base";

    internal static void ApplyDiscoveryDocumentHeaders(IHeaderDictionary headers)
    {
        headers["Cache-Control"] = DiscoveryCacheControl;
    }

    internal static Task WriteHtmlAsync(HttpContext context, CancellationToken cancellationToken = default)
        => WriteHtmlShellAsync(context, cancellationToken);

    private static async Task WriteHtmlShellAsync(HttpContext context, CancellationToken cancellationToken)
    {
        var normalizedPath = WikiHomeModule.NormalizeRequestPath(context.Request.Path.Value ?? "");
        var normalizedSearchQuery = WikiHomeModule.IsSearchRoute(normalizedPath)
            ? WikiHomeModule.NormalizeRequestSearchQuery(context.Request.Query["q"].ToString())
            : "";
        var pageTitle = WikiHomeModule.GetDocumentPageTitle(normalizedPath, normalizedSearchQuery);
        var pageSummary = WikiHomeModule.GetDocumentPageSummary(normalizedPath, normalizedSearchQuery);
        var robotsDirective = WikiHomeModule.GetDocumentRobotsDirective(normalizedPath);
        var isRegisteredPath = WikiHomeModule.IsRegisteredDocumentPath(normalizedPath);
        var isIndexablePath = WikiHomeModule.IsIndexableDocumentPath(normalizedPath);

        var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
        var relativeUrl = BuildRelativeUrl(normalizedPath, normalizedSearchQuery);
        var absoluteUrl = BuildAbsoluteUrl(context.Request, configuration, relativeUrl);
        var documentTitle = pageTitle + " | jazor.wiki";
        var scriptNonce = GenerateScriptNonce();
        var documentContentSecurityPolicy = BuildDocumentContentSecurityPolicy(scriptNonce);
        var responseContentSecurityPolicy = BuildResponseContentSecurityPolicy(scriptNonce);
        var pathBase = NormalizePathBase(context.Request.PathBase.Value);
        var browserModulePath = ResolveBrowserModulePath(context);
        var template = await LoadIndexTemplateAsync(context, cancellationToken);
        var renderedHtml = RenderIndexTemplate(
            template,
            documentTitle,
            pageSummary,
            absoluteUrl,
            robotsDirective,
            scriptNonce,
            documentContentSecurityPolicy,
            pathBase,
            browserModulePath);

        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.StatusCode = isRegisteredPath
            ? StatusCodes.Status200OK
            : StatusCodes.Status404NotFound;
        context.Response.Headers["Cache-Control"] = HtmlCacheControl;
        context.Response.Headers["Content-Security-Policy"] = responseContentSecurityPolicy;

        if (!isIndexablePath)
            context.Response.Headers["X-Robots-Tag"] = robotsDirective;

        if (HttpMethods.IsHead(context.Request.Method))
            return;

        await context.Response.WriteAsync(renderedHtml, cancellationToken);
    }

    private static async Task<string> LoadIndexTemplateAsync(HttpContext context, CancellationToken cancellationToken)
    {
        var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var templateFile = environment.ContentRootFileProvider.GetFileInfo(HtmlTemplateRelativePath);
        if (!templateFile.Exists)
            throw new InvalidOperationException("Wiki host could not locate " + HtmlTemplateRelativePath + ".");

        await using var stream = templateFile.CreateReadStream();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static string BuildRelativeUrl(string normalizedPath, string normalizedSearchQuery)
    {
        if (WikiHomeModule.IsSearchRoute(normalizedPath) && normalizedSearchQuery.Length > 0)
            return normalizedPath + QueryString.Create("q", normalizedSearchQuery).ToUriComponent();

        return normalizedPath;
    }

    // 静态导出没有真实的公开 Request Host；优先使用配置的 origin，再拼接 PathBase。
    // ASP.NET 部署未配置时仍从当前请求派生，保持本地和反向代理部署兼容。
    internal static string BuildAbsoluteUrl(HttpRequest request, IConfiguration configuration, string relativeUrl)
        => BuildSiteOrigin(request, configuration) + relativeUrl;

    internal static string BuildSiteOrigin(HttpRequest request, IConfiguration configuration)
    {
        var configuredOrigin = configuration["Wiki:SiteOrigin"];
        var origin = string.IsNullOrWhiteSpace(configuredOrigin)
            ? request.Scheme + "://" + request.Host.Value
            : configuredOrigin.Trim().TrimEnd('/');

        return origin + NormalizePathBase(request.PathBase.Value);
    }

    private static string RenderIndexTemplate(
        string template,
        string documentTitle,
        string pageSummary,
        string absoluteUrl,
        string robotsDirective,
        string scriptNonce,
        string contentSecurityPolicy,
        string pathBase,
        string browserModulePath)
    {
        if (template.Contains(MetadataTokenPrefix, StringComparison.Ordinal) == false)
            throw new InvalidOperationException("Wiki index template does not contain metadata tokens.");

        var htmlEncoder = HtmlEncoder.Default;
        var rendered = template;
        var faviconUrl = BuildAssetUrl(pathBase, "/favicon.svg");
        var siteCssUrl = BuildAssetUrl(pathBase, "/site.css");
        var browserModuleUrl = BuildAssetUrl(pathBase, browserModulePath);
        // These are the non-package Jazor module namespaces written by Emit. Keep them
        // path-base aware so a Debug module graph works below /docs as well as at root.
        // 这些是 Emit 写入的非 package Jazor 模块命名空间；必须随 PathBase 重写，
        // 才能同时支持根路径和 /docs 下的 Debug 模块图。
        var styleImportUrl = BuildAssetUrl(pathBase, "/jazor/style.mjs");
        var componentsImportBase = BuildAssetUrl(pathBase, "/jazor/components/");
        var systemImportBase = BuildAssetUrl(pathBase, "/jazor/System/");
        var vendorVueUrl = BuildAssetUrl(pathBase, "/vendor/vue@3.5.16.mjs");
        var soberUrl = BuildAssetUrl(pathBase, "/vendor/sober@1.1.10.min.js");
        rendered = ReplaceRequiredToken(rendered, TitleToken, htmlEncoder.Encode(documentTitle));
        rendered = ReplaceRequiredToken(rendered, DescriptionToken, htmlEncoder.Encode(pageSummary));
        rendered = ReplaceRequiredToken(rendered, CanonicalUrlToken, htmlEncoder.Encode(absoluteUrl));
        rendered = ReplaceRequiredToken(rendered, OpenGraphTitleToken, htmlEncoder.Encode(documentTitle));
        rendered = ReplaceRequiredToken(rendered, OpenGraphDescriptionToken, htmlEncoder.Encode(pageSummary));
        rendered = ReplaceRequiredToken(rendered, OpenGraphUrlToken, htmlEncoder.Encode(absoluteUrl));
        rendered = ReplaceRequiredToken(rendered, TwitterTitleToken, htmlEncoder.Encode(documentTitle));
        rendered = ReplaceRequiredToken(rendered, TwitterDescriptionToken, htmlEncoder.Encode(pageSummary));
        rendered = ReplaceRequiredToken(rendered, RobotsDirectiveToken, htmlEncoder.Encode(robotsDirective));
        rendered = ReplaceRequiredToken(rendered, ScriptNonceToken, htmlEncoder.Encode(scriptNonce));
        rendered = ReplaceRequiredToken(rendered, ContentSecurityPolicyToken, htmlEncoder.Encode(contentSecurityPolicy));
        rendered = ReplaceRequiredToken(rendered, PathBaseToken, htmlEncoder.Encode(pathBase));
        rendered = ReplaceRequiredToken(rendered, FaviconUrlToken, htmlEncoder.Encode(faviconUrl));
        rendered = ReplaceRequiredToken(rendered, SiteCssUrlToken, htmlEncoder.Encode(siteCssUrl));
        rendered = ReplaceRequiredToken(rendered, MainModuleUrlToken, htmlEncoder.Encode(browserModuleUrl));
        rendered = ReplaceRequiredToken(rendered, StyleImportUrlToken, htmlEncoder.Encode(styleImportUrl));
        rendered = ReplaceRequiredToken(rendered, ComponentsImportBaseToken, htmlEncoder.Encode(componentsImportBase));
        rendered = ReplaceRequiredToken(rendered, SystemImportBaseToken, htmlEncoder.Encode(systemImportBase));
        rendered = ReplaceRequiredToken(rendered, VendorVueUrlToken, htmlEncoder.Encode(vendorVueUrl));
        rendered = ReplaceRequiredToken(rendered, SoberUrlToken, htmlEncoder.Encode(soberUrl));

        if (rendered.Contains(MetadataTokenPrefix, StringComparison.Ordinal))
            throw new InvalidOperationException("Wiki index template contains unresolved metadata tokens after rendering.");

        return rendered;
    }

    private static string ReplaceRequiredToken(string template, string token, string value)
    {
        if (!template.Contains(token, StringComparison.Ordinal))
            throw new InvalidOperationException("Wiki index template is missing token " + token + ".");

        return template.Replace(token, value, StringComparison.Ordinal);
    }

    private static string GenerateScriptNonce()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    private static string NormalizePathBase(string? pathBase)
    {
        if (string.IsNullOrWhiteSpace(pathBase) || pathBase == "/")
            return string.Empty;

        return pathBase.EndsWith("/", StringComparison.Ordinal)
            ? pathBase[..^1]
            : pathBase;
    }

    private static string BuildAssetUrl(string pathBase, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Asset path is required.", nameof(path));

        if (!path.StartsWith("/", StringComparison.Ordinal))
            throw new ArgumentException("Asset path must start with '/'.", nameof(path));

        return pathBase.Length == 0
            ? path
            : pathBase + path;
    }

    private static string ResolveBrowserModulePath(HttpContext context)
    {
        var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var manifest = environment.ContentRootFileProvider.GetFileInfo("jazor/jazor-manifest.json");
        var mainModule = environment.ContentRootFileProvider.GetFileInfo("jazor/main.mjs");
        var bundle = environment.ContentRootFileProvider.GetFileInfo("jazor/bundle.js");

        // Debug materialization writes both main.mjs and its manifest. Prefer that pair so a
        // prior release bundle left beside a watch build cannot serve stale browser code.
        // Debug 会同时写入 main.mjs 与 manifest；优先这对文件，避免 watch 构建旁残留的旧 release bundle 被误加载。
        if (manifest.Exists && mainModule.Exists)
            return "/jazor/main.mjs";

        // Release owns only the browser bundle at this root unless SSR adds jazor/ssr/.
        // Release 根目录只保留浏览器 bundle；SSR 原始模块图位于独立的 jazor/ssr/。
        if (bundle.Exists)
            return "/jazor/bundle.js";

        // This fallback keeps a manually materialized debug graph usable even before a
        // manifest is available, without ever preferring a potentially stale bundle.
        // 该回退允许手工物化的 Debug 模块图在尚未写入 manifest 时使用，同时绝不优先陈旧 bundle。
        if (mainModule.Exists)
            return "/jazor/main.mjs";

        throw new InvalidOperationException("Wiki host could not locate jazor/bundle.js or jazor/main.mjs.");
    }

    private static string BuildDocumentContentSecurityPolicy(string scriptNonce)
    {
        return "default-src 'self'; " +
               "base-uri 'none'; " +
               "object-src 'none'; " +
               "form-action 'none'; " +
               "img-src 'self' data:; " +
               "font-src 'self'; " +
               "connect-src 'self'; " +
               "manifest-src 'self'; " +
               "worker-src 'self'; " +
               "script-src 'self' 'nonce-" + scriptNonce + "'; " +
               "style-src 'self' 'unsafe-inline'";
    }

    // `frame-ancestors` only works in an HTTP response header. GitHub Pages relies on
    // the HTML meta policy, so omit the unsupported directive there without weakening ASP.NET hosting.
    private static string BuildResponseContentSecurityPolicy(string scriptNonce)
        => BuildDocumentContentSecurityPolicy(scriptNonce) + "; frame-ancestors 'none'";
}
