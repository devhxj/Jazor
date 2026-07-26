#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

var options = ScriptArguments.Parse(args);

var repoRoot = WikiScriptHelpers.RequireRepoRoot();
var sampleRoot = Path.Combine(repoRoot, "src", "Wiki");
var hostProject = Path.Combine(sampleRoot, "Wiki.csproj");
var publishRoot = Path.Combine(repoRoot, ".tmp", "wiki-publish-smoke-" + Environment.ProcessId);
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");
var normalizedPathBase = WikiScriptHelpers.NormalizePathBase(options.PathBase);
var rootUrl = $"http://localhost:{options.Port}";
var healthUrl = rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/health");
var configuration = options.Publish && !options.ConfigurationWasExplicit ? "Release" : options.Configuration;

string hostRoot = sampleRoot;
string webRoot = Path.Combine(sampleRoot, "wwwroot");
string jazorRoot = Path.Combine(sampleRoot, "jazor");
string mainModulePath = Path.Combine(jazorRoot, "main.mjs");
string componentModulePath = Path.Combine(jazorRoot, "components", "wiki-home.mjs");
string manifestPath = Path.Combine(jazorRoot, "jazor-manifest.json");
string moduleTextPath = componentModulePath;
string indexTemplatePath = Path.Combine(sampleRoot, "host", "index.template.html");
string faviconPath = Path.Combine(webRoot, "favicon.svg");
string stdoutLog = Path.Combine(sampleRoot, $".wiki-smoke-{Environment.ProcessId}.stdout.log");
string stderrLog = Path.Combine(sampleRoot, $".wiki-smoke-{Environment.ProcessId}.stderr.log");

if (options.Publish && (options.Build || options.BuildLocal))
{
    throw new InvalidOperationException("--publish already performs its own publish build. Do not combine it with --build or --build-local.");
}

if (options.Publish)
{
    WikiScriptHelpers.EnsureDirectoryDeletedWithinRepo(repoRoot, publishRoot);
    var publishArguments = new List<string>
    {
        "publish",
        hostProject,
        "-c",
        configuration,
        "-o",
        publishRoot,
        "/m:1",
        "/p:BuildInParallel=false",
        "/nr:false",
        "-p:UseSharedCompilation=false"
    };

    if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
    {
        publishArguments.Add("-p:JazorIsolatedBaseOutputRoot=" + WikiScriptHelpers.ResolveBuildRoot(repoRoot, options.BaseOutputPath));
    }

    if (!string.IsNullOrWhiteSpace(options.BaseIntermediateOutputPath))
    {
        publishArguments.Add("-p:JazorIsolatedBaseIntermediateOutputRoot=" + WikiScriptHelpers.ResolveBuildRoot(repoRoot, options.BaseIntermediateOutputPath));
    }

    await WikiScriptHelpers.RunDotNetAsync(
        publishArguments,
        workdir: repoRoot,
        dotnetCliHome: dotnetCliHome);

    hostRoot = publishRoot;
    webRoot = Path.Combine(hostRoot, "wwwroot");
    jazorRoot = Path.Combine(webRoot, "jazor");
    mainModulePath = Path.Combine(jazorRoot, "main.mjs");
    componentModulePath = Path.Combine(jazorRoot, "components", "wiki-home.mjs");
    manifestPath = Path.Combine(jazorRoot, "jazor-manifest.json");
    moduleTextPath = componentModulePath;
    indexTemplatePath = Path.Combine(hostRoot, "host", "index.template.html");
    faviconPath = Path.Combine(webRoot, "favicon.svg");
    stdoutLog = Path.Combine(hostRoot, ".wiki-publish-smoke.stdout.log");
    stderrLog = Path.Combine(hostRoot, ".wiki-publish-smoke.stderr.log");

    var publishShadowJazorRoot = Path.Combine(hostRoot, "jazor");
    if (Directory.Exists(publishShadowJazorRoot))
    {
        throw new InvalidOperationException("Unexpected publish shadow directory: " + publishShadowJazorRoot + ". Publish output must serve /jazor only from wwwroot/jazor.");
    }
}
else if (options.BuildLocal)
{
    var buildLocalArguments = new List<string>
    {
        "run",
        "--file",
        Path.Combine("scripts", "csharp", "wiki-build-local.cs"),
        "--",
        "--configuration",
        configuration
    };

    if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
    {
        buildLocalArguments.Add("--base-output-path");
        buildLocalArguments.Add(options.BaseOutputPath);
    }

    if (!string.IsNullOrWhiteSpace(options.BaseIntermediateOutputPath))
    {
        buildLocalArguments.Add("--base-intermediate-output-path");
        buildLocalArguments.Add(options.BaseIntermediateOutputPath);
    }

    await WikiScriptHelpers.RunDotNetAsync(
        buildLocalArguments,
        workdir: repoRoot,
        dotnetCliHome: dotnetCliHome);
}
else if (options.Build)
{
    var buildArguments = new List<string>
    {
        "build",
        hostProject,
        "-c",
        configuration,
        "/m:1",
        "/p:BuildInParallel=false",
        "/nr:false",
        "-p:UseSharedCompilation=false"
    };

    if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
    {
        buildArguments.Add("-p:JazorIsolatedBaseOutputRoot=" + WikiScriptHelpers.ResolveBuildRoot(repoRoot, options.BaseOutputPath));
    }

    if (!string.IsNullOrWhiteSpace(options.BaseIntermediateOutputPath))
    {
        buildArguments.Add("-p:JazorIsolatedBaseIntermediateOutputRoot=" + WikiScriptHelpers.ResolveBuildRoot(repoRoot, options.BaseIntermediateOutputPath));
    }

    await WikiScriptHelpers.RunDotNetAsync(
        buildArguments,
        workdir: repoRoot,
        dotnetCliHome: dotnetCliHome);
}

