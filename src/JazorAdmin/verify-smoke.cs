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
    const string dataTableModulePath = "components/jazor-admin-release-table.mjs";
    const string formModulePath = "components/jazor-admin-settings-form.mjs";
    const string noticeModulePath = "components/jazor-admin-action-notice.mjs";
    const string errorPageModulePath = "components/jazor-admin-error-page.mjs";
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
        (dataTableModulePath, "JazorAdmin release table module"),
        (formModulePath, "JazorAdmin settings form module"),
        (noticeModulePath, "JazorAdmin action notice module"),
        (errorPageModulePath, "JazorAdmin error page module")
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

    foreach (var (relativePath, description) in componentModules.Where((_, index) => index == 0 || (index >= 4 && index != 9)))
    {
        var module = File.ReadAllText(Path.Combine(generatedOutputRoot, relativePath));
        AssertContains(module, "defineComponent", "Vue component wrapper in " + description);
        AssertContains(module, "function $renderDirect()", "direct VNode render function in " + description);
    }

    var appModule = File.ReadAllText(Path.Combine(generatedOutputRoot, appModulePath));
    var bootstrapModule = File.ReadAllText(Path.Combine(generatedOutputRoot, bootstrapModulePath));
    var routeCatalogModule = File.ReadAllText(Path.Combine(generatedOutputRoot, routeCatalogModulePath));
    var adminLayoutModule = File.ReadAllText(Path.Combine(generatedOutputRoot, "components/jazor-admin-tdesign-admin-layout.mjs"));
    var headerBarModule = File.ReadAllText(Path.Combine(generatedOutputRoot, "components/jazor-admin-tdesign-header-bar.mjs"));
    var dataTableModule = File.ReadAllText(Path.Combine(generatedOutputRoot, dataTableModulePath));
    var formModule = File.ReadAllText(Path.Combine(generatedOutputRoot, formModulePath));
    var noticeModule = File.ReadAllText(Path.Combine(generatedOutputRoot, noticeModulePath));
    var errorPageModule = File.ReadAllText(Path.Combine(generatedOutputRoot, errorPageModulePath));
    var manifest = File.ReadAllText(manifestPath);

    AssertContains(appModule, "defineComponent", "Vue component wrapper in JazorAdmin app module");
    AssertContains(appModule, "function $renderDirect()", "direct VNode render function in JazorAdmin app module");
    AssertContains(appModule, "JazorAdmin", "JazorAdmin app text in generated module");
    AssertContains(appModule, "useRoute", "Vue Router route injection in JazorAdmin app module");
    AssertContains(bootstrapModule, "createRouter", "Vue Router creation in JazorAdmin bootstrap module");
    AssertContains(bootstrapModule, "createWebHistory", "Vue Router web history in JazorAdmin bootstrap module");
    AssertContains(bootstrapModule, "RouterView", "Vue Router view in JazorAdmin bootstrap module");
    AssertContains(routeCatalogModule, "routeTarget", "strongly typed route target in admin route catalog module");
    AssertDoesNotContain(routeCatalogModule, "return path == null ? null : from(path)", "undefined union factory in admin route catalog module");
    AssertContains(adminLayoutModule, "data-shell-command", "sidebar toggle command in TDesign admin layout module");
    AssertContains(adminLayoutModule, "toggle-sidebar", "sidebar toggle command key in TDesign admin layout module");
    AssertContains(adminLayoutModule, "collapsedChanged", "controlled collapsed callback in TDesign admin layout module");
    AssertContains(adminLayoutModule, "horizontal: true", "top navigation variant in TDesign admin layout module");
    AssertContains(headerBarModule, "jazor-admin-tdesign-header__navigation", "navigation slot region in TDesign header bar module");
    AssertContains(dataTableModule, "aria-busy", "loading accessibility state in JazorAdmin release table module");
    AssertContains(dataTableModule, "jazor-admin-release-table__loading-row", "loading row in JazorAdmin release table module");
    AssertContains(dataTableModule, "jazor-admin-release-table__loading", "loading fallback in JazorAdmin release table module");
    AssertContains(appModule, "setTimeout", "Task.Delay async refresh lowering in JazorAdmin app module");
    AssertContains(formModule, "data-form-field", "field marker in JazorAdmin settings form module");
    AssertContains(formModule, "jazor-admin-settings-form__input", "text control in JazorAdmin settings form module");
    AssertContains(formModule, "jazor-admin-settings-form__select", "select control in JazorAdmin settings form module");
    AssertContains(formModule, "jazor-admin-settings-form__checkbox", "checkbox control in JazorAdmin settings form module");
    AssertContains(noticeModule, "data-notice-kind", "notice kind marker in JazorAdmin action notice module");
    AssertContains(noticeModule, "aria-live", "live-region semantics in JazorAdmin action notice module");
    AssertContains(noticeModule, "jazor-admin-action-notice--warning", "warning notice branch in JazorAdmin action notice module");
    AssertContains(noticeModule, "jazor-admin-action-notice--error", "error notice branch in JazorAdmin action notice module");
    AssertContains(noticeModule, "assertive", "assertive warning and error semantics in JazorAdmin action notice module");
    AssertContains(noticeModule, "Dismiss notice", "accessible dismiss command in JazorAdmin action notice module");
    AssertContains(errorPageModule, "data-error-kind", "typed error kind marker in JazorAdmin error page module");
    AssertContains(errorPageModule, "aria-labelledby", "error title accessibility relation in JazorAdmin error page module");
    AssertContains(errorPageModule, "aria-describedby", "error description accessibility relation in JazorAdmin error page module");
    AssertContains(errorPageModule, "data-error-action", "error recovery action in JazorAdmin error page module");
    foreach (var (relativePath, description) in componentModules)
        AssertContains(manifest, "\"" + relativePath + "\"", description + " manifest entry");
    AssertDoesNotContain(manifest, ".vue", "legacy SFC artifact in JazorAdmin manifest");
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
        File.Copy(
            Path.Combine(adminRoot, "wwwroot", "app.css"),
            Path.Combine(harnessRoot, "app.css"),
            overwrite: true);
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
                <link rel="stylesheet" href="/app.css">
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
                      throw new Error(`JazorAdmin router root did not mount for ${location.pathname}.`);
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
                    } else if (location.pathname === "/operations/releases") {
                      globalThis.__jazorAdminBrowserSmoke = {
                        ok: true,
                        mode: "deep-link",
                        pathname: location.pathname,
                        pageTitleText: document.querySelector('[data-page-region="title"]')?.textContent ?? "",
                        breadcrumbText: document.querySelector('[data-page-region="breadcrumb"]')?.textContent ?? "",
                        activeKey: document.querySelector("[data-navigation-selected-key]")?.getAttribute("data-navigation-selected-key") ?? "",
                        operationsExpanded: document.querySelector('[data-nav-key="operations"]')?.getAttribute("data-nav-expanded") === "true",
                        releaseQueueVisible: document.querySelector(".jazor-admin__release-section--focused") !== null
                      };
                    } else {
                    const text = document.body.textContent ?? "";
                    const initialDataTableRowCount = document.querySelectorAll(".jazor-admin-release-table__row").length;
                    const initialPageStatusText = document.querySelector(".jazor-admin-release-table__page-status")?.textContent ?? "";
                    const initialFirstReleaseName = document.querySelector('.jazor-admin-release-table__row .jazor-admin-release-table__cell[data-column-key="name"]')?.textContent ?? "";
                    const initialNoticePresent = document.querySelector(".jazor-admin-action-notice") !== null;
                    const initialSidebarItems = Array.from(document.querySelectorAll('[data-shell-region="sidebar"] [data-nav-kind]'))
                      .map((item) => item.textContent?.trim() ?? "");
                    const sidebarToggle = document.querySelector('[data-shell-command="toggle-sidebar"]');
                    const initialSidebarExpanded = sidebarToggle?.getAttribute("aria-expanded") ?? "";
                    sidebarToggle?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));
                    const shellCollapsed = document.querySelector('[data-shell-region="layout"]')?.getAttribute("data-shell-collapsed") === "true";
                    const collapsedSidebar = document.querySelector('[data-shell-region="sidebar"]');
                    let collapsedSidebarWidth = collapsedSidebar ? getComputedStyle(collapsedSidebar).width : "";
                    for (let attempt = 0; collapsedSidebar && collapsedSidebarWidth !== "64px" && attempt < 100; attempt++) {
                      await new Promise((resolve) => setTimeout(resolve, 10));
                      collapsedSidebarWidth = getComputedStyle(collapsedSidebar).width;
                    }
                    const collapsedSidebarExpanded = sidebarToggle?.getAttribute("aria-expanded") ?? "";
                    sidebarToggle?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));
                    const shellExpandedAgain = document.querySelector('[data-shell-region="layout"]')?.getAttribute("data-shell-collapsed") === "false";
                    document.querySelector('[data-action-key="refresh"]')
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    const refreshLoadingText = document.querySelector(".jazor-admin-release-table__loading")?.textContent ?? "";
                    const refreshLoadingRowCount = document.querySelectorAll(".jazor-admin-release-table__loading-row").length;
                    const refreshAriaBusy = document.querySelector(".jazor-admin-release-table")?.getAttribute("aria-busy") ?? "";
                    const refreshSearchDisabled = document.querySelector(".jazor-admin-release-table__search")?.hasAttribute("disabled") ?? false;
                    const refreshActionDisabled = document.querySelector('[data-action-key="refresh"]')?.hasAttribute("disabled") ?? false;
                    await new Promise((resolve) => setTimeout(resolve, 120));
                    await nextTick();

                    const afterRefreshNotice = document.querySelector(".jazor-admin-action-notice");
                    const afterRefreshActionStatusText = afterRefreshNotice?.textContent ?? "";
                    const afterRefreshNoticeKind = afterRefreshNotice?.getAttribute("data-notice-kind") ?? "";
                    const afterRefreshNoticeRole = afterRefreshNotice?.getAttribute("role") ?? "";
                    document.querySelector(".jazor-admin-action-notice__dismiss")?.click();
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));
                    const noticeDismissedAfterRefresh = document.querySelector(".jazor-admin-action-notice") === null;

                    const beforeToggleExpanded = document.querySelector('[data-nav-key="operations"]')?.getAttribute("data-nav-expanded") === "true";
                    document.querySelector('[data-nav-key="operations"] [data-nav-command="toggle"]')
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    const afterToggleExpanded = document.querySelector('[data-nav-key="operations"]')?.getAttribute("data-nav-expanded") === "true";

                    const serviceSortButton = document.querySelector('[data-column-key="name"] .jazor-admin-release-table__sort-button');
                    serviceSortButton
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    serviceSortButton
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    const afterSortFirstReleaseName = document.querySelector('.jazor-admin-release-table__row .jazor-admin-release-table__cell[data-column-key="name"]')?.textContent ?? "";
                    const serviceSortState = document.querySelector('[data-column-key="name"]')?.getAttribute("aria-sort") ?? "";

                    document.querySelector('[data-row-select-key="release.worker"]')
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    document.querySelector('[data-row-select-key="release.web"]')
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    document.querySelector('[data-row-key="release.web"]')
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    document.querySelector('[data-action-key="deploy"]')
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    const afterDeployNotice = document.querySelector(".jazor-admin-action-notice");
                    const afterDeployActionStatusText = afterDeployNotice?.textContent ?? "";
                    const afterDeployNoticeKind = afterDeployNotice?.getAttribute("data-notice-kind") ?? "";

                    document.querySelector(".jazor-admin-release-table__pagination .jazor-admin-release-table__page-button:last-child")
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    const afterNextDataTableRowCount = document.querySelectorAll(".jazor-admin-release-table__row").length;
                    const afterNextPageStatusText = document.querySelector(".jazor-admin-release-table__page-status")?.textContent ?? "";

                    const searchInput = document.querySelector(".jazor-admin-release-table__search");
                    if (searchInput instanceof HTMLInputElement) {
                      searchInput.value = "web";
                      searchInput.dispatchEvent(new InputEvent("input", { bubbles: true, cancelable: true, data: "web" }));
                    }
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    const releaseWebRow = document.querySelector('[data-row-key="release.web"]');
                    const releaseApiRow = document.querySelector('[data-row-key="release.api"]');
                    const releaseWorkerRow = document.querySelector('[data-row-key="release.worker"]');
                    const dataTableHeadingCount = document.querySelectorAll(".jazor-admin-release-table__heading").length;
                    const dataTableRowCount = document.querySelectorAll(".jazor-admin-release-table__row").length;
                    const tableSummaryText = document.querySelector(".jazor-admin-release-table__summary")?.textContent ?? "";
                    const selectedReleaseText = document.querySelector(".jazor-admin__selection")?.textContent ?? "";
                    const releaseWebSelected = releaseWebRow?.classList.contains("is-selected") ?? false;
                    const releaseApiSelected = releaseApiRow?.classList.contains("is-selected") ?? false;
                    const releaseWorkerVisible = releaseWorkerRow !== null;
                    const dashboardAppHtml = document.querySelector("#app")?.innerHTML ?? "";
                    const dashboardMetricCount = document.querySelectorAll(".jazor-admin__metric").length;

                    document.querySelector('[data-shell-region="sidebar"] [data-nav-key="workspace"]')
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    const workspaceItem = document.querySelector('[data-nav-key="workspace"]');
                    const dashboardItem = document.querySelector('[data-nav-key="dashboard"]');
                    const workspacePageTitleText = document.querySelector('[data-page-region="title"]')?.textContent ?? "";
                    const workspaceBreadcrumbText = document.querySelector('[data-page-region="breadcrumb"]')?.textContent ?? "";
                    const workspacePageHtml = document.querySelector('[data-page-region="body"]')?.innerHTML ?? "";
                    const workspaceSelectedAfterClick = workspaceItem?.getAttribute("data-nav-selected") === "true";
                    const dashboardSelectedAfterWorkspaceClick = dashboardItem?.getAttribute("data-nav-selected") === "true";
                    const workspacePathname = location.pathname;

                    document.querySelector('[data-shell-region="sidebar"] [data-nav-key="settings"]')
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    const settingsItem = document.querySelector('[data-nav-key="settings"]');
                    const settingsSelectedAfterClick = settingsItem?.getAttribute("data-nav-selected") === "true";
                    const settingsInitialStatusText = document.querySelector(".jazor-admin-settings-form__status")?.textContent ?? "";
                    const themeSelect = document.querySelector('[data-form-field="theme"]');
                    if (themeSelect instanceof HTMLSelectElement) {
                      themeSelect.value = "Dark";
                      themeSelect.dispatchEvent(new Event("change", { bubbles: true, cancelable: true }));
                    }
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    const navigationModeSelect = document.querySelector('[data-form-field="navigation-mode"]');
                    if (navigationModeSelect instanceof HTMLSelectElement) {
                      navigationModeSelect.value = "Header";
                      navigationModeSelect.dispatchEvent(new Event("change", { bubbles: true, cancelable: true }));
                    }
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    const topLayoutActive = document.querySelector('[data-shell-region="layout"]')?.getAttribute("data-shell-layout") === "top";
                    const topLayoutSidebarAbsent = document.querySelector('[data-shell-region="sidebar"]') === null;
                    const topNavigationLinkCount = document.querySelectorAll('[data-shell-region="navigation"] [data-nav-kind="item"]').length;
                    const topNavigationHtml = document.querySelector('[data-shell-region="navigation"]')?.innerHTML ?? "";
                    document.querySelector('[data-shell-region="navigation"] [data-nav-key="operations.audit"]')
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    for (let attempt = 0; attempt < 100 && location.pathname !== "/operations/audit"; attempt++) {
                      await new Promise((resolve) => setTimeout(resolve, 10));
                    }
                    await nextTick();
                    const topNavigationAuditTitle = document.querySelector('[data-page-region="title"]')?.textContent ?? "";
                    const topNavigationAuditActiveKey = document.querySelector('[data-shell-region="navigation"] [data-navigation-selected-key]')?.getAttribute("data-navigation-selected-key") ?? "";
                    document.querySelector('[data-shell-region="navigation"] [data-nav-key="settings"]')
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    for (let attempt = 0; attempt < 100 && location.pathname !== "/settings"; attempt++) {
                      await new Promise((resolve) => setTimeout(resolve, 10));
                    }
                    await nextTick();

                    const sidebarModeSelect = document.querySelector('[data-form-field="navigation-mode"]');
                    if (sidebarModeSelect instanceof HTMLSelectElement) {
                      sidebarModeSelect.value = "Sidebar";
                      sidebarModeSelect.dispatchEvent(new Event("change", { bubbles: true, cancelable: true }));
                    }
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));
                    const sidebarLayoutActive = document.querySelector('[data-shell-region="layout"]')?.getAttribute("data-shell-layout") === "sidebar";
                    const topNavigationAbsentAfterSidebar = document.querySelector('[data-shell-region="navigation"]') === null;

                    const releaseChannelInput = document.querySelector('[data-form-field="release-channel"]');
                    if (releaseChannelInput instanceof HTMLInputElement) {
                      releaseChannelInput.value = "beta";
                      releaseChannelInput.dispatchEvent(new Event("input", { bubbles: true, cancelable: true }));
                    }
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    const smokeRequiredCheckbox = document.querySelector('[data-form-field="smoke-required"]');
                    smokeRequiredCheckbox
                      ?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    document.querySelector('[data-form-action="submit"]')?.click();
                    await nextTick();
                    await new Promise((resolve) => setTimeout(resolve, 0));

                    const settingsPageHtml = document.querySelector('[data-page-region="body"]')?.innerHTML ?? "";
                    const settingsPageTitleText = document.querySelector('[data-page-region="title"]')?.textContent ?? "";
                    const settingsBreadcrumbText = document.querySelector('[data-page-region="breadcrumb"]')?.textContent ?? "";
                    const settingsStatusText = document.querySelector(".jazor-admin-settings-form__status")?.textContent ?? "";
                    const settingsPathname = location.pathname;

                    history.back();
                    for (let attempt = 0; attempt < 100 && location.pathname !== "/operations/audit"; attempt++) {
                      await new Promise((resolve) => setTimeout(resolve, 10));
                    }
                    await nextTick();
                    const historyBackPathname = location.pathname;
                    const historyBackPageTitleText = document.querySelector('[data-page-region="title"]')?.textContent ?? "";
                    const historyBackActiveKey = document.querySelector("[data-navigation-selected-key]")?.getAttribute("data-navigation-selected-key") ?? "";

                    history.forward();
                    for (let attempt = 0; attempt < 100 && location.pathname !== "/settings"; attempt++) {
                      await new Promise((resolve) => setTimeout(resolve, 10));
                    }
                    await nextTick();
                    const historyForwardPathname = location.pathname;
                    const historyForwardPageTitleText = document.querySelector('[data-page-region="title"]')?.textContent ?? "";
                    const historyForwardActiveKey = document.querySelector("[data-navigation-selected-key]")?.getAttribute("data-navigation-selected-key") ?? "";
                    const settingsFieldCount = document.querySelectorAll(".jazor-admin-settings-form__field, .jazor-admin-settings-form__checkbox-field").length;
                    const preferenceControlCount = document.querySelectorAll("[data-preference]").length;
                    const activeKeyAfterSettings = document.querySelector("[data-navigation-selected-key]")?.getAttribute("data-navigation-selected-key") ?? "";

                    const globalThemeSelect = document.querySelector('select[data-preference="theme"]');
                    if (globalThemeSelect instanceof HTMLSelectElement) {
                      globalThemeSelect.value = "dark";
                      globalThemeSelect.dispatchEvent(new Event("change", { bubbles: true }));
                    }
                    const globalLanguageSelect = document.querySelector('select[data-preference="language"]');
                    if (globalLanguageSelect instanceof HTMLSelectElement) {
                      globalLanguageSelect.value = "zh-CN";
                      globalLanguageSelect.dispatchEvent(new Event("change", { bubbles: true }));
                    }
                    document.querySelector('input[data-preference="grayscale"]')?.click();
                    await nextTick();
                    const preferenceTheme = document.querySelector(".jazor-admin-application")?.getAttribute("data-theme") ?? "";
                    const preferenceLanguage = document.querySelector(".jazor-admin-application")?.getAttribute("lang") ?? "";
                    const preferenceGrayscale = document.querySelector(".jazor-admin-application")?.getAttribute("data-grayscale") ?? "";
                    const localizedSettingsTitle = document.querySelector('[data-page-region="title"]')?.textContent ?? "";
                    const localizedSidebarToggleLabel = document.querySelector('[data-shell-command="toggle-sidebar"]')?.getAttribute("aria-label") ?? "";

                    document.querySelector('[data-access-command="lock"]')?.click();
                    for (let attempt = 0; attempt < 100 && location.pathname !== "/lock"; attempt++) await new Promise((resolve) => setTimeout(resolve, 10));
                    await nextTick();
                    const lockTitle = document.querySelector(".jazor-admin-access h1")?.textContent ?? "";
                    const unlockInput = document.querySelector('input[data-access-field="password"]');
                    if (unlockInput instanceof HTMLInputElement) {
                      unlockInput.value = "unlock";
                      unlockInput.dispatchEvent(new InputEvent("input", { bubbles: true, data: "unlock" }));
                    }
                    document.querySelector('[data-access-submit="lock"]')?.click();
                    for (let attempt = 0; attempt < 100 && location.pathname !== "/"; attempt++) await new Promise((resolve) => setTimeout(resolve, 10));
                    await nextTick();
                    const unlockPathname = location.pathname;

                    document.querySelector('[data-access-command="sign-out"]')?.click();
                    for (let attempt = 0; attempt < 100 && location.pathname !== "/login"; attempt++) await new Promise((resolve) => setTimeout(resolve, 10));
                    await nextTick();
                    const loginTitle = document.querySelector(".jazor-admin-access h1")?.textContent ?? "";
                    const loginPasswordInput = document.querySelector('input[data-access-field="password"]');
                    if (loginPasswordInput instanceof HTMLInputElement) {
                      loginPasswordInput.value = "login";
                      loginPasswordInput.dispatchEvent(new InputEvent("input", { bubbles: true, data: "login" }));
                    }
                    document.querySelector('[data-access-submit="login"]')?.click();
                    for (let attempt = 0; attempt < 100 && location.pathname !== "/"; attempt++) await new Promise((resolve) => setTimeout(resolve, 10));
                    await nextTick();
                    const loginPathname = location.pathname;
                    const payload = {
                      ok: true,
                      text,
                      appHtml: document.querySelector("#app")?.innerHTML ?? "",
                      dashboardAppHtml,
                      currentText: document.body.textContent ?? "",
                      workspacePageTitleText,
                      workspaceBreadcrumbText,
                      workspacePageHtml,
                      workspacePathname,
                      settingsPageTitleText,
                      settingsBreadcrumbText,
                      settingsPageHtml,
                      settingsPathname,
                      historyBackPathname,
                      historyBackPageTitleText,
                      historyBackActiveKey,
                      historyForwardPathname,
                      historyForwardPageTitleText,
                      historyForwardActiveKey,
                      preferenceTheme,
                      preferenceLanguage,
                      preferenceGrayscale,
                      localizedSettingsTitle,
                      lockTitle,
                      unlockPathname,
                      loginTitle,
                      loginPathname,
                      metricCount: dashboardMetricCount,
                      settingsFieldCount,
                      preferenceControlCount,
                      userText: document.querySelector(".jazor-admin__user")?.textContent ?? "",
                      sidebarItems: initialSidebarItems,
                      hasLegacyVueReference: Array.from(document.scripts).some((script) => (script.getAttribute("src") ?? "").includes(".vue")),
                      initialSidebarExpanded,
                      shellCollapsed,
                      collapsedSidebarWidth,
                      collapsedSidebarExpanded,
                      shellExpandedAgain,
                      localizedSidebarToggleLabel,
                      beforeToggleExpanded,
                      afterToggleExpanded,
                      workspaceSelected: workspaceSelectedAfterClick,
                      settingsSelected: settingsSelectedAfterClick,
                      dashboardSelected: dashboardSelectedAfterWorkspaceClick,
                      activeKey: activeKeyAfterSettings,
                      dataTableHeadingCount,
                      dataTableRowCount,
                      initialDataTableRowCount,
                      initialPageStatusText,
                      initialFirstReleaseName,
                      initialNoticePresent,
                      refreshLoadingText,
                      refreshLoadingRowCount,
                      refreshAriaBusy,
                      refreshSearchDisabled,
                      refreshActionDisabled,
                      afterRefreshActionStatusText,
                      afterRefreshNoticeKind,
                      afterRefreshNoticeRole,
                      noticeDismissedAfterRefresh,
                      afterDeployActionStatusText,
                      afterDeployNoticeKind,
                      afterSortFirstReleaseName,
                      serviceSortState,
                      afterNextDataTableRowCount,
                      afterNextPageStatusText,
                      searchValue: searchInput instanceof HTMLInputElement ? searchInput.value : "",
                      tableSummaryText,
                      releaseWebSelected,
                      releaseApiSelected,
                      releaseWorkerVisible,
                      selectedReleaseText,
                      settingsInitialStatusText,
                      settingsStatusText,
                      settingsThemeValue: themeSelect instanceof HTMLSelectElement ? themeSelect.value : "",
                      settingsNavigationModeValue: sidebarModeSelect instanceof HTMLSelectElement ? sidebarModeSelect.value : "",
                      settingsReleaseChannelValue: releaseChannelInput instanceof HTMLInputElement ? releaseChannelInput.value : "",
                      settingsSmokeRequiredChecked: smokeRequiredCheckbox instanceof HTMLInputElement ? smokeRequiredCheckbox.checked : true,
                      topLayoutActive,
                      topLayoutSidebarAbsent,
                      topNavigationLinkCount,
                      topNavigationHtml,
                      topNavigationAuditTitle,
                      topNavigationAuditActiveKey,
                      sidebarLayoutActive,
                      topNavigationAbsentAfterSidebar,
                      injectSmoke
                    };
                    globalThis.__jazorAdminBrowserSmoke = payload;
                    }
                  } catch (error) {
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

        var desktopLayout = root.GetProperty("desktopLayout");
        AssertContains(desktopLayout.GetProperty("shellDisplay").GetString() ?? string.Empty, "flex", "JazorAdmin desktop TDesign shell layout", desktopLayout.GetRawText());
        AssertContains(desktopLayout.GetProperty("shellDirection").GetString() ?? string.Empty, "row", "JazorAdmin desktop TDesign shell direction", desktopLayout.GetRawText());
        AssertContains(desktopLayout.GetProperty("sidebarWidth").GetString() ?? string.Empty, "240px", "JazorAdmin desktop TDesign sidebar width", desktopLayout.GetRawText());
        AssertContains(desktopLayout.GetProperty("mainDisplay").GetString() ?? string.Empty, "flex", "JazorAdmin desktop TDesign main layout", desktopLayout.GetRawText());
        AssertContains(desktopLayout.GetProperty("tableOverflowX").GetString() ?? string.Empty, "auto", "JazorAdmin desktop table overflow containment", desktopLayout.GetRawText());
        AssertJsonBoolean(desktopLayout, "stylesheetLoaded", true, "JazorAdmin desktop stylesheet load", desktopLayout.GetRawText());
        AssertJsonBoolean(desktopLayout, "documentFitsViewport", true, "JazorAdmin desktop document width", desktopLayout.GetRawText());
        AssertJsonBoolean(desktopLayout, "sidebarBeforeMain", true, "JazorAdmin desktop sidebar and main geometry", desktopLayout.GetRawText());

        var mobileLayout = root.GetProperty("mobileLayout");
        AssertJsonInt(mobileLayout, "viewportWidth", 390, "JazorAdmin mobile viewport width", mobileLayout.GetRawText());
        AssertContains(mobileLayout.GetProperty("shellDisplay").GetString() ?? string.Empty, "flex", "JazorAdmin mobile TDesign shell layout", mobileLayout.GetRawText());
        AssertContains(mobileLayout.GetProperty("shellDirection").GetString() ?? string.Empty, "column", "JazorAdmin mobile TDesign shell direction", mobileLayout.GetRawText());
        AssertJsonBoolean(mobileLayout, "sidebarFillsShell", true, "JazorAdmin mobile TDesign sidebar width", mobileLayout.GetRawText());
        AssertContains(mobileLayout.GetProperty("navigationDisplay").GetString() ?? string.Empty, "flex", "JazorAdmin mobile TDesign navigation layout", mobileLayout.GetRawText());
        AssertContains(mobileLayout.GetProperty("tableOverflowX").GetString() ?? string.Empty, "auto", "JazorAdmin mobile table overflow containment", mobileLayout.GetRawText());
        AssertJsonBoolean(mobileLayout, "stylesheetLoaded", true, "JazorAdmin mobile stylesheet load", mobileLayout.GetRawText());
        AssertJsonBoolean(mobileLayout, "documentFitsViewport", true, "JazorAdmin mobile document width", mobileLayout.GetRawText());
        AssertJsonBoolean(mobileLayout, "sidebarBeforeMain", true, "JazorAdmin mobile sidebar and main geometry", mobileLayout.GetRawText());

        var mobileNavigation = root.GetProperty("mobileNavigation");
        AssertJsonBoolean(mobileNavigation, "auditVisible", true, "JazorAdmin mobile child navigation visibility", mobileNavigation.GetRawText());
        AssertJsonInt(mobileNavigation, "childLinkCount", 2, "JazorAdmin mobile child navigation link count", mobileNavigation.GetRawText());
        AssertContains(mobileNavigation.GetProperty("pathname").GetString() ?? string.Empty, "/operations/audit", "JazorAdmin mobile child navigation location", mobileNavigation.GetRawText());
        AssertContains(mobileNavigation.GetProperty("pageTitleText").GetString() ?? string.Empty, "Audit Log", "JazorAdmin mobile child navigation page", mobileNavigation.GetRawText());
        AssertContains(mobileNavigation.GetProperty("activeKey").GetString() ?? string.Empty, "operations.audit", "JazorAdmin mobile child navigation selected key", mobileNavigation.GetRawText());
        AssertJsonBoolean(mobileNavigation, "documentFitsViewport", true, "JazorAdmin mobile child navigation document width", mobileNavigation.GetRawText());

        var serverDiagnostics = root.GetProperty("serverDiagnostics").EnumerateArray()
            .Select(static entry => entry.GetString() ?? string.Empty)
            .ToArray();
        if (!serverDiagnostics.Any(static entry => entry == "200 /app.css"))
            throw new InvalidOperationException("JazorAdmin browser smoke did not load /app.css successfully." + Environment.NewLine + root.GetRawText());

        var text = root.GetProperty("text").GetString() ?? string.Empty;
        AssertContains(text, "JazorAdmin", "JazorAdmin browser DOM title", root.GetRawText());
        AssertContains(text, "Dashboard", "JazorAdmin browser DOM page title", root.GetRawText());
        AssertContains(text, "Direct VNode", "JazorAdmin browser DOM direct VNode metric", root.GetRawText());
        AssertContains(text, "TDesign", "JazorAdmin browser DOM TDesign metric", root.GetRawText());
        AssertContains(text, "integration", "JazorAdmin browser DOM integration metric", root.GetRawText());
        AssertContains(text, "Release Queue", "JazorAdmin browser DOM release queue title", root.GetRawText());
        AssertContains(text, "Admin Web", "JazorAdmin browser DOM release table row", root.GetRawText());
        AssertContains(root.GetProperty("dashboardAppHtml").GetString() ?? string.Empty, "Filter releases", "JazorAdmin browser DOM data table search placeholder", root.GetRawText());
        AssertContains(root.GetProperty("workspacePageTitleText").GetString() ?? string.Empty, "Workspace", "JazorAdmin workspace page title after sidebar navigation", root.GetRawText());
        AssertContains(root.GetProperty("workspaceBreadcrumbText").GetString() ?? string.Empty, "Workspace", "JazorAdmin workspace breadcrumb after sidebar navigation", root.GetRawText());
        AssertContains(root.GetProperty("workspacePageHtml").GetString() ?? string.Empty, "Operator Workspace", "JazorAdmin workspace content after sidebar navigation", root.GetRawText());
        AssertDoesNotContain(root.GetProperty("workspacePageHtml").GetString() ?? string.Empty, "jazor-admin-release-table", "dashboard data table in workspace page body");
        AssertContains(root.GetProperty("workspacePathname").GetString() ?? string.Empty, "/workspace", "JazorAdmin workspace browser location", root.GetRawText());
        AssertContains(root.GetProperty("settingsPageTitleText").GetString() ?? string.Empty, "Settings", "JazorAdmin settings page title after sidebar navigation", root.GetRawText());
        AssertContains(root.GetProperty("settingsBreadcrumbText").GetString() ?? string.Empty, "Settings", "JazorAdmin settings breadcrumb after sidebar navigation", root.GetRawText());
        AssertContains(root.GetProperty("settingsPageHtml").GetString() ?? string.Empty, "Application Settings", "JazorAdmin settings page body", root.GetRawText());
        AssertContains(root.GetProperty("settingsPathname").GetString() ?? string.Empty, "/settings", "JazorAdmin settings browser location", root.GetRawText());
        AssertContains(root.GetProperty("settingsInitialStatusText").GetString() ?? string.Empty, "No settings have been saved.", "JazorAdmin initial settings status", root.GetRawText());
        AssertContains(root.GetProperty("settingsStatusText").GetString() ?? string.Empty, "Saved settings 1: Dark, smoke optional, Sidebar, beta", "JazorAdmin saved settings status", root.GetRawText());
        AssertContains(root.GetProperty("settingsThemeValue").GetString() ?? string.Empty, "Dark", "JazorAdmin settings theme select value", root.GetRawText());
        AssertContains(root.GetProperty("settingsNavigationModeValue").GetString() ?? string.Empty, "Sidebar", "JazorAdmin settings navigation mode select value", root.GetRawText());
        AssertJsonBoolean(root, "topLayoutActive", true, "JazorAdmin top layout mode", root.GetRawText());
        AssertJsonBoolean(root, "topLayoutSidebarAbsent", true, "JazorAdmin top layout sidebar removal", root.GetRawText());
        AssertJsonInt(root, "topNavigationLinkCount", 5, "JazorAdmin top navigation link count", root.GetRawText());
        AssertContains(root.GetProperty("topNavigationAuditTitle").GetString() ?? string.Empty, "Audit Log", "JazorAdmin top navigation route", root.GetRawText());
        AssertContains(root.GetProperty("topNavigationAuditActiveKey").GetString() ?? string.Empty, "operations.audit", "JazorAdmin top navigation selected key", root.GetRawText());
        AssertJsonBoolean(root, "sidebarLayoutActive", true, "JazorAdmin sidebar layout mode", root.GetRawText());
        AssertJsonBoolean(root, "topNavigationAbsentAfterSidebar", true, "JazorAdmin top navigation removal in sidebar mode", root.GetRawText());
        AssertContains(root.GetProperty("settingsReleaseChannelValue").GetString() ?? string.Empty, "beta", "JazorAdmin settings release channel input value", root.GetRawText());
        AssertJsonBoolean(root, "settingsSmokeRequiredChecked", false, "JazorAdmin settings smoke-required checkbox state", root.GetRawText());
        AssertContains(root.GetProperty("historyBackPathname").GetString() ?? string.Empty, "/operations/audit", "JazorAdmin browser history back location", root.GetRawText());
        AssertContains(root.GetProperty("historyBackPageTitleText").GetString() ?? string.Empty, "Audit Log", "JazorAdmin browser history back page", root.GetRawText());
        AssertContains(root.GetProperty("historyBackActiveKey").GetString() ?? string.Empty, "operations.audit", "JazorAdmin browser history back selected key", root.GetRawText());
        AssertContains(root.GetProperty("historyForwardPathname").GetString() ?? string.Empty, "/settings", "JazorAdmin browser history forward location", root.GetRawText());
        AssertContains(root.GetProperty("historyForwardPageTitleText").GetString() ?? string.Empty, "Settings", "JazorAdmin browser history forward page", root.GetRawText());
        AssertContains(root.GetProperty("historyForwardActiveKey").GetString() ?? string.Empty, "settings", "JazorAdmin browser history forward selected key", root.GetRawText());
        AssertContains(root.GetProperty("userText").GetString() ?? string.Empty, "admin@jazor", "JazorAdmin browser user region", root.GetRawText());
        AssertJsonInt(root, "metricCount", 3, "JazorAdmin browser metric card count", root.GetRawText());
        AssertJsonInt(root, "settingsFieldCount", 4, "JazorAdmin settings field count", root.GetRawText());
        AssertJsonInt(root, "preferenceControlCount", 3, "JazorAdmin global preference control count", root.GetRawText());
        AssertContains(root.GetProperty("initialSidebarExpanded").GetString() ?? string.Empty, "true", "JazorAdmin initial sidebar expanded state", root.GetRawText());
        AssertJsonBoolean(root, "shellCollapsed", true, "JazorAdmin controlled sidebar collapse", root.GetRawText());
        AssertContains(root.GetProperty("collapsedSidebarWidth").GetString() ?? string.Empty, "64px", "JazorAdmin collapsed TDesign sidebar width", root.GetRawText());
        AssertContains(root.GetProperty("collapsedSidebarExpanded").GetString() ?? string.Empty, "false", "JazorAdmin collapsed sidebar aria state", root.GetRawText());
        AssertJsonBoolean(root, "shellExpandedAgain", true, "JazorAdmin controlled sidebar restore", root.GetRawText());
        AssertContains(root.GetProperty("preferenceTheme").GetString() ?? string.Empty, "dark", "JazorAdmin global dark theme state", root.GetRawText());
        AssertContains(root.GetProperty("preferenceLanguage").GetString() ?? string.Empty, "zh-CN", "JazorAdmin global language state", root.GetRawText());
        AssertContains(root.GetProperty("preferenceGrayscale").GetString() ?? string.Empty, "true", "JazorAdmin global grayscale state", root.GetRawText());
        AssertContains(root.GetProperty("localizedSettingsTitle").GetString() ?? string.Empty, "设置", "JazorAdmin localized page title", root.GetRawText());
        AssertContains(root.GetProperty("localizedSidebarToggleLabel").GetString() ?? string.Empty, "收起侧边栏", "JazorAdmin localized sidebar toggle label", root.GetRawText());
        AssertContains(root.GetProperty("lockTitle").GetString() ?? string.Empty, "会话已锁定", "JazorAdmin lock screen title", root.GetRawText());
        AssertContains(root.GetProperty("unlockPathname").GetString() ?? string.Empty, "/", "JazorAdmin unlock navigation", root.GetRawText());
        AssertContains(root.GetProperty("loginTitle").GetString() ?? string.Empty, "登录 JazorAdmin", "JazorAdmin login screen title", root.GetRawText());
        AssertContains(root.GetProperty("loginPathname").GetString() ?? string.Empty, "/", "JazorAdmin login navigation", root.GetRawText());
        AssertJsonInt(root, "dataTableHeadingCount", 5, "JazorAdmin browser data table heading count", root.GetRawText());
        AssertJsonInt(root, "initialDataTableRowCount", 2, "JazorAdmin browser initial paged data table row count", root.GetRawText());
        AssertContains(root.GetProperty("initialPageStatusText").GetString() ?? string.Empty, "1 / 2", "JazorAdmin initial data table page status", root.GetRawText());
        AssertContains(root.GetProperty("initialFirstReleaseName").GetString() ?? string.Empty, "Admin API", "JazorAdmin initial data table first service", root.GetRawText());
        AssertContains(root.GetProperty("afterRefreshActionStatusText").GetString() ?? string.Empty, "Refreshed Dashboard", "JazorAdmin refresh action status", root.GetRawText());
        AssertContains(root.GetProperty("afterRefreshActionStatusText").GetString() ?? string.Empty, "Refreshes: 1", "JazorAdmin refresh action count", root.GetRawText());
        AssertContains(root.GetProperty("refreshLoadingText").GetString() ?? string.Empty, "Refreshing releases", "JazorAdmin refresh loading text", root.GetRawText());
        AssertJsonInt(root, "refreshLoadingRowCount", 1, "JazorAdmin refresh loading row count", root.GetRawText());
        AssertContains(root.GetProperty("refreshAriaBusy").GetString() ?? string.Empty, "true", "JazorAdmin refresh aria-busy state", root.GetRawText());
        AssertJsonBoolean(root, "refreshSearchDisabled", true, "JazorAdmin refresh search disabled state", root.GetRawText());
        AssertJsonBoolean(root, "refreshActionDisabled", true, "JazorAdmin refresh action disabled state", root.GetRawText());
        AssertContains(root.GetProperty("afterRefreshNoticeKind").GetString() ?? string.Empty, "success", "JazorAdmin refresh notice kind", root.GetRawText());
        AssertContains(root.GetProperty("afterRefreshNoticeRole").GetString() ?? string.Empty, "status", "JazorAdmin refresh notice role", root.GetRawText());
        AssertJsonBoolean(root, "initialNoticePresent", false, "JazorAdmin initial notice state", root.GetRawText());
        AssertJsonBoolean(root, "noticeDismissedAfterRefresh", true, "JazorAdmin dismissed refresh notice state", root.GetRawText());
        AssertContains(root.GetProperty("afterDeployActionStatusText").GetString() ?? string.Empty, "Bulk deploy requested for 2 releases", "JazorAdmin deploy action status", root.GetRawText());
        AssertContains(root.GetProperty("afterDeployActionStatusText").GetString() ?? string.Empty, "Deploys: 1", "JazorAdmin deploy action count", root.GetRawText());
        AssertContains(root.GetProperty("afterDeployNoticeKind").GetString() ?? string.Empty, "success", "JazorAdmin deploy notice kind", root.GetRawText());
        AssertContains(root.GetProperty("afterSortFirstReleaseName").GetString() ?? string.Empty, "Audit Worker", "JazorAdmin sorted data table first service", root.GetRawText());
        AssertContains(root.GetProperty("serviceSortState").GetString() ?? string.Empty, "descending", "JazorAdmin service column sort state", root.GetRawText());
        AssertJsonInt(root, "afterNextDataTableRowCount", 1, "JazorAdmin browser next-page data table row count", root.GetRawText());
        AssertContains(root.GetProperty("afterNextPageStatusText").GetString() ?? string.Empty, "2 / 2", "JazorAdmin next data table page status", root.GetRawText());
        AssertJsonInt(root, "dataTableRowCount", 1, "JazorAdmin browser filtered data table row count", root.GetRawText());
        AssertContains(root.GetProperty("searchValue").GetString() ?? string.Empty, "web", "JazorAdmin data table search input value", root.GetRawText());
        AssertContains(root.GetProperty("tableSummaryText").GetString() ?? string.Empty, "1 rows", "JazorAdmin filtered data table summary", root.GetRawText());
        AssertJsonBoolean(root, "beforeToggleExpanded", false, "JazorAdmin sidebar initial collapsed branch state", root.GetRawText());
        AssertJsonBoolean(root, "afterToggleExpanded", true, "JazorAdmin sidebar expanded after branch click", root.GetRawText());
        AssertJsonBoolean(root, "workspaceSelected", true, "JazorAdmin workspace item selected after click", root.GetRawText());
        AssertJsonBoolean(root, "settingsSelected", true, "JazorAdmin settings item selected after click", root.GetRawText());
        AssertJsonBoolean(root, "dashboardSelected", false, "JazorAdmin dashboard item deselected after workspace click", root.GetRawText());
        AssertContains(root.GetProperty("activeKey").GetString() ?? string.Empty, "settings", "JazorAdmin active sidebar key after settings click", root.GetRawText());
        AssertJsonBoolean(root, "releaseWebSelected", true, "JazorAdmin release.web row selected after click", root.GetRawText());
        AssertJsonBoolean(root, "releaseApiSelected", false, "JazorAdmin release.api row deselected after release.web click", root.GetRawText());
        AssertJsonBoolean(root, "releaseWorkerVisible", false, "JazorAdmin release.worker row hidden after search", root.GetRawText());
        AssertContains(root.GetProperty("selectedReleaseText").GetString() ?? string.Empty, "release.web", "JazorAdmin selected release text after table click", root.GetRawText());
        AssertContains(root.GetProperty("selectedReleaseText").GetString() ?? string.Empty, "Bulk selected: 2", "JazorAdmin selected release bulk count after table checkbox clicks", root.GetRawText());

        var deepLink = root.GetProperty("deepLink");
        AssertContains(deepLink.GetProperty("mode").GetString() ?? string.Empty, "deep-link", "JazorAdmin deep-link smoke mode", deepLink.GetRawText());
        AssertContains(deepLink.GetProperty("pathname").GetString() ?? string.Empty, "/operations/releases", "JazorAdmin deep-link browser location", deepLink.GetRawText());
        AssertContains(deepLink.GetProperty("pageTitleText").GetString() ?? string.Empty, "Releases", "JazorAdmin deep-link page title", deepLink.GetRawText());
        AssertContains(deepLink.GetProperty("breadcrumbText").GetString() ?? string.Empty, "Operations", "JazorAdmin deep-link parent breadcrumb", deepLink.GetRawText());
        AssertContains(deepLink.GetProperty("activeKey").GetString() ?? string.Empty, "operations.releases", "JazorAdmin deep-link selected key", deepLink.GetRawText());
        AssertJsonBoolean(deepLink, "operationsExpanded", true, "JazorAdmin deep-link expanded ancestor", deepLink.GetRawText());
        AssertJsonBoolean(deepLink, "releaseQueueVisible", true, "JazorAdmin deep-link release page body", deepLink.GetRawText());

        var internalError = root.GetProperty("internalError");
        AssertContains(internalError.GetProperty("mode").GetString() ?? string.Empty, "error", "JazorAdmin internal-error smoke mode", internalError.GetRawText());
        AssertContains(internalError.GetProperty("pathname").GetString() ?? string.Empty, "/error/500", "JazorAdmin internal-error browser location", internalError.GetRawText());
        AssertContains(internalError.GetProperty("errorKind").GetString() ?? string.Empty, "internal-server-error", "JazorAdmin internal-error kind", internalError.GetRawText());
        AssertContains(internalError.GetProperty("errorCode").GetString() ?? string.Empty, "500", "JazorAdmin internal-error code", internalError.GetRawText());
        AssertContains(internalError.GetProperty("errorTitle").GetString() ?? string.Empty, "Something went wrong", "JazorAdmin internal-error title", internalError.GetRawText());
        AssertContains(internalError.GetProperty("errorRole").GetString() ?? string.Empty, "alert", "JazorAdmin internal-error live role", internalError.GetRawText());
        AssertContains(internalError.GetProperty("returnPathname").GetString() ?? string.Empty, "/", "JazorAdmin internal-error recovery navigation", internalError.GetRawText());

        var notFound = root.GetProperty("notFound");
        AssertContains(notFound.GetProperty("mode").GetString() ?? string.Empty, "error", "JazorAdmin not-found smoke mode", notFound.GetRawText());
        AssertContains(notFound.GetProperty("pathname").GetString() ?? string.Empty, "/missing/admin/page", "JazorAdmin preserved unknown browser location", notFound.GetRawText());
        AssertContains(notFound.GetProperty("errorKind").GetString() ?? string.Empty, "not-found", "JazorAdmin not-found kind", notFound.GetRawText());
        AssertContains(notFound.GetProperty("errorCode").GetString() ?? string.Empty, "404", "JazorAdmin not-found code", notFound.GetRawText());
        AssertContains(notFound.GetProperty("errorTitle").GetString() ?? string.Empty, "Page not found", "JazorAdmin not-found title", notFound.GetRawText());
        AssertContains(notFound.GetProperty("errorRole").GetString() ?? string.Empty, "status", "JazorAdmin not-found status role", notFound.GetRawText());
        AssertContains(notFound.GetProperty("returnPathname").GetString() ?? string.Empty, "/", "JazorAdmin not-found recovery navigation", notFound.GetRawText());
        if (root.GetProperty("hasLegacyVueReference").GetBoolean())
            throw new InvalidOperationException("JazorAdmin browser smoke found a legacy .vue script reference.");

        var sidebarItems = root.GetProperty("sidebarItems").EnumerateArray()
            .Select(static item => item.GetString() ?? string.Empty)
            .ToArray();
        if (!sidebarItems.Any(static item => item.Contains("Dashboard", StringComparison.Ordinal)) ||
            !sidebarItems.Any(static item => item.Contains("Operations", StringComparison.Ordinal)) ||
            !sidebarItems.Any(static item => item.Contains("Settings", StringComparison.Ordinal)) ||
            !sidebarItems.Any(static item => item.Contains("Workspace", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("JazorAdmin browser smoke did not find expected sidebar navigation items.");
        }
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
              await page.navigate(`http://127.0.0.1:${server.port}/operations/releases`);
              const deepLink = await page.waitForSmoke();
              await page.send("Emulation.setDeviceMetricsOverride", {
                width: 390,
                height: 844,
                deviceScaleFactor: 1,
                mobile: false
              });
              await page.navigate(`http://127.0.0.1:${server.port}/operations/releases`);
              await page.waitForSmoke();
              result.mobileLayout = await page.readLayout();
              result.mobileNavigation = await page.exerciseMobileChildNavigation();
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
              const table = document.querySelector(".jazor-admin-release-table__table");
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
                tableOverflowX: table ? getComputedStyle(table).overflowX : "",
                stylesheetLoaded: Array.from(document.styleSheets).some((sheet) => sheet.href?.endsWith("/app.css")),
                documentFitsViewport: document.documentElement.scrollWidth <= innerWidth,
                sidebarBeforeMain: sidebarRect && mainRect
                  ? sidebarRect.right <= mainRect.left + 1 || sidebarRect.bottom <= mainRect.top + 1
                  : false
              };
            })()`);
          }

          exerciseMobileChildNavigation() {
            return this.evaluate(`(async () => {
              const branch = document.querySelector('[data-nav-key="operations"]');
              const auditLink = branch?.querySelector('[data-nav-key="operations.audit"]');
              const childLinkCount = branch?.querySelectorAll('[data-nav-kind="item"]').length ?? 0;
              const auditRect = auditLink?.getBoundingClientRect();
              const auditVisible = !!auditRect && auditRect.width > 0 && auditRect.height > 0;
              auditLink?.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
              for (let attempt = 0; attempt < 100 && location.pathname !== "/operations/audit"; attempt++) {
                await new Promise((resolve) => setTimeout(resolve, 10));
              }
              await new Promise((resolve) => setTimeout(resolve, 0));
              return {
                auditVisible,
                childLinkCount,
                pathname: location.pathname,
                pageTitleText: document.querySelector('[data-page-region="title"]')?.textContent ?? "",
                activeKey: document.querySelector("[data-navigation-selected-key]")?.getAttribute("data-navigation-selected-key") ?? "",
                documentFitsViewport: document.documentElement.scrollWidth <= innerWidth
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

        return new SmokeOptions(configuration, baseOutputPath, baseIntermediateOutputPath, generatedOutputRoot, frontendOnly, skipBrowser);
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
