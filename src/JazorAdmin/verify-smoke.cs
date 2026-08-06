using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var options = SmokeOptions.Parse(args);
var repoRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
var adminRoot = Path.Combine(repoRoot, "src", "JazorAdmin");
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

static void AssertGeneratedArtifacts(string generatedOutputRoot)
{
    const string appModulePath = "components/jazor-admin-app.mjs";
    const string bootstrapModulePath = "components/jazor-admin-bootstrap.mjs";
    const string routeCatalogModulePath = "components/jazor-admin-admin-route-catalog.mjs";
    const string errorPageModulePath = "components/jazor-admin-error-page.mjs";
    const string apiClientModulePath = "components/jazor-admin-api-client.mjs";
    const string organizationModulePath = "components/organization.mjs";
    const string accessControlModulePath = "components/access-control.mjs";
    const string accountModulePath = "components/accounts.mjs";
    const string configModulePath = "components/config.mjs";
    var componentModules = new[]
    {
        (appModulePath, "JazorAdmin app module"),
        (bootstrapModulePath, "JazorAdmin router bootstrap module"),
        ("components/jazor-admin-routes.mjs", "JazorAdmin route catalog module"),
        (routeCatalogModulePath, "admin route catalog module"),
        ("components/jazor-admin-application-frame.mjs", "admin application frame module"),
        ("components/jazor-admin-tdesign-admin-layout.mjs", "TDesign admin layout module"),
        ("components/jazor-admin-tdesign-sidebar-menu.mjs", "TDesign sidebar module"),
        ("components/jazor-admin-tdesign-page-container.mjs", "TDesign page container module"),
        ("components/jazor-admin-tdesign-header-bar.mjs", "TDesign header bar module"),
        ("components/jazor-admin-localization.mjs", "JazorAdmin localization module"),
        (errorPageModulePath, "JazorAdmin error page module"),
        (apiClientModulePath, "JazorAdmin API client module"),
        (organizationModulePath, "organization management module"),
        (accessControlModulePath, "access control management module"),
        (accountModulePath, "account management module"),
        (configModulePath, "OpenIddict configuration module")
    };
    var manifestPath = Path.Combine(generatedOutputRoot, "jazor-manifest.json");

    foreach (var (relativePath, description) in componentModules)
        AssertPathExists(Path.Combine(generatedOutputRoot, relativePath), "generated " + description);
    AssertPathExists(manifestPath, "generated manifest");

    foreach (var (relativePath, description) in componentModules)
    {
        var module = File.ReadAllText(Path.Combine(generatedOutputRoot, relativePath));
        AssertDoesNotContain(module, "createRenderContext", "render-context import in " + description);
        AssertDoesNotContain(module, ".vue", "legacy SFC reference in " + description);
        AssertDoesNotContain(module, "scope.buildRenderTree(builder)", "legacy scoped render-tree call in " + description);
        AssertDoesNotContain(module, "builder.finish()", "legacy render builder completion in " + description);
    }

    foreach (var (relativePath, description) in componentModules.Where((_, index) => index == 0 || (index >= 4 && index != 9 && index != 11)))
    {
        var module = File.ReadAllText(Path.Combine(generatedOutputRoot, relativePath));
        AssertContains(module, "defineComponent", "Vue component wrapper in " + description);
        AssertContains(module, "function $renderDirect()", "direct VNode render function in " + description);
    }

    var appModule = File.ReadAllText(Path.Combine(generatedOutputRoot, appModulePath));
    var bootstrapModule = File.ReadAllText(Path.Combine(generatedOutputRoot, bootstrapModulePath));
    var routesModule = File.ReadAllText(Path.Combine(generatedOutputRoot, "components/jazor-admin-routes.mjs"));
    var routeCatalogModule = File.ReadAllText(Path.Combine(generatedOutputRoot, routeCatalogModulePath));
    var adminLayoutModule = File.ReadAllText(Path.Combine(generatedOutputRoot, "components/jazor-admin-tdesign-admin-layout.mjs"));
    var headerBarModule = File.ReadAllText(Path.Combine(generatedOutputRoot, "components/jazor-admin-tdesign-header-bar.mjs"));
    var errorPageModule = File.ReadAllText(Path.Combine(generatedOutputRoot, errorPageModulePath));
    var apiClientModule = File.ReadAllText(Path.Combine(generatedOutputRoot, apiClientModulePath));
    var organizationModule = File.ReadAllText(Path.Combine(generatedOutputRoot, organizationModulePath));
    var accessControlModule = File.ReadAllText(Path.Combine(generatedOutputRoot, accessControlModulePath));
    var accountModule = File.ReadAllText(Path.Combine(generatedOutputRoot, accountModulePath));
    var configModule = File.ReadAllText(Path.Combine(generatedOutputRoot, configModulePath));
    var manifest = File.ReadAllText(manifestPath);

    AssertContains(appModule, "defineComponent", "Vue component wrapper in JazorAdmin app module");
    AssertContains(appModule, "function $renderDirect()", "direct VNode render function in JazorAdmin app module");
    AssertContains(appModule, "JazorAdmin", "JazorAdmin app text in generated module");
    AssertContains(appModule, "useRoute", "Vue Router route injection in JazorAdmin app module");
    AssertContains(appModule, "onMounted", "session restoration mount hook in JazorAdmin app module");
    AssertContains(appModule, "scope.onAfterRender(true)", "initial Razor lifecycle invocation in JazorAdmin app module");
    AssertContains(appModule, "restoreSession();", "mounted session restoration invocation in JazorAdmin app module");
    AssertContains(bootstrapModule, "createRouter", "Vue Router creation in JazorAdmin bootstrap module");
    AssertContains(bootstrapModule, "createWebHistory", "Vue Router web history in JazorAdmin bootstrap module");
    AssertContains(bootstrapModule, "RouterView", "Vue Router view in JazorAdmin bootstrap module");
    AssertContains(routeCatalogModule, "routeTarget", "strongly typed route target in admin route catalog module");
    AssertDoesNotContain(routeCatalogModule, "return path == null ? null : from(path)", "undefined union factory in admin route catalog module");
    AssertContains(routesModule, "organizations.structure", "organization structure route in JazorAdmin routes module");
    AssertContains(routesModule, "authorization.roles", "authorization roles route in JazorAdmin routes module");
    AssertContains(routesModule, "accounts", "account route in JazorAdmin routes module");
    AssertContains(routesModule, "configuration.clients", "OpenIddict client route in JazorAdmin routes module");
    AssertContains(routesModule, "configuration.scopes", "OpenIddict scope route in JazorAdmin routes module");
    AssertDoesNotContain(routesModule, "operations/releases", "retired release route in JazorAdmin routes module");
    AssertDoesNotContain(routesModule, "settings", "retired settings route in JazorAdmin routes module");
    AssertContains(adminLayoutModule, "data-shell-command", "sidebar toggle command in TDesign admin layout module");
    AssertContains(adminLayoutModule, "toggle-sidebar", "sidebar toggle command key in TDesign admin layout module");
    AssertContains(adminLayoutModule, "onUpdate:collapsed", "controlled collapsed Vue listener in TDesign admin layout module");
    AssertContains(adminLayoutModule, "horizontal: true", "top navigation variant in TDesign admin layout module");
    AssertContains(headerBarModule, "jazor-admin-tdesign-header__navigation", "navigation slot region in TDesign header bar module");
    AssertContains(errorPageModule, "data-error-kind", "typed error kind marker in JazorAdmin error page module");
    AssertContains(errorPageModule, "aria-labelledby", "error title accessibility relation in JazorAdmin error page module");
    AssertContains(errorPageModule, "aria-describedby", "error description accessibility relation in JazorAdmin error page module");
    AssertContains(errorPageModule, "data-error-action", "error recovery action in JazorAdmin error page module");
    AssertContains(apiClientModule, "getSession", "session transport in JazorAdmin API client module");
    AssertContains(organizationModule, "createChildOrganization", "child organization command in organization management module");
    AssertContains(organizationModule, "scope.onParametersSet();", "route parameter lifecycle in organization management module");
    AssertContains(accessControlModule, "replaceRoleGrants", "role grant command in access control management module");
    AssertContains(accessControlModule, "scope.onParametersSet();", "route parameter lifecycle in access control management module");
    AssertContains(accountModule, "getAccounts", "account query in account management module");
    AssertContains(accountModule, "resetAccountPassword", "account password command in account management module");
    AssertContains(accountModule, "scope.onAfterRender(true)", "initial Razor lifecycle invocation in account management module");
    AssertContains(configModule, "getClients", "OpenIddict client query in configuration module");
    AssertContains(configModule, "getScopes", "OpenIddict scope query in configuration module");
    AssertContains(configModule, "scope.onParametersSet();", "route parameter lifecycle in configuration module");
    foreach (var (relativePath, description) in componentModules)
        AssertContains(manifest, "\"" + relativePath + "\"", description + " manifest entry");
    AssertDoesNotContain(manifest, ".vue", "legacy SFC artifact in JazorAdmin manifest");
    AssertDoesNotContain(manifest, "release-table", "retired release table artifact in JazorAdmin manifest");
    AssertDoesNotContain(manifest, "settings-form", "retired settings form artifact in JazorAdmin manifest");
}

