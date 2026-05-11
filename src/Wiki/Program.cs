// Program.cs - ASP.NET Core 入口 / ASP.NET Core entry point
// 配置 Jazor 开发时重载、静态资源服务、路由回退和安全头
// Configures Jazor dev-time reload, static assets, route fallback, and security headers

using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;

// 构建应用 / Build the application
var builder = WebApplication.CreateBuilder(args);
// 配置 Jazor 开发时热重载监听目录 / Configure Jazor dev-time hot reload watch paths
builder.Services.AddJazorDevelopmentReload(options =>
{
    options.WatchRootPaths.Clear();
    options.WatchRootPaths.Add("jazor");
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

// Jazor 开发时静态资源中间件 / Jazor dev-time static assets middleware
app.UseJazorDevelopmentAssets(options =>
{
    options.OnPrepareResponse = Wiki.WikiHostShell.ApplyStaticAssetHeaders;
});
app.UseJazorDevelopmentReload();

// HTML 外壳回退 + 安全头中间件 / HTML shell fallback + security headers middleware
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        Wiki.WikiHostShell.ApplySecurityHeaders(context.Response.Headers);
        return Task.CompletedTask;
    });

    // 尝试为非文件路径提供 HTML 外壳 / Try to serve HTML shell for non-file paths
    if (await Wiki.WikiHostShell.TryHandleHtmlRequestAsync(context, context.RequestAborted))
        return;

    await next();
});

// Jazor 编译产物静态文件 / Jazor compiled output static files
app.UseJazorStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = Wiki.WikiHostShell.ApplyStaticAssetHeaders
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