WikiScriptHelpers.EnsureFileExists(mainModulePath, "emitted main module");
WikiScriptHelpers.EnsureFileExists(componentModulePath, "emitted wiki component module");
WikiScriptHelpers.EnsureFileExists(manifestPath, "emit manifest");
WikiScriptHelpers.EnsureFileExists(indexTemplatePath, "host HTML template");
WikiScriptHelpers.EnsureFileExists(faviconPath, "favicon asset");
WikiScriptHelpers.EnsureFileExists(moduleTextPath, "emitted docs shell module");

var indexTemplateContent = await File.ReadAllTextAsync(indexTemplatePath, Encoding.UTF8);
AssertContains(indexTemplateContent, "id=\"app\"", "Vue mount root in host HTML template");
AssertContains(indexTemplateContent, "__WIKI_SITE_CSS_URL__", "stylesheet token in host HTML template");
AssertContains(indexTemplateContent, "__WIKI_FAVICON_URL__", "favicon token in host HTML template");
AssertContains(indexTemplateContent, "__WIKI_MAIN_MODULE_URL__", "main module token in host HTML template");
AssertContains(indexTemplateContent, "\"System/\": \"__WIKI_SYSTEM_IMPORT_BASE__\"", "CLR runtime import-map token in host HTML template");
AssertContains(indexTemplateContent, "data-wiki-path-base=\"__WIKI_PATH_BASE__\"", "path-base token in host HTML template");
AssertContains(indexTemplateContent, "__WIKI_VENDOR_VUE_URL__", "vendored dependency marker in host HTML template");
AssertNotContains(indexTemplateContent, "unpkg.com", "forbidden CDN URL in host HTML template");

var siteCssPath = Path.Combine(webRoot, "site.css");
WikiScriptHelpers.EnsureFileExists(siteCssPath, "site.css");
var siteCssContent = await File.ReadAllTextAsync(siteCssPath, Encoding.UTF8);
foreach (var marker in new[]
{
    ".skip-link",
    ".breadcrumbs",
    ".meta-card",
    ".feedback-button",
    ".reading-progress-track",
    ".search-result-card",
    ".mobile-utility-bar",
    ".drawer-backdrop",
    "html[data-theme=\"light\"]"
})
{
    AssertContains(siteCssContent, marker, "Wiki shell CSS marker");
}

var mainModuleContent = await File.ReadAllTextAsync(mainModulePath, Encoding.UTF8);
AssertContains(mainModuleContent, "createApp(", "main entry marker in emitted module");
AssertContains(mainModuleContent, "app.mount(\"#app\")", "main entry marker in emitted module");
AssertImportedSystemModulesExist(mainModuleContent, jazorRoot, "emitted main module");

var moduleContent = await File.ReadAllTextAsync(moduleTextPath, Encoding.UTF8);
AssertImportedSystemModulesExist(moduleContent, jazorRoot, "emitted docs shell module");
foreach (var marker in new[]
{
    "概览",
    "页面未找到",
    "requested-route",
    "window.history.replaceState",
    "window.history.pushState",
    "window.onpopstate = onPopState",
    "window.onhashchange = onHashChange",
    "window.onscroll = onScroll",
    "window.navigator.clipboard",
    "clipboard.writeText",
    "wiki-nav-search-input",
    "search-result-card",
    "search-mark",
    "feedback-button",
    "drawer-backdrop",
    "浏览",
    "本页目录",
    "主题：深色",
    "复制页面链接",
    "有帮助",
    "需改进",
    "Jolt 已从转型分支退役",
    "d68aecbb00b23aa35735c9a269b2e987c7815b05"
})
{
    AssertContains(moduleContent, marker, "emitted docs shell marker");
}