static void AssertInjectGeneratedArtifacts(string generatedOutputRoot)
{
    var appPath = Path.Combine(generatedOutputRoot, "components", "jazor-admin-inject-app.mjs");
    var containerPath = Path.Combine(generatedOutputRoot, "components", "jazor-admin-inject-page-container.mjs");
    var manifestPath = Path.Combine(generatedOutputRoot, "jazor-manifest.json");
    AssertPathExists(appPath, "generated JazorAdmin VueInject app module");
    AssertPathExists(containerPath, "generated JazorAdmin VueInject container module");
    AssertPathExists(manifestPath, "generated JazorAdmin VueInject manifest");

    foreach (var modulePath in Directory.EnumerateFiles(
                 Path.Combine(generatedOutputRoot, "components"),
                 "*.mjs",
                 SearchOption.TopDirectoryOnly))
    {
        var module = File.ReadAllText(modulePath);
        var description = "JazorAdmin VueInject module " + Path.GetFileName(modulePath);
        AssertDoesNotContain(module, "createRenderContext", "render-context import in " + description);
        AssertDoesNotContain(module, ".vue", "legacy SFC reference in " + description);
        AssertDoesNotContain(module, "scope.buildRenderTree(builder)", "legacy scoped render-tree call in " + description);
        AssertDoesNotContain(module, "builder.finish()", "legacy render builder completion in " + description);
        AssertDoesNotContain(module, "slots.injected-extra", "invalid hyphenated slot access in " + description);
    }

    var app = File.ReadAllText(appPath);
    var container = File.ReadAllText(containerPath);
    var manifest = File.ReadAllText(manifestPath);
    AssertContains(app, "from \"./jazor-admin-inject-page-container.mjs\"", "VueInject implementation import");
    AssertDoesNotContain(app, "from \"./jazor-admin-page-container.mjs\"", "stale VueInject contract import");
    AssertContains(app, "injectedTitle", "VueInject runtime prop name");
    AssertContains(app, "\"injected-extra\"", "VueInject runtime slot name");
    AssertContains(container, "slots[\"injected-extra\"]", "VueInject bracket slot access");
    AssertContains(container, "href;", "RenderFragment helper pattern declaration");
    AssertContains(manifest, "\"components/jazor-admin-inject-app.mjs\"", "VueInject app manifest entry");
    AssertContains(manifest, "\"components/jazor-admin-inject-page-container.mjs\"", "VueInject container manifest entry");
}

