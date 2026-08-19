const baseUrl = process.argv[2];
const cdpPort = Number(process.argv[3]);
const verificationMode = process.argv[4] || "development";
const configuredPathBase = process.argv[5] || "";
const homeTitle = process.argv[6] || "";
const homeSummary = process.argv[7] || "";
const primaryPath = process.argv[8] || "";
const primaryTitle = process.argv[9] || "";
const relatedPath = process.argv[10] || "";

if (!baseUrl || !Number.isFinite(cdpPort) || !["development", "production"].includes(verificationMode) ||
    !homeTitle || !homeSummary || !primaryPath || !primaryTitle || !relatedPath) {
  throw new Error("Usage: node verify-browser.mjs <baseUrl> <cdpPort> <development|production> [pathBase] <homeTitle> <homeSummary> <primaryPath> <primaryTitle> <relatedPath>");
}

const isDevelopmentVerification = verificationMode === "development";
const pathBase = configuredPathBase && configuredPathBase !== "/"
  ? (configuredPathBase.endsWith("/") ? configuredPathBase.slice(0, -1) : configuredPathBase)
  : "";

function externalPath(logicalPath) {
  if (!logicalPath.startsWith("/")) {
    throw new Error(`Logical path must start with '/': ${logicalPath}`);
  }

  if (!pathBase) {
    return logicalPath;
  }

  if (logicalPath === "/") {
    return `${pathBase}/`;
  }

  return `${pathBase}${logicalPath}`;
}

function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

function reverseFind(values, predicate) {
  for (let index = values.length - 1; index >= 0; index -= 1) {
    if (predicate(values[index])) {
      return values[index];
    }
  }

  return null;
}

function toAbsoluteUrl(url, relativeTo = baseUrl) {
  return new URL(url, relativeTo).toString();
}

function toPathAndSearch(url, relativeTo = baseUrl) {
  const parsed = new URL(url, relativeTo);
  return `${parsed.pathname}${parsed.search}`;
}

function formatRemoteArg(arg) {
  if (Object.prototype.hasOwnProperty.call(arg, "value")) {
    return String(arg.value);
  }
  if (arg.description) {
    return String(arg.description);
  }
  return arg.type || "unknown";
}

async function connectToPageTarget() {
  const targets = await fetch(`http://127.0.0.1:${cdpPort}/json/list`).then(response => response.json());
  const pageTarget =
    targets.find(target => target.type === "page" && target.url === "about:blank") ||
    targets.find(target => target.type === "page");

  if (!pageTarget?.webSocketDebuggerUrl) {
    throw new Error("No page target with webSocketDebuggerUrl was found.");
  }

  const ws = new WebSocket(pageTarget.webSocketDebuggerUrl);
  await new Promise((resolve, reject) => {
    ws.addEventListener("open", () => resolve(), { once: true });
    ws.addEventListener("error", event => reject(event.error || new Error("WebSocket open failed.")), { once: true });
  });

  return ws;
}

