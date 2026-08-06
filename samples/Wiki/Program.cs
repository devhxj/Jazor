// Program.cs - ASP.NET Core 入口 / ASP.NET Core entry point
// 配置 Jazor 开发时重载、静态资源服务、路由回退和安全头
// Configures Jazor dev-time reload, static assets, route fallback, and security headers

using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;
using Wiki;

// 构建应用 / Build the application
var builder = JazorWebApplication.CreateBuilder(args);
// 配置 Jazor 开发时热重载监听目录 / Configure Jazor dev-time hot reload watch paths
builder.Services.AddJazorDevelopmentReload(options =>
{
    options.WatchRootPaths.Clear();
    options.WatchRootPaths.Add("wwwroot");
});

// 启动前验证路由目录完整性 / Validate route catalog integrity before startup
Wiki.WikiCatalogGuard.ValidateOrThrow();

var app = builder.Build();

// 处理子路径部署场景 / Handle sub-path deployment scenarios
var configuredPathBase = builder.Configuration["Wiki:PathBase"];

if (!string.IsNullOrWhiteSpace(configuredPathBase))
{
    if (!configuredPathBase.StartsWith('/', StringComparison.Ordinal))
        throw new InvalidOperationException("Wiki:PathBase must start with '/'.");

    if (configuredPathBase.Length > 1 && configuredPathBase.EndsWith('/', StringComparison.Ordinal))
        configuredPathBase = configuredPathBase[..^1];

    app.UsePathBase(configuredPathBase);
}

app.UseJazorHost(options =>
{
    options.SecurityHeaders.PermissionsPolicy =
        "accelerometer=(), autoplay=(), camera=(), display-capture=(), geolocation=(), gyroscope=(), " +
        "hid=(), microphone=(), payment=(), usb=(), clipboard-read=(self), clipboard-write=(self)";
    options.WebAssets.ImmutableCachePathPrefixes.Add("/vendor/");
});

app.UseJazorDevelopmentReload();

// HTML 外壳回退 / HTML shell fallback
app.UseJazorSpaFallback(
    WikiHostShell.WriteHtmlAsync,
    options =>
{
    options.ExcludedPathPrefixes.Add("/vendor");
    options.AllowedPathSuffixes.Add("/index.html");
});

// 健康检查端点 / Health check endpoint
app.MapGet("/health", (HttpContext context) =>
{
    context.Response.Headers["Cache-Control"] = "no-store";
    return Results.Ok("ok");
});

// SEO 发现文档端点 / SEO discovery document endpoints
app.MapMethods("/robots.txt", ["GET", "HEAD"], async context =>
{
    await Wiki.WikiHostDiscoveryDocuments.TryHandleAsync(context, context.RequestAborted);
});
app.MapMethods("/sitemap.xml", ["GET", "HEAD"], async context =>
{
    await Wiki.WikiHostDiscoveryDocuments.TryHandleAsync(context, context.RequestAborted);
});

app.Run();