var docsRoutes = new List<RouteExpectation>
{
    new("/", "概览 | jazor.wiki", "面向生产的 Jazor 文档外壳，完全使用 ECMAScript.Vue3 H 函数编写。", "index, follow"),
    new("/search", "搜索 | jazor.wiki", "通过子系统、路由片段、工作流或标签搜索完整 Wiki 语料库。", "noindex, nofollow"),
    new("/guides/getting-started", "快速开始 | jazor.wiki", "本地运行站点，理解路由模型，并端到端验证发射的 Wiki 宿主。", "index, follow"),
    new("/guides/project-lines", "项目线路 | jazor.wiki", "了解当前 Razor-to-Vue 转型主线、共享编译器基础和已经退役的 Jolt 历史边界。", "index, follow"),
    new("/guides/content-model", "内容模型 | jazor.wiki", "代码优先的页面元数据、显式章节和保持可读性的 C# 导航契约。", "index, follow"),
    new("/guides/navigation-discovery", "导航与发现 | jazor.wiki", "读者如何通过分组导航、章节目录、相关页面和 404 恢复在文档外壳中移动。", "index, follow"),
    new("/guides/information-architecture", "信息架构 | jazor.wiki", "路由、关注组、页面顺序和命名规则如何保持文档表面在增长时保持一致性。", "index, follow"),
    new("/guides/topic-index", "主题索引 | jazor.wiki", "使用以路由为先的索引，按关注点跳转到 Jazor 主题，而无需记住确切 URL。", "index, follow"),
    new("/guides/glossary", "术语表 | jazor.wiki", "编译器、运行时、宿主和文档术语在仓库中使用的共享词汇表。", "index, follow"),
    new("/guides/faq", "常见问题 | jazor.wiki", "贡献者首次接触 Jazor 或 Wiki 时最常见问题的简短回答。", "index, follow"),
    new("/guides/troubleshooting", "故障排除 | jazor.wiki", "从最常见的本地 Wiki、运行时模块和编译器边界故障中恢复。", "index, follow"),
    new("/engineering/h-function-authoring", "H 函数编写 | jazor.wiki", "为什么 H 函数是此 Wiki 的生产编写表面，以及保持其可维护性的约定。", "index, follow"),
    new("/engineering/compiler-overview", "编译器概览 | jazor.wiki", "编译器管线、活动契约和深入阅读方向的高级概览。", "index, follow"),
    new("/engineering/compiler-support-boundary", "编译器支持边界 | jazor.wiki", "受控输入、使用点验证、语义擦除和显式失败边界的活动编译器契约。", "index, follow"),
    new("/engineering/route-catalog-contract", "路由目录契约 | jazor.wiki", "为什么 `WikiHomeModule.RouteContract.cs` 是路由元数据、正文分发、目录锚点和相邻页面流的唯一注册面。", "index, follow"),
    new("/engineering/host-semantic-seams", "宿主语义接缝 | jazor.wiki", "WhiteList、Alias、Inline、Import 和 Compile 如何在支持的宿主语义面上划分职责。", "index, follow"),
    new("/engineering/import-emit-contract", "导入与发射契约 | jazor.wiki", "导入发现、模块 AST 组装、生成的目录和面向宿主的文件物化之间的稳定边界。", "index, follow"),
    new("/engineering/runtime-catalog", "CLR 运行时目录 | jazor.wiki", "CLR 导入 helper 如何变为浏览器可用的 `System/*` 运行时模块，以及哪些保障使该目录可安全发布。", "index, follow"),
    new("/engineering/jolt-host", "Jolt 宿主（历史） | jazor.wiki", "Jolt 已从转型分支退役；本页仅保留基线、能力范围和历史恢复入口。", "index, follow"),
    new("/engineering/razorvue-library-mode", "RazorVue 库模式 | jazor.wiki", "用于将 Razor 组件编译为 JavaScript 产物的构建时库模式，无需完整开发宿主。", "index, follow"),
    new("/engineering/vueroute-bindings", "VueRoute 绑定 | jazor.wiki", "独立的 Vue Router 绑定库、其宿主表面范围，以及将测试排除在编译器套件之外的拆分验证路径。", "index, follow"),
    new("/operations/content-governance", "内容治理 | jazor.wiki", "代码优先文档内容如何被拥有、编辑、审查和发布，而不偏离发射的产品外壳。", "index, follow"),
    new("/operations/deployment", "部署 | jazor.wiki", "构建输出、回退路由、冒烟验证和 Wiki 的静态交付契约。", "index, follow"),
    new("/operations/testing-verification", "测试与验证 | jazor.wiki", "编译器、发射和运维冒烟检查如何协同保护生产文档表面。", "index, follow")
};

