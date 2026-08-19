#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

var options = ScriptArguments.Parse(args);

var repoRoot = WikiScriptHelpers.RequireRepoRoot();
var sampleRoot = Path.Combine(repoRoot, "samples", "Wiki");
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
        "-p:UseSharedCompilation=false",
        // Wiki defaults to debug while developing. A publish gate must explicitly select the
        // release bundle regardless of the managed assembly configuration.
        "-p:JazorMode=release"
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
    jazorRoot = Path.Combine(hostRoot, "jazor");
    indexTemplatePath = Path.Combine(hostRoot, "host", "index.template.html");
    faviconPath = Path.Combine(webRoot, "favicon.svg");
    stdoutLog = Path.Combine(hostRoot, ".wiki-publish-smoke.stdout.log");
    stderrLog = Path.Combine(hostRoot, ".wiki-publish-smoke.stderr.log");

    if (!Directory.Exists(jazorRoot))
    {
        throw new InvalidOperationException("Published Jazor artifacts were not copied to: " + jazorRoot);
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

WikiScriptHelpers.EnsureFileExists(indexTemplatePath, "host HTML template");
WikiScriptHelpers.EnsureFileExists(faviconPath, "favicon asset");

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

var browserEntryPath = options.Publish ? "/jazor/bundle.js" : "/jazor/main.mjs";
if (options.Publish)
{
    AssertReleaseArtifacts(jazorRoot);
}
else
{
    AssertDebugArtifacts(jazorRoot);
}

// 目录由同一次 Wiki 构建生成，docs 增删页面时无需手工同步验证路由表。
// /search 是手写工具页，保留其带查询的专项断言，避免和 docs 页面循环重复。
var docsRoutes = ReadDocsRouteExpectations(Path.Combine(sampleRoot, "obj", "wiki", "WikiDocsContent.g.cs"));

var browserAssets = options.Publish
    ? new List<AssetExpectation>
    {
        new("/jazor/bundle.js", "createApp(", null, new[] { "ecmascript-style:v1", "WikiDocsContent", "RenderDocsPage" }),
        new("/jazor/bundle.js.map", "\"file\":\"bundle.js\"", "application/json", new[] { "main.mjs", "components/wiki-home.mjs", "components/wiki-styles.mjs" }),
        new("/site.css", ".wiki-shell", null, Array.Empty<string>()),
        new("/favicon.svg", "<svg", null, Array.Empty<string>()),
        new("/vendor/vue@3.5.16.mjs", "createApp(", null, Array.Empty<string>())
    }
    : new List<AssetExpectation>
    {
        new("/jazor/main.mjs", "createApp(", null, Array.Empty<string>()),
        new("/jazor/main.mjs.map", "\"file\":\"main.mjs\"", "application/json", new[] { "AppModule.cs", "\"sourcesContent\"" }),
        new("/jazor/components/wiki-home.mjs", "搜索文档页面", null, Array.Empty<string>()),
        new("/jazor/components/wiki-home.mjs.map", "\"file\":\"components/wiki-home.mjs\"", "application/json", new[] { "WikiHomeModule.cs", "WikiHomeModule.DocumentContract.cs", "\"sourcesContent\"" }),
        // Debug 图中 style() 走 Import：组件模块引用 style.mjs 运行时，版本标记由运行时携带
        new("/jazor/components/wiki-styles.mjs", "from \"style.mjs\"", null, new[] { "background-color" }),
        new("/jazor/style.mjs", "ecmascript-style:v1", null, Array.Empty<string>()),
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
        AssertContains(content, WikiScriptHelpers.GetExternalPath(normalizedPathBase, browserEntryPath), "browser entry in served route " + route.Path);
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
        AssertCspMetaUsesResponseNonce(response, content, "served route " + route.Path);
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
        AssertCspMetaUsesResponseNonce(response, content, "served route /search?q=compiler");
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
        AssertCspMetaUsesResponseNonce(response, content, "served unknown route /guides/missing-page");
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

List<RouteExpectation> ReadDocsRouteExpectations(string generatedCatalogPath)
{
    WikiScriptHelpers.EnsureFileExists(generatedCatalogPath, "generated Wiki docs catalog");
    var catalog = File.ReadAllText(generatedCatalogPath, Encoding.UTF8);
    var paths = ReadGeneratedStringArray(catalog, "PagePaths");
    var titles = ReadGeneratedStringArray(catalog, "PageTitles");
    var summaries = ReadGeneratedStringArray(catalog, "PageSummaries");

    if (paths.Count != titles.Count || paths.Count != summaries.Count)
    {
        throw new InvalidOperationException(
            "WikiDocsContent parallel route arrays have inconsistent lengths: paths=" + paths.Count +
            ", titles=" + titles.Count + ", summaries=" + summaries.Count + ".");
    }

    var routes = new List<RouteExpectation>();
    var foundSearch = false;
    for (var index = 0; index < paths.Count; index++)
    {
        var path = paths[index];
        if (path == "/search")
        {
            foundSearch = true;
            continue;
        }

        if (!path.StartsWith('/', StringComparison.Ordinal) || titles[index].Length == 0 || summaries[index].Length == 0)
        {
            throw new InvalidOperationException("Invalid generated Wiki route metadata at index " + index + ".");
        }

        routes.Add(new RouteExpectation(path, titles[index] + " | jazor.wiki", summaries[index], "index, follow"));
    }

    if (!foundSearch || routes.Count == 0 || !routes.Any(route => route.Path == "/"))
    {
        throw new InvalidOperationException("WikiDocsContent must contain both the root docs route and the /search utility route.");
    }

    return routes;
}

List<string> ReadGeneratedStringArray(string catalog, string arrayName)
{
    var declaration = "internal static readonly string[] " + arrayName;
    var declarationIndex = catalog.IndexOf(declaration, StringComparison.Ordinal);
    if (declarationIndex < 0)
    {
        throw new InvalidOperationException("WikiDocsContent is missing " + arrayName + ".");
    }

    var assignmentIndex = catalog.IndexOf('=', declarationIndex);
    var position = assignmentIndex < 0 ? -1 : catalog.IndexOf('[', assignmentIndex + 1);
    if (position < 0)
    {
        throw new InvalidOperationException("WikiDocsContent has no array initializer for " + arrayName + ".");
    }

    position++;
    var values = new List<string>();
    while (true)
    {
        SkipGeneratedWhitespace(catalog, ref position);
        if (position >= catalog.Length)
            throw new InvalidOperationException("WikiDocsContent array " + arrayName + " is not terminated.");

        if (catalog[position] == ']')
            return values;

        if (catalog[position] != '"')
            throw new InvalidOperationException("Unexpected token in WikiDocsContent array " + arrayName + ".");

        var valueStart = ++position;
        while (position < catalog.Length && catalog[position] != '"')
        {
            if (catalog[position] == '\\')
                position++;
            position++;
        }

        if (position >= catalog.Length)
            throw new InvalidOperationException("Unterminated string in WikiDocsContent array " + arrayName + ".");

        // importer 的 CsString 只会写入 \\n、\\t、\\\" 和 \\\\，Regex.Unescape 与该受控契约一一对应。
        var escapedValue = catalog.Substring(valueStart, position - valueStart);
        values.Add(Regex.Unescape(escapedValue));
        position++;

        SkipGeneratedWhitespace(catalog, ref position);
        if (position < catalog.Length && catalog[position] == ',')
            position++;
    }
}

void SkipGeneratedWhitespace(string text, ref int position)
{
    while (position < text.Length && char.IsWhiteSpace(text[position]))
        position++;
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

void AssertCspMetaUsesResponseNonce(HttpResponseMessage response, string html, string description)
{
    var responsePolicy = WikiScriptHelpers.GetHeaderValue(response, "Content-Security-Policy")
        ?? throw new InvalidOperationException("Missing Content-Security-Policy for " + description + ".");
    var nonceMatch = Regex.Match(responsePolicy, @"script-src[^;]*'nonce-(?<nonce>[^']+)'", RegexOptions.CultureInvariant);
    if (!nonceMatch.Success)
    {
        throw new InvalidOperationException("Content-Security-Policy does not contain a script nonce for " + description + ".");
    }

    var metaMatch = Regex.Match(
        html,
        @"<meta\s+http-equiv=""Content-Security-Policy""\s+content=""(?<policy>[^""]*)""\s*/?>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    if (!metaMatch.Success)
    {
        throw new InvalidOperationException("Missing CSP meta tag for " + description + ".");
    }

    var metaPolicy = WebUtility.HtmlDecode(metaMatch.Groups["policy"].Value);
    AssertContains(metaPolicy, "nonce-" + nonceMatch.Groups["nonce"].Value, "CSP meta nonce matching response header for " + description);
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

void AssertDebugArtifacts(string artifactRoot)
{
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "main.mjs"), "emitted main module");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "main.mjs.map"), "emitted main source map");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "jazor-manifest.json"), "emit manifest");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "components", "wiki-home.mjs"), "emitted Wiki component module");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "components", "wiki-home.mjs.map"), "emitted Wiki component source map");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "components", "wiki-styles.mjs"), "emitted Wiki CSS module");
}

void AssertReleaseArtifacts(string artifactRoot)
{
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "bundle.js"), "production browser bundle");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "bundle.js.map"), "production browser bundle source map");

    // A normal browser release must not accidentally publish the debug module graph. SSR has
    // a separate jazor/ssr/ root, so these root-level paths remain an unambiguous check.
    // 正常浏览器 release 不应误发布 Debug 模块图；SSR 使用独立 jazor/ssr/ 根目录，因此这些根路径可明确检查。
    foreach (var unexpectedPath in new[]
    {
        Path.Combine(artifactRoot, "main.mjs"),
        Path.Combine(artifactRoot, "jazor-manifest.json"),
        Path.Combine(artifactRoot, "style.mjs"),
        Path.Combine(artifactRoot, "components")
    })
    {
        if (File.Exists(unexpectedPath) || Directory.Exists(unexpectedPath))
        {
            throw new InvalidOperationException("Release publish unexpectedly retained debug artifact: " + unexpectedPath);
        }
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
