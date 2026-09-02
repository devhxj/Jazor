using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

var options = SmokeOptions.Parse(args);
var repoRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
var adminRoot = Path.Combine(repoRoot, "samples", "JazorAdmin");
var buildScript = Path.Combine(adminRoot, "build-local.cs");
var generatedOutputRoot = string.IsNullOrWhiteSpace(options.GeneratedOutputRoot)
    ? Path.Combine(repoRoot, ".tmp", "sample-smoke", "JazorAdmin", options.Configuration, "jazor")
    : ResolvePath(options.GeneratedOutputRoot, repoRoot);
var injectGeneratedOutputRoot = Path.Combine(
    repoRoot,
    ".tmp",
    "sample-smoke",
    "JazorAdmin",
    "InjectSmoke",
    options.Configuration,
    "jazor");
var baseOutputPath = options.BaseOutputPath ?? Path.Combine(repoRoot, ".tmp", "jazoradmin-smoke-out");
var baseIntermediateOutputPath = options.BaseIntermediateOutputPath ?? Path.Combine(repoRoot, ".tmp", "jazoradmin-smoke-obj");

SetCommonEnvironment(repoRoot);
AssertConciseTypeNames(repoRoot);

if (!options.FrontendOnly)
{
    CleanDirectory(generatedOutputRoot, repoRoot);
    // Let build-local create a unique current-source package version. Pinning a released version
    // here could let restore select NuGet.org instead of the local artifact under test.
    // 由 build-local 生成唯一的当前源码包版本；固定已发布版本可能让还原误选 NuGet.org 包。
    RunDotNet(repoRoot,
    [
        "run",
        "--no-launch-profile",
        "--file",
        buildScript,
        "--",
        "--configuration", options.Configuration,
        "--jazor-dir", generatedOutputRoot,
        "--inject-jazor-dir", injectGeneratedOutputRoot,
        "--base-output-path", baseOutputPath,
        "--base-intermediate-output-path", baseIntermediateOutputPath
    ]);
}

AssertPathExists(ResolveAssemblyPath(options.Configuration, baseOutputPath), "JazorAdmin assembly");
AssertPathExists(ResolveInjectAssemblyPath(options.Configuration, baseOutputPath), "JazorAdmin InjectSmoke assembly");
AssertGeneratedArtifacts(generatedOutputRoot);
AssertInjectGeneratedArtifacts(injectGeneratedOutputRoot);
if (!options.SkipBrowser)
    await VerifyBrowserSmokeAsync(repoRoot, adminRoot, generatedOutputRoot, injectGeneratedOutputRoot);

Console.WriteLine("JazorAdmin sample smoke verification passed.");
Console.WriteLine(options.SkipBrowser
    ? "Verified: local package consumption, native and VueInject JazorAdmin rebuilds, generated render-function .mjs artifacts, and manifests. Browser verification was skipped."
    : "Verified: local package consumption, native and VueInject JazorAdmin rebuilds, generated render-function .mjs artifacts, manifests, and browser mount smoke.");

