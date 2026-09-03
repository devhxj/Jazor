// WikiHostShell.cs - 服务端 HTML 外壳渲染器 / Server-side HTML shell renderer
// 负责 HTML 模板渲染、安全头注入、CSP 策略、元数据 token 替换
// Handles HTML template rendering, security headers, CSP policy, metadata token replacement

using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
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
    private const string BrowserImportMapToken = "__WIKI_BROWSER_IMPORT_MAP__";
    private const string SoberUrlToken = "__WIKI_SOBER_URL__";
    private const string BrowserImportMapRelativePath = "jazor/importmap.json";
    private const string DebugBrowserModulePath = "/jazor/main.mjs";
    private const string ReleaseBrowserModulePath = "/jazor/bundle.js";
    private const string MaterializedAssetPrefix = "/jazor";
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
        var browserImportMap = browserModulePath == DebugBrowserModulePath
            ? await LoadBrowserImportMapAsync(context, pathBase, cancellationToken)
            : CreateEmptyBrowserImportMap();
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
            browserModulePath,
            browserImportMap);

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
        string browserModulePath,
        string browserImportMap)
    {
        if (template.Contains(MetadataTokenPrefix, StringComparison.Ordinal) == false)
            throw new InvalidOperationException("Wiki index template does not contain metadata tokens.");

        var htmlEncoder = HtmlEncoder.Default;
        var rendered = template;
        var faviconUrl = BuildAssetUrl(pathBase, "/favicon.svg");
        var siteCssUrl = BuildAssetUrl(pathBase, "/site.css");
        var browserModuleUrl = BuildAssetUrl(pathBase, browserModulePath);
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
        // JsonSerializer's default encoder keeps the trusted generated map safe inside a script element.
        // 这里必须原样注入 JSON；HTML 编码会把引号改成实体，浏览器将无法解析 import map。
        rendered = ReplaceRequiredToken(rendered, BrowserImportMapToken, browserImportMap);
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

    private static async Task<string> LoadBrowserImportMapAsync(
        HttpContext context,
        string pathBase,
        CancellationToken cancellationToken)
    {
        var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var importMapFile = environment.ContentRootFileProvider.GetFileInfo(BrowserImportMapRelativePath);
        if (!importMapFile.Exists)
            throw new InvalidOperationException("Wiki host could not locate " + BrowserImportMapRelativePath + ".");

        await using var stream = importMapFile.CreateReadStream();
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!document.RootElement.TryGetProperty("imports", out var importsElement) ||
            importsElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                "Wiki browser import map must contain an object property named 'imports': " +
                BrowserImportMapRelativePath + ".");
        }

        var imports = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var import in importsElement.EnumerateObject())
        {
            if (import.Value.ValueKind != JsonValueKind.String || import.Value.GetString() is not { } target)
            {
                throw new InvalidOperationException(
                    "Wiki browser import map entries must be strings: " + BrowserImportMapRelativePath + ".");
            }

            imports.Add(import.Name, RewriteBrowserImportTarget(target, pathBase));
        }

        return JsonSerializer.Serialize(new { imports });
    }

    private static string CreateEmptyBrowserImportMap()
        => JsonSerializer.Serialize(new { imports = new SortedDictionary<string, string>(StringComparer.Ordinal) });

    private static string RewriteBrowserImportTarget(string target, string pathBase)
    {
        if (!target.StartsWith(MaterializedAssetPrefix, StringComparison.Ordinal) ||
            (target.Length > MaterializedAssetPrefix.Length && target[MaterializedAssetPrefix.Length] != '/'))
        {
            return target;
        }

        return pathBase + target;
    }

    private static string ResolveBrowserModulePath(HttpContext context)
    {
        var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var manifest = environment.ContentRootFileProvider.GetFileInfo("jazor/jazor-manifest.json");
        var importMap = environment.ContentRootFileProvider.GetFileInfo(BrowserImportMapRelativePath);
        var mainModule = environment.ContentRootFileProvider.GetFileInfo("jazor/main.mjs");
        var bundle = environment.ContentRootFileProvider.GetFileInfo("jazor/bundle.js");

        // Debug is one complete graph: never select a stale release bundle or a partial module graph.
        // Debug 三个入口文件必须同时存在；缺任意一个都直接失败，禁止回退到陈旧 Release bundle。
        if (manifest.Exists && importMap.Exists && mainModule.Exists)
            return DebugBrowserModulePath;

        if (manifest.Exists || importMap.Exists || mainModule.Exists)
        {
            throw new InvalidOperationException(
                "Wiki host found an incomplete Debug artifact graph. Expected jazor/jazor-manifest.json, " +
                BrowserImportMapRelativePath + ", and jazor/main.mjs together.");
        }

        // Release owns only the browser bundle at this root unless SSR adds jazor/ssr/.
        // Release 根目录只保留浏览器 bundle；SSR 原始模块图位于独立的 jazor/ssr/。
        if (bundle.Exists)
            return ReleaseBrowserModulePath;

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