var browserAssets = new List<AssetExpectation>
{
    new("/jazor/main.mjs", "createApp(", null, Array.Empty<string>()),
    new("/jazor/main.mjs.map", "\"file\":\"main.mjs\"", "application/json", new[] { "AppModule.cs", "\"sourcesContent\"" }),
    new("/jazor/components/wiki-home.mjs", "搜索文档页面", null, Array.Empty<string>()),
    new("/jazor/components/wiki-home.mjs.map", "\"file\":\"components/wiki-home.mjs\"", "application/json", new[] { "WikiHomeModule.cs", "WikiHomeModule.DocumentContract.cs", "\"sourcesContent\"" }),
    new("/jazor/System/StringModule.js", "export", null, Array.Empty<string>()),
    new("/site.css", ".wiki-shell", null, Array.Empty<string>()),
    new("/favicon.svg", "<svg", null, Array.Empty<string>()),
    new("/vendor/vue@3.5.16.mjs", "createApp(", null, Array.Empty<string>())
};

var discoveryDocuments = new List<DiscoveryExpectation>
{
    new("/robots.txt", $"Sitemap: {rootUrl}{WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/sitemap.xml")}", "", "text/plain; charset=utf-8", "public, max-age=300, must-revalidate"),
    new("/sitemap.xml", $"<loc>{rootUrl}{WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/")}</loc>", $"<loc>{rootUrl}{WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/search")}</loc>", "application/xml; charset=utf-8", "public, max-age=300, must-revalidate")
};