static void AssertConciseTypeNames(string repoRoot)
{
    var prefixedType = new Regex(
        @"\b(?:class|interface|struct|enum|record(?:\s+(?:class|struct))?)\s+JazorAdmin[A-Za-z0-9_]*\b",
        RegexOptions.CultureInvariant);
    var sourceRoots = new[]
    {
        Path.Combine(repoRoot, "samples", "JazorAdmin"),
        Path.Combine(repoRoot, "samples", "JazorAdmin.Test"),
        Path.Combine(repoRoot, "src", "Jazor.Admin")
    };
    var violations = sourceRoots
        .SelectMany(sourceRoot => Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Select(path => (SourceRoot: sourceRoot, Path: path)))
        .Where(path =>
        {
            var relativePath = Path.GetRelativePath(path.SourceRoot, path.Path);
            return !relativePath.StartsWith("bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
                   !relativePath.StartsWith("obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
                   !relativePath.StartsWith(".tmp" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        })
        .SelectMany(item => File.ReadLines(item.Path)
            .Select((line, index) => new { item.Path, Line = index + 1, Text = line })
            .Where(item => prefixedType.IsMatch(item.Text)))
        .Select(item => Path.GetRelativePath(repoRoot, item.Path) + ":" + item.Line + ": " + item.Text.Trim())
        .ToArray();

    if (violations.Length > 0)
    {
        throw new InvalidOperationException(
            "JazorAdmin is the product namespace, not a type-name prefix. Use a concise contextual type name:" +
            Environment.NewLine + string.Join(Environment.NewLine, violations));
    }
}

static void AssertGeneratedArtifacts(string generatedOutputRoot)
{
    var manifestPath = Path.Combine(generatedOutputRoot, "jazor-manifest.json");
    AssertPathExists(manifestPath, "generated manifest");
    var modulePaths = ReadModulePaths(manifestPath);
    var appModulePath = RequireModulePath(modulePaths, "JazorAdmin.App");
    var bootstrapModulePath = RequireModulePath(modulePaths, "JazorAdmin.Bootstrap");
    var routesModulePath = RequireModulePath(modulePaths, "JazorAdmin.Routes");
    var routeCatalogModulePath = RequireModulePath(modulePaths, "Jazor.Admin.AdminRouteCatalog");
    var applicationFrameModulePath = RequireModulePath(modulePaths, "Jazor.Admin.ApplicationFrame");
    var adminLayoutModulePath = RequireModulePath(modulePaths, "JazorAdmin.TDesignLayout");
    var sidebarModulePath = RequireModulePath(modulePaths, "JazorAdmin.TDesignSidebarMenu");
    var pageContainerModulePath = RequireModulePath(modulePaths, "JazorAdmin.TDesignPageContainer");
    var headerBarModulePath = RequireModulePath(modulePaths, "JazorAdmin.TDesignHeaderBar");
    var localizationModulePath = RequireModulePath(modulePaths, "JazorAdmin.Localization");
    var errorPageModulePath = RequireModulePath(modulePaths, "JazorAdmin.ErrorPage");
    var apiClientModulePath = RequireModulePath(modulePaths, "JazorAdmin.ApiClient");
    var organizationModulePath = RequireModulePath(modulePaths, "JazorAdmin.OrganizationPage");
    var accessControlModulePath = RequireModulePath(modulePaths, "JazorAdmin.AccessControlPage");
    var accountModulePath = RequireModulePath(modulePaths, "JazorAdmin.AccountPage");
    var ssoAppModulePath = RequireModulePath(modulePaths, "JazorAdmin.SsoAppPage");
    var ssoScopeModulePath = RequireModulePath(modulePaths, "JazorAdmin.SsoScopePage");
    var ssoGrantModulePath = RequireModulePath(modulePaths, "JazorAdmin.SsoGrantPage");
    var settingsModulePath = RequireModulePath(modulePaths, "JazorAdmin.SettingsPage");
    var schedulesModulePath = RequireModulePath(modulePaths, "JazorAdmin.SchedulePage");
    var dashboardModulePath = RequireModulePath(modulePaths, "JazorAdmin.DashboardPage");
    var auditModulePath = RequireModulePath(modulePaths, "JazorAdmin.AuditPage");
    var iconBarModulePath = RequireModulePath(modulePaths, "JazorAdmin.IconBar");
    var routeTabsModulePath = RequireModulePath(modulePaths, "JazorAdmin.RouteTabs");
    var routeBreadcrumbModulePath = RequireModulePath(modulePaths, "JazorAdmin.RouteBreadcrumb");
    var componentModules = new[]
    {
        (appModulePath, "JazorAdmin app module"),
        (bootstrapModulePath, "JazorAdmin router bootstrap module"),
        (routesModulePath, "JazorAdmin route catalog module"),
        (routeCatalogModulePath, "admin route catalog module"),
        (applicationFrameModulePath, "admin application frame module"),
        (adminLayoutModulePath, "TDesign admin layout module"),
        (sidebarModulePath, "TDesign sidebar module"),
        (pageContainerModulePath, "TDesign page container module"),
        (headerBarModulePath, "TDesign header bar module"),
        (localizationModulePath, "JazorAdmin localization module"),
        (errorPageModulePath, "JazorAdmin error page module"),
        (apiClientModulePath, "JazorAdmin API client module"),
        (organizationModulePath, "organization management module"),
        (accessControlModulePath, "access control management module"),
        (accountModulePath, "account management module"),
        (ssoAppModulePath, "SSO application module"),
        (ssoScopeModulePath, "SSO scope module"),
        (ssoGrantModulePath, "SSO grant module"),
        (settingsModulePath, "configuration center module"),
        (schedulesModulePath, "task scheduling module"),
        (dashboardModulePath, "administration dashboard module"),
        (auditModulePath, "audit log module"),
        (iconBarModulePath, "JazorAdmin IconBar module"),
        (routeTabsModulePath, "JazorAdmin route tabs module"),
        (routeBreadcrumbModulePath, "JazorAdmin route breadcrumb module")
    };
    foreach (var (relativePath, description) in componentModules)
        AssertPathExists(Path.Combine(generatedOutputRoot, relativePath), "generated " + description);

    foreach (var (relativePath, description) in componentModules)
    {
        var module = File.ReadAllText(Path.Combine(generatedOutputRoot, relativePath));
        AssertDoesNotContain(module, ".vue", "legacy SFC reference in " + description);
        AssertDoesNotContain(module, "scope.buildRenderTree(builder)", "legacy scoped render-tree call in " + description);
        AssertDoesNotContain(module, "builder.finish()", "legacy render builder completion in " + description);
    }

    // Management pages must consume the public TDesign bindings directly. Keep this assertion at
    // the emitted-module boundary so a reintroduced sample-local bridge cannot hide in a package.
    var nativeTDesignModules = new[]
    {
        (organizationModulePath, "organization management module"),
        (accessControlModulePath, "access control management module"),
        (accountModulePath, "account management module"),
        (ssoAppModulePath, "SSO application module"),
        (ssoScopeModulePath, "SSO scope module"),
        (ssoGrantModulePath, "SSO grant module"),
        (settingsModulePath, "configuration center module"),
        (schedulesModulePath, "task scheduling module"),
        (dashboardModulePath, "administration dashboard module"),
        (auditModulePath, "audit log module")
    };
    foreach (var (relativePath, description) in nativeTDesignModules)
    {
        var module = File.ReadAllText(Path.Combine(generatedOutputRoot, relativePath));
        AssertContains(module, "from \"tdesign-vue-next\"", "direct TDesign binding import in " + description);
        AssertDoesNotContain(module, "AdminControls", "sample-local controls bridge in " + description);
        AssertDoesNotContain(module, "AdminInput", "sample-local input bridge in " + description);
        AssertDoesNotContain(module, "AdminForm", "sample-local form bridge in " + description);
        AssertDoesNotContain(module, "AdminTable", "sample-local table bridge in " + description);
        AssertDoesNotContain(module, "AdminToggle", "sample-local toggle bridge in " + description);
    }

    foreach (var (relativePath, description) in componentModules.Where((_, index) => index == 0 || (index >= 4 && index != 9 && index != 11)))
    {
        var module = File.ReadAllText(Path.Combine(generatedOutputRoot, relativePath));
        AssertContains(module, "defineComponent", "Vue component wrapper in " + description);
        AssertContains(module, "function $renderDirect()", "direct VNode render function in " + description);
    }

    var appModule = File.ReadAllText(Path.Combine(generatedOutputRoot, appModulePath));
    var bootstrapModule = File.ReadAllText(Path.Combine(generatedOutputRoot, bootstrapModulePath));
    var routesModule = File.ReadAllText(Path.Combine(generatedOutputRoot, routesModulePath));
    var routeCatalogModule = File.ReadAllText(Path.Combine(generatedOutputRoot, routeCatalogModulePath));
    var adminLayoutModule = File.ReadAllText(Path.Combine(generatedOutputRoot, adminLayoutModulePath));
    var sidebarModule = File.ReadAllText(Path.Combine(generatedOutputRoot, sidebarModulePath));
    var pageContainerModule = File.ReadAllText(Path.Combine(generatedOutputRoot, pageContainerModulePath));
    var headerBarModule = File.ReadAllText(Path.Combine(generatedOutputRoot, headerBarModulePath));
    var errorPageModule = File.ReadAllText(Path.Combine(generatedOutputRoot, errorPageModulePath));
    var apiClientModule = File.ReadAllText(Path.Combine(generatedOutputRoot, apiClientModulePath));
    var organizationModule = File.ReadAllText(Path.Combine(generatedOutputRoot, organizationModulePath));
    var accessControlModule = File.ReadAllText(Path.Combine(generatedOutputRoot, accessControlModulePath));
    var accountModule = File.ReadAllText(Path.Combine(generatedOutputRoot, accountModulePath));
    var ssoAppModule = File.ReadAllText(Path.Combine(generatedOutputRoot, ssoAppModulePath));
    var ssoScopeModule = File.ReadAllText(Path.Combine(generatedOutputRoot, ssoScopeModulePath));
    var ssoGrantModule = File.ReadAllText(Path.Combine(generatedOutputRoot, ssoGrantModulePath));
    var settingsModule = File.ReadAllText(Path.Combine(generatedOutputRoot, settingsModulePath));
    var schedulesModule = File.ReadAllText(Path.Combine(generatedOutputRoot, schedulesModulePath));
    var dashboardModule = File.ReadAllText(Path.Combine(generatedOutputRoot, dashboardModulePath));
    var auditModule = File.ReadAllText(Path.Combine(generatedOutputRoot, auditModulePath));
    var iconBarModule = File.ReadAllText(Path.Combine(generatedOutputRoot, iconBarModulePath));
    var routeTabsModule = File.ReadAllText(Path.Combine(generatedOutputRoot, routeTabsModulePath));
    var routeBreadcrumbModule = File.ReadAllText(Path.Combine(generatedOutputRoot, routeBreadcrumbModulePath));
    var manifest = File.ReadAllText(manifestPath);

    AssertContains(appModule, "defineComponent", "Vue component wrapper in JazorAdmin app module");
    AssertContains(appModule, "function $renderDirect()", "direct VNode render function in JazorAdmin app module");
    AssertContains(appModule, "JazorAdmin", "JazorAdmin app text in generated module");
    AssertContains(appModule, "useRoute", "Vue Router route injection in JazorAdmin app module");
    AssertContains(appModule, "onMounted", "session restoration mount hook in JazorAdmin app module");
    AssertContains(appModule, "scope.OnAfterRender(firstRender)", "initial Razor lifecycle invocation in JazorAdmin app module");
    AssertContains(appModule, "RestoreSession();", "mounted session restoration invocation in JazorAdmin app module");
    AssertContains(bootstrapModule, "createRouter", "Vue Router creation in JazorAdmin bootstrap module");
    AssertContains(bootstrapModule, "createWebHistory", "Vue Router web history in JazorAdmin bootstrap module");
    AssertContains(bootstrapModule, "RouterView", "Vue Router view in JazorAdmin bootstrap module");
    AssertContains(bootstrapModule, "export function Boot", "JazorAdmin bootstrap export name");
    AssertContains(routeCatalogModule, "routeTarget", "strongly typed route target in admin route catalog module");
    AssertDoesNotContain(routeCatalogModule, "return path == null ? null : from(path)", "undefined union factory in admin route catalog module");
    AssertContains(routesModule, "organizations.structure", "organization structure route in JazorAdmin routes module");
    AssertContains(routesModule, "authorization.roles", "authorization roles route in JazorAdmin routes module");
    AssertContains(routesModule, "accounts", "account route in JazorAdmin routes module");
    AssertContains(routesModule, "sso.applications", "OpenIddict application route in JazorAdmin routes module");
    AssertContains(routesModule, "sso.scopes", "OpenIddict scope route in JazorAdmin routes module");
    AssertContains(routesModule, "sso.authorizations", "OpenIddict authorization route in JazorAdmin routes module");
    AssertContains(routesModule, "sso.tokens", "OpenIddict token route in JazorAdmin routes module");
    AssertContains(routesModule, "settings", "configuration center route in JazorAdmin routes module");
    AssertContains(routesModule, "schedules", "task scheduling route in JazorAdmin routes module");
    AssertDoesNotContain(routesModule, "operations/releases", "retired release route in JazorAdmin routes module");
    AssertContains(adminLayoutModule, "data-shell-command", "sidebar toggle command in TDesign admin layout module");
    AssertContains(adminLayoutModule, "toggle-sidebar", "sidebar toggle command key in TDesign admin layout module");
    AssertContains(adminLayoutModule, "CollapsedChanged", "controlled collapsed component callback in JazorAdmin layout module");
    AssertContains(adminLayoutModule, "Horizontal: true", "top navigation variant in JazorAdmin layout module");
    AssertContains(adminLayoutModule, "variant: \"text\"", "TDesign text button variant in admin layout module");
    AssertContains(sidebarModule, "theme: MenuTheme()", "TDesign theme propagation in sidebar module");
    AssertContains(pageContainerModule, "return \"primary\";", "TDesign primary button theme in page container module");
    AssertContains(pageContainerModule, "align: \"center\"", "TDesign centered action layout in page container module");
    AssertContains(iconBarModule, "data-iconbar", "IconBar root marker in JazorAdmin IconBar module");
    AssertContains(iconBarModule, "data-iconbar-key", "IconBar item marker in JazorAdmin IconBar module");
    AssertContains(iconBarModule, "Button, HeadMenu, Icon, Menu, MenuItem, Popup", "TDesign IconBar control imports in JazorAdmin IconBar module");
    // Direct render lowering now emits Vue block calls (openBlock + createBlock) for proven
    // child shapes instead of plain h(...) calls; assert the current block emission contract.
    AssertContains(iconBarModule, "createBlock(Menu,", "TDesign collapsed menu in JazorAdmin IconBar module");
    AssertContains(iconBarModule, "createBlock(HeadMenu,", "TDesign head menu in JazorAdmin IconBar module");
    AssertContains(iconBarModule, "createBlock(MenuItem,", "TDesign menu item in JazorAdmin IconBar module");
    AssertContains(iconBarModule, "operations:", "TDesign IconBar operations slot in JazorAdmin IconBar module");
    AssertContains(iconBarModule, "ja-iconbar__quick-actions", "IconBar floating action group in JazorAdmin IconBar module");
    AssertContains(iconBarModule, "toggle-quick-actions", "IconBar floating action trigger in JazorAdmin IconBar module");
    AssertDoesNotContain(iconBarModule, "ja-iconbar__link", "legacy custom IconBar link in JazorAdmin IconBar module");
    AssertContains(headerBarModule, "HeadMenu", "TDesign Starter head menu in header bar module");
    AssertContains(headerBarModule, "operations:", "TDesign Header operations slot in header bar module");
    AssertContains(headerBarModule, "ja-tdesign-header__navigation", "navigation slot region in TDesign header bar module");
    AssertContains(headerBarModule, "data-shell-logo-visible", "Header logo visibility contract in JazorAdmin header module");
    AssertContains(routeTabsModule, "Tabs", "TDesign route tabs in JazorAdmin module");
    AssertContains(routeTabsModule, "dragSort: true", "Starter route tab drag sorting");
    AssertContains(routeTabsModule, "onRemove: Remove", "Starter route tab removal handler");
    AssertContains(routeTabsModule, "context-menu", "Starter route tab context menu");
    AssertContains(routeBreadcrumbModule, "maxItemWidth: \"150px\"", "Starter route breadcrumb width");
    AssertContains(routeBreadcrumbModule, "data-route-breadcrumb-current", "Starter route breadcrumb current marker");
    AssertContains(errorPageModule, "data-error-kind", "typed error kind marker in JazorAdmin error page module");
    AssertContains(errorPageModule, "aria-labelledby", "error title accessibility relation in JazorAdmin error page module");
    AssertContains(errorPageModule, "aria-describedby", "error description accessibility relation in JazorAdmin error page module");
    AssertContains(errorPageModule, "data-error-action", "error recovery action in JazorAdmin error page module");
    AssertContains(apiClientModule, "GetSession", "session transport in JazorAdmin API client module");
    AssertContains(apiClientModule, "credentials: \"same-origin\"", "Fetch credentials WebIDL enum mapping in JazorAdmin API client module");
    AssertDoesNotContain(apiClientModule, "headers: null", "omitted optional Fetch headers in JazorAdmin API client module");
    AssertDoesNotContain(apiClientModule, "body: null", "omitted optional Fetch body in JazorAdmin API client module");
    AssertContains(organizationModule, "CreateChildOrganization", "child organization command in organization management module");
    AssertContains(organizationModule, "scope.OnParametersSet();", "route parameter lifecycle in organization management module");
    AssertContains(accessControlModule, "ReplaceRoleGrants", "role grant command in access control management module");
    AssertContains(accessControlModule, "scope.OnParametersSet();", "route parameter lifecycle in access control management module");
    AssertContains(accountModule, "GetAccounts", "account query in account management module");
    AssertContains(accountModule, "ResetAccountPassword", "account password command in account management module");
    AssertContains(accountModule, "scope.OnAfterRender(firstRender)", "initial Razor lifecycle invocation in account management module");
    AssertContains(ssoAppModule, "GetApps", "OpenIddict application query in application module");
    AssertContains(ssoAppModule, "RotateAppSecret", "OpenIddict secret rotation in application module");
    AssertContains(ssoScopeModule, "GetScopes", "OpenIddict scope query in scope module");
    AssertContains(ssoScopeModule, "UpdateScope", "OpenIddict scope update in scope module");
    AssertContains(ssoGrantModule, "GetAuthorizations", "OpenIddict authorization query in grant module");
    AssertContains(ssoGrantModule, "GetTokens", "OpenIddict token query in grant module");
    AssertContains(ssoGrantModule, "scope.OnParametersSet();", "route parameter lifecycle in grant module");
    AssertContains(settingsModule, "CreateSetting", "configuration center create command");
    AssertContains(settingsModule, "UpdateSetting", "configuration center update command");
    AssertContains(schedulesModule, "TriggerSchedule", "task scheduling manual run command");
    AssertContains(schedulesModule, "GetScheduleRuns", "task scheduling history query");
    AssertContains(dashboardModule, "SignInTrendItems", "audit-backed sign-in trend projection");
    AssertContains(dashboardModule, "TokenIssuances", "dashboard token issuance KPI");
    AssertContains(dashboardModule, "data-portal", "dashboard application portal marker");
    AssertContains(dashboardModule, "PortalApplications", "dashboard portal application data");
    AssertContains(auditModule, "GetAudit", "audit log query transport");
    AssertContains(auditModule, "data-audit-filter", "audit filter markers");
    AssertContains(auditModule, "data-audit-command", "audit filter commands");
    AssertContains(auditModule, "data-audit-event", "audit row marker");
    foreach (var (relativePath, description) in componentModules)
        AssertContains(manifest, "\"" + relativePath + "\"", description + " manifest entry");
    // The manifest legitimately carries the "jazor.vue" runtime provider id; the retired-SFC
    // contract is about generated module paths, so exclude the provider id before checking.
    AssertDoesNotContain(
        manifest.Replace("\"jazor.vue\"", string.Empty, StringComparison.Ordinal),
        ".vue",
        "legacy SFC artifact in JazorAdmin manifest");
    AssertDoesNotContain(manifest, "release-table", "retired release table artifact in JazorAdmin manifest");
    AssertDoesNotContain(manifest, "settings-form", "retired settings form artifact in JazorAdmin manifest");
}

static void AssertInjectGeneratedArtifacts(string generatedOutputRoot)
{
    var manifestPath = Path.Combine(generatedOutputRoot, "jazor-manifest.json");
    AssertPathExists(manifestPath, "generated JazorAdmin VueInject manifest");
    var modulePaths = ReadModulePaths(manifestPath);
    var appModulePath = RequireModulePath(modulePaths, "JazorAdmin.InjectSmoke.InjectApp");
    var containerModulePath = RequireModulePath(modulePaths, "JazorAdmin.InjectSmoke.InjectPageContainer");
    var appPath = Path.Combine(generatedOutputRoot, appModulePath);
    var containerPath = Path.Combine(generatedOutputRoot, containerModulePath);
    AssertPathExists(appPath, "generated JazorAdmin VueInject app module");
    AssertPathExists(containerPath, "generated JazorAdmin VueInject container module");

    foreach (var modulePath in Directory.EnumerateFiles(
                 Path.Combine(generatedOutputRoot, "components"),
                 "*.mjs",
                 SearchOption.AllDirectories))
    {
        var module = File.ReadAllText(modulePath);
        var description = "JazorAdmin VueInject module " + Path.GetFileName(modulePath);
        AssertDoesNotContain(module, ".vue", "legacy SFC reference in " + description);
        AssertDoesNotContain(module, "scope.buildRenderTree(builder)", "legacy scoped render-tree call in " + description);
        AssertDoesNotContain(module, "builder.finish()", "legacy render builder completion in " + description);
        AssertDoesNotContain(module, "slots.injected-extra", "invalid hyphenated slot access in " + description);
    }

    var app = File.ReadAllText(appPath);
    var container = File.ReadAllText(containerPath);
    var manifest = File.ReadAllText(manifestPath);
    AssertContains(app, "from \"./page.mjs\"", "VueInject implementation import");
    AssertDoesNotContain(app, "from \"../admin/page.mjs\"", "stale VueInject contract import");
    AssertContains(app, "injectedTitle", "VueInject runtime prop name");
    AssertContains(app, "\"injected-extra\"", "VueInject runtime slot name");
    AssertContains(container, "slots[\"injected-extra\"]", "VueInject bracket slot access");
    AssertContains(container, "href;", "RenderFragment helper pattern declaration");
    AssertContains(manifest, "\"" + appModulePath + "\"", "VueInject app manifest entry");
    AssertContains(manifest, "\"" + containerModulePath + "\"", "VueInject container manifest entry");
}

static IReadOnlyDictionary<string, string> ReadModulePaths(string manifestPath)
{
    using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
    return document.RootElement.GetProperty("modules")
        .EnumerateArray()
        .ToDictionary(
            static module => module.GetProperty("typeName").GetString()!,
            static module => module.GetProperty("path").GetString()!,
            StringComparer.Ordinal);
}

static string RequireModulePath(IReadOnlyDictionary<string, string> paths, string typeName)
    => paths.TryGetValue(typeName, out var path)
        ? path
        : throw new InvalidOperationException($"Generated manifest did not contain module type '{typeName}'.");

static string ReadHarnessImportMap(string generatedOutputRoot)
{
    var path = Path.Combine(generatedOutputRoot, "importmap.json");
    AssertPathExists(path, "generated JazorAdmin browser import map");

    var importMap = JsonNode.Parse(File.ReadAllText(path))?.AsObject()
        ?? throw new InvalidOperationException("Generated JazorAdmin browser import map is not a JSON object.");
    var imports = importMap["imports"]?.AsObject()
        ?? throw new InvalidOperationException("Generated JazorAdmin browser import map has no imports object.");

    // The sample host serves artifacts under /jazor, while this isolated harness serves the same
    // copied tree at its document root. Rewrite only URL targets, preserving every emitted specifier.
    // 示例宿主从 /jazor 提供产物；隔离 harness 将同一份复制产物放在文档根目录，只改 URL
    // 目标，完整保留 Emit 生成的 specifier 集合和按需依赖闭包。
    foreach (var import in imports.ToArray())
    {
        var target = import.Value?.GetValue<string>()
            ?? throw new InvalidOperationException("Generated JazorAdmin browser import map contains an empty target for '" + import.Key + "'.");
        imports[import.Key] = RewriteHarnessArtifactPath(target);
    }

    return importMap.ToJsonString();
}

static string ReadHarnessStyleLinks(string generatedOutputRoot)
{
    var path = Path.Combine(generatedOutputRoot, "manifest.json");
    AssertPathExists(path, "generated JazorAdmin browser manifest");

    using var document = JsonDocument.Parse(File.ReadAllText(path));
    if (!document.RootElement.TryGetProperty("styles", out var styles) || styles.ValueKind != JsonValueKind.Array)
        return string.Empty;

    return string.Join(
        Environment.NewLine,
        styles.EnumerateArray().Select(style =>
        {
            var href = style.GetString()
                ?? throw new InvalidOperationException("Generated JazorAdmin browser manifest contains an empty stylesheet path.");
            return "<link rel=\"stylesheet\" href=\"" + RewriteHarnessArtifactPath(href) + "\">";
        }));
}

static string RewriteHarnessArtifactPath(string path)
{
    const string hostedArtifactRoot = "/jazor/";
    return path.StartsWith(hostedArtifactRoot, StringComparison.Ordinal)
        ? "/" + path[hostedArtifactRoot.Length..]
        : path;
}

static async Task VerifyBrowserSmokeAsync(
    string repoRoot,
    string adminRoot,
    string generatedOutputRoot,
    string injectGeneratedOutputRoot)
{
    var adminModules = ReadModulePaths(Path.Combine(generatedOutputRoot, "jazor-manifest.json"));
    var injectModules = ReadModulePaths(Path.Combine(injectGeneratedOutputRoot, "jazor-manifest.json"));
    var bootstrapModulePath = RequireModulePath(adminModules, "JazorAdmin.Bootstrap");
    var injectAppModulePath = RequireModulePath(injectModules, "JazorAdmin.InjectSmoke.InjectApp");
    var browserPath = ResolveBrowserExecutable();
    if (browserPath is null)
    {
        Console.WriteLine("JazorAdmin browser smoke skipped: Microsoft Edge, Chrome, or Chromium was not found. Set RAZORVUE_BROWSER_EXE to enable it.");
        return;
    }

    var denoPath = ResolveDenoHostRuntime(repoRoot);

    // Keep the browser harness on the exact materialized dependency closure. A hand-written map
    // only covered Vue and TDesign, so newly used on-demand libraries such as VuIcons were never
    // resolvable here even though the production artifact was complete.
    // 浏览器 harness 必须消费物化产物本身的依赖闭包；手写 map 只覆盖 Vue/TDesign，会遗漏
    // VuIcons 这类新加入的按需库，导致 harness 与真实产物脱节。
    var browserImportMap = ReadHarnessImportMap(generatedOutputRoot);
    var browserStyleLinks = ReadHarnessStyleLinks(generatedOutputRoot);

    var harnessRoot = Path.Combine(repoRoot, ".tmp", "sample-smoke", "JazorAdmin", "browser-" + Environment.ProcessId);
    SweepStaleBrowserHarnessRoots(Path.GetDirectoryName(harnessRoot)!, repoRoot);
    CleanDirectory(harnessRoot, repoRoot);
    Directory.CreateDirectory(harnessRoot);
    try
    {
        CopyDirectory(generatedOutputRoot, harnessRoot);
        CopyInjectBrowserArtifacts(injectGeneratedOutputRoot, harnessRoot);
        // The generated modules reference sample-owned branding by absolute path. Mirror the
        // packaged static directory so browser smoke verifies those paths instead of masking 404s.
        CopyDirectory(Path.Combine(adminRoot, "wwwroot", "brand"), Path.Combine(harnessRoot, "brand"));
        var faviconPath = Path.Combine(adminRoot, "wwwroot", "favicon.ico");
        if (File.Exists(faviconPath))
            File.Copy(faviconPath, Path.Combine(harnessRoot, "favicon.ico"), overwrite: true);
        var indexPath = Path.Combine(harnessRoot, "index.html");
        await File.WriteAllTextAsync(
            indexPath,
            $$"""
            <!doctype html>
            <html>
              <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>JazorAdmin browser smoke</title>
                {{browserStyleLinks}}
                <script type="importmap">
                {{browserImportMap}}
                </script>
              </head>
              <body>
                <div id="app"></div>
                <div id="inject-app"></div>
                <script>
                  addEventListener("error", (event) => {
                    // Chromium reports this asynchronous ResizeObserver delivery warning as a
                    // window error while responsive chart layout settles. It has no exception
                    // object or application stack and must not mask real browser failures.
                    const message = event.error?.message ?? event.message ?? "";
                    if (message === "ResizeObserver loop completed with undelivered notifications." ||
                        message === "ResizeObserver loop limit exceeded") {
                      return;
                    }
                    globalThis.__jazorAdminBrowserSmoke = {
                      ok: false,
                      message: message || "Browser module error",
                      stack: event.error?.stack ?? ""
                    };
                  });
                  addEventListener("unhandledrejection", (event) => {
                    globalThis.__jazorAdminBrowserSmoke = {
                      ok: false,
                      message: event.reason instanceof Error ? event.reason.message : String(event.reason),
                      stack: event.reason instanceof Error ? event.reason.stack : ""
                    };
                  });

                  // The browser harness validates generated RazorVue behavior in isolation. API
                  // semantics and real sign-in are covered by JazorAdmin.Test against the host.
                  // 浏览器 harness 只验证生成的 RazorVue 行为；真实 API 和登录由宿主集成测试覆盖。
                  const organization = {
                    id: "5e1246c9-9d41-48bb-bd50-02cadc780911",
                    code: "north-hub",
                    displayName: "North Hub"
                  };
                  const roles = [{
                    id: "ed671921-3218-451c-a92f-671cb5b83119",
                    code: "organization-admin",
                    displayName: "Organization administrator"
                  }];
                  const operations = [
                    { resource: "organizations", operation: "read", displayName: "Read" },
                    { resource: "organizations", operation: "manage", displayName: "Manage" },
                    { resource: "authorization", operation: "read", displayName: "Read" },
                    { resource: "authorization", operation: "manage", displayName: "Manage" }
                  ];
                  const accounts = [{
                    id: "9d04612a-86ed-4971-84f0-50ded72a8a30",
                    email: "platform.admin@example.test",
                    displayName: "Platform administrator",
                    enabled: true,
                    platformAdministrator: true
                  }];
                  const applications = [{
                    id: "c05e9d5a-e435-47cb-92f6-ca31f46ca8a5",
                    clientId: "jazoradmin-spa",
                    displayName: "JazorAdmin SPA",
                    profile: "interactive",
                    applicationType: "web",
                    clientType: "public",
                    consentType: "implicit",
                    requirePkce: true,
                    redirectUris: ["http://localhost/auth/callback"],
                    postLogoutRedirectUris: ["http://localhost/login"],
                    endpoints: ["authorization", "token", "end_session"],
                    grantTypes: ["authorization_code", "refresh_token"],
                    responseTypes: ["code"],
                    scopes: ["openid", "profile", "jazoradmin_api"]
                  }];
                  const scopes = [{
                    id: "c4351a3e-8f51-4e98-a505-6500c9d591e7",
                    name: "jazoradmin_api",
                    displayName: "JazorAdmin API",
                    description: "Administration API",
                    resources: ["jazoradmin_api"]
                  }];
                  const authorizations = [{
                    id: "authorization-smoke",
                    applicationId: applications[0].id,
                    clientId: "jazoradmin-spa",
                    subject: "smoke-operator",
                    status: "valid",
                    type: "permanent",
                    scopes: ["openid", "profile"],
                    createdAt: "2026-08-07T00:00:00Z"
                  }];
                  const tokens = [{
                    id: "token-smoke",
                    applicationId: applications[0].id,
                    clientId: "jazoradmin-spa",
                    authorizationId: authorizations[0].id,
                    subject: "smoke-operator",
                    status: "valid",
                    type: "access_token",
                    createdAt: "2026-08-07T00:00:00Z",
                    expiresAt: "2026-08-07T01:00:00Z",
                    redeemedAt: null
                  }];
                  const settings = [{
                    key: "ui.page-size",
                    group: "ui",
                    label: "Page size",
                    description: "Default row count for management tables.",
                    kind: "number",
                    value: "25",
                    updatedAt: "2026-08-07T00:00:00Z"
                  }];
                  const settingsRequests = [];
                  const schedules = [{
                    key: "openid-prune",
                    name: "Prune expired OpenID records",
                    description: "Removes invalid OpenIddict tokens and detached authorizations older than 14 days.",
                    cron: "0 15 2 * * ?",
                    enabled: true,
                    nextRunAt: "2026-08-08T02:15:00Z",
                    lastRunAt: null,
                    lastStatus: null,
                    lastMessage: null
                  }];
                  const scheduleRuns = [];
                  const auditEvents = [
                    {
                      id: "audit-created-application",
                      occurredAt: "2026-08-19T08:30:00Z",
                      actorId: "smoke-operator",
                      actorName: "Smoke operator",
                      action: "created",
                      objectType: "sso-application",
                      objectId: "audit-worker",
                      summary: "Audit worker"
                    },
                    {
                      id: "audit-issued-token",
                      occurredAt: "2026-08-18T11:15:00Z",
                      actorId: "smoke-operator",
                      actorName: "Smoke operator",
                      action: "issued",
                      objectType: "oidc-token",
                      objectId: "token-smoke",
                      summary: "Authorization code"
                    }
                  ];
                  const json = (data, status = 200) => Promise.resolve(new Response(JSON.stringify(data), {
                    status,
                    headers: { "content-type": "application/json" }
                  }));
                  globalThis.fetch = (input, init = {}) => {
                    const url = new URL(typeof input === "string" ? input : input.url, location.href);
                    const method = String(init.method ?? "GET").toUpperCase();
                    const body = init.body ? JSON.parse(String(init.body)) : null;
                    if (url.pathname === "/api/auth/session") return json({
                      userId: "74b0b0be-4b91-461f-9d57-c2f94aed4842",
                      email: "smoke.operator@example.test",
                      displayName: "Smoke operator",
                      roles: ["platform-administrator"],
                      organizations: [organization]
                    });
                    if (url.pathname === "/api/overview/") {
                      const recentRuns = Array.from({ length: 7 }, (_, index) => {
                        const day = new Date(Date.now() - (6 - index) * 86400000);
                        return {
                          date: day.toISOString().slice(0, 10),
                          succeeded: index + 1,
                          failed: index % 2
                        };
                      });
                      const recentAudit = Array.from({ length: 7 }, (_, index) => ({
                        date: new Date(Date.now() - (6 - index) * 86400000).toISOString().slice(0, 10),
                        signIns: index + 1,
                        tokenIssuances: index + 2
                      }));
                      return json({
                        accounts: 12,
                        enabledAccounts: 11,
                        organizations: 3,
                        organizationRoles: 5,
                        platformRoles: 1,
                        applications: 4,
                        scopes: 3,
                        authorizations: 2,
                        tokens: 6,
                        settings: 8,
                        schedules: 2,
                        enabledSchedules: 2,
                        recentRuns,
                        auditEvents: auditEvents.length,
                        tokenIssuances: 9,
                        recentAudit,
                        portalApplications: [{
                          clientId: "jazoradmin-demo-client",
                          displayName: "JazorAdmin Operations Demo",
                          launchUri: "http://127.0.0.1:49735"
                        }]
                      });
                    }
                    if (url.pathname === "/api/audit/" && method === "GET") {
                      const actor = url.searchParams.get("actor")?.trim().toLowerCase() ?? "";
                      const objectType = url.searchParams.get("object")?.trim().toLowerCase() ?? "";
                      const action = url.searchParams.get("action")?.trim().toLowerCase() ?? "";
                      const values = auditEvents.filter((event) =>
                        (!actor || event.actorName.toLowerCase().includes(actor) || event.actorId.toLowerCase().includes(actor)) &&
                        (!objectType || event.objectType.toLowerCase().includes(objectType)) &&
                        (!action || event.action.toLowerCase().includes(action)));
                      return json(values);
                    }
                    if (url.pathname === "/api/notifications/") return json([{
                      id: "notification-smoke",
                      source: "schedule",
                      title: "OpenID prune",
                      status: "failed",
                      startedAt: "2026-08-14T08:30:00.0000000Z",
                      message: "Simulated failure"
                    }]);
                    if (url.pathname === "/api/organizations/") return json([organization]);
                    if (url.pathname.endsWith("/members")) return json([{
                      membershipId: "5e7e599d-c9a3-40d6-bd1d-5ce2d3e976d8",
                      userId: "06b75c0b-89d3-4302-94c9-d5f356a04d7a",
                      email: "operator@example.test",
                      displayName: "Organization operator",
                      roles
                    }]);
                    if (url.pathname.endsWith("/authorization-resources")) return json(operations);
                    if (url.pathname.endsWith("/grants")) return json(operations.slice(0, 2));
                    if (url.pathname.endsWith("/roles")) return json(roles);
                    if (url.pathname.startsWith("/api/organizations/")) return json({
                      ...organization,
                      parentId: null,
                      children: [{
                        id: "f32d974e-2fe5-4429-851c-70d9e2ec72c2",
                        code: "north-clinic",
                        displayName: "North Clinic"
                      }]
                    });
                    if (url.pathname === "/api/accounts/") return json(accounts);
                    if (url.pathname === "/api/sso/applications" && method === "GET") return json(applications);
                    if (url.pathname === "/api/sso/applications" && method === "POST") {
                      const application = {
                        id: `application-${applications.length + 1}`,
                        clientId: body.ClientId,
                        displayName: body.DisplayName,
                        applicationType: body.ApplicationType,
                        clientType: body.ClientType,
                        consentType: body.ConsentType,
                        requirePkce: body.RequirePkce,
                        redirectUris: body.RedirectUris,
                        postLogoutRedirectUris: body.PostLogoutRedirectUris,
                        endpoints: body.Endpoints,
                        grantTypes: body.GrantTypes,
                        responseTypes: body.ResponseTypes,
                        scopes: body.Scopes,
                        profile: body.GrantTypes.includes("client_credentials")
                          ? "machine"
                          : body.Endpoints.includes("introspection") ? "api" : "interactive"
                      };
                      applications.push(application);
                      return json({
                        app: application,
                        secret: application.clientType === "confidential" ? `${application.profile}-secret-smoke` : null
                      }, 201);
                    }
                    if (url.pathname.startsWith("/api/sso/applications/") && method === "PUT") {
                      const id = url.pathname.split("/").at(-1);
                      const index = applications.findIndex((value) => value.id === id);
                      applications[index] = {
                        ...applications[index],
                        displayName: body.DisplayName,
                        applicationType: body.ApplicationType,
                        clientType: body.ClientType,
                        consentType: body.ConsentType,
                        requirePkce: body.RequirePkce,
                        redirectUris: body.RedirectUris,
                        postLogoutRedirectUris: body.PostLogoutRedirectUris,
                        endpoints: body.Endpoints,
                        grantTypes: body.GrantTypes,
                        responseTypes: body.ResponseTypes,
                        scopes: body.Scopes
                      };
                      return json({ app: applications[index], secret: null });
                    }
                    if (url.pathname.endsWith("/secret") && method === "POST") return json({ secret: "rotated-secret-smoke" });
                    if (url.pathname === "/api/sso/scopes" && method === "GET") return json(scopes);
                    if (url.pathname === "/api/sso/scopes" && method === "POST") {
                      const scope = {
                        id: `scope-${scopes.length + 1}`,
                        name: body.Name,
                        displayName: body.DisplayName,
                        description: body.Description,
                        resources: body.Resources
                      };
                      scopes.push(scope);
                      return json(scope, 201);
                    }
                    if (url.pathname.startsWith("/api/sso/scopes/") && method === "PUT") {
                      const id = url.pathname.split("/").at(-1);
                      const index = scopes.findIndex((value) => value.id === id);
                      scopes[index] = {
                        ...scopes[index],
                        displayName: body.DisplayName,
                        description: body.Description,
                        resources: body.Resources
                      };
                      return json(scopes[index]);
                    }
                    if (url.pathname === "/api/sso/authorizations" && method === "GET") return json(authorizations);
                    if (url.pathname === "/api/sso/authorizations/authorization-smoke/revoke" && method === "POST") {
                      authorizations[0].status = "revoked";
                      return Promise.resolve(new Response(null, { status: 204 }));
                    }
                    if (url.pathname === "/api/sso/tokens" && method === "GET") return json(tokens);
                    if (url.pathname === "/api/sso/tokens/token-smoke/revoke" && method === "POST") {
                      tokens[0].status = "revoked";
                      return Promise.resolve(new Response(null, { status: 204 }));
                    }
                    if (url.pathname === "/api/settings/" && method === "GET") {
                      settingsRequests.push({ method, keys: settings.map((item) => item.key) });
                      return json(settings);
                    }
                    if (url.pathname === "/api/settings/" && method === "POST") {
                      const setting = {
                        key: body.Key,
                        group: body.Group,
                        label: body.Label,
                        description: body.Description,
                        kind: body.Kind,
                        value: body.Value,
                        updatedAt: "2026-08-07T00:00:00Z"
                      };
                      settings.push(setting);
                      settingsRequests.push({ method, keys: settings.map((item) => item.key) });
                      return json(setting, 201);
                    }
                    if (url.pathname.startsWith("/api/settings/") && method === "PUT") {
                      const key = url.pathname.split("/").at(-1);
                      const index = settings.findIndex((item) => item.key === key);
                      settings[index] = {
                        ...settings[index],
                        group: body.Group,
                        label: body.Label,
                        description: body.Description,
                        kind: body.Kind,
                        value: body.Value,
                        updatedAt: "2026-08-07T00:00:00Z"
                      };
                      return json(settings[index]);
                    }
                    if (url.pathname.startsWith("/api/settings/") && method === "DELETE") {
                      const key = url.pathname.split("/").at(-1);
                      settings.splice(settings.findIndex((item) => item.key === key), 1);
                      settingsRequests.push({ method, key, keys: settings.map((item) => item.key) });
                      return Promise.resolve(new Response(null, { status: 204 }));
                    }
                    if (url.pathname === "/api/schedules/" && method === "GET") return json(schedules);
                    if (url.pathname === "/api/schedules/openid-prune/runs" && method === "GET") return json(scheduleRuns);
                    if (url.pathname === "/api/schedules/openid-prune" && method === "PUT") {
                      schedules[0] = {
                        ...schedules[0],
                        cron: body.Cron,
                        enabled: body.Enabled,
                        nextRunAt: body.Enabled ? "2026-08-08T02:15:00Z" : null
                      };
                      return json(schedules[0]);
                    }
                    if (url.pathname === "/api/schedules/openid-prune/run" && method === "POST") {
                      scheduleRuns.unshift({
                        id: `run-${scheduleRuns.length + 1}`,
                        trigger: "manual",
                        status: "succeeded",
                        startedAt: "2026-08-07T00:00:00Z",
                        finishedAt: "2026-08-07T00:00:01Z",
                        message: "Pruned 0 token(s) and 0 authorization(s)."
                      });
                      return Promise.resolve(new Response(null, { status: 202 }));
                    }
                    if (method !== "GET") return Promise.resolve(new Response(null, { status: 204 }));
                    return json({ title: "Not Found" }, 404);
                  };
                  localStorage.setItem("jazoradmin.starter.style.showBreadcrumb", "true");
                </script>
                <script type="module">
                  import { createApp, nextTick } from "vue";
                  import "/{{bootstrapModulePath}}";
                  import InjectApp from "/{{injectAppModulePath}}";

                  try {
                    createApp(InjectApp).mount("#inject-app");
                    for (let attempt = 0; attempt < 100 && !document.querySelector('[data-route-breadcrumb], .ja-error'); attempt++) {
                      await new Promise((resolve) => setTimeout(resolve, 10));
                    }
                    if (!document.querySelector('[data-route-breadcrumb], .ja-error')) {
                      throw new Error(`JazorAdmin router root did not mount for ${location.pathname}. Body: ${document.body.textContent ?? ""}`);
                    }
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));
                    const injectRoot = document.querySelector('[data-vue-inject="page-container"]');
                    if (!injectRoot) {
                      throw new Error("JazorAdmin VueInject companion did not mount.");
                    }
                    const injectInitialCount = document.querySelector("[data-inject-count]")?.textContent?.trim() ?? "";
                    document.querySelector('[data-inject-action="verify"]')
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    const injectSmoke = {
                      marker: injectRoot.getAttribute("data-vue-inject") ?? "",
                      title: injectRoot.querySelector("h1")?.textContent ?? "",
                      breadcrumbs: injectRoot.querySelector("nav")?.textContent ?? "",
                      extra: injectRoot.querySelector('[data-inject-slot="extra"]')?.textContent ?? "",
                      content: injectRoot.querySelector('[data-inject-slot="content"]')?.textContent ?? "",
                      initialCount: injectInitialCount,
                      updatedCount: injectRoot.querySelector("[data-inject-count]")?.textContent?.trim() ?? ""
                    };
                    const errorPage = document.querySelector(".ja-error");
                    if (errorPage) {
                      const requestedPathname = location.pathname;
                      const errorKind = errorPage.getAttribute("data-error-kind") ?? "";
                      const errorRole = errorPage.getAttribute("role") ?? "";
                      const errorCode = document.querySelector(".ja-error__code")?.textContent ?? "";
                      const errorTitle = document.querySelector(".ja-error h1")?.textContent ?? "";
                      const errorDescription = document.querySelector(".ja-error p")?.textContent ?? "";
                      document.querySelector('[data-error-action="home"]')?.click();
                      for (let attempt = 0; attempt < 100 && location.pathname !== "/"; attempt++) {
                        await new Promise((resolve) => setTimeout(resolve, 10));
                      }
                      await nextTick();
                      globalThis.__jazorAdminBrowserSmoke = {
                        ok: true,
                        mode: "error",
                        pathname: requestedPathname,
                        errorKind,
                        errorRole,
                        errorCode,
                        errorTitle,
                        errorDescription,
                        returnPathname: location.pathname
                      };
                    } else if (location.pathname === "/organizations/structure") {
                      for (let attempt = 0; attempt < 100 && document.querySelector(".t-loading"); attempt++) {
                        await new Promise((resolve) => setTimeout(resolve, 10));
                      }
                      globalThis.__jazorAdminBrowserSmoke = {
                        ok: true,
                        mode: "deep-link",
                        pathname: location.pathname,
                        pageTitleText: document.querySelector('[data-route-breadcrumb-current="true"]')?.textContent ?? "",
                        breadcrumbText: document.querySelector('[data-route-breadcrumb]')?.textContent ?? "",
                        breadcrumbDisplay: (() => {
                          const breadcrumb = document.querySelector('.ja-route-breadcrumb');
                          return breadcrumb ? getComputedStyle(breadcrumb).display : "";
                        })(),
                        breadcrumbItemsInline: (() => {
                          const breadcrumb = document.querySelector('.ja-route-breadcrumb');
                          const items = breadcrumb ? Array.from(breadcrumb.children) : [];
                          if (items.length < 2) return items.length === 1;
                          const firstTop = items[0].getBoundingClientRect().top;
                          return items.every((item) => Math.abs(item.getBoundingClientRect().top - firstTop) <= 1);
                        })(),
                        activeKey: document.querySelector("[data-navigation-selected-key]")?.getAttribute("data-navigation-selected-key") ?? "",
                        organizationPanelVisible: document.querySelector('[data-management-area="organizations"]') !== null
                      };
                    } else if (location.pathname === "/sso/applications") {
                      for (let attempt = 0; attempt < 100 && document.querySelector(".t-loading"); attempt++) {
                        await new Promise((resolve) => setTimeout(resolve, 10));
                      }
                      globalThis.__jazorAdminBrowserSmoke = {
                        ok: true,
                        mode: "management-layout",
                        pathname: location.pathname,
                        applicationPanelVisible: document.querySelector('[data-sso-view="applications"]') !== null,
                        documentFitsViewport: document.documentElement.scrollWidth <= innerWidth
                      };
                    } else {
                      const waitFor = async (predicate, description) => {
                        for (let attempt = 0; attempt < 120; attempt++) {
                          await nextTick();
                          if (predicate()) return;
                          await new Promise((resolve) => setTimeout(resolve, 10));
                        }
                        const details = typeof description === "function" ? description() : description;
                        throw new Error(`Timed out waiting for ${details}.`);
                      };
                      const click = async (selector, description) => {
                        const target = document.querySelector(selector);
                        if (!(target instanceof HTMLElement)) throw new Error(`Missing ${description}.`);
                        target.click();
                        await nextTick();
                      };
                      const waitForTitle = (title) => waitFor(
                        () => document.querySelector('[data-route-breadcrumb-current="true"]')?.textContent?.includes(title) === true,
                        `${title} page`);
                      const setInput = async (selector, value) => {
                        const target = document.querySelector(selector);
                        if (!(target instanceof HTMLInputElement)) throw new Error(`Missing input ${selector}.`);
                        Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, "value").set.call(target, value);
                        target.dispatchEvent(new Event("input", { bubbles: true }));
                        target.dispatchEvent(new Event("change", { bubbles: true }));
                        await nextTick();
                      };
                      const setTextArea = async (selector, value) => {
                        const target = document.querySelector(selector);
                        if (!(target instanceof HTMLTextAreaElement)) throw new Error(`Missing textarea ${selector}.`);
                        Object.getOwnPropertyDescriptor(HTMLTextAreaElement.prototype, "value").set.call(target, value);
                        target.dispatchEvent(new Event("input", { bubbles: true }));
                        target.dispatchEvent(new Event("change", { bubbles: true }));
                        await nextTick();
                      };

                      await waitFor(() => document.querySelectorAll(".ja-metric").length === 4, "administration overview");
                      const dashboardText = document.querySelector('[data-page-region="body"]')?.textContent ?? "";
                      const metricCount = document.querySelectorAll(".ja-metric").length;
                      const iconBarItemCount = document.querySelectorAll("[data-iconbar-key]").length;
                      const iconBarBrandCount = document.querySelectorAll('[data-iconbar-mode="rail"] .ja-iconbar__brand').length;
                      const headerBrandCount = document.querySelectorAll('[data-shell-region="head-menu"] .ja-brand-logo').length;
                      const quickActionsToggle = document.querySelector('[data-iconbar-command="toggle-quick-actions"]');
                      if (!(quickActionsToggle instanceof HTMLElement)) throw new Error("IconBar floating action trigger is missing.");
                      quickActionsToggle.click();
                      await waitFor(
                        () => document.querySelector('[data-iconbar-quick-actions]') !== null,
                        "IconBar floating action group");
                      // Documentation/Assistant quick actions are external-link anchors; Account/Sign-out stay buttons.
                      const quickActionNames = Array.from(document.querySelectorAll('[data-iconbar-quick-actions] button, [data-iconbar-quick-actions] a'))
                        .map((action) => action.getAttribute("aria-label") ?? "");
                      quickActionsToggle.click();
                      await nextTick();
                      const userText = document.querySelector(".ja-starter-user")?.textContent ?? "";
                      const organizationPickerValue = document.querySelector("[data-organization-picker]")?.value ?? "";

                      const sidebarToggle = document.querySelector('[data-shell-command="toggle-sidebar"]');
                      const initialSidebarExpanded = sidebarToggle?.getAttribute("aria-expanded") ?? "";
                      sidebarToggle?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                      await nextTick();
                      await waitFor(
                        () => document.querySelector('[data-shell-region="layout"]')?.getAttribute("data-shell-collapsed") === "true",
                        "collapsed shell");
                      await waitFor(
                        () => getComputedStyle(document.querySelector('[data-shell-region="sidebar"]')).width === "64px",
                        "collapsed primary rail transition");
                      const collapsedSidebar = document.querySelector('[data-shell-region="sidebar"]');
                      const collapsedSidebarWidth = collapsedSidebar ? getComputedStyle(collapsedSidebar).width : "";
                      const collapsedSecondaryMenuPresent = document.querySelector('[data-shell-region="secondary-menu"]') !== null;
                      sidebarToggle?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                      await waitFor(
                        () => document.querySelector('[data-shell-region="layout"]')?.getAttribute("data-shell-collapsed") === "false",
                        "expanded shell");

                      // Navigation search: type a Chinese query, pick the first result, and land on the page.
                      const searchInput = document.querySelector('[data-search-input]');
                      if (!(searchInput instanceof HTMLInputElement)) throw new Error("Header search input is missing.");
                      searchInput.value = "账户";
                      searchInput.dispatchEvent(new Event("input", { bubbles: true }));
                      await waitFor(() => document.querySelector('[data-search-panel]') !== null, "navigation search panel");
                      await waitFor(
                        () => document.querySelectorAll('[data-search-panel] [data-search-key]').length > 0,
                        "navigation search results");
                      const searchResultKeys = Array.from(document.querySelectorAll('[data-search-panel] [data-search-key]'))
                        .map((button) => button.getAttribute("data-search-key") ?? "");
                      await click('[data-search-panel] [data-search-key]', "navigation search result");
                      await waitFor(() => location.pathname === "/accounts", "navigation search navigation");
                      const searchSelectedTitle = document.querySelector('[data-route-breadcrumb-current="true"]')?.textContent ?? "";
                      const searchPanelClosedAfterNavigation = document.querySelector('[data-search-panel]') === null;

                      // Notification bell: the mocked failed run must appear with its badge.
                      const notificationBadgeBeforeOpen = document.querySelector('[data-notification-count]')?.textContent ?? "";
                      await click('[data-notification-command="toggle"]', "notification bell");
                      await waitFor(() => document.querySelector('[data-notifications-panel]') !== null, "notification panel");
                      await waitFor(
                        () => document.querySelector('[data-notifications-panel] li')?.textContent?.includes("OpenID prune") === true,
                        "notification failed run entry");
                      const notificationEntryText = document.querySelector('[data-notifications-panel] li')?.textContent ?? "";
                      await click('[data-notification-command="toggle"]', "notification bell close");
                      const notificationPanelClosed = document.querySelector('[data-notifications-panel]') === null;

                      // Zones own the IconBar tier; module pages are reached through the scoped
                      // secondary menu. Clicking a zone lands on its first leaf route.
                      await click('[data-iconbar-key="iam"]', "identity zone IconBar item");
                      await waitForTitle("组织架构");
                      await waitFor(() => document.querySelector('[data-management-area="organizations"] .ja-form__summary') !== null, "organization details");
                      const organizationPathname = location.pathname;
                      const organizationTitle = document.querySelector('[data-route-breadcrumb-current="true"]')?.textContent ?? "";
                      const organizationText = document.querySelector('[data-management-area="organizations"]')?.textContent ?? "";
                      const childOrganizationCount = document.querySelectorAll(".ja-item-list li").length;

                      await click('[data-nav-key="organizations.members"] a', "members navigation item");
                      await waitForTitle("成员管理");
                      await waitFor(() => document.querySelectorAll('[data-management-area="organizations"] .t-table tbody tr').length === 1, "member table");
                      const membersPathname = location.pathname;
                      const memberRowText = document.querySelector('[data-management-area="organizations"] .t-table tbody tr')?.textContent ?? "";
                      await click('[data-organization-command="edit-roles"]', "member role editor command");
                      const memberRoleEditorVisible = document.querySelector('[data-organization-command="save-roles"]') !== null;

                      await click('[data-nav-key="authorization.roles"] a', "role grants navigation item");
                      await waitForTitle("角色与授权");
                      await waitFor(() => document.querySelector('[data-management-area="authorization"] .ja-option-list') !== null, "role grants");
                      const accessPathname = location.pathname;
                      const roleCount = document.querySelectorAll(".ja-role-list li").length;
                      const grantCount = document.querySelectorAll('[data-management-area="authorization"] .ja-option-list .ja-form__option').length;

                      await click('[data-nav-key="authorization.resources"] a', "resource operations navigation item");
                      await waitForTitle("资源操作");
                      await waitFor(() => document.querySelectorAll('[data-management-area="authorization"] .t-table tbody tr').length === 4, "resource operation table");
                      const resourcesPathname = location.pathname;
                      const resourceOperationCount = document.querySelectorAll('[data-management-area="authorization"] .t-table tbody tr').length;
                      const resourceText = document.querySelector('[data-management-area="authorization"]')?.textContent ?? "";

                      await click('[data-nav-key="accounts"] a', "accounts navigation item");
                      await waitForTitle("账户管理");
                      await waitFor(() => document.querySelectorAll('[data-management-area="accounts"] .t-table tbody tr').length === 1, "account table");
                      const accountsPathname = location.pathname;
                      const accountRowText = document.querySelector('[data-management-area="accounts"] .t-table tbody tr')?.textContent ?? "";

                      await click('[data-nav-key="sso.applications"] a', "OpenIddict application navigation item");
                      await waitForTitle("OpenID 应用");
                      await waitFor(() => document.querySelectorAll('[data-sso-view="applications"] .t-table tbody tr').length === 1, "OpenIddict application table");
                      const applicationsPathname = location.pathname;
                      const applicationRowText = document.querySelector('[data-sso-view="applications"] .t-table tbody tr')?.textContent ?? "";

                      await click('[data-sso-command="new-application"]', "new OpenIddict application command");
                      await click('[data-sso-profile="machine"] label', "machine application profile");
                      await setInput('[data-app-field="clientId"] input', "audit-worker");
                      await setInput('[data-app-field="displayName"] input', "Audit worker");
                      await click('[data-sso-command="save-application"]', "save machine application command");
                      await waitFor(() => document.querySelector('[data-sso-application="audit-worker"]') !== null, "machine application row");
                      await waitFor(() => document.querySelector('[data-issued-secret] code')?.textContent?.includes("machine-secret-smoke") === true, "machine client secret");
                      const machineSecret = document.querySelector('[data-issued-secret] code')?.textContent ?? "";

                      await click('[data-sso-command="new-application"]', "new API application command");
                      // The profiles group only re-renders after the reset click flushes; wait for it
                      // explicitly instead of assuming the single click-helper tick is enough.
                      await waitFor(() => document.querySelector('[data-sso-profile="api"]') !== null, "API application profile group");
                      await click('[data-sso-profile="api"] label', "API application profile");
                      await setInput('[data-app-field="clientId"] input', "audit-api");
                      await setInput('[data-app-field="displayName"] input', "Audit API");
                      await click('[data-sso-command="save-application"]', "save API application command");
                      await waitFor(() => document.querySelector('[data-sso-application="audit-api"]') !== null, "API application row");
                      const applicationCount = document.querySelectorAll('[data-sso-view="applications"] .t-table tbody tr').length;

                      await click('[data-nav-key="sso.scopes"] a', "OpenIddict scope navigation item");
                      await waitForTitle("OpenID Scope");
                      await waitFor(() => document.querySelectorAll('[data-sso-view="scopes"] .t-table tbody tr').length === 1, "OpenIddict scope table");
                      const scopesPathname = location.pathname;
                      await setInput('[data-scope-field="displayName"] input', "JazorAdmin API audited");
                      await click('[data-sso-command="save-scope"]', "save OpenIddict scope command");
                      await waitFor(() => document.querySelector('[data-sso-scope="jazoradmin_api"]')?.textContent?.includes("audited") === true, "updated OpenIddict scope row");
                      const scopeRowText = document.querySelector('[data-sso-scope="jazoradmin_api"]')?.textContent ?? "";

                      await click('[data-nav-key="sso.authorizations"] a', "OpenIddict authorization navigation item");
                      await waitForTitle("授权记录");
                      await waitFor(() => document.querySelector('[data-sso-view="authorizations"] [data-sso-authorization]') !== null, "OpenIddict authorization table");
                      const authorizationsPathname = location.pathname;
                      await click('[data-sso-view="authorizations"] [data-sso-command="revoke-authorization"]', "revoke authorization command");
                      // The data anchor identifies the first cell; status renders in a sibling
                      // TDesign column, so assert the row rather than the anchor text itself.
                      await waitFor(() => document.querySelector('[data-sso-authorization]')?.closest("tr")?.textContent?.includes("revoked") === true, "revoked OpenIddict authorization");
                      const authorizationRowText = document.querySelector('[data-sso-authorization]')?.closest("tr")?.textContent ?? "";

                      await click('[data-nav-key="sso.tokens"] a', "OpenIddict token navigation item");
                      await waitForTitle("令牌");
                      await waitFor(() => document.querySelector('[data-sso-view="tokens"] [data-sso-token]') !== null, "OpenIddict token table");
                      const tokensPathname = location.pathname;
                      await click('[data-sso-view="tokens"] [data-sso-command="revoke-token"]', "revoke token command");
                      await waitFor(() => document.querySelector('[data-sso-token]')?.closest("tr")?.textContent?.includes("revoked") === true, "revoked OpenIddict token");
                      const tokenRowText = document.querySelector('[data-sso-token]')?.closest("tr")?.textContent ?? "";

                      await click('[data-iconbar-key="operations"]', "platform operations zone IconBar item");
                      await waitForTitle("配置中心");
                      await waitFor(() => document.querySelector('[data-management-area="settings"]') !== null, "configuration center");
                      const settingsPathname = location.pathname;
                      await click('[data-settings-command="new"]', "new setting command");
                      await setInput('[data-management-area="settings"] [data-setting-field="key"] input', "feature.smoke.enabled");
                      await setInput('[data-management-area="settings"] [data-setting-field="group"] input', "feature");
                      await setInput('[data-management-area="settings"] [data-setting-field="label"] input', "Smoke feature");
                      await setTextArea('[data-management-area="settings"] [data-setting-field="value"] textarea', "enabled");
                      await click('[data-settings-command="save"]', "save setting command");
                      await waitFor(() => document.querySelector('[data-setting-key="feature.smoke.enabled"]') !== null, "created setting row");
                      const settingRowText = document.querySelector('[data-setting-key="feature.smoke.enabled"]')?.textContent ?? "";

                      // M2 empty-state acceptance: delete every setting (two-step confirm) and
                      // wait for the TDesign empty state on the settings panel.
                      for (let round = 0; round < 8; round++) {
                        const settingRows = document.querySelectorAll('[data-management-area="settings"] [data-setting-key]');
                        if (settingRows.length === 0) break;
                        // TDesign may render fixed action columns in a sibling table, so the
                        // selection editor is the stable source of the row being removed.
                        const selectedKeyInput = document.querySelector('[data-management-area="settings"] [data-setting-field="key"] input');
                        const settingKey = selectedKeyInput instanceof HTMLInputElement ? selectedKeyInput.value : "";
                        if (!settingKey)
                          throw new Error("Settings table did not select a row for deletion.");
                        const deleteCommand = document.querySelector('[data-management-area="settings"] [data-settings-command="delete"]');
                        if (!(deleteCommand instanceof HTMLElement)) throw new Error("Missing setting delete command.");
                        const initialDeleteText = deleteCommand.textContent;
                        await click('[data-management-area="settings"] [data-settings-command="delete"]', "arm setting delete");
                        await waitFor(
                          () => document.querySelector('[data-management-area="settings"] [data-settings-command="delete"]')?.textContent !== initialDeleteText,
                          "setting delete confirmation");
                        await click('[data-management-area="settings"] [data-settings-command="delete"]', "confirm setting delete");
                        await waitFor(
                          () => document.querySelector('[data-management-area="settings"] [data-setting-key="' + settingKey + '"]') === null,
                          "settings row removed");
                      }
                      // This action exists only in the TEmpty branch. Do not couple the sample
                      // contract to a third-party internal CSS class.
                      await waitFor(
                        () => document.querySelector('[data-management-area="settings"] [data-settings-command="new-empty"]') !== null,
                        () => {
                          const area = document.querySelector('[data-management-area="settings"]');
                          return "settings empty state. State=" + JSON.stringify({
                            editCount: area?.querySelectorAll('[data-settings-command="edit"]').length ?? -1,
                            deleteCount: area?.querySelectorAll('[data-settings-command="delete"]').length ?? -1,
                            emptyCount: area?.querySelectorAll('.t-empty').length ?? -1,
                            loadingCount: area?.querySelectorAll('.t-loading').length ?? -1,
                            requests: settingsRequests,
                            text: area?.textContent?.replace(/\s+/g, " ").trim() ?? ""
                          });
                        });
                      const settingsEmptyVisible = document.querySelector('[data-management-area="settings"] [data-settings-command="new-empty"]') !== null;

                      await click('[data-nav-key="schedules"] a', "task scheduling navigation item");
                      await waitForTitle("任务调度");
                      await waitFor(() => document.querySelector('[data-management-area="schedules"]') !== null, "task scheduling center");
                      await waitFor(() => document.querySelector('[data-management-area="schedules"] [data-schedule-key]') !== null, "scheduled task table");
                      await waitFor(
                        () => document.querySelector('[data-management-area="schedules"] [data-schedule-field="cron"] input')?.value.length > 0,
                        "selected scheduled task");
                      const schedulesPathname = location.pathname;
                      await click('[data-schedule-command="run"]', "manual schedule run command");
                      // The anchor belongs to the Started column; status is rendered in a
                      // sibling TDesign column, so assert the entire execution row.
                      await waitFor(() => document.querySelector('[data-schedule-run]')?.closest("tr")?.textContent?.includes("succeeded") === true, "manual schedule execution");
                      const scheduleRunText = document.querySelector('[data-schedule-run]')?.closest("tr")?.textContent ?? "";

                      await click('[data-nav-key="audit"] a', "audit log navigation item");
                      await waitForTitle("审计日志");
                      await waitFor(
                        () => document.querySelectorAll('[data-management-area="audit"] [data-audit-event]').length === 2,
                        "audit event table");
                      const auditPathname = location.pathname;
                      const auditInitialCount = document.querySelectorAll('[data-management-area="audit"] [data-audit-event]').length;
                      await setInput('[data-audit-filter="object"] input', "sso-application");
                      await setInput('[data-audit-filter="action"] input', "created");
                      await click('[data-audit-command="apply"]', "apply audit filters");
                      await waitFor(
                        () => document.querySelectorAll('[data-management-area="audit"] [data-audit-event]').length === 1,
                        "filtered audit event table");
                      const auditFilteredCount = document.querySelectorAll('[data-management-area="audit"] [data-audit-event]').length;
                      const auditFilteredRowText = document.querySelector('[data-management-area="audit"] .t-table tbody tr')?.textContent ?? "";
                      await click('[data-audit-command="clear"]', "clear audit filters");
                      await waitFor(
                        () => document.querySelectorAll('[data-management-area="audit"] [data-audit-event]').length === 2,
                        "cleared audit event filters");
                      const auditClearedCount = document.querySelectorAll('[data-management-area="audit"] [data-audit-event]').length;

                      await click('[data-iconbar-key="dashboard"]', "dashboard IconBar item");
                      await waitForTitle("工作台");
                      const dashboardReturnPathname = location.pathname;

                      globalThis.__jazorAdminBrowserSmoke = {
                        ok: true,
                        mode: "app",
                        pageTitleText: document.querySelector('[data-route-breadcrumb-current="true"]')?.textContent ?? "",
                        dashboardText,
                        metricCount,
                        iconBarItemCount,
                        iconBarBrandCount,
                        headerBrandCount,
                        quickActionNames,
                        userText,
                        organizationPickerValue,
                        initialSidebarExpanded,
                        collapsedSidebarWidth,
                        collapsedSecondaryMenuPresent,
                        organizationPathname,
                        organizationTitle,
                        organizationText,
                        childOrganizationCount,
                        membersPathname,
                        memberRowText,
                        memberRoleEditorVisible,
                        accessPathname,
                        roleCount,
                        grantCount,
                        resourcesPathname,
                        resourceOperationCount,
                        resourceText,
                        accountsPathname,
                        accountRowText,
                        applicationsPathname,
                        applicationRowText,
                        applicationCount,
                        machineSecret,
                        scopesPathname,
                        scopeRowText,
                        authorizationsPathname,
                        authorizationRowText,
                        tokensPathname,
                        tokenRowText,
                        settingsPathname,
                        settingRowText,
                        settingsEmptyVisible,
                        schedulesPathname,
                        scheduleRunText,
                        auditPathname,
                        auditInitialCount,
                        auditFilteredCount,
                        auditFilteredRowText,
                        auditClearedCount,
                        dashboardReturnPathname,
                        searchResultKeys,
                        searchSelectedTitle,
                        searchPanelClosedAfterNavigation,
                        notificationBadgeBeforeOpen,
                        notificationEntryText,
                        notificationPanelClosed,
                        injectSmoke,
                        hasLegacyVueReference: Array.from(document.scripts)
                          .some((script) => script.getAttribute("src")?.endsWith(".vue"))
                      };
                    }                  } catch (error) {
                    globalThis.__jazorAdminBrowserSmoke = {
                      ok: false,
                      message: error instanceof Error ? error.message : String(error),
                      stack: error instanceof Error ? error.stack : ""
                    };
                  }
                </script>
              </body>
            </html>
            """,
            Encoding.UTF8);

        var testPath = Path.Combine(harnessRoot, "jazoradmin-browser-smoke.mjs");
        await File.WriteAllTextAsync(
            testPath,
            BuildBrowserSmokeTestScript(browserPath),
            Encoding.UTF8);
        var browser = await RunProcessAsync(
            denoPath,
            harnessRoot,
            ["run", "--quiet", "-A", testPath],
            TimeSpan.FromSeconds(180));
        if (browser.ExitCode != 0)
            throw new InvalidOperationException("JazorAdmin browser smoke failed." + Environment.NewLine + browser);

        using var payload = ReadJsonLinePayload(browser.StandardOutput, "JazorAdmin browser");
        var root = payload.RootElement;
        if (!root.GetProperty("ok").GetBoolean())
            throw new InvalidOperationException(
                "JazorAdmin browser smoke script failed: " +
                root.GetProperty("message").GetString() +
                Environment.NewLine +
                root.GetRawText());

        var injectSmoke = root.GetProperty("injectSmoke");
        AssertContains(injectSmoke.GetProperty("marker").GetString() ?? string.Empty, "page-container", "JazorAdmin VueInject marker", injectSmoke.GetRawText());
        AssertContains(injectSmoke.GetProperty("title").GetString() ?? string.Empty, "Injected administration page", "JazorAdmin VueInject title", injectSmoke.GetRawText());
        AssertContains(injectSmoke.GetProperty("breadcrumbs").GetString() ?? string.Empty, "Home", "JazorAdmin VueInject breadcrumb", injectSmoke.GetRawText());
        AssertContains(injectSmoke.GetProperty("extra").GetString() ?? string.Empty, "Extra slot preserved", "JazorAdmin VueInject named slot", injectSmoke.GetRawText());
        AssertContains(injectSmoke.GetProperty("content").GetString() ?? string.Empty, "Default content preserved", "JazorAdmin VueInject default slot", injectSmoke.GetRawText());
        AssertContains(injectSmoke.GetProperty("initialCount").GetString() ?? string.Empty, "0", "JazorAdmin VueInject initial action count", injectSmoke.GetRawText());
        AssertContains(injectSmoke.GetProperty("updatedCount").GetString() ?? string.Empty, "1", "JazorAdmin VueInject updated action count", injectSmoke.GetRawText());

        AssertContains(root.GetProperty("pageTitleText").GetString() ?? string.Empty, "工作台", "JazorAdmin browser dashboard title", root.GetRawText());
        AssertContains(root.GetProperty("dashboardText").GetString() ?? string.Empty, "组织访问", "JazorAdmin browser administration overview", root.GetRawText());
        AssertJsonInt(root, "metricCount", 4, "JazorAdmin administration metric count", root.GetRawText());
        AssertJsonInt(root, "iconBarItemCount", 3, "JazorAdmin IconBar zone count", root.GetRawText());
        AssertJsonInt(root, "iconBarBrandCount", 1, "JazorAdmin IconBar single brand mark", root.GetRawText());
        AssertJsonInt(root, "headerBrandCount", 0, "JazorAdmin mixed layout header duplicate brand", root.GetRawText());
        foreach (var action in new[] { "文档", "助手", "账号", "退出登录" })
            AssertJsonStringArrayContains(root, "quickActionNames", action, "JazorAdmin IconBar floating action", root.GetRawText());
        AssertContains(root.GetProperty("userText").GetString() ?? string.Empty, "Smoke operator", "JazorAdmin browser session account", root.GetRawText());
        AssertContains(
            string.Join(",", root.GetProperty("searchResultKeys").EnumerateArray().Select(value => value.GetString() ?? "")),
            "accounts",
            "JazorAdmin navigation search results",
            root.GetRawText());
        AssertContains(root.GetProperty("searchSelectedTitle").GetString() ?? string.Empty, "账户管理", "JazorAdmin navigation search destination", root.GetRawText());
        AssertJsonBoolean(root, "searchPanelClosedAfterNavigation", true, "JazorAdmin navigation search panel closes after navigation", root.GetRawText());
        AssertContains(root.GetProperty("notificationBadgeBeforeOpen").GetString() ?? string.Empty, "1", "JazorAdmin notification badge count", root.GetRawText());
        AssertContains(root.GetProperty("notificationEntryText").GetString() ?? string.Empty, "OpenID prune", "JazorAdmin notification failed run", root.GetRawText());
        AssertJsonBoolean(root, "notificationPanelClosed", true, "JazorAdmin notification panel closes on toggle", root.GetRawText());
        AssertContains(root.GetProperty("organizationPickerValue").GetString() ?? string.Empty, "5e1246c9", "JazorAdmin browser organization selection", root.GetRawText());
        AssertContains(root.GetProperty("initialSidebarExpanded").GetString() ?? string.Empty, "true", "JazorAdmin initial sidebar state", root.GetRawText());
        AssertContains(root.GetProperty("collapsedSidebarWidth").GetString() ?? string.Empty, "64px", "JazorAdmin collapsed IconBar width", root.GetRawText());
        AssertJsonBoolean(root, "collapsedSecondaryMenuPresent", false, "JazorAdmin collapsed secondary menu occupancy", root.GetRawText());

        AssertContains(root.GetProperty("organizationPathname").GetString() ?? string.Empty, "/organizations/structure", "JazorAdmin organization structure navigation", root.GetRawText());
        AssertContains(root.GetProperty("organizationTitle").GetString() ?? string.Empty, "组织架构", "JazorAdmin organization structure title", root.GetRawText());
        AssertContains(root.GetProperty("organizationText").GetString() ?? string.Empty, "North Clinic", "JazorAdmin child organization display", root.GetRawText());
        AssertJsonInt(root, "childOrganizationCount", 1, "JazorAdmin child organization count", root.GetRawText());

        AssertContains(root.GetProperty("membersPathname").GetString() ?? string.Empty, "/organizations/members", "JazorAdmin members navigation", root.GetRawText());
        AssertContains(root.GetProperty("memberRowText").GetString() ?? string.Empty, "Organization operator", "JazorAdmin member row", root.GetRawText());
        AssertJsonBoolean(root, "memberRoleEditorVisible", true, "JazorAdmin member role editor", root.GetRawText());

        AssertContains(root.GetProperty("accessPathname").GetString() ?? string.Empty, "/authorization/roles", "JazorAdmin role grant navigation", root.GetRawText());
        AssertJsonInt(root, "roleCount", 1, "JazorAdmin role list count", root.GetRawText());
        AssertJsonInt(root, "grantCount", 4, "JazorAdmin resource grant count", root.GetRawText());
        AssertContains(root.GetProperty("resourcesPathname").GetString() ?? string.Empty, "/authorization/resources", "JazorAdmin resource operations navigation", root.GetRawText());
        AssertJsonInt(root, "resourceOperationCount", 4, "JazorAdmin resource operation count", root.GetRawText());
        AssertDoesNotContain(root.GetProperty("resourceText").GetString() ?? string.Empty, "Recruitment", "JazorAdmin removed recruitment resource");
        AssertContains(root.GetProperty("accountsPathname").GetString() ?? string.Empty, "/accounts", "JazorAdmin account navigation", root.GetRawText());
        AssertContains(root.GetProperty("accountRowText").GetString() ?? string.Empty, "平台管理员", "JazorAdmin account row", root.GetRawText());
        AssertContains(root.GetProperty("applicationsPathname").GetString() ?? string.Empty, "/sso/applications", "JazorAdmin OpenIddict application navigation", root.GetRawText());
        AssertContains(root.GetProperty("applicationRowText").GetString() ?? string.Empty, "JazorAdmin SPA", "JazorAdmin OpenIddict application row", root.GetRawText());
        AssertJsonInt(root, "applicationCount", 3, "JazorAdmin created Machine and API applications", root.GetRawText());
        AssertContains(root.GetProperty("machineSecret").GetString() ?? string.Empty, "machine-secret-smoke", "JazorAdmin one-time client secret", root.GetRawText());
        AssertContains(root.GetProperty("scopesPathname").GetString() ?? string.Empty, "/sso/scopes", "JazorAdmin OpenIddict scope navigation", root.GetRawText());
        AssertContains(root.GetProperty("scopeRowText").GetString() ?? string.Empty, "audited", "JazorAdmin OpenIddict scope edit", root.GetRawText());
        AssertContains(root.GetProperty("authorizationsPathname").GetString() ?? string.Empty, "/sso/authorizations", "JazorAdmin OpenIddict authorization navigation", root.GetRawText());
        AssertContains(root.GetProperty("authorizationRowText").GetString() ?? string.Empty, "revoked", "JazorAdmin OpenIddict authorization revocation", root.GetRawText());
        AssertContains(root.GetProperty("tokensPathname").GetString() ?? string.Empty, "/sso/tokens", "JazorAdmin OpenIddict token navigation", root.GetRawText());
        AssertContains(root.GetProperty("tokenRowText").GetString() ?? string.Empty, "revoked", "JazorAdmin OpenIddict token revocation", root.GetRawText());
        AssertContains(root.GetProperty("settingsPathname").GetString() ?? string.Empty, "/settings", "JazorAdmin configuration center navigation", root.GetRawText());
        AssertContains(root.GetProperty("settingRowText").GetString() ?? string.Empty, "Smoke feature", "JazorAdmin configuration center create", root.GetRawText());
        AssertJsonBoolean(root, "settingsEmptyVisible", true, "JazorAdmin configuration center empty state", root.GetRawText());
        AssertContains(root.GetProperty("schedulesPathname").GetString() ?? string.Empty, "/schedules", "JazorAdmin task scheduling navigation", root.GetRawText());
        AssertContains(root.GetProperty("scheduleRunText").GetString() ?? string.Empty, "succeeded", "JazorAdmin task scheduling manual run", root.GetRawText());
        AssertContains(root.GetProperty("auditPathname").GetString() ?? string.Empty, "/audit", "JazorAdmin audit navigation", root.GetRawText());
        AssertJsonInt(root, "auditInitialCount", 2, "JazorAdmin initial audit event count", root.GetRawText());
        AssertJsonInt(root, "auditFilteredCount", 1, "JazorAdmin filtered audit event count", root.GetRawText());
        AssertContains(root.GetProperty("auditFilteredRowText").GetString() ?? string.Empty, "sso-application", "JazorAdmin filtered audit object", root.GetRawText());
        AssertContains(root.GetProperty("auditFilteredRowText").GetString() ?? string.Empty, "created", "JazorAdmin filtered audit action", root.GetRawText());
        AssertJsonInt(root, "auditClearedCount", 2, "JazorAdmin cleared audit event count", root.GetRawText());
        AssertContains(root.GetProperty("dashboardReturnPathname").GetString() ?? string.Empty, "/", "JazorAdmin dashboard IconBar return", root.GetRawText());
        if (root.GetProperty("hasLegacyVueReference").GetBoolean())
            throw new InvalidOperationException("JazorAdmin browser smoke found a legacy .vue script reference.");

        var englishLanguage = root.GetProperty("englishLanguage");
        AssertContains(englishLanguage.GetProperty("value").GetString() ?? string.Empty, "en-US", "JazorAdmin English language selection", englishLanguage.GetRawText());
        AssertContains(englishLanguage.GetProperty("title").GetString() ?? string.Empty, "Accounts", "JazorAdmin English localized title", englishLanguage.GetRawText());

        var routeTabs = root.GetProperty("routeTabs");
        AssertJsonStringArrayContains(routeTabs, "before", "/", "JazorAdmin permanent home tab", routeTabs.GetRawText());
        AssertJsonStringArrayContains(routeTabs, "before", "/organizations/structure", "JazorAdmin visited organization tab", routeTabs.GetRawText());
        AssertJsonStringArrayContains(routeTabs, "before", "/organizations/members", "JazorAdmin visited member tab", routeTabs.GetRawText());
        AssertContains(routeTabs.GetProperty("pathname").GetString() ?? string.Empty, "/", "JazorAdmin home tab navigation", routeTabs.GetRawText());
        if (!int.TryParse(routeTabs.GetProperty("active").GetString(), out var routeTabCount) || routeTabCount < 3)
            throw new InvalidOperationException("JazorAdmin route tab count did not preserve visited routes: " + routeTabs.GetRawText());

        var theme = root.GetProperty("theme");
        AssertContains(theme.GetProperty("applicationClass").GetString() ?? string.Empty, "ja-application--dark", "JazorAdmin dark application theme", theme.GetRawText());
        AssertContains(theme.GetProperty("headerClass").GetString() ?? string.Empty, "t-menu--dark", "JazorAdmin dark TDesign header", theme.GetRawText());
        if (string.Equals(theme.GetProperty("before").GetString(), theme.GetProperty("after").GetString(), StringComparison.Ordinal))
            throw new InvalidOperationException("JazorAdmin theme switch did not change the content surface: " + theme.GetRawText());

        var desktopLayout = root.GetProperty("desktopLayout");
        AssertContains(desktopLayout.GetProperty("shellDirection").GetString() ?? string.Empty, "row", "JazorAdmin desktop shell direction", desktopLayout.GetRawText());
        AssertContains(desktopLayout.GetProperty("iconBarDirection").GetString() ?? string.Empty, "column", "JazorAdmin desktop IconBar direction", desktopLayout.GetRawText());
        AssertJsonBoolean(desktopLayout, "iconBarBeforeSecondary", true, "JazorAdmin desktop IconBar order", desktopLayout.GetRawText());
        AssertJsonBoolean(desktopLayout, "sidebarBeforeMain", true, "JazorAdmin desktop sidebar order", desktopLayout.GetRawText());
        AssertContains(desktopLayout.GetProperty("sidebarOverflow").GetString() ?? string.Empty, "hidden", "JazorAdmin desktop sidebar scroll ownership", desktopLayout.GetRawText());
        AssertContains(desktopLayout.GetProperty("secondaryMenuOverflowX").GetString() ?? string.Empty, "hidden", "JazorAdmin secondary menu horizontal overflow", desktopLayout.GetRawText());
        AssertContains(desktopLayout.GetProperty("secondaryMenuOverflowY").GetString() ?? string.Empty, "hidden", "JazorAdmin secondary menu wrapper overflow", desktopLayout.GetRawText());
        AssertContains(desktopLayout.GetProperty("secondaryTitleText").GetString() ?? string.Empty, "工作台", "JazorAdmin secondary menu primary title", desktopLayout.GetRawText());
        AssertJsonBoolean(desktopLayout, "secondaryTitleAlignedWithHeader", true, "JazorAdmin secondary title header alignment", desktopLayout.GetRawText());
        AssertJsonBoolean(desktopLayout, "secondaryBodyStartsAfterTitle", true, "JazorAdmin secondary menu scroll body starts below title", desktopLayout.GetRawText());
        AssertContains(desktopLayout.GetProperty("nativeMenuOverflowX").GetString() ?? string.Empty, "hidden", "JazorAdmin native secondary menu horizontal overflow", desktopLayout.GetRawText());
        AssertContains(desktopLayout.GetProperty("nativeMenuOverflowY").GetString() ?? string.Empty, "auto", "JazorAdmin native secondary menu vertical overflow", desktopLayout.GetRawText());
        AssertJsonBoolean(desktopLayout, "secondaryMenuHasHorizontalOverflow", false, "JazorAdmin secondary menu horizontal scroll", desktopLayout.GetRawText());
        AssertJsonBoolean(desktopLayout, "documentFitsViewport", true, "JazorAdmin desktop viewport fit", desktopLayout.GetRawText());
        AssertJsonBoolean(desktopLayout, "styleRuntimeLoaded", true, "JazorAdmin desktop style runtime", desktopLayout.GetRawText());

        var mobileLayout = root.GetProperty("mobileLayout");
        AssertContains(mobileLayout.GetProperty("shellDirection").GetString() ?? string.Empty, "column", "JazorAdmin mobile shell direction", mobileLayout.GetRawText());
        AssertJsonBoolean(mobileLayout, "sidebarFillsShell", true, "JazorAdmin mobile sidebar width", mobileLayout.GetRawText());
        AssertContains(mobileLayout.GetProperty("mobileMenuDisplay").GetString() ?? string.Empty, "flex", "JazorAdmin mobile TDesign navigation display", mobileLayout.GetRawText());
        AssertJsonBoolean(mobileLayout, "mobileMenuFillsSidebar", true, "JazorAdmin mobile navigation width", mobileLayout.GetRawText());
        AssertJsonBoolean(mobileLayout, "mobileBrandCentered", true, "JazorAdmin mobile brand grid", mobileLayout.GetRawText());
        AssertJsonBoolean(mobileLayout, "mobileRailHidden", true, "JazorAdmin mobile desktop IconBar hidden", mobileLayout.GetRawText());
        AssertJsonBoolean(mobileLayout, "sidebarBeforeMain", true, "JazorAdmin mobile sidebar order", mobileLayout.GetRawText());
        AssertJsonBoolean(mobileLayout, "documentFitsViewport", true, "JazorAdmin mobile viewport fit", mobileLayout.GetRawText());
        AssertJsonBoolean(mobileLayout, "styleRuntimeLoaded", true, "JazorAdmin mobile style runtime", mobileLayout.GetRawText());

        var mobileManagement = root.GetProperty("mobileManagement");
        AssertContains(mobileManagement.GetProperty("mode").GetString() ?? string.Empty, "management-layout", "JazorAdmin mobile management smoke mode", mobileManagement.GetRawText());
        AssertContains(mobileManagement.GetProperty("pathname").GetString() ?? string.Empty, "/sso/applications", "JazorAdmin mobile application location", mobileManagement.GetRawText());
        AssertJsonBoolean(mobileManagement, "applicationPanelVisible", true, "JazorAdmin mobile application page", mobileManagement.GetRawText());
        AssertJsonBoolean(mobileManagement, "documentFitsViewport", true, "JazorAdmin mobile application viewport fit", mobileManagement.GetRawText());

        var deepLink = root.GetProperty("deepLink");
        AssertContains(deepLink.GetProperty("mode").GetString() ?? string.Empty, "deep-link", "JazorAdmin deep-link smoke mode", deepLink.GetRawText());
        AssertContains(deepLink.GetProperty("pathname").GetString() ?? string.Empty, "/organizations/structure", "JazorAdmin deep-link browser location", deepLink.GetRawText());
        AssertContains(deepLink.GetProperty("pageTitleText").GetString() ?? string.Empty, "组织架构", "JazorAdmin deep-link page title", deepLink.GetRawText());
        AssertContains(deepLink.GetProperty("breadcrumbText").GetString() ?? string.Empty, "组织机构", "JazorAdmin deep-link breadcrumb", deepLink.GetRawText());
        AssertContains(deepLink.GetProperty("breadcrumbDisplay").GetString() ?? string.Empty, "flex", "JazorAdmin deep-link breadcrumb layout", deepLink.GetRawText());
        AssertJsonBoolean(deepLink, "breadcrumbItemsInline", true, "JazorAdmin deep-link breadcrumb single line", deepLink.GetRawText());
        AssertContains(deepLink.GetProperty("activeKey").GetString() ?? string.Empty, "organizations.structure", "JazorAdmin deep-link selected key", deepLink.GetRawText());
        AssertJsonBoolean(deepLink, "organizationPanelVisible", true, "JazorAdmin deep-link organization page body", deepLink.GetRawText());
        var internalError = root.GetProperty("internalError");
        AssertContains(internalError.GetProperty("mode").GetString() ?? string.Empty, "error", "JazorAdmin internal-error smoke mode", internalError.GetRawText());
        AssertContains(internalError.GetProperty("pathname").GetString() ?? string.Empty, "/error/500", "JazorAdmin internal-error browser location", internalError.GetRawText());
        AssertContains(internalError.GetProperty("errorKind").GetString() ?? string.Empty, "internal-server-error", "JazorAdmin internal-error kind", internalError.GetRawText());
        AssertContains(internalError.GetProperty("errorCode").GetString() ?? string.Empty, "500", "JazorAdmin internal-error code", internalError.GetRawText());
        AssertContains(internalError.GetProperty("errorTitle").GetString() ?? string.Empty, "系统暂时无法处理请求", "JazorAdmin internal-error title", internalError.GetRawText());
        AssertContains(internalError.GetProperty("errorRole").GetString() ?? string.Empty, "alert", "JazorAdmin internal-error live role", internalError.GetRawText());
        AssertContains(internalError.GetProperty("returnPathname").GetString() ?? string.Empty, "/", "JazorAdmin internal-error recovery navigation", internalError.GetRawText());

        var notFound = root.GetProperty("notFound");
        AssertContains(notFound.GetProperty("mode").GetString() ?? string.Empty, "error", "JazorAdmin not-found smoke mode", notFound.GetRawText());
        AssertContains(notFound.GetProperty("pathname").GetString() ?? string.Empty, "/missing/admin/page", "JazorAdmin preserved unknown browser location", notFound.GetRawText());
        AssertContains(notFound.GetProperty("errorKind").GetString() ?? string.Empty, "not-found", "JazorAdmin not-found kind", notFound.GetRawText());
        AssertContains(notFound.GetProperty("errorCode").GetString() ?? string.Empty, "404", "JazorAdmin not-found code", notFound.GetRawText());
        AssertContains(notFound.GetProperty("errorTitle").GetString() ?? string.Empty, "页面不存在", "JazorAdmin not-found title", notFound.GetRawText());
        AssertContains(notFound.GetProperty("errorRole").GetString() ?? string.Empty, "status", "JazorAdmin not-found status role", notFound.GetRawText());
        AssertContains(notFound.GetProperty("returnPathname").GetString() ?? string.Empty, "/", "JazorAdmin not-found recovery navigation", notFound.GetRawText());
        if (root.GetProperty("hasLegacyVueReference").GetBoolean())
            throw new InvalidOperationException("JazorAdmin browser smoke found a legacy .vue script reference.");

    }
    finally
    {
        await TryDeleteHarnessRootAsync(harnessRoot);
    }
}

static string BuildBrowserSmokeTestScript(string browserPath)
{
    var browserPathJson = JsonSerializer.Serialize(browserPath, SmokeJsonContext.Default.String);
    return $$"""
        const root = new URL("./", import.meta.url);
        const browserPath = {{browserPathJson}};

        async function run() {
          const server = await startServer(root);
          const browser = await startBrowser(browserPath);
          let page = null;
          try {
            page = await connectToPage(browser.port);
            try {
              await page.send("Page.enable");
              await page.send("Runtime.enable");
              await page.send("Log.enable");
              await page.send("Network.enable");
              await page.send("Emulation.setDeviceMetricsOverride", {
                width: 1440,
                height: 1000,
                deviceScaleFactor: 1,
                mobile: false
              });
              await page.navigate(`http://127.0.0.1:${server.port}/`);
              const result = await page.waitForSmoke();
              result.desktopLayout = await page.readLayout();
              await page.navigate(`http://127.0.0.1:${server.port}/organizations/structure`);
              const deepLink = await page.waitForSmoke();
              // Language switching is verified on a real module page: the header dropdown must
              // relocalize navigation and breadcrumbs without a reload.
              await page.navigate(`http://127.0.0.1:${server.port}/accounts`);
              // Self-contained probe: waitForSmoke() would run the full app flow, which expects
              // the dashboard metrics that do not exist on this page. Language state is not
              // persisted, so the following full-page navigation resets the UI back to Chinese.
              const englishLanguage = await page.evaluate(`(async () => {
                const waitFor = async (predicate, message) => {
                  for (let attempt = 0; attempt < 100; attempt++) {
                    if (predicate()) return;
                    await new Promise((resolvePromise) => setTimeout(resolvePromise, 10));
                  }
                  throw new Error(message);
                };
                await waitFor(
                  () => document.querySelector('[data-preference="language"]') instanceof HTMLElement,
                  "language menu trigger");
                const trigger = document.querySelector('[data-preference="language"]');
                if (!(trigger instanceof HTMLElement)) throw new Error("Language menu trigger is missing.");
                trigger.click();
                await waitFor(
                  () => Array.from(document.querySelectorAll('.t-dropdown__item')).some((item) => item.textContent?.trim() === "English"),
                  "English language item");
                const english = Array.from(document.querySelectorAll('.t-dropdown__item'))
                  .find((item) => item.textContent?.trim() === "English");
                if (!(english instanceof HTMLElement)) throw new Error("English language item is missing.");
                english.click();
                await waitFor(
                  () => document.querySelector('[data-route-breadcrumb-current="true"]')?.textContent?.includes("Accounts") === true,
                  "English localized title");
                return {
                  value: document.querySelector('.ja-application')?.getAttribute('lang') ?? "",
                  title: document.querySelector('[data-route-breadcrumb-current="true"]')?.textContent?.trim() ?? ""
                };
              })()`);
              await page.navigate(`http://127.0.0.1:${server.port}/`);
              await page.waitForSmoke();
              result.routeTabs = await page.evaluate(`(async () => {
                const waitFor = async (predicate, message) => {
                  for (let attempt = 0; attempt < 100; attempt++) {
                    if (predicate()) return;
                    await new Promise((resolvePromise) => setTimeout(resolvePromise, 10));
                  }
                  throw new Error(message);
                };
                const paths = () => Array.from(document.querySelectorAll('[data-route-tab-label]'))
                  .map((tab) => tab.getAttribute('data-route-tab-label'));
                if (!document.querySelector('[data-route-tabs]')) throw new Error("Route tabs are missing.");
                const identityZone = document.querySelector('[data-iconbar-key="iam"]');
                if (!(identityZone instanceof HTMLElement)) throw new Error("Identity zone IconBar item is missing.");
                identityZone.click();
                await waitFor(() => location.pathname === "/organizations/structure", "organization route navigation");
                const membersNavigation = document.querySelector('[data-nav-key="organizations.members"] a');
                if (!(membersNavigation instanceof HTMLElement)) throw new Error("Members navigation item is missing.");
                membersNavigation.click();
                await waitFor(() => location.pathname === "/organizations/members", "members route navigation");
                const membersTab = document.querySelector('[data-route-tab-label="/organizations/members"]');
                if (!(membersTab instanceof HTMLElement)) throw new Error("Visited members tab is missing.");
                const before = paths();
                membersTab.click();
                await waitFor(() => location.pathname === "/organizations/members", "members tab activation");
                const active = document.querySelector('[data-route-tabs]')?.getAttribute('data-route-tabs-count') ?? "";
                const homeTab = document.querySelector('[data-route-tab-label="/"]');
                if (!(homeTab instanceof HTMLElement)) throw new Error("Home tab is missing.");
                homeTab.click();
                await waitFor(() => location.pathname === "/", "home tab navigation");
                return { before, active, after: paths(), pathname: location.pathname };
              })()`);
              result.theme = await page.evaluate(`(async () => {
                const waitFor = async (predicate, message) => {
                  for (let attempt = 0; attempt < 100; attempt++) {
                    if (predicate()) return;
                    await new Promise((resolvePromise) => setTimeout(resolvePromise, 10));
                  }
                  throw new Error(message);
                };
                const trigger = document.querySelector('[data-preference="setting"]');
                if (!(trigger instanceof HTMLElement)) throw new Error("Global settings trigger is missing.");
                const before = getComputedStyle(document.querySelector('[data-shell-region="content"]')).backgroundColor;
                trigger.click();
                await waitFor(
                  () => document.querySelector('[data-starter-settings]') !== null,
                  "global settings drawer");
                const dark = document.querySelector('[data-starter-settings] input[value="dark"]')?.closest("label");
                if (!(dark instanceof HTMLElement)) throw new Error("Dark theme option is missing.");
                dark.click();
                await waitFor(
                  () => document.querySelector('.ja-application')?.classList.contains('ja-application--dark') === true,
                  "dark application theme");
                await waitFor(
                  () => {
                    const content = document.querySelector('[data-shell-region="content"]');
                    return content instanceof HTMLElement && getComputedStyle(content).backgroundColor !== before;
                  },
                  "dark content surface");
                const header = document.querySelector('[data-shell-region="head-menu"]');
                const application = document.querySelector('.ja-application');
                const content = document.querySelector('[data-shell-region="content"]');
                const layout = document.querySelector('[data-shell-region="layout"]');
                return {
                  before,
                  after: getComputedStyle(content).backgroundColor,
                  applicationClass: application?.className ?? "",
                  headerClass: header?.className ?? "",
                  pageToken: getComputedStyle(application).getPropertyValue('--td-bg-color-page'),
                  containerToken: getComputedStyle(application).getPropertyValue('--td-bg-color-container'),
                  layoutBackground: getComputedStyle(layout).backgroundColor
                };
              })()`);
              await page.send("Emulation.setDeviceMetricsOverride", {
                width: 390,
                height: 844,
                deviceScaleFactor: 1,
                mobile: false
              });
              await page.navigate(`http://127.0.0.1:${server.port}/organizations/structure`);
              await page.waitForSmoke();
              result.mobileLayout = await page.readLayout();
              await page.navigate(`http://127.0.0.1:${server.port}/sso/applications`);
              result.mobileManagement = await page.waitForSmoke();
              await page.navigate(`http://127.0.0.1:${server.port}/error/500`);
              const internalError = await page.waitForSmoke();
              await page.navigate(`http://127.0.0.1:${server.port}/missing/admin/page`);
              const notFound = await page.waitForSmoke();
              result.deepLink = deepLink;
              result.englishLanguage = englishLanguage;
              result.internalError = internalError;
              result.notFound = notFound;
              result.diagnostics = page.diagnostics;
              result.serverDiagnostics = server.diagnostics;
              console.log(JSON.stringify(result));
            } catch (error) {
              const message = error instanceof Error ? error.message : String(error);
              throw new Error(`${message} Browser: ${JSON.stringify(page.diagnostics)} Server: ${JSON.stringify(server.diagnostics)}`);
            }
          } finally {
            await browser.dispose(page);
            await server.dispose();
          }
        }

        async function startServer(root) {
          const diagnostics = [];
          let resolvePort;
          const listening = new Promise((resolvePromise) => resolvePort = resolvePromise);
          const server = Deno.serve({
            hostname: "127.0.0.1",
            port: 0,
            onListen: ({ port }) => resolvePort(port)
          }, async (request) => {
            const url = new URL(request.url);
            const relativePath = url.pathname === "/" ? "index.html" : decodeURIComponent(url.pathname.slice(1));
            const fileUrl = new URL(relativePath, root);
            if (!fileUrl.href.startsWith(root.href)) {
              diagnostics.push(`403 ${url.pathname}`);
              return new Response("Forbidden", { status: 403 });
            }

            try {
              const contents = await Deno.readFile(fileUrl);
              diagnostics.push(`200 ${url.pathname}`);
              return new Response(contents, { headers: responseHeaders(contentType(fileUrl.pathname)) });
            } catch {
              if (extension(fileUrl.pathname) === "") {
                const contents = await Deno.readFile(new URL("index.html", root));
                diagnostics.push(`200 fallback ${url.pathname}`);
                return new Response(contents, { headers: responseHeaders("text/html; charset=utf-8") });
              }
              diagnostics.push(`404 ${url.pathname} -> ${fileUrl.pathname}`);
              return new Response("Not Found", { status: 404 });
            }
          });

          const port = await listening;
          return {
            port,
            diagnostics,
            dispose: () => server.shutdown()
          };
        }

        function responseHeaders(contentType) {
          // Each route remains a fresh HTML request, while immutable generated modules use the
          // normal browser cache. Forcing 22 full cold starts exhausts Chromium's local sockets
          // and tests the harness limit instead of route behavior.
          return {
            "content-type": contentType,
            "cache-control": contentType.startsWith("text/html") ? "no-store" : "public, max-age=300"
          };
        }

        function contentType(path) {
          switch (extension(path)) {
            case ".html": return "text/html; charset=utf-8";
            case ".mjs":
            case ".js": return "text/javascript; charset=utf-8";
            case ".css": return "text/css; charset=utf-8";
            case ".json": return "application/json; charset=utf-8";
            default: return "application/octet-stream";
          }
        }

        function extension(path) {
          const fileName = path.slice(path.lastIndexOf("/") + 1);
          const dot = fileName.lastIndexOf(".");
          return dot <= 0 ? "" : fileName.slice(dot);
        }

        async function killBrowserProcessTree(process) {
          // SIGKILL only terminates the root browser process; on Windows the Chromium
          // child processes (crashpad handler, storage service, renderers, ...) can
          // survive as orphans and keep profile file locks. / 只杀浏览器主进程会留下
          // 持有 profile 文件锁的孤儿子进程，必须整树终止。
          if (Deno.build.os === "windows") {
            const killer = new Deno.Command("taskkill", {
              args: ["/PID", String(process.pid), "/T", "/F"],
              stdin: "null",
              stdout: "null",
              stderr: "null"
            }).spawn();
            await killer.status;
            return;
          }
          try {
            process.kill("SIGKILL");
          } catch {
          }
        }

        async function startBrowser(browserPath) {
          const port = await reservePort();
          const userDataDir = `${Deno.cwd()}/.browser-profile`;
          const process = new Deno.Command(browserPath, {
            args: [
              "--headless=new",
              "--disable-gpu",
              "--disable-dev-shm-usage",
              "--no-first-run",
              "--no-default-browser-check",
              "--no-sandbox",
              `--remote-debugging-port=${port}`,
              `--user-data-dir=${userDataDir}`,
              "about:blank"
            ],
            stdin: "null",
            stdout: "null",
            stderr: "null"
          }).spawn();

          let exited = false;
          const exitPromise = process.status.then((status) => {
            exited = true;
            return status;
          });
          const deadline = Date.now() + 15000;
          while (Date.now() < deadline) {
            if (exited) {
              throw new Error("Browser exited before CDP was ready.");
            }
            try {
              const response = await fetch(`http://127.0.0.1:${port}/json/list`, { cache: "no-store" });
              if (response.ok) {
                return {
                  port,
                  dispose: async (page) => {
                    if (!exited && page) {
                      // Graceful CDP close lets Chromium stop its own child processes
                      // (the crashpad handler holds profile file locks); tree-kill is
                      // only the fallback. / 优雅关闭让 Chromium 自行收尾子进程，
                      // 失败或超时再整树强杀，避免孤儿进程锁住 profile 目录。
                      try {
                        await Promise.race([page.send("Browser.close"), delay(2000)]);
                      } catch {
                      }
                    }
                    if (!exited) {
                      await killBrowserProcessTree(process);
                    }
                    await exitPromise;
                  }
                };
              }
            } catch {
            }
            await delay(100);
          }

          if (!exited) {
            await killBrowserProcessTree(process);
            await exitPromise;
          }
          throw new Error("Timed out waiting for browser CDP.");
        }

        async function reservePort() {
          const listener = Deno.listen({ hostname: "127.0.0.1", port: 0 });
          const port = listener.addr.port;
          listener.close();
          return port;
        }

        async function connectToPage(port) {
          const targets = await fetch(`http://127.0.0.1:${port}/json/list`, { cache: "no-store" })
            .then((response) => response.json());
          const target = targets.find((candidate) => candidate.type === "page" && candidate.webSocketDebuggerUrl);
          if (!target) {
            throw new Error("Browser CDP did not expose a page target.");
          }

          const socket = new WebSocket(target.webSocketDebuggerUrl);
          await new Promise((resolvePromise, reject) => {
            socket.addEventListener("open", resolvePromise, { once: true });
            socket.addEventListener("error", () => reject(new Error("CDP websocket failed to open.")), { once: true });
          });
          return new Page(socket);
        }

        class Page {
          nextId = 1;
          pending = new Map();
          loadResolvers = [];
          diagnostics = [];

          constructor(socket) {
            this.socket = socket;
            socket.addEventListener("message", (event) => this.handle(JSON.parse(String(event.data))));
          }

          send(method, params = {}) {
            const id = this.nextId++;
            const promise = new Promise((resolvePromise, reject) => this.pending.set(id, { resolve: resolvePromise, reject }));
            this.socket.send(JSON.stringify({ id, method, params }));
            return promise;
          }

          async navigate(url) {
            const loaded = new Promise((resolvePromise) => this.loadResolvers.push(resolvePromise));
            await this.send("Page.navigate", { url });
            await loaded;
          }

          async evaluate(expression) {
            const response = await this.send("Runtime.evaluate", { expression, returnByValue: true, awaitPromise: true });
            if (response.exceptionDetails) {
              throw new Error(response.exceptionDetails.exception?.description ?? response.exceptionDetails.text ?? "Runtime.evaluate failed.");
            }
            return response.result?.value;
          }

          async waitForSmoke() {
            const deadline = Date.now() + 10000;
            while (Date.now() < deadline) {
              const result = await this.evaluate("globalThis.__jazorAdminBrowserSmoke ?? null");
              if (result !== null) {
                if (result.ok === false) {
                  const href = await this.evaluate("location.href");
                  throw new Error(`JazorAdmin page smoke failed at ${href}: ${result.message ?? "Unknown error"}. Diagnostics: ${JSON.stringify(this.diagnostics)}`);
                }
                return result;
              }
              await delay(100);
            }
            const body = await this.evaluate("document.body ? document.body.textContent : ''");
            const href = await this.evaluate("location.href");
            throw new Error(`Timed out waiting for JazorAdmin browser smoke result at ${href}. Diagnostics: ${JSON.stringify(this.diagnostics)} Body: ${body}`);
          }

          readLayout() {
            return this.evaluate(`(() => {
              const shell = document.querySelector('[data-shell-region="layout"]');
              const sidebar = document.querySelector('[data-shell-region="sidebar"]');
              const main = document.querySelector('[data-shell-region="main"]');
              const railIconBar = document.querySelector('[data-iconbar-mode="rail"]');
              const iconBar = railIconBar;
              const secondaryMenu = document.querySelector('.ja-tdesign-sidebar-shell__menu');
              const secondaryTitle = document.querySelector('[data-shell-region="secondary-title"]');
              const secondaryMenuBody = document.querySelector('[data-shell-region="secondary-menu"]');
              const mobileMenu = document.querySelector('.ja-tdesign-sidebar-shell__mobile-menu');
              const mobileBrand = document.querySelector('.ja-tdesign-sidebar-shell__mobile-brand');
              const navigation = document.querySelector('[data-navigation-orientation="vertical"]');
              const nativeMenu = secondaryMenu?.querySelector('.t-menu--scroll');
              const shellStyle = shell ? getComputedStyle(shell) : null;
              const sidebarStyle = sidebar ? getComputedStyle(sidebar) : null;
              const mainStyle = main ? getComputedStyle(main) : null;
              const iconBarStyle = iconBar ? getComputedStyle(iconBar) : null;
              const navigationStyle = navigation ? getComputedStyle(navigation) : null;
              const shellRect = shell?.getBoundingClientRect();
              const sidebarRect = sidebar?.getBoundingClientRect();
              const mainRect = main?.getBoundingClientRect();
              const iconBarRect = iconBar?.getBoundingClientRect();
              const secondaryMenuRect = secondaryMenu?.getBoundingClientRect();
              const secondaryTitleRect = secondaryTitle?.getBoundingClientRect();
              const secondaryMenuBodyRect = secondaryMenuBody?.getBoundingClientRect();
              const mobileMenuRect = mobileMenu?.getBoundingClientRect();
              const mobileBrandRect = mobileBrand?.getBoundingClientRect();
              return {
                viewportWidth: innerWidth,
                shellDisplay: shellStyle?.display ?? "",
                shellDirection: shellStyle?.flexDirection ?? "",
                sidebarWidth: sidebarStyle?.width ?? "",
                sidebarOverflow: sidebarStyle?.overflow ?? "",
                mainDisplay: mainStyle?.display ?? "",
                iconBarDirection: iconBarStyle?.flexDirection ?? "",
                navigationDisplay: navigationStyle?.display ?? "",
                secondaryMenuOverflowX: secondaryMenu ? getComputedStyle(secondaryMenu).overflowX : "",
                secondaryMenuOverflowY: secondaryMenu ? getComputedStyle(secondaryMenu).overflowY : "",
                secondaryTitleText: secondaryTitle?.textContent?.trim() ?? "",
                secondaryTitleHeight: secondaryTitleRect?.height ?? 0,
                secondaryTitleAlignedWithHeader: !!secondaryTitleRect && Math.abs(secondaryTitleRect.height - 64) <= 1,
                secondaryBodyStartsAfterTitle: !!secondaryTitleRect && !!secondaryMenuBodyRect && Math.abs(secondaryMenuBodyRect.top - secondaryTitleRect.bottom) <= 1,
                nativeMenuOverflowX: nativeMenu ? getComputedStyle(nativeMenu).overflowX : "",
                nativeMenuOverflowY: nativeMenu ? getComputedStyle(nativeMenu).overflowY : "",
                secondaryMenuHasHorizontalOverflow: !!secondaryMenu && secondaryMenu.scrollWidth > secondaryMenu.clientWidth,
                sidebarFillsShell: !!shellRect && !!sidebarRect && Math.abs(shellRect.width - sidebarRect.width) <= 1,
                iconBarFillsSidebar: !!sidebarRect && !!iconBarRect && Math.abs(sidebarRect.width - iconBarRect.width) <= 1,
                secondaryMenuFillsSidebar: !!sidebarRect && !!secondaryMenuRect && Math.abs(sidebarRect.width - secondaryMenuRect.width) <= 1,
                mobileMenuDisplay: mobileMenu ? getComputedStyle(mobileMenu).display : "",
                mobileMenuFillsSidebar: !!sidebarRect && !!mobileMenuRect && Math.abs(sidebarRect.width - mobileMenuRect.width) <= 1,
                mobileBrandCentered: !!mobileBrandRect && Math.abs(mobileBrandRect.width - 64) <= 1 && Math.abs(mobileBrandRect.height - 64) <= 1,
                mobileRailHidden: !!railIconBar && getComputedStyle(railIconBar).display === "none",
                styleRuntimeLoaded: document.querySelector("style#ecmascript-style") !== null,
                documentFitsViewport: document.documentElement.scrollWidth <= innerWidth,
                iconBarBeforeSecondary: iconBarRect && secondaryMenuRect
                  ? iconBarRect.right <= secondaryMenuRect.left + 1 || iconBarRect.bottom <= secondaryMenuRect.top + 1
                  : false,
                sidebarBeforeMain: sidebarRect && mainRect
                  ? sidebarRect.right <= mainRect.left + 1 || sidebarRect.bottom <= mainRect.top + 1
                  : false
              };
            })()`);
          }

          handle(message) {
            if (message.id !== undefined) {
              const pending = this.pending.get(message.id);
              if (pending === undefined) {
                return;
              }
              this.pending.delete(message.id);
              if (message.error) {
                pending.reject(new Error(message.error.message ?? JSON.stringify(message.error)));
              } else {
                pending.resolve(message.result);
              }
              return;
            }
            if (message.method === "Page.loadEventFired") {
              for (const resolvePromise of this.loadResolvers.splice(0)) {
                resolvePromise();
              }
              return;
            }
            if (message.method === "Runtime.exceptionThrown") {
              this.diagnostics.push(message.params?.exceptionDetails?.exception?.description ?? message.params?.exceptionDetails?.text ?? "Runtime exception");
              return;
            }
            if (message.method === "Runtime.consoleAPICalled") {
              const args = (message.params?.args ?? []).map((arg) => arg.value ?? arg.description ?? "").join(" ");
              this.diagnostics.push(`console.${message.params?.type ?? "log"}: ${args}`);
              return;
            }
            if (message.method === "Log.entryAdded") {
              const entry = message.params?.entry;
              this.diagnostics.push(`${entry?.level ?? "log"}: ${entry?.text ?? ""} ${entry?.url ?? ""}`);
              return;
            }
            if (message.method === "Network.responseReceived") {
              const response = message.params?.response;
              if (response && response.status >= 400) {
                this.diagnostics.push(`network ${response.status}: ${response.url ?? ""}`);
              }
              return;
            }
            if (message.method === "Network.loadingFailed") {
              this.diagnostics.push(`network failed: ${message.params?.errorText ?? ""} ${message.params?.blockedReason ?? ""}`);
            }
          }
        }

        function delay(ms) {
          return new Promise((resolvePromise) => setTimeout(resolvePromise, ms));
        }

        await run();
        """;
}

static string FindRepositoryRoot(string startDirectory)
{
    var current = new DirectoryInfo(Path.GetFullPath(startDirectory));
    while (current is not null)
    {
        if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
            return current.FullName;

        current = current.Parent;
    }

    throw new InvalidOperationException("Cannot locate repository root (Jazor.slnx).");
}

static string ResolvePath(string path, string repoRoot)
    => Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));

static void SetCommonEnvironment(string repoRoot)
{
    Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", Path.Combine(repoRoot, ".dotnet"));
    Environment.SetEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1");
    Environment.SetEnvironmentVariable("MSBUILDDISABLENODEREUSE", "1");
    Environment.SetEnvironmentVariable("UseSharedCompilation", "false");
}

static string ResolveAssemblyPath(string configuration, string baseOutputPath)
{
    var isolatedRoot = GetBuildRoot(baseOutputPath);
    return Path.Combine(isolatedRoot, "JazorAdmin", "bin", configuration, "net11.0", "JazorAdmin.dll");
}

static string ResolveInjectAssemblyPath(string configuration, string baseOutputPath)
{
    var isolatedRoot = GetBuildRoot(baseOutputPath);
    return Path.Combine(
        isolatedRoot,
        "JazorAdmin.InjectSmoke",
        "bin",
        configuration,
        "net11.0",
        "JazorAdmin.InjectSmoke.dll");
}

static string GetBuildRoot(string path)
{
    var fullPath = Path.GetFullPath(path);
    return fullPath.EndsWith(Path.DirectorySeparatorChar)
        ? fullPath
        : fullPath + Path.DirectorySeparatorChar;
}

static void CleanDirectory(string path, string repoRoot)
{
    var fullPath = Path.GetFullPath(path);
    var fullRoot = Path.GetFullPath(repoRoot);
    if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Refusing to delete a path outside the repository root: " + fullPath);

    if (Directory.Exists(fullPath))
        Directory.Delete(fullPath, recursive: true);
}

// Stale browser-<pid> harness roots embed the dead run's PID, so no later run reclaims
// them after an abnormal exit (zombie browser processes locking the profile, or a
// swallowed delete failure); sweep them at startup. Locked ones usually belong to a
// concurrent run or lingering browser processes and are skipped with a note.
// 旧运行残留的 browser-<pid> 目录内嵌已退出运行的 PID，异常退出后不会再有人清理；
// 启动时清扫一遍，仍被占用的目录（并发运行或残留浏览器进程）跳过并提示。
static void SweepStaleBrowserHarnessRoots(string parent, string repoRoot)
{
    if (!Directory.Exists(parent))
        return;

    foreach (var directory in Directory.GetDirectories(parent, "browser-*"))
    {
        try
        {
            CleanDirectory(directory, repoRoot);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            Console.WriteLine("JazorAdmin browser smoke skipped a locked stale harness directory: " + directory + " (" + error.Message + ")");
        }
    }
}

// Profile handles are released asynchronously after the browser tree dies, so an
// immediate delete can fail transiently; retry briefly and warn loudly instead of
// silently leaking a 100+ MB harness directory.
// 浏览器进程树终止后文件句柄异步释放，立即删除可能瞬时失败；重试几次，仍失败时
// 明确告警而不是静默残留上百 MB 的 harness 目录。
static async Task TryDeleteHarnessRootAsync(string harnessRoot)
{
    const int maxAttempts = 5;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            if (Directory.Exists(harnessRoot))
                Directory.Delete(harnessRoot, recursive: true);
            return;
        }
        catch (IOException) when (attempt < maxAttempts)
        {
            await Task.Delay(500);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            Console.WriteLine("JazorAdmin browser smoke could not remove the browser harness directory: " + harnessRoot + " (" + error.Message + ")");
            return;
        }
    }
}