async function main() {
  const ws = await connectToPageTarget();
  let nextId = 1;
  const pending = new Map();
  const consoleErrors = [];
  const exceptions = [];
  const networkFailures = [];
  const scriptParsedEvents = [];
  const webSocketCreatedEvents = [];
  let loadResolvers = [];
  let withinDocumentResolvers = [];
  let mainFrameId = null;

  ws.addEventListener("message", event => {
    const message = JSON.parse(event.data);

    if (message.id) {
      const entry = pending.get(message.id);
      if (!entry) {
        return;
      }

      pending.delete(message.id);
      if (message.error) {
        entry.reject(new Error(message.error.message || JSON.stringify(message.error)));
      } else {
        entry.resolve(message.result);
      }
      return;
    }

    if (message.method === "Runtime.consoleAPICalled") {
      if (message.params && (message.params.type === "error" || message.params.type === "assert")) {
        consoleErrors.push((message.params.args || []).map(formatRemoteArg).join(" "));
      }
    }

    if (message.method === "Runtime.exceptionThrown") {
      const details = message.params?.exceptionDetails;
      exceptions.push(details?.exception?.description || details?.text || "Unknown runtime exception");
    }

    if (message.method === "Network.loadingFailed") {
      networkFailures.push({
        url: message.params?.url || "",
        errorText: message.params?.errorText || "",
        canceled: message.params?.canceled === true,
        type: message.params?.type || ""
      });
    }

    if (message.method === "Network.webSocketCreated") {
      webSocketCreatedEvents.push({
        requestId: message.params?.requestId || "",
        url: message.params?.url || ""
      });
    }

    if (message.method === "Debugger.scriptParsed") {
      scriptParsedEvents.push({
        scriptId: message.params?.scriptId || "",
        url: message.params?.url || "",
        sourceMapURL: message.params?.sourceMapURL || "",
        isModule: message.params?.isModule === true
      });
    }

    if (message.method === "Page.loadEventFired") {
      const resolvers = loadResolvers;
      loadResolvers = [];
      for (const resolve of resolvers) {
        resolve();
      }
    }

    if (message.method === "Page.navigatedWithinDocument") {
      const resolvers = withinDocumentResolvers;
      withinDocumentResolvers = [];
      for (const resolve of resolvers) {
        resolve(message.params?.url || "");
      }
    }

    if (message.method === "Page.frameNavigated") {
      const frame = message.params?.frame;
      if (frame && !frame.parentId && frame.id) {
        mainFrameId = frame.id;
      }
    }
  });

  function send(method, params = {}) {
    const id = nextId++;
    const promise = new Promise((resolve, reject) => pending.set(id, { resolve, reject }));
    ws.send(JSON.stringify({ id, method, params }));
    return promise;
  }

  function waitForLoad(timeoutMs = 15000) {
    return new Promise((resolve, reject) => {
      const timer = setTimeout(() => reject(new Error("Timed out waiting for Page.loadEventFired.")), timeoutMs);
      loadResolvers.push(() => {
        clearTimeout(timer);
        resolve();
      });
    });
  }

  function waitForWithinDocumentNavigation(timeoutMs = 15000) {
    return new Promise((resolve, reject) => {
      const timer = setTimeout(() => reject(new Error("Timed out waiting for Page.navigatedWithinDocument.")), timeoutMs);
      withinDocumentResolvers.push(url => {
        clearTimeout(timer);
        resolve(url);
      });
    });
  }

  async function evaluate(expression) {
    const result = await send("Runtime.evaluate", {
      expression,
      returnByValue: true,
      awaitPromise: true
    });

    if (result.exceptionDetails) {
      throw new Error(result.exceptionDetails.text || "Runtime.evaluate failed.");
    }

    return result.result?.value;
  }

  async function navigate(url) {
    const load = waitForLoad();
    const sameDocument = waitForWithinDocumentNavigation();
    await send("Page.navigate", { url });
    await Promise.race([load, sameDocument]);
    await delay(1200);
  }

  async function waitUntil(expression, timeoutMs = 8000, intervalMs = 100) {
    const started = Date.now();
    while (Date.now() - started < timeoutMs) {
      const value = await evaluate(expression);
      if (value) {
        return value;
      }
      await delay(intervalMs);
    }

    throw new Error(`Timed out waiting for condition: ${expression}`);
  }

  async function waitForState(getValue, description, timeoutMs = 8000, intervalMs = 100) {
    const started = Date.now();
    while (Date.now() - started < timeoutMs) {
      const value = getValue();
      if (value) {
        return value;
      }

      await delay(intervalMs);
    }

    throw new Error(`Timed out waiting for ${description}.`);
  }

  function findLatestScriptParsedByPath(scriptPath) {
    const expectedUrl = toAbsoluteUrl(scriptPath);
    return reverseFind(
      scriptParsedEvents,
      entry => toAbsoluteUrl(entry.url) === expectedUrl);
  }

  async function inspectSourceMap(check) {
    const script = await waitForState(
      () => {
        const candidate = findLatestScriptParsedByPath(check.scriptPath);
        return candidate && candidate.sourceMapURL ? candidate : null;
      },
      `Debugger.scriptParsed for ${check.scriptPath}`);
    const resolvedSourceMapUrl = toAbsoluteUrl(script.sourceMapURL, script.url);
    const response = await fetch(resolvedSourceMapUrl, { cache: "no-store" });
    const responseText = await response.text();
    let sourceMap = null;
    let parseError = "";

    try {
      sourceMap = JSON.parse(responseText);
    } catch (error) {
      parseError = error && error.message ? error.message : String(error);
    }

    const sources = Array.isArray(sourceMap?.sources) ? sourceMap.sources : [];
    const sourcesContent = Array.isArray(sourceMap?.sourcesContent) ? sourceMap.sourcesContent : [];

    return {
      label: check.label,
      scriptUrl: script.url,
      isModule: script.isModule,
      scriptId: script.scriptId,
      sourceMapURL: script.sourceMapURL,
      resolvedSourceMapUrl,
      sourceMapPath: toPathAndSearch(resolvedSourceMapUrl),
      httpStatus: response.status,
      httpContentType: response.headers.get("content-type") || "",
      parseError,
      mapFile: sourceMap?.file || "",
      sourceCount: sources.length,
      sourcesContentCount: sourcesContent.length,
      missingSources: check.expectedSources.filter(source => !sources.includes(source)),
      missingSourceContentMarkers: check.expectedSourceContentMarkers.filter(
        marker => !sourcesContent.some(sourceText => typeof sourceText === "string" && sourceText.includes(marker)))
    };
  }

  await send("Page.enable");
  await send("Runtime.enable");
  await send("Network.enable");
  await send("Network.setCacheDisabled", { cacheDisabled: true });
  await send("Debugger.enable");
  await send("Emulation.setDeviceMetricsOverride", {
    width: 1440,
    height: 1024,
    deviceScaleFactor: 1,
    mobile: false
  });

  const report = {};
  const failures = [];

  await navigate(baseUrl + externalPath("/"));
  report.runtime = {
    verificationMode,
    pathBase
  };
  const reloadClientPath = externalPath("/@jazor/client");
  const reloadSocketPath = externalPath("/@jazor/reload");
  const reloadClientUrl = toAbsoluteUrl(reloadClientPath);
  const reloadSocketUrl = `${new URL(baseUrl).protocol === "https:" ? "wss" : "ws"}://${new URL(baseUrl).host}${reloadSocketPath}`;
  const reloadClientResponse = await fetch(reloadClientUrl, { cache: "no-store" });
  const reloadClientText = reloadClientResponse.ok ? await reloadClientResponse.text() : "";
  report.runtime.reloadClientInjected = await evaluate(`(function(){
    const script = document.querySelector('script[src="${reloadClientPath}"]');
    return !!script;
  })()`);
  report.runtime.reloadClientStatus = reloadClientResponse.status;
  report.runtime.reloadClientContentType = reloadClientResponse.headers.get("content-type") || "";
  report.runtime.reloadClientHasSocketPath = reloadClientText.includes('const socketPath = "/@jazor/reload";');
  report.runtime.reloadSocketObserved = false;

  if (isDevelopmentVerification) {
    const reloadSocketConnection = await waitForState(
      () => reverseFind(webSocketCreatedEvents, entry => entry.url === reloadSocketUrl),
      `development reload websocket ${reloadSocketUrl}`,
      10000,
      100).catch(() => null);
    report.runtime.reloadSocketObserved = reloadSocketConnection !== null;

    if (!report.runtime.reloadClientInjected) {
      failures.push("Development verification did not inject the /@jazor/client script into the served HTML.");
    }
    if (report.runtime.reloadClientStatus !== 200) {
      failures.push(`Development verification expected /@jazor/client to return HTTP 200 but received ${report.runtime.reloadClientStatus}.`);
    }
    if (!report.runtime.reloadClientContentType.includes("text/javascript")) {
      failures.push(`Development verification expected /@jazor/client to be JavaScript but received '${report.runtime.reloadClientContentType}'.`);
    }
    if (!report.runtime.reloadClientHasSocketPath) {
      failures.push("Development reload client script did not contain the expected /@jazor/reload socket path.");
    }
    if (!report.runtime.reloadSocketObserved) {
      failures.push("Development verification did not observe the /@jazor/reload websocket connection from the injected browser client.");
    }
  } else {
    report.runtime.reloadSocketObserved = webSocketCreatedEvents.some(entry => entry.url === reloadSocketUrl);

    if (report.runtime.reloadClientInjected) {
      failures.push("Production verification unexpectedly injected the /@jazor/client development script.");
    }
    if (report.runtime.reloadClientStatus !== 404) {
      failures.push(`Production verification expected /@jazor/client to stay unavailable but received HTTP ${report.runtime.reloadClientStatus}.`);
    }
    if (report.runtime.reloadSocketObserved) {
      failures.push("Production verification unexpectedly observed a /@jazor/reload websocket connection.");
    }
  }

  const debuggerChecks = isDevelopmentVerification
    ? [
      {
        label: "main",
        scriptPath: externalPath("/jazor/main.mjs"),
        sourceMapPath: externalPath("/jazor/main.mjs.map"),
        moduleFile: "main.mjs",
        expectedSources: [
          "AppModule.cs"
        ],
        expectedSourceContentMarkers: [
          "[ECMAScriptModule(\"main.mjs\")]",
          "public static class AppModule",
          "app.Mount(\"#app\")"
        ]
      },
      {
        label: "wiki-home",
        scriptPath: externalPath("/jazor/components/wiki-home.mjs"),
        sourceMapPath: externalPath("/jazor/components/wiki-home.mjs.map"),
        moduleFile: "components/wiki-home.mjs",
        expectedSources: [
          "WikiHomeModule.cs",
          "WikiHomeModule.DocsPage.cs",
          "WikiHomeModule.RouteContract.cs",
          "obj/wiki/WikiDocsContent.g.cs"
        ],
        expectedSourceContentMarkers: [
          "public static partial class WikiHomeModule",
          "internal static IVNode RenderDocsPage(int pageIndex)",
          "internal static string[] PagePaths => WikiDocsContent.PagePaths;"
        ]
      }
    ]
    : [
      {
        label: "bundle",
        scriptPath: externalPath("/jazor/bundle.js"),
        sourceMapPath: externalPath("/jazor/bundle.js.map"),
        moduleFile: "bundle.js",
        expectedSources: [
          "main.mjs",
          "components/wiki-home.mjs",
          "components/wiki-styles.mjs",
          "style.mjs"
        ],
        expectedSourceContentMarkers: [
          "ecmascript-style:v1",
          "createApp("
        ]
      }
    ];

  report.debugger = {
    sourceMaps: []
  };
  for (const check of debuggerChecks) {
    const inspectedSourceMap = await inspectSourceMap(check);
    report.debugger.sourceMaps.push(inspectedSourceMap);

    if (!inspectedSourceMap.isModule) {
      failures.push(`Debugger parsed ${check.scriptPath} without module semantics.`);
    }
    if (inspectedSourceMap.sourceMapPath !== check.sourceMapPath) {
      failures.push(`Debugger resolved ${check.scriptPath} to unexpected source map path: ${inspectedSourceMap.sourceMapPath}`);
    }
    if (inspectedSourceMap.httpStatus !== 200) {
      failures.push(`Source map ${check.sourceMapPath} returned HTTP ${inspectedSourceMap.httpStatus}.`);
    }
    if (!inspectedSourceMap.httpContentType.includes("application/json")) {
      failures.push(`Source map ${check.sourceMapPath} returned unexpected content type '${inspectedSourceMap.httpContentType}'.`);
    }
    if (inspectedSourceMap.parseError) {
      failures.push(`Source map ${check.sourceMapPath} could not be parsed as JSON: ${inspectedSourceMap.parseError}`);
    }
    if (inspectedSourceMap.mapFile !== check.moduleFile) {
      failures.push(`Source map ${check.sourceMapPath} declared unexpected file '${inspectedSourceMap.mapFile}'.`);
    }
    if (inspectedSourceMap.missingSources.length > 0) {
      failures.push(`Source map ${check.sourceMapPath} did not retain the expected original C# sources: ${inspectedSourceMap.missingSources.join(", ")}`);
    }
    if (inspectedSourceMap.sourcesContentCount < inspectedSourceMap.sourceCount) {
      failures.push(`Source map ${check.sourceMapPath} did not carry sourcesContent for every original source.`);
    }
    if (inspectedSourceMap.missingSourceContentMarkers.length > 0) {
      failures.push(`Source map ${check.sourceMapPath} did not retain the expected C# source content markers: ${inspectedSourceMap.missingSourceContentMarkers.join(", ")}`);
    }
  }

  report.home = {
    title: await evaluate("document.title || ''"),
    description: await evaluate(`(function(){
      const element = document.querySelector('meta[name="description"]');
      return element ? (element.getAttribute('content') || '') : '';
    })()`),
    robots: await evaluate(`(function(){
      const element = document.querySelector('meta[name="robots"]');
      return element ? (element.getAttribute('content') || '') : '';
    })()`),
    canonical: await evaluate(`(function(){
      const element = document.querySelector('link[rel="canonical"]');
      return element ? (element.getAttribute('href') || '') : '';
    })()`),
    mounted: await evaluate(`(function(){
      const title = document.querySelector('.brand-title');
      return title ? (title.textContent || '').includes('Jazor 官方文档') : false;
    })()`),
    emptyShell: await evaluate(`(function(){
      const element = document.querySelector('#app');
      if (!element) return true;
      const html = (element.innerHTML || '').trim();
      return html === '' || html === '<!---->';
    })()`),
    primaryLinkCount: await evaluate(`Array.from(document.querySelectorAll('a')).filter(node => node.getAttribute('href') === '${externalPath(primaryPath)}').length`)
  };

  if (!report.home.mounted) {
    failures.push("Home page did not mount the expected shell content.");
  }
  if (report.home.emptyShell) {
    failures.push("Home page rendered an empty app shell.");
  }
  if (report.home.primaryLinkCount < 1) {
    failures.push("Home page did not render the generated primary docs route link.");
  }
  if (report.home.title !== homeTitle) {
    failures.push(`Home page title was unexpected before SPA navigation: ${report.home.title}`);
  }
  if (report.home.description !== homeSummary) {
    failures.push(`Home page description was unexpected before SPA navigation: ${report.home.description}`);
  }
  if (report.home.robots !== "index, follow") {
    failures.push(`Home page robots directive was unexpected before SPA navigation: ${report.home.robots}`);
  }
  if (!report.home.canonical.endsWith(externalPath("/"))) {
    failures.push(`Home page canonical URL was unexpected before SPA navigation: ${report.home.canonical}`);
  }

  report.home.clickedPrimaryRoute = await evaluate(`(function(){
    const link = Array.from(document.querySelectorAll('a')).find(node => node.getAttribute('href') === '${externalPath(primaryPath)}');
    if (!link) return false;
    link.click();
    return true;
  })()`);

  if (!report.home.clickedPrimaryRoute) {
    failures.push("Could not click the generated primary docs route from the mounted home page.");
  } else {
    await waitUntil(`location.pathname === '${externalPath(primaryPath)}'`);
    await waitUntil(`document.activeElement && document.activeElement.id === 'wiki-main-content'`, 5000, 100).catch(() => null);
    await waitUntil(`(function(){
      const element = document.querySelector('p[aria-live="polite"]');
      return element && (element.textContent || '').trim() === '已打开 ${primaryTitle}。';
    })()`, 5000, 100).catch(() => null);
    await delay(300);

    report.home.pathAfterClick = await evaluate("location.pathname || ''");
    report.home.activeElementId = await evaluate("document.activeElement ? document.activeElement.id : ''");
    report.home.liveText = await evaluate(`(function(){
      const element = document.querySelector('p[aria-live="polite"]');
      return element ? (element.textContent || '').trim() : '';
    })()`);

    if (report.home.pathAfterClick !== externalPath(primaryPath)) {
      failures.push(`SPA navigation from home did not reach the generated primary route: ${report.home.pathAfterClick}`);
    }
    if (report.home.activeElementId !== "wiki-main-content") {
      failures.push(`Route change did not focus the main content region: ${report.home.activeElementId}`);
    }
    if (report.home.liveText !== `已打开 ${primaryTitle}。`) {
      failures.push(`Route change live-region announcement was unexpected: ${report.home.liveText}`);
    }
  }

  report.style = {
    route: await evaluate("location.pathname || ''"),
    styleIdPresent: await evaluate("!!document.querySelector('style#ecmascript-style')"),
    styleText: await evaluate(`(function(){
      const style = document.querySelector('style#ecmascript-style');
      return style ? (style.textContent || '') : '';
    })()`)
  };
  report.style.generatedClassCount = (report.style.styleText.match(/\.ecs-[a-z0-9_-]+/gi) || []).length;

  if (report.style.route !== externalPath(primaryPath)) {
    failures.push(`Generated docs route was unexpected while verifying managed styles: ${report.style.route}`);
  }
  if (!report.style.styleIdPresent) {
    failures.push("ECMAScript.Style did not create the managed #ecmascript-style element.");
  }
  if (!report.style.styleText.includes("ecmascript-style:v1")) {
    failures.push("ECMAScript.Style managed element did not contain its version marker.");
  }
  if (report.style.generatedClassCount < 1) {
    failures.push("ECMAScript.Style managed element did not contain a generated ecs-* selector from WikiStyleSheet.cs.");
  }
  if (!/padding:4px 8px;/.test(report.style.styleText)) {
    failures.push(`ECMAScript.Style did not emit the expected padding shorthand: ${report.style.styleText}`);
  }

  const primarySectionId = await evaluate(`(function(){
    const link = document.querySelector('.toc-link');
    if (!link) return '';
    const href = link.getAttribute('href') || '';
    const hashIndex = href.lastIndexOf('#');
    return hashIndex >= 0 ? href.slice(hashIndex + 1) : '';
  })()`);
  if (!primarySectionId) {
    failures.push("Generated primary docs route did not expose a section anchor for hash navigation.");
  }

  report.gettingStarted = {
    initialTheme: await evaluate("document.documentElement.getAttribute('data-theme') || ''")
  };

  await evaluate(`(function(){
    window.dispatchEvent(new KeyboardEvent('keydown', { key: '/', bubbles: true }));
    return true;
  })()`);
  await delay(200);
  report.gettingStarted.focusAfterSlashShortcut = await evaluate("document.activeElement ? document.activeElement.id : ''");
  if (report.gettingStarted.focusAfterSlashShortcut !== "wiki-nav-search-input") {
    failures.push(`Slash shortcut did not focus nav search: ${report.gettingStarted.focusAfterSlashShortcut}`);
  }

  report.gettingStarted.themeToggleClicked = await evaluate(`(function(){
    // Sober 外壳：主题切换是 s-icon-button，语义在 aria-label 上
    const button = document.querySelector('.wiki-theme-toggle');
    if (!button || !(button.getAttribute('aria-label') || '').includes('主题：')) return false;
    button.click();
    return true;
  })()`);
  if (!report.gettingStarted.themeToggleClicked) {
    failures.push("Theme toggle button was not found.");
  }
  await delay(300);

  report.gettingStarted.themeAfterToggle = await evaluate("document.documentElement.getAttribute('data-theme') || ''");
  report.gettingStarted.storedTheme = await evaluate("localStorage.getItem('jazor.wiki.theme') || ''");

  if (report.gettingStarted.themeToggleClicked && report.gettingStarted.themeAfterToggle === report.gettingStarted.initialTheme) {
    failures.push("Theme did not change after toggling.");
  }
  if (report.gettingStarted.themeToggleClicked && report.gettingStarted.storedTheme !== report.gettingStarted.themeAfterToggle) {
    failures.push("Theme preference was not persisted to localStorage.");
  }

  report.gettingStarted.feedbackClicked = await evaluate(`(function(){
    const button = Array.from(document.querySelectorAll('.feedback-button')).find(node => (node.textContent || '').includes('有帮助'));
    if (!button) return false;
    button.click();
    return true;
  })()`);
  if (!report.gettingStarted.feedbackClicked) {
    failures.push("Helpful feedback button was not found.");
  }
  await delay(300);

  report.gettingStarted.feedback = {
    stored: await evaluate(`localStorage.getItem('jazor.wiki.feedback:${primaryPath}') || ''`),
    activeCount: await evaluate("document.querySelectorAll('.feedback-button-active').length"),
    liveText: await evaluate(`(function(){
      const element = document.querySelector('p[aria-live="polite"]');
      return element ? (element.textContent || '').trim() : '';
    })()`)
  };

  if (report.gettingStarted.feedbackClicked && report.gettingStarted.feedback.stored !== "helpful") {
    failures.push(`Feedback preference did not persist the expected value: ${report.gettingStarted.feedback.stored}`);
  }
  if (report.gettingStarted.feedbackClicked && report.gettingStarted.feedback.activeCount < 1) {
    failures.push("Feedback active state was not applied.");
  }

  report.gettingStarted.pagePermalinkClicked = await evaluate(`(function(){
    const button = document.querySelector('.page-permalink');
    if (!button) return false;
    button.click();
    return true;
  })()`);
  if (!report.gettingStarted.pagePermalinkClicked) {
    failures.push("Page permalink button was not found.");
  }
  await delay(500);

  report.gettingStarted.pagePermalinkLabel = await evaluate(`(function(){
    const button = document.querySelector('.page-permalink');
    return button ? (button.textContent || '').trim() : '';
  })()`);
  if (report.gettingStarted.pagePermalinkClicked && !["已复制", "链接已就绪"].includes(report.gettingStarted.pagePermalinkLabel)) {
    failures.push(`Page permalink feedback was unexpected: ${report.gettingStarted.pagePermalinkLabel}`);
  }

  report.gettingStarted.codeCopyClicked = await evaluate(`(function(){
    const button = document.querySelector('.code-copy-button');
    if (!button) return false;
    button.click();
    return true;
  })()`);
  if (!report.gettingStarted.codeCopyClicked) {
    failures.push("Code copy button was not found.");
  }
  await delay(500);

  report.gettingStarted.codeCopyLabel = await evaluate(`(function(){
    const button = document.querySelector('.code-copy-button');
    return button ? (button.textContent || '').trim() : '';
  })()`);
  if (report.gettingStarted.codeCopyClicked && !["已复制", "复制不可用"].includes(report.gettingStarted.codeCopyLabel)) {
    failures.push(`Code copy feedback was unexpected: ${report.gettingStarted.codeCopyLabel}`);
  }

  report.gettingStarted.sectionPermalinkClicked = await evaluate(`(function(){
    const button = document.querySelector('.section-permalink');
    if (!button) return false;
    button.click();
    return true;
  })()`);
  if (!report.gettingStarted.sectionPermalinkClicked) {
    failures.push("Section permalink button was not found.");
  }
  await delay(500);

  report.gettingStarted.sectionPermalinkLabel = await evaluate(`(function(){
    const button = document.querySelector('.section-permalink');
    return button ? (button.textContent || '').trim() : '';
  })()`);
  report.gettingStarted.hashAfterSectionPermalink = await evaluate("location.hash || ''");
  if (report.gettingStarted.sectionPermalinkClicked && !["已复制", "链接已就绪"].includes(report.gettingStarted.sectionPermalinkLabel)) {
    failures.push(`Section permalink feedback was unexpected: ${report.gettingStarted.sectionPermalinkLabel}`);
  }
  if (report.gettingStarted.sectionPermalinkClicked && report.gettingStarted.hashAfterSectionPermalink.length === 0) {
    failures.push("Section permalink did not update location hash.");
  }

  await navigate(baseUrl + externalPath(primaryPath));
  report.gettingStarted.persistedAfterReload = {
    theme: await evaluate("document.documentElement.getAttribute('data-theme') || ''"),
    storedTheme: await evaluate("localStorage.getItem('jazor.wiki.theme') || ''"),
    feedbackStored: await evaluate(`localStorage.getItem('jazor.wiki.feedback:${primaryPath}') || ''`),
    feedbackActiveCount: await evaluate("document.querySelectorAll('.feedback-button-active').length")
  };

  if (report.gettingStarted.persistedAfterReload.theme !== report.gettingStarted.themeAfterToggle) {
    failures.push(`Theme did not rehydrate after reload: ${report.gettingStarted.persistedAfterReload.theme}`);
  }
  if (report.gettingStarted.persistedAfterReload.feedbackStored !== "helpful") {
    failures.push(`Feedback did not persist after reload: ${report.gettingStarted.persistedAfterReload.feedbackStored}`);
  }
  if (report.gettingStarted.persistedAfterReload.feedbackActiveCount < 1) {
    failures.push("Feedback active state did not rehydrate after reload.");
  }

  await evaluate(`(function(){
    window.scrollTo(0, Math.max(document.documentElement.scrollHeight, document.body.scrollHeight));
    return true;
  })()`);
  await delay(1000);
  report.scrollRestore = {
    scrollBeforeRouteChange: await evaluate("window.pageYOffset || 0")
  };

  report.scrollRestore.relatedPageClicked = await evaluate(`(function(){
    const link = Array.from(document.querySelectorAll('a')).find(node => node.getAttribute('href') === '${externalPath(relatedPath)}');
    if (!link) return false;
    link.click();
    return true;
  })()`);
  if (!report.scrollRestore.relatedPageClicked) {
    failures.push("Could not find the generated related-page link for scroll restoration verification.");
  } else {
    await waitUntil(`location.pathname === '${externalPath(relatedPath)}'`);
    await evaluate("history.back(); true");
    await waitUntil(`location.pathname === '${externalPath(primaryPath)}'`);
    await delay(1000);

    report.scrollRestore.pathAfterBack = await evaluate("location.pathname || ''");
    report.scrollRestore.hashAfterBack = await evaluate("location.hash || ''");
    report.scrollRestore.scrollAfterBack = await evaluate("window.pageYOffset || 0");

    if (report.scrollRestore.pathAfterBack !== externalPath(primaryPath)) {
      failures.push(`Back navigation did not return to the generated primary route: ${report.scrollRestore.pathAfterBack}`);
    }
    if (report.scrollRestore.hashAfterBack !== "") {
      failures.push(`Back navigation returned an unexpected hash-bearing URL: ${report.scrollRestore.hashAfterBack}`);
    }
    if (report.scrollRestore.scrollBeforeRouteChange > 120 && report.scrollRestore.scrollAfterBack < 120) {
      failures.push(`Scroll position did not restore after back navigation: ${report.scrollRestore.scrollBeforeRouteChange} -> ${report.scrollRestore.scrollAfterBack}`);
    }
  }

  await navigate(baseUrl + externalPath("/search") + "?q=compiler");
  report.search = {
    title: await evaluate("document.title || ''"),
    description: await evaluate(`(function(){
      const element = document.querySelector('meta[name="description"]');
      return element ? (element.getAttribute('content') || '') : '';
    })()`),
    robots: await evaluate(`(function(){
      const element = document.querySelector('meta[name="robots"]');
      return element ? (element.getAttribute('content') || '') : '';
    })()`),
    canonical: await evaluate(`(function(){
      const element = document.querySelector('link[rel="canonical"]');
      return element ? (element.getAttribute('href') || '') : '';
    })()`),
    inputValue: await evaluate("document.querySelector('#wiki-search-input') ? document.querySelector('#wiki-search-input').value : ''"),
    resultCount: await evaluate("document.querySelectorAll('.search-result-card').length"),
    markCount: await evaluate("document.querySelectorAll('.search-mark').length")
  };

  if (report.search.inputValue !== "compiler") {
    failures.push(`Search query did not hydrate correctly: ${report.search.inputValue}`);
  }
  if (report.search.resultCount < 1) {
    failures.push("Search route returned no results for compiler.");
  }
  if (report.search.title !== "搜索: compiler | jazor.wiki") {
    failures.push(`Search title was unexpected after query hydration: ${report.search.title}`);
  }
  if (report.search.description !== '搜索结果："compiler"，覆盖路由元数据、标签、页面正文和章节标题。') {
    failures.push(`Search description was unexpected after query hydration: ${report.search.description}`);
  }
  if (report.search.robots !== "noindex, nofollow") {
    failures.push(`Search robots directive was unexpected after query hydration: ${report.search.robots}`);
  }
  if (!report.search.canonical.endsWith(`${externalPath("/search")}?q=compiler`)) {
    failures.push(`Search canonical URL was unexpected after query hydration: ${report.search.canonical}`);
  }

  report.search.clearClicked = await evaluate(`(function(){
    const button = document.querySelector('.search-clear');
    if (!button) return false;
    button.click();
    return true;
  })()`);
  if (!report.search.clearClicked) {
    failures.push("Search clear button was not found.");
  }
  await delay(300);

  report.search.clearedInputValue = await evaluate("document.querySelector('#wiki-search-input') ? document.querySelector('#wiki-search-input').value : ''");
  report.search.searchAfterClear = await evaluate("location.search || ''");
  if (report.search.clearClicked && report.search.clearedInputValue !== "") {
    failures.push(`Search clear did not empty the input: ${report.search.clearedInputValue}`);
  }
  if (report.search.clearClicked && report.search.searchAfterClear !== "") {
    failures.push(`Search clear did not remove the query string: ${report.search.searchAfterClear}`);
  }

  await navigate(baseUrl + externalPath("/guides/missing-page"));
  report.notFound = {
    title: await evaluate("document.title || ''"),
    description: await evaluate(`(function(){
      const element = document.querySelector('meta[name="description"]');
      return element ? (element.getAttribute('content') || '') : '';
    })()`),
    robots: await evaluate(`(function(){
      const element = document.querySelector('meta[name="robots"]');
      return element ? (element.getAttribute('content') || '') : '';
    })()`),
    canonical: await evaluate(`(function(){
      const element = document.querySelector('link[rel="canonical"]');
      return element ? (element.getAttribute('href') || '') : '';
    })()`),
    hasHeading: await evaluate("Array.from(document.querySelectorAll('h1,h2,h3')).some(node => (node.textContent || '').includes('页面未找到'))"),
    suggestionCount: await evaluate("document.querySelectorAll('.route-card').length"),
    requestedPathShown: await evaluate("document.body.textContent.includes('/guides/missing-page')")
  };

  if (!report.notFound.hasHeading) {
    failures.push("Not-found route did not render the expected heading.");
  }
  if (report.notFound.suggestionCount < 1) {
    failures.push("Not-found route rendered no recovery suggestions.");
  }
  if (!report.notFound.requestedPathShown) {
    failures.push("Not-found route did not show the requested path.");
  }
  if (report.notFound.title !== "页面未找到 | jazor.wiki") {
    failures.push(`Not-found title was unexpected: ${report.notFound.title}`);
  }
  if (report.notFound.description !== "当前路径未在 Wiki 页面目录中注册。") {
    failures.push(`Not-found description was unexpected: ${report.notFound.description}`);
  }
  if (report.notFound.robots !== "noindex, nofollow") {
    failures.push(`Not-found robots directive was unexpected: ${report.notFound.robots}`);
  }
  if (!report.notFound.canonical.endsWith(externalPath("/guides/missing-page"))) {
    failures.push(`Not-found canonical URL was unexpected: ${report.notFound.canonical}`);
  }

  report.notFound.recoveryClicked = await evaluate(`(function(){
    const link = document.querySelector('.route-card');
    if (!link) return false;
    link.click();
    return true;
  })()`);
  if (!report.notFound.recoveryClicked) {
    failures.push("Not-found recovery card was not found.");
  } else {
    await waitUntil(`location.pathname !== '${externalPath("/guides/missing-page")}'`);
    report.notFound.recoveredPath = await evaluate("location.pathname || ''");
    if (report.notFound.recoveredPath === externalPath("/guides/missing-page")) {
      failures.push("Not-found recovery card did not navigate away from the missing route.");
    }
  }

  await navigate(baseUrl + externalPath(primaryPath) + "#" + primarySectionId);
  report.hashNavigation = {
    locationHash: await evaluate("location.hash || ''"),
    tocActiveCount: await evaluate("document.querySelectorAll('.toc-link-active').length"),
    docActiveCount: await evaluate("document.querySelectorAll('.doc-section-active').length"),
    activeSectionId: await evaluate(`(function(){
      const section = document.querySelector('.doc-section-active');
      return section ? (section.id || '') : '';
    })()`)
  };

  if (report.hashNavigation.locationHash !== "#" + primarySectionId) {
    failures.push(`Direct hash navigation did not preserve the expected hash: ${report.hashNavigation.locationHash}`);
  }
  if (report.hashNavigation.activeSectionId !== primarySectionId) {
    failures.push(`Direct hash navigation did not activate the expected section: ${report.hashNavigation.activeSectionId}`);
  }
  if (report.hashNavigation.tocActiveCount < 1 || report.hashNavigation.docActiveCount < 1) {
    failures.push("Direct hash navigation did not activate both TOC and document section state.");
  }

  await send("Emulation.setDeviceMetricsOverride", {
    width: 390,
    height: 844,
    deviceScaleFactor: 1,
    mobile: true
  });
  await delay(300);

  report.mobile = {};
  // Sober s-drawer：开合状态由抽屉内部 slot 包裹层 class 与导航面板几何决定
  const mobileDrawerState = () => evaluate(`(function(){
    const drawer = document.querySelector('s-drawer');
    const startSlot = drawer && drawer.shadowRoot ? drawer.shadowRoot.querySelector('slot[name=start]') : null;
    const scrim = drawer && drawer.shadowRoot ? drawer.shadowRoot.querySelector('.scrim') : null;
    const nav = document.querySelector('#wiki-nav-rail');
    const toc = document.querySelector('#wiki-toc-rail');
    const rect = (el) => { if (!el) return null; const r = el.getBoundingClientRect(); return { x: Math.round(r.x), w: Math.round(r.width) }; };
    return {
      startSlotClass: startSlot ? (startSlot.className || '') : '',
      scrimClass: scrim ? (scrim.className || '') : '',
      navRect: rect(nav),
      tocRect: rect(toc)
    };
  })()`);
  const isPanelVisibleOnScreen = (rect) => !!rect && rect.w > 0 && rect.x >= 0 && rect.x < 390;

  report.mobile.navClicked = await evaluate(`(function(){
    const button = document.querySelector('.wiki-menu-button');
    if (!button) return false;
    button.click();
    return true;
  })()`);
  if (!report.mobile.navClicked) {
    failures.push("Mobile navigation menu button was not found.");
  }
  await delay(400);

  report.mobile.stateAfterNavOpen = await mobileDrawerState();
  if (report.mobile.navClicked) {
    if (!isPanelVisibleOnScreen(report.mobile.stateAfterNavOpen.navRect)) {
      failures.push("Mobile nav drawer did not reveal the navigation panel on screen.");
    }
    if (!report.mobile.stateAfterNavOpen.startSlotClass.includes("show-laptop") ||
        !report.mobile.stateAfterNavOpen.scrimClass.includes("show-laptop")) {
      failures.push("Mobile nav drawer did not switch the drawer slots into overlay mode.");
    }
  }

  report.mobile.backdropClicked = await evaluate(`(function(){
    const drawer = document.querySelector('s-drawer');
    const scrim = drawer && drawer.shadowRoot ? drawer.shadowRoot.querySelector('.scrim') : null;
    if (!scrim) return false;
    scrim.click();
    return true;
  })()`);
  await delay(400);

  report.mobile.stateAfterBackdrop = await mobileDrawerState();
  if (report.mobile.backdropClicked && isPanelVisibleOnScreen(report.mobile.stateAfterBackdrop.navRect)) {
    failures.push("Mobile nav drawer did not close after clicking the scrim.");
  }

  report.mobile.tocClicked = await evaluate(`(function(){
    const button = document.querySelector('.wiki-toc-button');
    if (!button) return false;
    button.click();
    return true;
  })()`);
  if (!report.mobile.tocClicked) {
    failures.push("Mobile TOC drawer button was not found.");
  }
  await delay(400);

  report.mobile.stateAfterTocOpen = await mobileDrawerState();
  report.mobile.tocLinkCount = await evaluate("document.querySelectorAll('.toc-link').length");

  if (report.mobile.tocClicked && !isPanelVisibleOnScreen(report.mobile.stateAfterTocOpen.tocRect)) {
    failures.push("Mobile TOC drawer did not reveal the TOC panel on screen.");
  }
  if (report.mobile.tocClicked && report.mobile.tocLinkCount < 1) {
    failures.push("Mobile TOC drawer opened without visible TOC links.");
  }

  report.mobile.tocLinkClicked = await evaluate(`(function(){
    const link = document.querySelector('#wiki-toc-rail .toc-link');
    if (!link) return false;
    link.click();
    return true;
  })()`);
  if (!report.mobile.tocLinkClicked) {
    failures.push("Mobile TOC link was not found.");
  }
  await delay(500);

  report.mobile.hashAfterTocClick = await evaluate("location.hash || ''");
  report.mobile.stateAfterTocLink = await mobileDrawerState();
  if (report.mobile.tocLinkClicked && report.mobile.hashAfterTocClick.length === 0) {
    failures.push("Mobile TOC link did not update the hash.");
  }
  if (report.mobile.tocLinkClicked && isPanelVisibleOnScreen(report.mobile.stateAfterTocLink.tocRect)) {
    failures.push("Mobile TOC drawer remained open after selecting a section link.");
  }

  const actionableNetworkFailures = networkFailures.filter(entry => {
    if (entry.canceled || entry.errorText === "net::ERR_ABORTED") {
      return false;
    }

    // Some Chromium/CDP runs emit empty loadingFailed stylesheet events during document swaps.
    // Ignore those placeholders and only fail on attributable network errors.
    if (!entry.url && !entry.errorText) {
      return false;
    }

    return true;
  });
  if (actionableNetworkFailures.length > 0) {
    failures.push(`Network failures observed: ${actionableNetworkFailures.map(entry => `${entry.type}:${entry.errorText}:${entry.url}`).join(" | ")}`);
  }
  if (consoleErrors.length > 0) {
    failures.push(`Console errors observed: ${consoleErrors.join(" | ")}`);
  }
  if (exceptions.length > 0) {
    failures.push(`Runtime exceptions observed: ${exceptions.join(" | ")}`);
  }

  console.log(JSON.stringify({
    report,
    consoleErrors,
    exceptions,
    networkFailures: actionableNetworkFailures,
    scriptParsedCount: scriptParsedEvents.length,
    webSocketCount: webSocketCreatedEvents.length
  }, null, 2));

  ws.close();

  if (failures.length > 0) {
    for (const failure of failures) {
      console.error(failure);
    }
    process.exit(1);
  }
}

main().catch(error => {
  console.error(error && error.stack ? error.stack : String(error));
  process.exit(1);
});
