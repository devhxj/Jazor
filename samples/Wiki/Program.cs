// Program.cs - ASP.NET Core 入口 / ASP.NET Core entry point
// 配置标准项目 Vite 代理、静态资源服务、路由回退和安全头
// Configures the standard project's Vite proxy, static assets, route fallback, and security headers

using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;
using Wiki;

// 构建应用 / Build the application
var builder = JazorWebApplication.CreateBuilder(args);
builder.AddJazorFrontend(options =>
{
    options.PathBase = builder.Configuration["Wiki:PathBase"] ?? string.Empty;
    options.Vite.ServerOrigin = new Uri(
        builder.Configuration["Wiki:JavaScriptServer"] ?? JazorViteServerOptions.DefaultServerOrigin.AbsoluteUri);
});

// 启动前验证路由目录完整性 / Validate route catalog integrity before startup
Wiki.WikiCatalogGuard.ValidateOrThrow();

var app = builder.Build();
app.UseJazorPathBase();
app.UseJazorFrontend(options =>
{
    options.SecurityHeaders.PermissionsPolicy =
        "accelerometer=(), autoplay=(), camera=(), display-capture=(), geolocation=(), gyroscope=(), " +
        "hid=(), microphone=(), payment=(), usb=(), clipboard-read=(self), clipboard-write=(self)";
    options.Assets.ImmutableCachePathPrefixes.Add("/vendor/");
});

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