static void RunDotNet(string workdir, IReadOnlyList<string> arguments)
{
    using var process = StartProcess("dotnet", arguments, workdir);
    process.WaitForExit();
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"Process failed with exit code {process.ExitCode}: dotnet {string.Join(' ', arguments)}");
}

static async Task<BrowserSmokeProcessResult> RunProcessAsync(
    string fileName,
    string workingDirectory,
    IReadOnlyList<string> arguments,
    TimeSpan timeout)
{
    var startInfo = new ProcessStartInfo(fileName)
    {
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    foreach (var argument in arguments)
        startInfo.ArgumentList.Add(argument);

    using var process = new Process { StartInfo = startInfo };
    process.Start();
    var standardOutput = process.StandardOutput.ReadToEndAsync();
    var standardError = process.StandardError.ReadToEndAsync();

    var timedOut = false;
    using var timeoutSource = new CancellationTokenSource(timeout);
    try
    {
        await process.WaitForExitAsync(timeoutSource.Token);
    }
    catch (OperationCanceledException)
    {
        timedOut = true;
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch
        {
        }

        await process.WaitForExitAsync();
    }

    var output = await standardOutput;
    var error = await standardError;
    return timedOut
        ? new BrowserSmokeProcessResult(-1, output, "Process timed out after " + timeout + "." + Environment.NewLine + error)
        : new BrowserSmokeProcessResult(process.ExitCode, output, error);
}

static JsonDocument ReadJsonLinePayload(string output, string markerDescription)
{
    foreach (var line in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Reverse())
    {
        var trimmed = line.Trim();
        if (!trimmed.StartsWith("{", StringComparison.Ordinal) ||
            !trimmed.EndsWith("}", StringComparison.Ordinal))
        {
            continue;
        }

        try
        {
            return JsonDocument.Parse(trimmed);
        }
        catch (JsonException)
        {
        }
    }

    throw new InvalidOperationException("Process output did not contain the " + markerDescription + " JSON smoke payload." + Environment.NewLine + output);
}

static Process StartProcess(string fileName, IReadOnlyList<string> arguments, string workdir)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = fileName,
        WorkingDirectory = workdir,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    foreach (var argument in arguments)
        startInfo.ArgumentList.Add(argument);

    var process = new Process { StartInfo = startInfo };
    process.Start();
    return process;
}

