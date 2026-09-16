using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

namespace Jazor.AspNetCore;

/// <summary>Provides Jazor host middleware for artifacts, static assets, security headers, and SPA fallbacks.</summary>
public static class JazorExtensions
{
    private const string SourceMapContentType = "application/json";
    private const string MutableAssetCacheControl = "no-cache, must-revalidate";
    private const string ImmutableAssetCacheControl = "public, max-age=31536000, immutable";
    private const string XContentTypeOptionsHeaderName = "X-Content-Type-Options";
    private const string ReferrerPolicyHeaderName = "Referrer-Policy";
    private const string XFrameOptionsHeaderName = "X-Frame-Options";
    private const string CrossOriginOpenerPolicyHeaderName = "Cross-Origin-Opener-Policy";
    private const string CrossOriginResourcePolicyHeaderName = "Cross-Origin-Resource-Policy";
    private const string PermissionsPolicyHeaderName = "Permissions-Policy";
    private const string XPermittedCrossDomainPoliciesHeaderName = "X-Permitted-Cross-Domain-Policies";

    /// <summary>Registers Jazor security headers and generated/browser asset hosting.</summary>
    /// <remarks>依次注册响应头和资源托管；SSR、SPA fallback 与开发 reload 需单独注册。放在 UsePathBase 之后、fallback 之前。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorHost(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseJazorHost(configure: null);
    }

    /// <summary>Registers Jazor security headers and generated/browser asset hosting.</summary>
    /// <remarks>依次注册响应头和资源托管；SSR、SPA fallback 与开发 reload 需单独注册。放在 UsePathBase 之后、fallback 之前。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="configure">配置此入口的选项；允许 null 的重载使用默认值。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorHost(
        this IApplicationBuilder app,
        Action<JazorHostOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = new JazorHostOptions();
        configure?.Invoke(options);

        app.UseJazorSecurityHeaders(options.SecurityHeaders);
        app.UseJazorAssets(options.Assets);
        return app;
    }

    /// <summary>Applies Jazor's default response security headers.</summary>
    /// <remarks>响应开始前只填充尚不存在的响应头；null 或空白配置不写入。AdditionalHeaders 最后应用，也不覆盖已有值。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorSecurityHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseJazorSecurityHeaders(configure: null);
    }

    /// <summary>Applies configured Jazor response security headers.</summary>
    /// <remarks>响应开始前只填充尚不存在的响应头；null 或空白配置不写入。AdditionalHeaders 最后应用，也不覆盖已有值。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="configure">配置此入口的选项；允许 null 的重载使用默认值。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorSecurityHeaders(
        this IApplicationBuilder app,
        Action<JazorSecurityHeaderOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = new JazorSecurityHeaderOptions();
        configure?.Invoke(options);
        return app.UseJazorSecurityHeaders(options);
    }

    /// <summary>Applies the supplied Jazor response security headers.</summary>
    /// <remarks>响应开始前只填充尚不存在的响应头；null 或空白配置不写入。AdditionalHeaders 最后应用，也不覆盖已有值。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="options">用于本次中间件注册的选项。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorSecurityHeaders(
        this IApplicationBuilder app,
        JazorSecurityHeaderOptions options)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(options);

        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(static state =>
            {
                var (response, headerOptions) = ((HttpResponse, JazorSecurityHeaderOptions))state;
                ApplySecurityHeaders(response.Headers, headerOptions);
                return Task.CompletedTask;
            }, (context.Response, options));

            await next();
        });

        return app;
    }

    /// <summary>Serves static files with source-map content-type support.</summary>
    /// <remarks>只为 StaticFiles 的内容类型提供器补充 .map → application/json；不设置缓存策略。需要 Jazor 默认缓存头时使用 UseJazorAssets 或 UseJazorArtifacts。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorStaticFiles(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseJazorStaticFiles(new StaticFileOptions());
    }

    /// <summary>Serves configured static files with source-map content-type support.</summary>
    /// <remarks>只为 StaticFiles 的内容类型提供器补充 .map → application/json；不设置缓存策略。需要 Jazor 默认缓存头时使用 UseJazorAssets 或 UseJazorArtifacts。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="configure">配置此入口的选项；允许 null 的重载使用默认值。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorStaticFiles(
        this IApplicationBuilder app,
        Action<StaticFileOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new StaticFileOptions();
        configure(options);
        return app.UseJazorStaticFiles(options);
    }

    /// <summary>Serves static files using the supplied options and source-map content-type support.</summary>
    /// <remarks>只为 StaticFiles 的内容类型提供器补充 .map → application/json；不设置缓存策略。需要 Jazor 默认缓存头时使用 UseJazorAssets 或 UseJazorArtifacts。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="options">用于本次中间件注册的选项。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorStaticFiles(
        this IApplicationBuilder app,
        StaticFileOptions options)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(options);

        options.ContentTypeProvider = WrapContentTypeProvider(options.ContentTypeProvider);

        app.UseStaticFiles(options);
        return app;
    }

    /// <summary>Mounts the generated content-root artifact graph at <c>/jazor</c>.</summary>
    /// <remarks>默认目录为 ContentRootPath/jazor，URL 为 /jazor。注册时至少一个探测文件存在才挂载；首次构建晚于注册时需要重启宿主。挂载后默认对缺失产物返回 404。默认缓存为 no-cache, must-revalidate；显式 immutable 前缀使用一年缓存。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorArtifacts(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseJazorArtifacts(configure: (Action<JazorArtifactOptions>?)null);
    }

    /// <summary>Mounts the generated content-root artifact graph with configurable discovery.</summary>
    /// <remarks>默认目录为 ContentRootPath/jazor，URL 为 /jazor。注册时至少一个探测文件存在才挂载；首次构建晚于注册时需要重启宿主。挂载后默认对缺失产物返回 404。默认缓存为 no-cache, must-revalidate；显式 immutable 前缀使用一年缓存。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="configure">配置此入口的选项；允许 null 的重载使用默认值。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorArtifacts(
        this IApplicationBuilder app,
        Action<JazorArtifactOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = new JazorArtifactOptions();
        configure?.Invoke(options);
        return app.UseJazorArtifacts(options);
    }

    /// <summary>Mounts the generated content-root artifact graph using the supplied options.</summary>
    /// <remarks>默认目录为 ContentRootPath/jazor，URL 为 /jazor。注册时至少一个探测文件存在才挂载；首次构建晚于注册时需要重启宿主。挂载后默认对缺失产物返回 404。默认缓存为 no-cache, must-revalidate；显式 immutable 前缀使用一年缓存。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="options">用于本次中间件注册的选项。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorArtifacts(
        this IApplicationBuilder app,
        JazorArtifactOptions options)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(options);

        if (!TryCreateArtifactMount(app, options, out var mountContext))
            return app;

        var staticFileOptions = new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(mountContext.RootPath),
            RequestPath = mountContext.RequestPath,
            ContentTypeProvider = CreateContentTypeProvider(),
            OnPrepareResponse = CreateStaticAssetResponseHandler(mountContext.OnPrepareResponse, mountContext.ImmutableCachePathPrefixes)
        };

        app.UseJazorStaticFiles(staticFileOptions);

        if (mountContext.ReturnNotFoundOnMiss)
        {
            // A missing generated module is an asset failure, not an SPA navigation.
            // End the branch here so a later fallback cannot turn it into HTML 200.
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments(mountContext.RequestPath))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                await next();
            });
        }

        return app;
    }

    /// <summary>Serves generated Jazor artifacts followed by ordinary web-root assets.</summary>
    /// <remarks>先挂载生成产物，再执行默认文件解析及 web root 静态文件托管，避免 wwwroot 中旧产物遮蔽新文件。默认补充 nosniff 与 no-cache, must-revalidate；回调最后运行，可覆盖默认头。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorAssets(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseJazorAssets(configure: null);
    }

    /// <summary>Serves generated Jazor artifacts and web-root assets with configuration.</summary>
    /// <remarks>先挂载生成产物，再执行默认文件解析及 web root 静态文件托管，避免 wwwroot 中旧产物遮蔽新文件。默认补充 nosniff 与 no-cache, must-revalidate；回调最后运行，可覆盖默认头。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="configure">配置此入口的选项；允许 null 的重载使用默认值。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorAssets(
        this IApplicationBuilder app,
        Action<JazorAssetOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = new JazorAssetOptions();
        configure?.Invoke(options);
        return app.UseJazorAssets(options);
    }

    /// <summary>Serves generated Jazor artifacts and web-root assets using the supplied options.</summary>
    /// <remarks>先挂载生成产物，再执行默认文件解析及 web root 静态文件托管，避免 wwwroot 中旧产物遮蔽新文件。默认补充 nosniff 与 no-cache, must-revalidate；回调最后运行，可覆盖默认头。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="options">用于本次中间件注册的选项。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorAssets(
        this IApplicationBuilder app,
        JazorAssetOptions options)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(options);

        // /jazor is a dedicated generated-artifact mount. It must run before the
        // generic web-root provider so a stale wwwroot/jazor directory cannot shadow
        // the graph emitted to the project or publish root.
        if (options.ServeArtifacts)
        {
            app.UseJazorArtifacts(artifactOptions =>
            {
                artifactOptions.ProbeRelativePaths.Clear();
                foreach (var probeRelativePath in options.ArtifactProbeRelativePaths)
                    artifactOptions.ProbeRelativePaths.Add(probeRelativePath);

                if (options.OnPrepareResponse is not null)
                    artifactOptions.OnPrepareResponse = options.OnPrepareResponse;

                artifactOptions.ImmutableCachePathPrefixes.Clear();
                foreach (var prefix in options.ImmutableCachePathPrefixes)
                    artifactOptions.ImmutableCachePathPrefixes.Add(prefix);

                options.ConfigureArtifacts?.Invoke(artifactOptions);
            });
        }

        if (options.ServeDefaultFiles)
            app.UseDefaultFiles();

        if (options.ServeWebRoot)
        {
            var staticFileOptions = new StaticFileOptions();
            staticFileOptions.OnPrepareResponse = CreateStaticAssetResponseHandler(options.OnPrepareResponse, options.ImmutableCachePathPrefixes);

            app.UseJazorStaticFiles(staticFileOptions);
        }

        return app;
    }

    /// <summary>Writes custom SPA HTML for eligible, otherwise-unhandled navigation requests.</summary>
    /// <remarks>仅对没有已选 endpoint 的 GET/HEAD 导航，在下游返回 404 且响应未开始时写 HTML。默认排除 API、资源路径与带扩展名路径；无 Accept 可通过，显式 Accept 必须包含可接受的 HTML。自定义 writer 负责 ContentType 和 HEAD 空响应体。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="writeHtml">写入 HTML 的回调；取消令牌为 RequestAborted，需自行处理 ContentType 与 HEAD。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorSpaFallback(
        this IApplicationBuilder app,
        Func<HttpContext, CancellationToken, Task> writeHtml)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(writeHtml);

        return app.UseJazorSpaFallback(writeHtml, configure: null);
    }

    /// <summary>Uses a web-root HTML file as the SPA fallback document.</summary>
    /// <remarks>仅对没有已选 endpoint 的 GET/HEAD 导航，在下游返回 404 且响应未开始时写 HTML。默认排除 API、资源路径与带扩展名路径；无 Accept 可通过，显式 Accept 必须包含可接受的 HTML。自定义 writer 负责 ContentType 和 HEAD 空响应体。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="webRootPagePath">相对 WebRootFileProvider 的 HTML 路径，例如 index.html；请求时不存在则抛出异常。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorSpaFallback(
        this IApplicationBuilder app,
        string webRootPagePath)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseJazorSpaFallback(webRootPagePath, configure: null);
    }

    /// <summary>Uses a configured web-root HTML file as the SPA fallback document.</summary>
    /// <remarks>仅对没有已选 endpoint 的 GET/HEAD 导航，在下游返回 404 且响应未开始时写 HTML。默认排除 API、资源路径与带扩展名路径；无 Accept 可通过，显式 Accept 必须包含可接受的 HTML。自定义 writer 负责 ContentType 和 HEAD 空响应体。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="webRootPagePath">相对 WebRootFileProvider 的 HTML 路径，例如 index.html；请求时不存在则抛出异常。</param>
    /// <param name="configure">配置此入口的选项；允许 null 的重载使用默认值。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorSpaFallback(
        this IApplicationBuilder app,
        string webRootPagePath,
        Action<JazorSpaFallbackOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(app);
        var writeHtml = CreateStaticSpaFallbackWriter(webRootPagePath);
        return app.UseJazorSpaFallback(writeHtml, configure);
    }

    /// <summary>Uses a web-root HTML file and the supplied SPA fallback options.</summary>
    /// <remarks>仅对没有已选 endpoint 的 GET/HEAD 导航，在下游返回 404 且响应未开始时写 HTML。默认排除 API、资源路径与带扩展名路径；无 Accept 可通过，显式 Accept 必须包含可接受的 HTML。自定义 writer 负责 ContentType 和 HEAD 空响应体。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="webRootPagePath">相对 WebRootFileProvider 的 HTML 路径，例如 index.html；请求时不存在则抛出异常。</param>
    /// <param name="options">用于本次中间件注册的选项。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorSpaFallback(
        this IApplicationBuilder app,
        string webRootPagePath,
        JazorSpaFallbackOptions options)
    {
        ArgumentNullException.ThrowIfNull(app);
        var writeHtml = CreateStaticSpaFallbackWriter(webRootPagePath);
        return app.UseJazorSpaFallback(writeHtml, options);
    }

    /// <summary>Writes custom SPA HTML with configurable eligibility rules.</summary>
    /// <remarks>仅对没有已选 endpoint 的 GET/HEAD 导航，在下游返回 404 且响应未开始时写 HTML。默认排除 API、资源路径与带扩展名路径；无 Accept 可通过，显式 Accept 必须包含可接受的 HTML。自定义 writer 负责 ContentType 和 HEAD 空响应体。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="writeHtml">写入 HTML 的回调；取消令牌为 RequestAborted，需自行处理 ContentType 与 HEAD。</param>
    /// <param name="configure">配置此入口的选项；允许 null 的重载使用默认值。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorSpaFallback(
        this IApplicationBuilder app,
        Func<HttpContext, CancellationToken, Task> writeHtml,
        Action<JazorSpaFallbackOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(writeHtml);

        var options = new JazorSpaFallbackOptions();
        configure?.Invoke(options);
        return app.UseJazorSpaFallback(writeHtml, options);
    }

    /// <summary>Writes custom SPA HTML using the supplied eligibility rules.</summary>
    /// <remarks>仅对没有已选 endpoint 的 GET/HEAD 导航，在下游返回 404 且响应未开始时写 HTML。默认排除 API、资源路径与带扩展名路径；无 Accept 可通过，显式 Accept 必须包含可接受的 HTML。自定义 writer 负责 ContentType 和 HEAD 空响应体。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="writeHtml">写入 HTML 的回调；取消令牌为 RequestAborted，需自行处理 ContentType 与 HEAD。</param>
    /// <param name="options">用于本次中间件注册的选项。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorSpaFallback(
        this IApplicationBuilder app,
        Func<HttpContext, CancellationToken, Task> writeHtml,
        JazorSpaFallbackOptions options)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(writeHtml);
        ArgumentNullException.ThrowIfNull(options);

        var fallbackContext = JazorSpaFallbackContext.Create(options);
        app.Use(async (context, next) =>
        {
            if (!IsJazorSpaFallbackCandidate(context, fallbackContext))
            {
                await next();
                return;
            }

            await next();

            if (context.Response.HasStarted
                || context.Response.StatusCode != StatusCodes.Status404NotFound
                || !IsJazorSpaFallbackCandidate(context, fallbackContext))
            {
                return;
            }

            context.Response.StatusCode = StatusCodes.Status200OK;
            await writeHtml(context, context.RequestAborted);
        });

        return app;
    }

    private static bool TryCreateArtifactMount(
        IApplicationBuilder app,
        JazorArtifactOptions options,
        out ArtifactMount mountContext)
    {
        var environment = app.ApplicationServices.GetService<IWebHostEnvironment>()
            ?? throw new InvalidOperationException("Jazor artifacts require IWebHostEnvironment from the ASP.NET Core host.");

        var requestPath = NormalizeRequestPath(options.RequestPath);
        var rootPath = ResolveArtifactRootPath(environment.ContentRootPath, options);
        if (!HasArtifactProbe(rootPath, options.ProbeRelativePaths))
        {
            mountContext = null!;
            return false;
        }

        mountContext = new ArtifactMount(
            requestPath,
            rootPath,
            options.OnPrepareResponse,
            options.ImmutableCachePathPrefixes.ToArray(),
            options.ReturnNotFoundOnMiss);
        return true;
    }

    private static PathString NormalizeRequestPath(PathString requestPath)
    {
        if (!requestPath.HasValue || string.IsNullOrWhiteSpace(requestPath.Value))
            throw new ArgumentException("Jazor artifact request path is required.", nameof(requestPath));

        if (!requestPath.Value.StartsWith('/'))
            throw new ArgumentException("Jazor artifact request path must start with '/'.", nameof(requestPath));

        if (string.Equals(requestPath.Value, "/", StringComparison.Ordinal))
            throw new ArgumentException("Jazor artifact request path cannot be the application root.", nameof(requestPath));

        return requestPath;
    }

    private static PathString NormalizeSpaFallbackPathPrefix(PathString pathPrefix)
    {
        if (!pathPrefix.HasValue || string.IsNullOrWhiteSpace(pathPrefix.Value))
            throw new ArgumentException("Jazor SPA fallback excluded path prefixes cannot be empty.", nameof(pathPrefix));

        if (!pathPrefix.Value.StartsWith('/'))
            throw new ArgumentException("Jazor SPA fallback excluded path prefixes must start with '/'.", nameof(pathPrefix));

        return pathPrefix;
    }

    private static bool IsJazorSpaFallbackCandidate(HttpContext context, JazorSpaFallbackContext fallbackContext)
    {
        var request = context.Request;
        if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
            return false;

        if (context.GetEndpoint() is not null)
            return false;

        var requestPath = request.Path;
        foreach (var excludedPathPrefix in fallbackContext.ExcludedPathPrefixes)
        {
            if (requestPath.StartsWithSegments(excludedPathPrefix, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (!IsAllowedSpaFallbackPath(requestPath, fallbackContext))
            return false;

        if (fallbackContext.RequireHtmlAcceptHeader && !AcceptsHtmlDocument(request))
            return false;

        return true;
    }

    private static bool IsAllowedSpaFallbackPath(PathString requestPath, JazorSpaFallbackContext fallbackContext)
    {
        var pathValue = requestPath.Value ?? string.Empty;
        if (!Path.HasExtension(pathValue))
            return true;

        foreach (var allowedPathSuffix in fallbackContext.AllowedPathSuffixes)
        {
            if (pathValue.EndsWith(allowedPathSuffix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool AcceptsHtmlDocument(HttpRequest request)
    {
        var acceptHeaders = request.GetTypedHeaders().Accept;
        // Browsers may omit Accept on direct navigations; treat that as HTML navigation.
        if (acceptHeaders is null || acceptHeaders.Count == 0)
            return true;

        foreach (var mediaType in acceptHeaders)
        {
            if (IsHtmlDocumentMediaType(mediaType))
                return true;
        }

        return false;
    }

    private static Func<HttpContext, CancellationToken, Task> CreateStaticSpaFallbackWriter(string webRootPagePath)
    {
        var normalizedWebRootPagePath = NormalizeStaticSpaFallbackPagePath(webRootPagePath);
        return async (context, cancellationToken) =>
        {
            var environment = context.RequestServices.GetService<IWebHostEnvironment>()
                ?? throw new InvalidOperationException("Jazor SPA fallback requires IWebHostEnvironment from the ASP.NET Core host.");
            var pageFile = environment.WebRootFileProvider.GetFileInfo(normalizedWebRootPagePath);
            if (!pageFile.Exists)
            {
                throw new InvalidOperationException(
                    $"Jazor SPA fallback static page was not found in web root: '{normalizedWebRootPagePath}'.");
            }

            context.Response.ContentType ??= "text/html; charset=utf-8";
            if (HttpMethods.IsHead(context.Request.Method))
                return;

            await using var stream = pageFile.CreateReadStream();
            await stream.CopyToAsync(context.Response.Body, cancellationToken);
        };
    }

    private static bool IsHtmlDocumentMediaType(MediaTypeHeaderValue mediaType)
    {
        if (mediaType.Quality is <= 0)
            return false;

        var value = mediaType.MediaType.Value;
        return !string.IsNullOrWhiteSpace(value)
            && (string.Equals(value, "text/html", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "application/xhtml+xml", StringComparison.OrdinalIgnoreCase));
    }

    private static FileExtensionContentTypeProvider CreateContentTypeProvider()
    {
        var provider = new FileExtensionContentTypeProvider();
        provider.Mappings[".map"] = SourceMapContentType;
        return provider;
    }

    private static Action<StaticFileResponseContext> CreateStaticAssetResponseHandler(
        Action<StaticFileResponseContext>? configure,
        IEnumerable<string> immutableCachePathPrefixes)
    {
        return context =>
        {
            ApplyDefaultStaticAssetHeaders(context, immutableCachePathPrefixes);
            configure?.Invoke(context);
        };
    }

    private static void ApplyDefaultStaticAssetHeaders(
        StaticFileResponseContext context,
        IEnumerable<string> immutableCachePathPrefixes)
    {
        var headers = context.Context.Response.Headers;
        if (!headers.ContainsKey(XContentTypeOptionsHeaderName))
            headers[XContentTypeOptionsHeaderName] = "nosniff";

        if (!headers.ContainsKey(HeaderNames.CacheControl))
            headers.CacheControl = MatchesImmutableCachePrefix(context.Context.Request.Path.Value, immutableCachePathPrefixes)
                ? ImmutableAssetCacheControl
                : MutableAssetCacheControl;
    }

    private static bool MatchesImmutableCachePrefix(string? requestPath, IEnumerable<string> immutableCachePathPrefixes)
    {
        if (string.IsNullOrWhiteSpace(requestPath))
            return false;

        foreach (var prefix in immutableCachePathPrefixes)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                continue;

            if (requestPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static void ApplySecurityHeaders(IHeaderDictionary headers, JazorSecurityHeaderOptions options)
    {
        ApplyHeaderIfConfigured(headers, XContentTypeOptionsHeaderName, options.XContentTypeOptions);
        ApplyHeaderIfConfigured(headers, ReferrerPolicyHeaderName, options.ReferrerPolicy);
        ApplyHeaderIfConfigured(headers, XFrameOptionsHeaderName, options.XFrameOptions);
        ApplyHeaderIfConfigured(headers, CrossOriginOpenerPolicyHeaderName, options.CrossOriginOpenerPolicy);
        ApplyHeaderIfConfigured(headers, CrossOriginResourcePolicyHeaderName, options.CrossOriginResourcePolicy);
        ApplyHeaderIfConfigured(headers, PermissionsPolicyHeaderName, options.PermissionsPolicy);
        ApplyHeaderIfConfigured(headers, XPermittedCrossDomainPoliciesHeaderName, options.XPermittedCrossDomainPolicies);

        foreach (var header in options.AdditionalHeaders)
            ApplyHeaderIfConfigured(headers, header.Key, header.Value);
    }

    private static void ApplyHeaderIfConfigured(IHeaderDictionary headers, string headerName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (!headers.ContainsKey(headerName))
            headers[headerName] = value;
    }

    private static IContentTypeProvider WrapContentTypeProvider(IContentTypeProvider? contentTypeProvider)
    {
        var innerProvider = contentTypeProvider ?? CreateContentTypeProvider();

        if (innerProvider is FileExtensionContentTypeProvider fileExtensionProvider)
        {
            fileExtensionProvider.Mappings[".map"] = SourceMapContentType;
            return fileExtensionProvider;
        }

        return new SourceMapAwareContentTypeProvider(innerProvider);
    }

    private static string ResolveArtifactRootPath(string contentRootPath, JazorArtifactOptions options)
    {
        var configuredPath = options.RootPath;
        if (!string.IsNullOrWhiteSpace(configuredPath))
            return GetFullPath(contentRootPath, configuredPath);

        if (string.IsNullOrWhiteSpace(options.DirectoryName))
            throw new ArgumentException("Jazor artifact directory name is required when no explicit root path is configured.", nameof(options));

        return Path.GetFullPath(Path.Combine(contentRootPath, options.DirectoryName));
    }

    private static bool HasArtifactProbe(string rootPath, IEnumerable<string> probeRelativePaths)
    {
        var sawProbe = false;

        foreach (var probeRelativePath in probeRelativePaths)
        {
            sawProbe = true;
            var probePath = ResolveProbePath(rootPath, probeRelativePath);
            if (File.Exists(probePath))
                return true;
        }

        if (!sawProbe)
            throw new ArgumentException("At least one Jazor artifact probe path is required.", nameof(probeRelativePaths));

        return false;
    }

    private static string ResolveProbePath(string rootPath, string probeRelativePath)
    {
        if (string.IsNullOrWhiteSpace(probeRelativePath))
            throw new ArgumentException("Jazor artifact probe path cannot be empty.", nameof(probeRelativePath));

        if (Path.IsPathRooted(probeRelativePath))
            throw new ArgumentException("Jazor artifact probe path must be relative.", nameof(probeRelativePath));

        var candidatePath = Path.GetFullPath(Path.Combine(rootPath, probeRelativePath));
        if (!IsPathWithinRoot(rootPath, candidatePath))
            throw new ArgumentException("Jazor artifact probe path must stay within the configured root.", nameof(probeRelativePath));

        return candidatePath;
    }

    private static string GetFullPath(string contentRootPath, string configuredPath)
    {
        var resolvedPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(contentRootPath, configuredPath);
        return Path.GetFullPath(resolvedPath);
    }

    private static bool IsPathWithinRoot(string rootPath, string candidatePath)
    {
        var normalizedRootPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var normalizedCandidatePath = Path.GetFullPath(candidatePath);

        return normalizedCandidatePath.StartsWith(
            normalizedRootPath + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalizedCandidatePath, normalizedRootPath, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class SourceMapAwareContentTypeProvider(IContentTypeProvider innerProvider) : IContentTypeProvider
    {
        public bool TryGetContentType(string subpath, out string contentType)
        {
            if (subpath.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
            {
                contentType = SourceMapContentType;
                return true;
            }

            return innerProvider.TryGetContentType(subpath, out contentType!);
        }
    }

    private sealed record ArtifactMount(
        PathString RequestPath,
        string RootPath,
        Action<StaticFileResponseContext>? OnPrepareResponse,
        IReadOnlyList<string> ImmutableCachePathPrefixes,
        bool ReturnNotFoundOnMiss);

    private sealed record JazorSpaFallbackContext(
        IReadOnlyList<PathString> ExcludedPathPrefixes,
        IReadOnlyList<string> AllowedPathSuffixes,
        bool RequireHtmlAcceptHeader)
    {
        public static JazorSpaFallbackContext Create(JazorSpaFallbackOptions options)
        {
            var excludedPathPrefixes = options.ExcludedPathPrefixes
                .Select(NormalizeSpaFallbackPathPrefix)
                .Distinct()
                .ToArray();
            var allowedPathSuffixes = options.AllowedPathSuffixes
                .Select(NormalizeSpaFallbackAllowedSuffix)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new JazorSpaFallbackContext(excludedPathPrefixes, allowedPathSuffixes, options.RequireHtmlAcceptHeader);
        }
    }

    private static string NormalizeSpaFallbackAllowedSuffix(string suffix)
    {
        if (string.IsNullOrWhiteSpace(suffix))
            throw new ArgumentException("Jazor SPA fallback allowed path suffixes cannot be empty.", nameof(suffix));

        var normalized = suffix.Trim();
        if (!normalized.StartsWith('/'))
            throw new ArgumentException("Jazor SPA fallback allowed path suffixes must start with '/'.", nameof(suffix));

        return normalized;
    }

    private static string NormalizeStaticSpaFallbackPagePath(string webRootPagePath)
    {
        if (string.IsNullOrWhiteSpace(webRootPagePath))
            throw new ArgumentException("Jazor SPA fallback static page path cannot be empty.", nameof(webRootPagePath));

        var normalized = webRootPagePath.Trim().Replace('\\', '/');
        if (!normalized.StartsWith('/'))
            throw new ArgumentException("Jazor SPA fallback static page path must start with '/'.", nameof(webRootPagePath));

        if (!Path.HasExtension(normalized))
            throw new ArgumentException("Jazor SPA fallback static page path must point to a file with an extension.", nameof(webRootPagePath));

        var relativePath = normalized.TrimStart('/');
        if (relativePath.Length == 0)
            throw new ArgumentException("Jazor SPA fallback static page path cannot point to the web root itself.", nameof(webRootPagePath));

        var segments = relativePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Any(static segment => segment == ".."))
            throw new ArgumentException("Jazor SPA fallback static page path cannot escape the web root.", nameof(webRootPagePath));

        return string.Join("/", segments);
    }
}