Process? hostProcess = null;
var keepLogs = false;
try
{
    var hostArguments = options.Publish
        ? new[] { "Wiki.dll", "--urls", rootUrl }
        : new[] { "run", "--project", hostProject, "--no-launch-profile", "-c", configuration, "--no-build", "--no-restore", "--urls", rootUrl };

    hostProcess = WikiScriptHelpers.StartProcess(
        fileName: "dotnet",
        arguments: hostArguments,
        workdir: options.Publish ? hostRoot : sampleRoot,
        environment:
        [
            new KeyValuePair<string, string?>("DOTNET_CLI_HOME", dotnetCliHome),
            new KeyValuePair<string, string?>("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1"),
            new KeyValuePair<string, string?>("ASPNETCORE_URLS", rootUrl),
            new KeyValuePair<string, string?>("Wiki__PathBase", normalizedPathBase)
        ],
        stdoutLogPath: stdoutLog,
        stderrLogPath: stderrLog);

    using var healthResponse = await WikiScriptHelpers.WaitForHttpOkAsync(
        healthUrl,
        hostProcess,
        TimeSpan.FromSeconds(options.StartupTimeoutSeconds),
        failureContext: $"See logs: {stdoutLog} ; {stderrLog}");
    var healthBody = (await healthResponse.Content.ReadAsStringAsync()).Trim().Trim('"');
    if (healthBody != "ok")
    {
        throw new InvalidOperationException("Unexpected /health response body: '" + healthBody + "'");
    }

    foreach (var asset in browserAssets)
    {
        using var response = await WikiScriptHelpers.GetAsync(rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, asset.Path));
        EnsureStatusCode(response, HttpStatusCode.OK, asset.Path);
        var content = await response.Content.ReadAsStringAsync();
        AssertContains(content, asset.Snippet, "served browser asset " + asset.Path);
        foreach (var extraSnippet in asset.ExtraSnippets)
        {
            AssertContains(content, extraSnippet, "served browser asset " + asset.Path);
        }

        if (!string.IsNullOrWhiteSpace(asset.ContentType))
        {
            AssertHeaderEquals(response, "Content-Type", asset.ContentType!, "Content-Type for served browser asset " + asset.Path);
        }

        AssertHeaderEquals(response, "Referrer-Policy", "strict-origin-when-cross-origin", "Referrer-Policy for served browser asset " + asset.Path);
        AssertHeaderEquals(response, "X-Content-Type-Options", "nosniff", "X-Content-Type-Options for served browser asset " + asset.Path);
        AssertHeaderEquals(response, "X-Frame-Options", "DENY", "X-Frame-Options for served browser asset " + asset.Path);

        var expectedCacheControl = asset.Path.StartsWith("/vendor/", StringComparison.Ordinal)
            ? "public, max-age=31536000, immutable"
            : "no-cache, must-revalidate";
        AssertHeaderEquals(response, "Cache-Control", expectedCacheControl, "Cache-Control for served browser asset " + asset.Path);
    }

    foreach (var document in discoveryDocuments)
    {
        using var response = await WikiScriptHelpers.GetAsync(rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, document.Path));
        EnsureStatusCode(response, HttpStatusCode.OK, document.Path);
        var content = await response.Content.ReadAsStringAsync();
        AssertContains(content, document.Snippet, "served discovery document " + document.Path);
        if (!string.IsNullOrWhiteSpace(document.MissingSnippet))
        {
            AssertNotContains(content, document.MissingSnippet!, "forbidden discovery document marker in " + document.Path);
        }

        AssertHeaderEquals(response, "Content-Type", document.ContentType, "Content-Type for served discovery document " + document.Path);
        AssertHeaderEquals(response, "Cache-Control", document.CacheControl, "Cache-Control for served discovery document " + document.Path);
        AssertHeaderEquals(response, "Referrer-Policy", "strict-origin-when-cross-origin", "Referrer-Policy for served discovery document " + document.Path);
        AssertHeaderEquals(response, "X-Content-Type-Options", "nosniff", "X-Content-Type-Options for served discovery document " + document.Path);
        AssertHeaderEquals(response, "X-Frame-Options", "DENY", "X-Frame-Options for served discovery document " + document.Path);
    }

    foreach (var route in docsRoutes)
    {
        using var response = await WikiScriptHelpers.GetAsync(rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, route.Path));
        EnsureStatusCode(response, HttpStatusCode.OK, route.Path);
        var content = await response.Content.ReadAsStringAsync();
        AssertContains(content, "id=\"app\"", "Vue mount root in served route " + route.Path);
        AssertContains(content, WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/jazor/main.mjs"), "main module entry in served route " + route.Path);
        AssertContains(content, "\"System/\": \"" + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/jazor/System/") + "\"", "CLR runtime import-map entry in served route " + route.Path);
        AssertContains(content, "data-wiki-path-base=\"" + normalizedPathBase + "\"", "path-base marker in served route " + route.Path);
        AssertRouteMetadata(content, route, rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, route.Path), "served route " + route.Path);

        AssertHeaderEquals(response, "Referrer-Policy", "strict-origin-when-cross-origin", "Referrer-Policy for served route " + route.Path);
        AssertHeaderEquals(response, "X-Content-Type-Options", "nosniff", "X-Content-Type-Options for served route " + route.Path);
        AssertHeaderEquals(response, "X-Frame-Options", "DENY", "X-Frame-Options for served route " + route.Path);
        AssertHeaderEquals(response, "Cache-Control", "no-cache, must-revalidate", "Cache-Control for served route " + route.Path);
        AssertHeaderEquals(response, "Cross-Origin-Opener-Policy", "same-origin", "Cross-Origin-Opener-Policy for served route " + route.Path);
        AssertHeaderEquals(response, "Cross-Origin-Resource-Policy", "same-origin", "Cross-Origin-Resource-Policy for served route " + route.Path);
        AssertHeaderEquals(response, "X-Permitted-Cross-Domain-Policies", "none", "X-Permitted-Cross-Domain-Policies for served route " + route.Path);
        AssertHeaderEquals(response, "Permissions-Policy", "accelerometer=(), autoplay=(), camera=(), display-capture=(), geolocation=(), gyroscope=(), hid=(), microphone=(), payment=(), usb=(), clipboard-read=(self), clipboard-write=(self)", "Permissions-Policy for served route " + route.Path);
        AssertHeaderMatches(response, "Content-Security-Policy", "script-src 'self' 'nonce-[^']+'", "Content-Security-Policy nonce for served route " + route.Path);
        AssertContains(content, "script type=\"importmap\" nonce=\"", "importmap nonce marker in served route " + route.Path);
        if (route.Robots == "noindex, nofollow")
        {
            AssertHeaderEquals(response, "X-Robots-Tag", "noindex, nofollow", "X-Robots-Tag for served route " + route.Path);
        }
    }

    using (var response = await WikiScriptHelpers.GetAsync(rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/search") + "?q=compiler"))
    {
        EnsureStatusCode(response, HttpStatusCode.OK, "/search?q=compiler");
        var content = await response.Content.ReadAsStringAsync();
        AssertRouteMetadata(content,
            new RouteExpectation("/search?q=compiler", "搜索: compiler | jazor.wiki", "搜索结果：\"compiler\"，覆盖路由元数据、标签、页面正文和章节标题。", "noindex, nofollow"),
            rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/search") + "?q=compiler",
            "served route /search?q=compiler");
        AssertHeaderEquals(response, "X-Robots-Tag", "noindex, nofollow", "X-Robots-Tag for served route /search?q=compiler");
    }

    using (var response = await WikiScriptHelpers.GetAsync(rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/guides/missing-page"), allowNonSuccess: true))
    {
        EnsureStatusCode(response, HttpStatusCode.NotFound, "/guides/missing-page");
        var content = await response.Content.ReadAsStringAsync();
        AssertRouteMetadata(content,
            new RouteExpectation("/guides/missing-page", "页面未找到 | jazor.wiki", "当前路径未在 Wiki 页面目录中注册。", "noindex, nofollow"),
            rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/guides/missing-page"),
            "served unknown route /guides/missing-page");
        AssertHeaderEquals(response, "X-Robots-Tag", "noindex, nofollow", "X-Robots-Tag for served unknown route /guides/missing-page");
    }

    Console.WriteLine(options.Publish
        ? "Wiki publish smoke verification passed."
        : "Wiki smoke verification passed.");
}
catch
{
    keepLogs = true;
    throw;
}
finally
{
    if (hostProcess is not null && !hostProcess.HasExited)
    {
        hostProcess.Kill(entireProcessTree: true);
        await hostProcess.WaitForExitAsync();
    }

    if (!keepLogs)
    {
        foreach (var logPath in new[] { stdoutLog, stderrLog })
        {
            if (File.Exists(logPath))
            {
                await WikiScriptHelpers.RemoveFileWithRetryAsync(logPath);
            }
        }

        if (options.Publish && Directory.Exists(publishRoot))
        {
            await WikiScriptHelpers.RemoveDirectoryWithRetryAsync(publishRoot);
        }
    }
}

void EnsureStatusCode(HttpResponseMessage response, HttpStatusCode expectedStatusCode, string path)
{
    if (response.StatusCode != expectedStatusCode)
    {
        throw new InvalidOperationException($"Unexpected {path} status code: {(int)response.StatusCode}");
    }
}

void AssertContains(string text, string snippet, string description)
{
    if (!text.Contains(snippet, StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Missing " + description + ": expected to find '" + snippet + "'.");
    }
}

void AssertNotContains(string text, string snippet, string description)
{
    if (text.Contains(snippet, StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Unexpected " + description + ": found '" + snippet + "'.");
    }
}

void AssertHeaderEquals(HttpResponseMessage response, string headerName, string expectedValue, string description)
{
    var actualValue = WikiScriptHelpers.GetHeaderValue(response, headerName);
    if (actualValue is null)
    {
        throw new InvalidOperationException("Missing " + description + ": response header '" + headerName + "' was not present.");
    }

    var actualComparable = string.Join(',', actualValue.Split(',').Select(segment => segment.Trim()).Where(segment => segment.Length > 0).OrderBy(segment => segment, StringComparer.Ordinal));
    var expectedComparable = string.Join(',', expectedValue.Split(',').Select(segment => segment.Trim()).Where(segment => segment.Length > 0).OrderBy(segment => segment, StringComparer.Ordinal));
    if (!string.Equals(actualComparable, expectedComparable, StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Unexpected " + description + ": expected '" + expectedValue + "', actual '" + actualValue + "'.");
    }
}

void AssertHeaderMatches(HttpResponseMessage response, string headerName, string pattern, string description)
{
    var actualValue = WikiScriptHelpers.GetHeaderValue(response, headerName);
    if (actualValue is null)
    {
        throw new InvalidOperationException("Missing " + description + ": response header '" + headerName + "' was not present.");
    }

    if (!Regex.IsMatch(actualValue, pattern))
    {
        throw new InvalidOperationException("Unexpected " + description + ": expected pattern '" + pattern + "', actual '" + actualValue + "'.");
    }
}

void AssertRouteMetadata(string html, RouteExpectation expected, string expectedAbsoluteUrl, string description)
{
    var encoder = HtmlEncoder.Default;
    var encodedTitle = encoder.Encode(expected.Title);
    var encodedDescription = encoder.Encode(expected.Description);
    var encodedRobots = encoder.Encode(expected.Robots);
    var encodedAbsoluteUrl = encoder.Encode(expectedAbsoluteUrl);

    AssertContains(html, "<title>" + encodedTitle + "</title>", description + " title");
    AssertContains(html, $"meta name=\"description\" content=\"{encodedDescription}\"", description + " description");
    AssertContains(html, $"meta name=\"robots\" content=\"{encodedRobots}\"", description + " robots");
    AssertContains(html, $"link rel=\"canonical\" href=\"{encodedAbsoluteUrl}\"", description + " canonical");
    AssertContains(html, $"meta property=\"og:title\" content=\"{encodedTitle}\"", description + " og:title");
    AssertContains(html, $"meta property=\"og:description\" content=\"{encodedDescription}\"", description + " og:description");
    AssertContains(html, $"meta property=\"og:url\" content=\"{encodedAbsoluteUrl}\"", description + " og:url");
    AssertContains(html, $"meta name=\"twitter:title\" content=\"{encodedTitle}\"", description + " twitter:title");
    AssertContains(html, $"meta name=\"twitter:description\" content=\"{encodedDescription}\"", description + " twitter:description");
}

void AssertImportedSystemModulesExist(string text, string jazorRootPath, string description)
{
    foreach (Match match in Regex.Matches(text, "from \"(System/[^\"]+\\.js)\""))
    {
        var relativePath = match.Groups[1].Value;
        var physicalPath = Path.Combine(jazorRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        WikiScriptHelpers.EnsureFileExists(physicalPath, description + " dependency " + relativePath);
    }
}

internal sealed record ScriptArguments
{
    public int Port { get; init; } = 4173;

    public string Configuration { get; init; } = "Debug";

    public bool ConfigurationWasExplicit { get; init; }

    public string? BaseOutputPath { get; init; }

    public string? BaseIntermediateOutputPath { get; init; }

    public string? PathBase { get; init; }

    public bool Build { get; init; }

    public bool BuildLocal { get; init; }

    public bool Publish { get; init; }

    public int StartupTimeoutSeconds { get; init; } = 30;

    public static ScriptArguments Parse(string[] args)
    {
        var result = new ScriptArguments();
        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--port":
                    result = result with { Port = int.Parse(GetValue(args, ref index, argument)) };
                    break;
                case "--configuration":
                    result = result with
                    {
                        Configuration = GetValue(args, ref index, argument),
                        ConfigurationWasExplicit = true
                    };
                    break;
                case "--base-output-path":
                    result = result with { BaseOutputPath = GetValue(args, ref index, argument) };
                    break;
                case "--base-intermediate-output-path":
                    result = result with { BaseIntermediateOutputPath = GetValue(args, ref index, argument) };
                    break;
                case "--path-base":
                    result = result with { PathBase = GetValue(args, ref index, argument) };
                    break;
                case "--build":
                    result = result with { Build = true };
                    break;
                case "--build-local":
                    result = result with { BuildLocal = true };
                    break;
                case "--publish":
                    result = result with { Publish = true };
                    break;
                case "--startup-timeout-seconds":
                    result = result with { StartupTimeoutSeconds = int.Parse(GetValue(args, ref index, argument)) };
                    break;
                case "--help":
                case "-h":
                    WriteUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new InvalidOperationException("Unknown argument: " + argument);
            }
        }

        return result;
    }

    private static string GetValue(string[] args, ref int index, string argumentName)
    {
        if (index + 1 >= args.Length)
        {
            throw new InvalidOperationException("Missing value for " + argumentName);
        }

        index++;
        return args[index];
    }

    private static void WriteUsage()
    {
        Console.WriteLine("Usage: dotnet run --file scripts/csharp/wiki-verify-smoke.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --port <number>");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --base-output-path <path>");
        Console.WriteLine("  --base-intermediate-output-path <path>");
        Console.WriteLine("  --path-base </docs>");
        Console.WriteLine("  --build");
        Console.WriteLine("  --build-local");
        Console.WriteLine("  --publish");
        Console.WriteLine("  --startup-timeout-seconds <seconds>");
    }
}

internal sealed record RouteExpectation(string Path, string Title, string Description, string Robots);

internal sealed record AssetExpectation(string Path, string Snippet, string? ContentType, string[] ExtraSnippets);

internal sealed record DiscoveryExpectation(string Path, string Snippet, string? MissingSnippet, string ContentType, string CacheControl);

internal static class WikiScriptHelpers
{
    public static string RequireRepoRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root containing Jazor.slnx was not found from the current directory upward.");
    }

    public static string NormalizePathBase(string? pathBase)
    {
        if (string.IsNullOrWhiteSpace(pathBase) || pathBase == "/")
        {
            return string.Empty;
        }

        if (!pathBase.StartsWith('/', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("--path-base must start with '/'.");
        }

        return pathBase.Length > 1 && pathBase.EndsWith('/', StringComparison.Ordinal)
            ? pathBase[..^1]
            : pathBase;
    }

    public static string GetExternalPath(string normalizedPathBase, string logicalPath)
    {
        if (string.IsNullOrWhiteSpace(logicalPath) || !logicalPath.StartsWith('/', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Logical path must start with '/': " + logicalPath);
        }

        if (string.IsNullOrEmpty(normalizedPathBase))
        {
            return logicalPath;
        }

        return logicalPath == "/"
            ? normalizedPathBase + "/"
            : normalizedPathBase + logicalPath;
    }

    public static string ResolveBuildRoot(string repoRoot, string path)
    {
        if (path.Contains("$(", StringComparison.Ordinal))
        {
            return path;
        }

        var resolved = Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(repoRoot, path));

        return resolved.EndsWith(Path.DirectorySeparatorChar)
            ? resolved
            : resolved + Path.DirectorySeparatorChar;
    }

    public static async Task RunDotNetAsync(
        IReadOnlyList<string> arguments,
        string workdir,
        string dotnetCliHome,
        CancellationToken cancellationToken = default)
    {
        using var process = StartProcess(
            fileName: "dotnet",
            arguments: arguments,
            workdir: workdir,
            environment:
            [
                new KeyValuePair<string, string?>("DOTNET_CLI_HOME", dotnetCliHome),
                new KeyValuePair<string, string?>("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1")
            ]);

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Process failed with exit code {process.ExitCode}: dotnet {string.Join(' ', arguments)}");
        }
    }

    public static Process StartProcess(
        string fileName,
        IReadOnlyList<string> arguments,
        string workdir,
        IReadOnlyList<KeyValuePair<string, string?>>? environment = null,
        string? stdoutLogPath = null,
        string? stderrLogPath = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workdir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = stdoutLogPath is not null,
            RedirectStandardError = stderrLogPath is not null
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        if (environment is not null)
        {
            foreach (var entry in environment)
            {
                if (entry.Value is null)
                {
                    startInfo.Environment.Remove(entry.Key);
                }
                else
                {
                    startInfo.Environment[entry.Key] = entry.Value;
                }
            }
        }

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start process: " + fileName);

        if (stdoutLogPath is not null)
        {
            _ = RedirectAsync(process.StandardOutput, stdoutLogPath);
        }

        if (stderrLogPath is not null)
        {
            _ = RedirectAsync(process.StandardError, stderrLogPath);
        }

        return process;
    }

    public static async Task<HttpResponseMessage> WaitForHttpOkAsync(
        string url,
        Process process,
        TimeSpan timeout,
        string? failureContext = null,
        CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(3)
        };

        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (process.HasExited)
            {
                throw new InvalidOperationException("Process exited before responding on " + url + FormatFailureContext(failureContext));
            }

            try
            {
                var response = await client.GetAsync(url, cancellationToken);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response;
                }
            }
            catch
            {
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        }

        throw new TimeoutException("Timed out waiting for " + url + FormatFailureContext(failureContext));
    }

    public static async Task<HttpResponseMessage> GetAsync(string url, bool allowNonSuccess = false, CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        var response = await client.GetAsync(url, cancellationToken);
        if (!allowNonSuccess && !response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Unexpected status code {(int)response.StatusCode} for {url}");
        }

        return response;
    }

    public static string? GetHeaderValue(HttpResponseMessage response, string headerName)
    {
        if (response.Headers.TryGetValues(headerName, out var values))
        {
            return string.Join(", ", values);
        }

        if (response.Content.Headers.TryGetValues(headerName, out values))
        {
            return string.Join(", ", values);
        }

        return null;
    }

    public static void EnsureFileExists(string path, string description)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Missing " + description + ": " + path, path);
        }
    }

    public static void EnsureDirectoryDeletedWithinRepo(string repoRoot, string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        var fullRepoRoot = Path.GetFullPath(repoRoot);
        var fullPath = Path.GetFullPath(path);
        if (!fullPath.StartsWith(fullRepoRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to delete outside repository root: " + fullPath);
        }

        Directory.Delete(fullPath, recursive: true);
    }

    public static async Task RemoveFileWithRetryAsync(string path, int attempts = 6, int delayMilliseconds = 250)
    {
        for (var attempt = 0; attempt < attempts; attempt++)
        {
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                File.Delete(path);
                return;
            }
            catch when (attempt < attempts - 1)
            {
                await Task.Delay(delayMilliseconds);
            }
        }
    }

    public static async Task RemoveDirectoryWithRetryAsync(string path, int attempts = 6, int delayMilliseconds = 250)
    {
        for (var attempt = 0; attempt < attempts; attempt++)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch when (attempt < attempts - 1)
            {
                await Task.Delay(delayMilliseconds);
            }
        }
    }

    private static async Task RedirectAsync(StreamReader? reader, string? logPath)
    {
        if (reader is null || logPath is null)
        {
            return;
        }

        await using var writer = new StreamWriter(logPath, append: false, Encoding.UTF8);
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line is null)
            {
                break;
            }

            await writer.WriteLineAsync(line);
        }
    }

    private static string FormatFailureContext(string? failureContext)
        => string.IsNullOrWhiteSpace(failureContext) ? string.Empty : ". " + failureContext;
}