static void AssertPathExists(string path, string description)
{
    if (!File.Exists(path) && !Directory.Exists(path))
        throw new FileNotFoundException($"Missing {description}: {path}");
}

static void AssertContains(string text, string snippet, string description, string? details = null)
{
    if (!text.Contains(snippet, StringComparison.Ordinal))
        throw new InvalidOperationException($"Missing {description}: expected to find '{snippet}'." + FormatDetails(details));
}

static void AssertDoesNotContain(string text, string snippet, string description)
{
    if (text.Contains(snippet, StringComparison.Ordinal))
        throw new InvalidOperationException($"Unexpected {description}: found '{snippet}'.");
}

static void AssertJsonInt(JsonElement root, string propertyName, int expected, string description, string? details = null)
{
    var actual = root.GetProperty(propertyName).GetInt32();
    if (actual != expected)
        throw new InvalidOperationException($"Unexpected {description}: expected {expected}, got {actual}." + FormatDetails(details));
}

static void AssertJsonBoolean(JsonElement root, string propertyName, bool expected, string description, string? details = null)
{
    var actual = root.GetProperty(propertyName).GetBoolean();
    if (actual != expected)
        throw new InvalidOperationException($"Unexpected {description}: expected {expected}, got {actual}." + FormatDetails(details));
}