static async Task VerifyBrowserSmokeAsync(
    string repoRoot,
    string adminRoot,
    string generatedOutputRoot,
    string injectGeneratedOutputRoot)
{
    var browserPath = ResolveBrowserExecutable();
    if (browserPath is null)
    {
        Console.WriteLine("JazorAdmin browser smoke skipped: Microsoft Edge, Chrome, or Chromium was not found. Set RAZORVUE_BROWSER_EXE to enable it.");
        return;
    }

    var denoPath = ResolveDenoExecutable(repoRoot);

    var vueRuntime = Path.Combine(repoRoot, "node_modules", "vue", "dist", "vue.runtime.esm-browser.prod.js");
    if (!File.Exists(vueRuntime))
    {
        Console.WriteLine("JazorAdmin browser smoke skipped: missing node_modules/vue/dist/vue.runtime.esm-browser.prod.js.");
        return;
    }
    var vueRouterRuntime = await ResolveVueRouterRuntimeAsync(repoRoot);
    var tDesignRuntime = await ResolveTDesignRuntimeAsync(repoRoot);
    var tDesignStyle = await ResolveTDesignStyleAsync(repoRoot);

    var harnessRoot = Path.Combine(repoRoot, ".tmp", "sample-smoke", "JazorAdmin", "browser-" + Environment.ProcessId);
    CleanDirectory(harnessRoot, repoRoot);
    Directory.CreateDirectory(harnessRoot);
    try
    {
        CopyDirectory(generatedOutputRoot, harnessRoot);
        CopyInjectBrowserArtifacts(injectGeneratedOutputRoot, harnessRoot);
        var vendorRoot = Path.Combine(harnessRoot, "vendor");
        Directory.CreateDirectory(vendorRoot);
        File.Copy(vueRuntime, Path.Combine(vendorRoot, "vue.runtime.esm-browser.prod.js"), overwrite: true);
        File.Copy(vueRouterRuntime, Path.Combine(vendorRoot, "vue-router.esm-browser.prod.js"), overwrite: true);
        File.Copy(tDesignRuntime, Path.Combine(vendorRoot, "tdesign-vue-next.bundle.mjs"), overwrite: true);
        File.Copy(tDesignStyle, Path.Combine(vendorRoot, "tdesign-vue-next.css"), overwrite: true);
        // TDesign's browser ESM bundle imports this environment shim; Deno remains the smoke host.
        var nodeRuntimeRoot = Path.Combine(harnessRoot, "node");
        Directory.CreateDirectory(nodeRuntimeRoot);
        await File.WriteAllTextAsync(
            Path.Combine(nodeRuntimeRoot, "process.mjs"),
            """
            const process = { env: Object.create(null) };
            export { process };
            export default process;
            """,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        var indexPath = Path.Combine(harnessRoot, "index.html");
        await File.WriteAllTextAsync(
            indexPath,
            """
            <!doctype html>
            <html>
              <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>JazorAdmin browser smoke</title>
                <link rel="stylesheet" href="/vendor/tdesign-vue-next.css">
                <script type="importmap">
                {
                  "imports": {
                    "vue": "/vendor/vue.runtime.esm-browser.prod.js",
                    "npm:vue@3": "/vendor/vue.runtime.esm-browser.prod.js",
                    "npm:vue@3.mjs": "/vendor/vue.runtime.esm-browser.prod.js",
                    "npm:vue-router@4": "/vendor/vue-router.esm-browser.prod.js",
                    "npm:vue-router@4.mjs": "/vendor/vue-router.esm-browser.prod.js",
                    "tdesign-vue-next": "/vendor/tdesign-vue-next.bundle.mjs",
                    "npm:tdesign-vue-next": "/vendor/tdesign-vue-next.bundle.mjs",
                    "style.mjs": "/style.mjs",
                    "@jazor/vue-runtime/": "/@jazor/vue-runtime/",
                    "components/": "/components/",
                    "System/": "/System/"
                  }
                }
                </script>
              </head>
              <body>
                <div id="app"></div>
                <div id="inject-app"></div>
                <script>
                  addEventListener("error", (event) => {
                    globalThis.__jazorAdminBrowserSmoke = {
                      ok: false,
                      message: event.error?.message ?? event.message ?? "Browser module error",
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
                  const clients = [{
                    id: "c05e9d5a-e435-47cb-92f6-ca31f46ca8a5",
                    clientId: "jazoradmin-spa",
                    displayName: "JazorAdmin SPA",
                    redirectUris: ["http://localhost/auth/callback"],
                    postLogoutRedirectUris: ["http://localhost/login"],
                    scopes: ["openid", "profile", "jazoradmin_api"]
                  }];
                  const scopes = [{
                    id: "c4351a3e-8f51-4e98-a505-6500c9d591e7",
                    name: "jazoradmin_api",
                    displayName: "JazorAdmin API",
                    resources: ["jazoradmin_api"]
                  }];
                  const json = (data, status = 200) => Promise.resolve(new Response(JSON.stringify(data), {
                    status,
                    headers: { "content-type": "application/json" }
                  }));
                  globalThis.fetch = (input, init = {}) => {
                    const url = new URL(typeof input === "string" ? input : input.url, location.href);
                    const method = String(init.method ?? "GET").toUpperCase();
                    if (method !== "GET") return Promise.resolve(new Response(null, { status: 204 }));
                    if (url.pathname === "/api/auth/session") return json({
                      userId: "74b0b0be-4b91-461f-9d57-c2f94aed4842",
                      email: "smoke.operator@example.test",
                      displayName: "Smoke operator",
                      roles: ["platform-administrator"],
                      organizations: [organization]
                    });
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
                    if (url.pathname === "/api/configuration/clients") return json(clients);
                    if (url.pathname === "/api/configuration/scopes") return json(scopes);
                    return json({ title: "Not Found" }, 404);
                  };
                </script>
                <script type="module">
                  import { createApp, nextTick } from "vue";
                  import App from "/components/jazor-admin-app.mjs";
                  import InjectApp from "/components/jazor-admin-inject-app.mjs";
                  import { boot } from "/components/jazor-admin-bootstrap.mjs";

                  try {
                    boot("#app", App);
                    createApp(InjectApp).mount("#inject-app");
                    for (let attempt = 0; attempt < 100 && !document.querySelector('[data-page-region="title"], .jazor-admin-error'); attempt++) {
                      await new Promise((resolve) => setTimeout(resolve, 10));
                    }
                    if (!document.querySelector('[data-page-region="title"], .jazor-admin-error')) {
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
                    const errorPage = document.querySelector(".jazor-admin-error");
                    if (errorPage) {
                      const requestedPathname = location.pathname;
                      const errorKind = errorPage.getAttribute("data-error-kind") ?? "";
                      const errorRole = errorPage.getAttribute("role") ?? "";
                      const errorCode = document.querySelector(".jazor-admin-error__code")?.textContent ?? "";
                      const errorTitle = document.querySelector(".jazor-admin-error h1")?.textContent ?? "";
                      const errorDescription = document.querySelector(".jazor-admin-error p")?.textContent ?? "";
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
                      for (let attempt = 0; attempt < 100 && document.querySelector(".jazor-admin-management__loading"); attempt++) {
                        await new Promise((resolve) => setTimeout(resolve, 10));
                      }
                      globalThis.__jazorAdminBrowserSmoke = {
                        ok: true,
                        mode: "deep-link",
                        pathname: location.pathname,
                        pageTitleText: document.querySelector('[data-page-region="title"]')?.textContent ?? "",
                        breadcrumbText: document.querySelector('[data-page-region="breadcrumb"]')?.textContent ?? "",
                        activeKey: document.querySelector("[data-navigation-selected-key]")?.getAttribute("data-navigation-selected-key") ?? "",
                        organizationPanelVisible: document.querySelector('[data-management-area="organizations"]') !== null
                      };
                    } else {
                      const waitFor = async (predicate, description) => {
                        for (let attempt = 0; attempt < 120; attempt++) {
                          await nextTick();
                          if (predicate()) return;
                          await new Promise((resolve) => setTimeout(resolve, 10));
                        }
                        throw new Error(`Timed out waiting for ${description}.`);
                      };
                      const click = async (selector, description) => {
                        const target = document.querySelector(selector);
                        if (!(target instanceof HTMLElement)) throw new Error(`Missing ${description}.`);
                        target.click();
                        await nextTick();
                      };
                      const waitForTitle = (title) => waitFor(
                        () => document.querySelector('[data-page-region="title"]')?.textContent?.includes(title) === true,
                        `${title} page`);

                      await waitFor(() => document.querySelectorAll(".jazor-admin-overview__metric").length === 4, "administration overview");
                      const dashboardText = document.querySelector('[data-page-region="body"]')?.textContent ?? "";
                      const metricCount = document.querySelectorAll(".jazor-admin-overview__metric").length;
                      const railSectionCount = document.querySelectorAll("[data-rail-section]").length;
                      const userText = document.querySelector(".jazor-admin__user")?.textContent ?? "";
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
                        "collapsed sidebar transition");
                      const collapsedSidebar = document.querySelector('[data-shell-region="sidebar"]');
                      const collapsedSidebarWidth = collapsedSidebar ? getComputedStyle(collapsedSidebar).width : "";
                      sidebarToggle?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                      await waitFor(
                        () => document.querySelector('[data-shell-region="layout"]')?.getAttribute("data-shell-collapsed") === "false",
                        "expanded shell");

                      await click('[data-rail-section="organizations"]', "organizations rail item");
                      await waitForTitle("组织架构");
                      await waitFor(() => document.querySelector(".jazor-admin-management__details") !== null, "organization details");
                      const organizationPathname = location.pathname;
                      const organizationTitle = document.querySelector('[data-page-region="title"]')?.textContent ?? "";
                      const organizationText = document.querySelector('[data-management-area="organizations"]')?.textContent ?? "";
                      const childOrganizationCount = document.querySelectorAll(".jazor-admin-management__item-list li").length;

                      await click('[data-nav-key="organizations.members"] a', "members navigation item");
                      await waitForTitle("成员管理");
                      await waitFor(() => document.querySelectorAll(".jazor-admin-management__table tbody tr").length === 1, "member table");
                      const membersPathname = location.pathname;
                      const memberRowText = document.querySelector(".jazor-admin-management__table tbody tr")?.textContent ?? "";
                      await click(".jazor-admin-management__text-button", "member role editor command");
                      const memberRoleEditorVisible = document.querySelector(".jazor-admin-management__role-editor") !== null;

                      await click('[data-rail-section="authorization"]', "authorization rail item");
                      await waitForTitle("角色与授权");
                      await waitFor(() => document.querySelector(".jazor-admin-management__grant-list") !== null, "role grants");
                      const accessPathname = location.pathname;
                      const roleCount = document.querySelectorAll(".jazor-admin-management__role-list li").length;
                      const grantCount = document.querySelectorAll(".jazor-admin-management__grant-list .jazor-admin-management__check").length;

                      await click('[data-nav-key="authorization.resources"] a', "resource operations navigation item");
                      await waitForTitle("资源操作");
                      await waitFor(() => document.querySelectorAll(".jazor-admin-management__table tbody tr").length === 4, "resource operation table");
                      const resourcesPathname = location.pathname;
                      const resourceOperationCount = document.querySelectorAll(".jazor-admin-management__table tbody tr").length;
                      const resourceText = document.querySelector('[data-management-area="authorization"]')?.textContent ?? "";

                      await click('[data-rail-section="accounts"]', "accounts rail item");
                      await waitForTitle("账户管理");
                      await waitFor(() => document.querySelectorAll('[data-management-area="accounts"] .jazor-admin-management__table tbody tr').length === 1, "account table");
                      const accountsPathname = location.pathname;
                      const accountRowText = document.querySelector('[data-management-area="accounts"] .jazor-admin-management__table tbody tr')?.textContent ?? "";

                      await click('[data-rail-section="configuration"]', "configuration rail item");
                      await waitForTitle("OpenID 客户端");
                      await waitFor(() => document.querySelectorAll('[data-management-area="configuration"] .jazor-admin-management__table tbody tr').length === 1, "OpenIddict client table");
                      const clientsPathname = location.pathname;
                      const clientRowText = document.querySelector('[data-management-area="configuration"] .jazor-admin-management__table tbody tr')?.textContent ?? "";

                      await click('[data-nav-key="configuration.scopes"] a', "OpenIddict scope navigation item");
                      await waitForTitle("OpenID Scope");
                      await waitFor(() => document.querySelectorAll('[data-management-area="configuration"] .jazor-admin-management__table tbody tr').length === 1, "OpenIddict scope table");
                      const scopesPathname = location.pathname;
                      const scopeRowText = document.querySelector('[data-management-area="configuration"] .jazor-admin-management__table tbody tr')?.textContent ?? "";

                      await click('[data-rail-section="dashboard"]', "dashboard rail item");
                      await waitForTitle("仪表盘");
                      const dashboardReturnPathname = location.pathname;

                      globalThis.__jazorAdminBrowserSmoke = {
                        ok: true,
                        mode: "app",
                        pageTitleText: document.querySelector('[data-page-region="title"]')?.textContent ?? "",
                        dashboardText,
                        metricCount,
                        railSectionCount,
                        userText,
                        organizationPickerValue,
                        initialSidebarExpanded,
                        collapsedSidebarWidth,
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
                        clientsPathname,
                        clientRowText,
                        scopesPathname,
                        scopeRowText,
                        dashboardReturnPathname,
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
            TimeSpan.FromSeconds(60));
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

        AssertContains(root.GetProperty("pageTitleText").GetString() ?? string.Empty, "仪表盘", "JazorAdmin browser dashboard title", root.GetRawText());
        AssertContains(root.GetProperty("dashboardText").GetString() ?? string.Empty, "Organization access", "JazorAdmin browser administration overview", root.GetRawText());
        AssertJsonInt(root, "metricCount", 4, "JazorAdmin administration metric count", root.GetRawText());
        AssertJsonInt(root, "railSectionCount", 5, "JazorAdmin primary rail section count", root.GetRawText());
        AssertContains(root.GetProperty("userText").GetString() ?? string.Empty, "Smoke operator", "JazorAdmin browser session account", root.GetRawText());
        AssertContains(root.GetProperty("organizationPickerValue").GetString() ?? string.Empty, "5e1246c9", "JazorAdmin browser organization selection", root.GetRawText());
        AssertContains(root.GetProperty("initialSidebarExpanded").GetString() ?? string.Empty, "true", "JazorAdmin initial sidebar state", root.GetRawText());
        AssertContains(root.GetProperty("collapsedSidebarWidth").GetString() ?? string.Empty, "64px", "JazorAdmin collapsed rail width", root.GetRawText());

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
        AssertContains(root.GetProperty("accountRowText").GetString() ?? string.Empty, "Platform administrator", "JazorAdmin account row", root.GetRawText());
        AssertContains(root.GetProperty("clientsPathname").GetString() ?? string.Empty, "/configuration/clients", "JazorAdmin OpenIddict client navigation", root.GetRawText());
        AssertContains(root.GetProperty("clientRowText").GetString() ?? string.Empty, "JazorAdmin SPA", "JazorAdmin OpenIddict client row", root.GetRawText());
        AssertContains(root.GetProperty("scopesPathname").GetString() ?? string.Empty, "/configuration/scopes", "JazorAdmin OpenIddict scope navigation", root.GetRawText());
        AssertContains(root.GetProperty("scopeRowText").GetString() ?? string.Empty, "JazorAdmin API", "JazorAdmin OpenIddict scope row", root.GetRawText());
        AssertContains(root.GetProperty("dashboardReturnPathname").GetString() ?? string.Empty, "/", "JazorAdmin dashboard rail return", root.GetRawText());
        if (root.GetProperty("hasLegacyVueReference").GetBoolean())
            throw new InvalidOperationException("JazorAdmin browser smoke found a legacy .vue script reference.");

        var deepLink = root.GetProperty("deepLink");
        AssertContains(deepLink.GetProperty("mode").GetString() ?? string.Empty, "deep-link", "JazorAdmin deep-link smoke mode", deepLink.GetRawText());
        AssertContains(deepLink.GetProperty("pathname").GetString() ?? string.Empty, "/organizations/structure", "JazorAdmin deep-link browser location", deepLink.GetRawText());
        AssertContains(deepLink.GetProperty("pageTitleText").GetString() ?? string.Empty, "组织架构", "JazorAdmin deep-link page title", deepLink.GetRawText());
        AssertContains(deepLink.GetProperty("breadcrumbText").GetString() ?? string.Empty, "组织机构", "JazorAdmin deep-link breadcrumb", deepLink.GetRawText());
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
        try
        {
            if (Directory.Exists(harnessRoot))
                Directory.Delete(harnessRoot, recursive: true);
        }
        catch
        {
        }
    }
}

static async Task<string> ResolveVueRouterRuntimeAsync(string repoRoot)
{
    const string version = "4.6.4";
    var installedPath = Path.Combine(
        repoRoot,
        "node_modules",
        "vue-router",
        "dist",
        "vue-router.esm-browser.prod.js");
    if (File.Exists(installedPath))
        return installedPath;

    return await ResolveCachedBrowserAssetAsync(
        repoRoot,
        "vue-router-" + version + ".esm-browser.prod.js",
        "https://cdn.jsdelivr.net/npm/vue-router@" + version + "/dist/vue-router.esm-browser.prod.js",
        "createRouter",
        "Vue Router runtime");
}

static Task<string> ResolveTDesignRuntimeAsync(string repoRoot)
{
    const string version = "1.20.5";
    return ResolveCachedBrowserAssetAsync(
        repoRoot,
        "tdesign-vue-next-" + version + ".bundle.mjs",
        "https://esm.sh/tdesign-vue-next@" + version + "/es2022/tdesign-vue-next.bundle.mjs?external=vue",
        "from\"vue\"",
        "TDesign Vue Next runtime");
}

static Task<string> ResolveTDesignStyleAsync(string repoRoot)
{
    const string version = "1.20.5";
    return ResolveCachedBrowserAssetAsync(
        repoRoot,
        "tdesign-vue-next-" + version + ".css",
        "https://cdn.jsdelivr.net/npm/tdesign-vue-next@" + version + "/dist/tdesign.min.css",
        ".t-layout",
        "TDesign Vue Next stylesheet");
}

static async Task<string> ResolveCachedBrowserAssetAsync(
    string repoRoot,
    string fileName,
    string sourceUrl,
    string requiredMarker,
    string description)
{
    var cacheDirectory = Path.Combine(repoRoot, ".tmp", "browser-runtime");
    var cachePath = Path.Combine(cacheDirectory, fileName);
    if (File.Exists(cachePath))
        return cachePath;

    Directory.CreateDirectory(cacheDirectory);
    using var client = new HttpClient();
    var source = await client.GetStringAsync(sourceUrl);
    if (!source.Contains(requiredMarker, StringComparison.Ordinal))
        throw new InvalidOperationException("Downloaded " + description + " is missing '" + requiredMarker + "'.");

    await File.WriteAllTextAsync(cachePath, source, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    return cachePath;
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
          try {
            const page = await connectToPage(browser.port);
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
              await page.send("Emulation.setDeviceMetricsOverride", {
                width: 390,
                height: 844,
                deviceScaleFactor: 1,
                mobile: false
              });
              await page.navigate(`http://127.0.0.1:${server.port}/organizations/structure`);
              await page.waitForSmoke();
              result.mobileLayout = await page.readLayout();
              await page.navigate(`http://127.0.0.1:${server.port}/error/500`);
              const internalError = await page.waitForSmoke();
              await page.navigate(`http://127.0.0.1:${server.port}/missing/admin/page`);
              const notFound = await page.waitForSmoke();
              result.deepLink = deepLink;
              result.internalError = internalError;
              result.notFound = notFound;
              result.diagnostics = page.diagnostics;
              result.serverDiagnostics = server.diagnostics;
              console.log(JSON.stringify(result));
            } catch (error) {
              const message = error instanceof Error ? error.message : String(error);
              throw new Error(`${message} Server: ${JSON.stringify(server.diagnostics)}`);
            }
          } finally {
            await browser.dispose();
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
          return { "content-type": contentType, "cache-control": "no-store" };
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
                  dispose: async () => {
                    if (!exited) {
                      process.kill("SIGKILL");
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
            process.kill("SIGKILL");
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
              const navigation = document.querySelector('[data-navigation-orientation="vertical"]');
              const shellStyle = shell ? getComputedStyle(shell) : null;
              const sidebarStyle = sidebar ? getComputedStyle(sidebar) : null;
              const mainStyle = main ? getComputedStyle(main) : null;
              const navigationStyle = navigation ? getComputedStyle(navigation) : null;
              const shellRect = shell?.getBoundingClientRect();
              const sidebarRect = sidebar?.getBoundingClientRect();
              const mainRect = main?.getBoundingClientRect();
              return {
                viewportWidth: innerWidth,
                shellDisplay: shellStyle?.display ?? "",
                shellDirection: shellStyle?.flexDirection ?? "",
                sidebarWidth: sidebarStyle?.width ?? "",
                mainDisplay: mainStyle?.display ?? "",
                navigationDisplay: navigationStyle?.display ?? "",
                sidebarFillsShell: !!shellRect && !!sidebarRect && Math.abs(shellRect.width - sidebarRect.width) <= 1,
                styleRuntimeLoaded: document.querySelector("style#jazor-css") !== null,
                documentFitsViewport: document.documentElement.scrollWidth <= innerWidth,
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
    var sourceComponents = Path.Combine(sourceRoot, "components");
    var targetComponents = Path.Combine(targetRoot, "components");
    Directory.CreateDirectory(targetComponents);
    foreach (var source in Directory.EnumerateFiles(sourceComponents, "jazor-admin-inject-*.mjs*"))
    {
        File.Copy(source, Path.Combine(targetComponents, Path.GetFileName(source)), overwrite: true);
    }
}

static string ResolveDenoExecutable(string repoRoot)
{
    var explicitPath = Environment.GetEnvironmentVariable("JAZOR_DENO_EXE")?.Trim();
    if (!string.IsNullOrWhiteSpace(explicitPath))
    {
        var fullPath = Path.GetFullPath(explicitPath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Explicit JAZOR_DENO_EXE path does not exist: " + fullPath);

        return fullPath;
    }

    var executableName = OperatingSystem.IsWindows() ? "deno.exe" : "deno";
    var candidates = new List<string>();
    AddDenoRuntimeCandidates(candidates, Path.Combine(repoRoot, "src", "Jazor.Emit", "bin"), executableName);

    var packageRoot = Path.Combine(repoRoot, ".dotnet", ".nuget", "packages");
    if (Directory.Exists(packageRoot))
    {
        foreach (var runtimePackage in Directory.EnumerateDirectories(packageRoot, "denohost.runtime.*"))
            AddDenoRuntimeCandidates(candidates, runtimePackage, executableName);
    }

    var denoPath = candidates.FirstOrDefault(File.Exists) ?? TryResolveExecutable("deno");
    return denoPath ?? throw new FileNotFoundException(
        "Bundled Deno runtime was not found. Build Jazor.Emit so DenoHost runtime assets are restored, or set JAZOR_DENO_EXE.");
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
        Console.WriteLine("Usage: dotnet run --no-launch-profile --file src/JazorAdmin/verify-smoke.cs -- [options]");
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