static void AssertJsonStringArrayContains(JsonElement root, string propertyName, string expected, string description, string? details = null)
{
    var actual = root.GetProperty(propertyName);
    if (actual.ValueKind != JsonValueKind.Array || !actual.EnumerateArray().Any(item => item.GetString() == expected))
    {
        throw new InvalidOperationException(
            $"Missing {description}: expected '{expected}'." +
            FormatDetails(details ?? actual.GetRawText()));
    }
}

static string FormatDetails(string? details)
    => string.IsNullOrWhiteSpace(details)
        ? string.Empty
        : Environment.NewLine + details;

static void CopyDirectory(string sourceRoot, string targetRoot)
{
    foreach (var directory in Directory.EnumerateDirectories(sourceRoot, "*", SearchOption.AllDirectories))
        Directory.CreateDirectory(Path.Combine(targetRoot, Path.GetRelativePath(sourceRoot, directory)));

    foreach (var file in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
    {
        var target = Path.Combine(targetRoot, Path.GetRelativePath(sourceRoot, file));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(file, target, overwrite: true);
    }
}

static void CopyInjectBrowserArtifacts(string sourceRoot, string targetRoot)
{
    var source = Path.Combine(sourceRoot, "components", "inject");
    var target = Path.Combine(targetRoot, "components", "inject");
    CopyDirectory(source, target);
}

static string ResolveDenoHostRuntime(string repoRoot)
{
    var executableName = OperatingSystem.IsWindows() ? "deno.exe" : "deno";
    var candidates = new List<string>();

    var packageRoot = Path.Combine(repoRoot, ".dotnet", ".nuget", "packages");
    if (Directory.Exists(packageRoot))
    {
        foreach (var runtimePackage in Directory.EnumerateDirectories(packageRoot, "denohost.runtime.*"))
            AddDenoRuntimeCandidates(candidates, runtimePackage, executableName);
    }

    var denoPath = candidates.FirstOrDefault(File.Exists);
    return denoPath ?? throw new FileNotFoundException(
        "DenoHost runtime was not restored with the local Jazor package.");
}

static void AddDenoRuntimeCandidates(ICollection<string> candidates, string root, string executableName)
{
    if (!Directory.Exists(root))
        return;

    foreach (var candidate in Directory
        .EnumerateFiles(root, executableName, SearchOption.AllDirectories)
        .OrderByDescending(static path => path, StringComparer.OrdinalIgnoreCase))
    {
        candidates.Add(candidate);
    }
}

static string? ResolveBrowserExecutable()
{
    var explicitPath = Environment.GetEnvironmentVariable("RAZORVUE_BROWSER_EXE")?.Trim();
    if (string.IsNullOrWhiteSpace(explicitPath))
        explicitPath = Environment.GetEnvironmentVariable("RAZORVUE_BROWSER_PATH")?.Trim();
    if (!string.IsNullOrWhiteSpace(explicitPath))
        return File.Exists(explicitPath) ? explicitPath : null;

    var candidates = OperatingSystem.IsWindows()
        ? new[]
        {
            @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            "msedge.exe",
            "chrome.exe"
        }
        : OperatingSystem.IsMacOS()
            ? new[]
            {
                "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge",
                "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                "microsoft-edge",
                "google-chrome",
                "chromium"
            }
            : new[]
            {
                "microsoft-edge",
                "microsoft-edge-stable",
                "google-chrome",
                "google-chrome-stable",
                "chromium",
                "chromium-browser"
            };

    foreach (var candidate in candidates)
    {
        var resolved = TryResolveExecutable(candidate);
        if (resolved is not null)
            return resolved;
    }

    return null;
}

static string? TryResolveExecutable(string candidate)
{
    if (candidate.Contains(Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
        candidate.Contains(Path.AltDirectorySeparatorChar, StringComparison.Ordinal) ||
        candidate.Contains(':', StringComparison.Ordinal))
    {
        return File.Exists(candidate) ? candidate : null;
    }

    var path = Environment.GetEnvironmentVariable("PATH") ?? "";
    var extensions = OperatingSystem.IsWindows()
        ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT")
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
        : [""];

    foreach (var directory in path.Split(Path.PathSeparator))
    {
        if (string.IsNullOrWhiteSpace(directory))
            continue;

        foreach (var extension in extensions)
        {
            var fileName = candidate.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                ? candidate
                : candidate + extension;
            var fullPath = Path.Combine(directory, fileName);
            if (File.Exists(fullPath))
                return fullPath;
        }
    }

    return null;
}

internal sealed record SmokeOptions(
    string Configuration,
    string? BaseOutputPath,
    string? BaseIntermediateOutputPath,
    string? GeneratedOutputRoot,
    bool FrontendOnly,
    bool SkipBrowser)
{
    public static SmokeOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Debug";
        string? baseOutputPath = null;
        string? baseIntermediateOutputPath = null;
        string? generatedOutputRoot = null;
        var frontendOnly = false;
        var skipBrowser = false;

        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            switch (argument)
            {
                case "--configuration":
                case "-Configuration":
                case "-c":
                    configuration = RequireValue(arguments, ref index, argument);
                    break;
                case "--base-output-path":
                case "-BaseOutputPath":
                    baseOutputPath = RequireValue(arguments, ref index, argument);
                    break;
                case "--base-intermediate-output-path":
                case "-BaseIntermediateOutputPath":
                    baseIntermediateOutputPath = RequireValue(arguments, ref index, argument);
                    break;
                case "--generated-output-root":
                case "-GeneratedOutputRoot":
                    generatedOutputRoot = RequireValue(arguments, ref index, argument);
                    break;
                case "--frontend-only":
                case "-FrontendOnly":
                    frontendOnly = true;
                    break;
                case "--skip-browser":
                case "-SkipBrowser":
                    skipBrowser = true;
                    break;
                case "--help":
                case "-h":
                    WriteUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported argument: " + argument);
            }
        }

        return new SmokeOptions(
            configuration,
            baseOutputPath,
            baseIntermediateOutputPath,
            generatedOutputRoot,
            frontendOnly,
            skipBrowser);
    }

    private static string RequireValue(IReadOnlyList<string> arguments, ref int index, string option)
    {
        var nextIndex = index + 1;
        if (nextIndex >= arguments.Count)
            throw new InvalidOperationException("Missing value for " + option + ".");

        index = nextIndex;
        return arguments[index];
    }

    private static void WriteUsage()
    {
        Console.WriteLine("Usage: dotnet run --no-launch-profile --file samples/JazorAdmin/verify-smoke.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --base-output-path <path>");
        Console.WriteLine("  --base-intermediate-output-path <path>");
        Console.WriteLine("  --generated-output-root <path>");
        Console.WriteLine("  --frontend-only");
        Console.WriteLine("  --skip-browser");
    }
}

[JsonSerializable(typeof(string))]
internal sealed partial class SmokeJsonContext : JsonSerializerContext
{
}

internal sealed record BrowserSmokeProcessResult(int ExitCode, string StandardOutput, string StandardError)
{
    public override string ToString()
        => "ExitCode: " + ExitCode + Environment.NewLine +
           "STDOUT:" + Environment.NewLine +
           StandardOutput + Environment.NewLine +
           "STDERR:" + Environment.NewLine +
           StandardError;
}
