import { _d52d7114d5c1b839 } from "System/StringModule.js";
import { defineComponent, h, nextTick, ref } from "npm:vue@3";
function contentModelBody() {
  return h("div", { class: "doc-body" }, [pageSection("page-contract", "Page contract", [h("p", "Each page owns explicit route metadata in code: path, group, title, summary, and status. That is enough to drive the shell, page hero, and previous/next navigation without introducing a hidden content layer."), h("ul", [h("li", "Paths are real URLs and part of the hosting contract."), h("li", "Summaries are short product-facing explanations, not internal engineering notes."), h("li", "Statuses communicate maturity without inventing a versioning system for every page.")])]), pageSection("navigation-contract", "Navigation contract", [h("p", "Navigation is explicit by design. The left rail is grouped by product concern, and the right rail is generated from section-level anchors that live beside the content they describe."), h("div", { class: "check-grid" }, [checkCard("Left rail", "Stable page entry points grouped by user concern."), checkCard("Article body", "Readable sections authored directly in H functions."), checkCard("Right rail", "Anchor-level TOC for fast scanning and direct linking.")])]), pageSection("editing-rules", "Editing rules", [h("p", "The site is code-first, but it should not read like arbitrary application code. Editing rules keep it readable for documentation work."), h("ul", [h("li", "Keep each section short enough to scan without opening generated output."), h("li", "Prefer explicit helpers like `PageSection`, `Callout`, and `CodeBlock` over generic DSL layers."), h("li", "Treat navigation metadata and section anchors as part of the product contract.")]), callout("Do not optimize for cleverness", "If a docs page becomes hard to edit in C#, the answer is usually clearer H composition, not a new meta-language.")])]);
}
const overviewPath = "/";
const gettingStartedPath = "/guides/getting-started";
const contentModelPath = "/guides/content-model";
const hFunctionAuthoringPath = "/engineering/h-function-authoring";
const deploymentPath = "/operations/deployment";
let currentPathRef;
let currentHashRef;
let copiedSectionRef;
let permalinkReadySectionRef;
let navFilterRef;
let permalinkFeedbackResetTimerId;
export let component = defineComponent({ name: "WikiHome", setup: setup });
function setup() {
  let requestedPath = "/";
  let requestedHash = "";
  let location = document.location;
  if (location !== null) {
    requestedPath = normalizePath(location.pathname);
    requestedHash = normalizeHash(location.hash);
    let requestedUrl = buildUrl(requestedPath, requestedHash);
    if (requestedPath !== location.pathname || getHashFragment(requestedHash) !== location.hash)
      window.history.replaceState(requestedUrl, "", requestedUrl);
  }
  let currentPath = ref(requestedPath);
  let currentHash = ref(requestedHash);
  let copiedSection = ref("");
  let permalinkReadySection = ref("");
  let navFilter = ref("");
  currentPathRef = currentPath;
  currentHashRef = currentHash;
  copiedSectionRef = copiedSection;
  permalinkReadySectionRef = permalinkReadySection;
  navFilterRef = navFilter;
  syncDocumentState(requestedPath);
  window.onpopstate = onPopState;
  window.onhashchange = onHashChange;
  if (requestedHash.length > 0)
    queueScrollToHashAnchor(requestedHash);
  return () => {
    return render(currentPath.value, currentHash.value, navFilter.value);
  };
}
function render(currentPath, currentHash, navFilter) {
  let article = notFoundArticle(currentPath);
  let toc = emptyTocRail();
  if (isKnownPage(currentPath)) {
    article = documentColumn(currentPath);
    toc = TocRail(currentPath, currentHash);
  }
  return h("main", { class: "wiki-shell" }, [siteHeader(), h("div", { class: "wiki-layout" }, [navigationRail(currentPath, navFilter), article, toc]), siteFooter()]);
}
function siteHeader() {
  return h("header", { class: "site-header" }, [h("div", { class: "site-header-inner" }, [h("div", { class: "site-brand" }, [h("p", { class: "brand-kicker" }, "jazor.wiki"), h("h1", { class: "brand-title" }, "Production Docs Built with Vue 3 H Functions"), h("p", { class: "brand-summary" }, "A real documentation shell for Jazor, with H-function-authored layout and product-facing routes.")]), h("div", { class: "brand-actions" }, [headerLink("/guides/getting-started", "Get Started"), headerLink("/operations/deployment", "Deploy It")])])]);
}
function navigationRail(currentPath, navFilter) {
  let foundationLinks = [];
  let engineeringLinks = [];
  let operationsLinks = [];
  appendNavLinkIfVisible(foundationLinks, "/", "Overview", "What ships now, why the site exists, and the MVP boundary.", currentPath, navFilter);
  appendNavLinkIfVisible(foundationLinks, "/guides/getting-started", "Getting Started", "Run the site locally, understand routes, and verify the emitted host.", currentPath, navFilter);
  appendNavLinkIfVisible(foundationLinks, "/guides/content-model", "Content Model", "How pages, anchors, and navigation stay maintainable in a code-first docs site.", currentPath, navFilter);
  appendNavLinkIfVisible(engineeringLinks, "/engineering/h-function-authoring", "H-Function Authoring", "Why H functions own the shell and what rules keep the authoring path stable.", currentPath, navFilter);
  appendNavLinkIfVisible(operationsLinks, "/operations/deployment", "Deployment", "Build outputs, route fallback, and the smoke-verification contract.", currentPath, navFilter);
  let visibleCount = foundationLinks.length + engineeringLinks.length + operationsLinks.length;
  let railChildren = [h("div", { class: "rail-card nav-search-card" }, [h("p", { class: "rail-kicker" }, "Find a page"), h("p", { class: "rail-copy" }, "Filter routes, titles, summaries, status, and group labels without leaving the current page."), h("div", { class: "nav-search-row" }, [h("input", {
    class: "nav-search-input",
    type: "search",
    placeholder: "Search docs pages",
    autocomplete: "off",
    value: navFilter,
    ...createNavFilterInputEvents()
  }), h("button", {
    class: "nav-search-clear",
    type: "button",
    disabled: navFilter.length === 0,
    ...createClearNavFilterEvents()
  }, "Clear")]), h("p", { class: "nav-search-status" }, getNavFilterStatus(navFilter, visibleCount))]), h("div", { class: "rail-card" }, [h("p", { class: "rail-kicker" }, "Product map"), h("p", { class: "rail-copy" }, "Wiki is now the product-facing docs shell for Jazor. Routes, structure, deployment flow, and page discovery are treated as user-facing contracts.")])];
  if (foundationLinks.length > 0)
    railChildren.push(navGroup("Foundation", foundationLinks.slice()));
  if (engineeringLinks.length > 0)
    railChildren.push(navGroup("Engineering", engineeringLinks.slice()));
  if (operationsLinks.length > 0)
    railChildren.push(navGroup("Operations", operationsLinks.slice()));
  if (visibleCount === 0) {
    railChildren.push(h("div", { class: "rail-card nav-search-empty" }, [h("p", { class: "nav-search-empty-title" }, "No pages match the current filter."), h("p", { class: "nav-search-empty-summary" }, "Search by route fragment, product group, page title, status, or summary copy.")]));
  }
  return h("aside", { class: "nav-rail" }, railChildren.slice());
}
function appendNavLinkIfVisible(links, path, title, summary, currentPath, navFilter) {
  if (!matchesPageFilter(path, navFilter))
    return;
  links.push(navLink(path, title, summary, currentPath));
}
function getNavFilterStatus(navFilter, visibleCount) {
  if (navFilter.length === 0)
    return "Showing all " + 5 + " registered docs pages.";
  if (visibleCount === 1)
    return "1 page matches \"" + navFilter + "\".";
  return visibleCount + " pages match \"" + navFilter + "\".";
}
function documentColumn(currentPath) {
  return h("article", { class: "doc-column" }, [documentHero(currentPath), documentBody(currentPath), pagePager(currentPath)]);
}
function documentHero(currentPath) {
  return h("header", { class: "doc-hero" }, [h("div", { class: "hero-meta-row" }, [h("span", { class: "hero-group" }, getPageGroup(currentPath)), h("span", { class: "hero-status" }, getPageStatus(currentPath))]), h("h1", { class: "doc-title" }, getPageTitle(currentPath)), h("p", { class: "doc-summary" }, getPageSummary(currentPath))]);
}
function documentBody(currentPath) {
  let pageIndex = getPageIndex(currentPath);
  if (pageIndex === 0)
    return overviewBody();
  if (pageIndex === 1)
    return gettingStartedBody();
  if (pageIndex === 2)
    return contentModelBody();
  if (pageIndex === 3)
    return hFunctionAuthoringBody();
  return deploymentBody();
}
function notFoundArticle(currentPath) {
  return h("article", { class: "doc-column" }, [h("header", { class: "doc-hero" }, [h("div", { class: "hero-meta-row" }, [h("span", { class: "hero-group" }, "Routing"), h("span", { class: "hero-status" }, "Not Found")]), h("h1", { class: "doc-title" }, "Page Not Found"), h("p", { class: "doc-summary" }, "The current path is not registered in the Wiki route map. Use the navigation rail or return to the overview.")]), h("div", { class: "doc-body" }, [pageSection("requested-route", "Requested route", [h("p", "Wiki route fallback is working, but this specific path does not map to a registered page."), codeBlock("Requested path", currentPath)]), pageSection("recover", "Recover", [h("ul", [h("li", "Return to the overview page and re-enter from the left navigation."), h("li", "If this route should exist, add it to the route constants and body branch map."), h("li", "Rerun `verify-smoke.ps1` after registering the route.")]), h("p", [h("a", {
    class: "pager-link pager-link-single",
    href: "/",
    ...createRouteClickEvents()
  }, "Open the overview page")])])])]);
}
function pagePager(currentPath) {
  let previousNode = emptyPagerSlot();
  let nextNode = emptyPagerSlot();
  let previousPath = getPreviousPath(currentPath);
  let nextPath = getNextPath(currentPath);
  if (previousPath.length > 0)
    previousNode = pagerLink("Previous", previousPath, getPageTitle(previousPath));
  if (nextPath.length > 0)
    nextNode = pagerLink("Next", nextPath, getPageTitle(nextPath));
  return h("nav", { class: "pager" }, [previousNode, nextNode]);
}
function siteFooter() {
  return h("footer", { class: "site-footer" }, [h("p", "jazor.wiki now runs as a real docs shell: H-function authored, statically emitted, and route-fallback ready."), h("p", "Health endpoint: /health | Primary routes: /, /guides/getting-started, /engineering/h-function-authoring, /operations/deployment")]);
}
function normalizePath(pathname) {
  if (pathname.length === 0)
    return "/";
  let normalized = pathname;
  if (normalized === "/index.html")
    normalized = "/";
  else if (normalized.endsWith("/index.html"))
    normalized = normalized.substring(0, 0 + (normalized.length - "/index.html".length));
  if (normalized.length > 1 && normalized.endsWith("/"))
    normalized = normalized.substring(0, 0 + (normalized.length - 1));
  if (normalized.length === 0)
    return "/";
  return normalized;
}
function normalizeHash(hash) {
  if (hash.length === 0)
    return "";
  if (hash.startsWith("#"))
    return hash.substring(1);
  return hash;
}
function getHashFragment(hash) {
  if (hash.length === 0)
    return "";
  return "#" + hash;
}
function buildUrl(path, hash) {
  return path + getHashFragment(hash);
}
function syncDocumentState(currentPath) {
  if (isKnownPage(currentPath))
    document.title = getPageTitle(currentPath) + " | jazor.wiki";
  else
    document.title = "Page Not Found | jazor.wiki";
}
function getCurrentPathRef() {
  return currentPathRef;
}
function getCurrentHashRef() {
  return currentHashRef;
}
function getCopiedSectionRef() {
  return copiedSectionRef;
}
function getPermalinkReadySectionRef() {
  return permalinkReadySectionRef;
}
function getNavFilterRef() {
  return navFilterRef;
}
function setNavFilter(value) {
  let navFilter = getNavFilterRef();
  if (navFilter === null)
    return;
  navFilter.value = value.trim();
}
function setCopiedSection(value) {
  let copiedSection = getCopiedSectionRef();
  if (copiedSection === null)
    return;
  copiedSection.value = value;
}
function setPermalinkReadySection(value) {
  let permalinkReadySection = getPermalinkReadySectionRef();
  if (permalinkReadySection === null)
    return;
  permalinkReadySection.value = value;
}
function showCopiedSection(sectionId) {
  setPermalinkReadySection("");
  setCopiedSection(sectionId);
  queuePermalinkFeedbackReset();
}
function showPermalinkReady(sectionId) {
  setCopiedSection("");
  setPermalinkReadySection(sectionId);
  queuePermalinkFeedbackReset();
}
function queuePermalinkFeedbackReset() {
  if (permalinkFeedbackResetTimerId !== 0)
    window.clearTimeout(permalinkFeedbackResetTimerId);
  permalinkFeedbackResetTimerId = window.setTimeout(resetPermalinkFeedback, 1800);
}
function resetPermalinkFeedback() {
  setCopiedSection("");
  setPermalinkReadySection("");
  permalinkFeedbackResetTimerId = 0;
}
function queueScrollToHashAnchor(hash) {
  if (hash.length === 0)
    return;
  nextTick(() => {
    scrollToHashAnchor(hash);
    return;
  });
}
function scrollToHashAnchor(hash) {
  let __patin$098171ed70511ce0af2cca16, sectionElement;
  if (!(__patin$098171ed70511ce0af2cca16 = document.getElementById(hash), __patin$098171ed70511ce0af2cca16 instanceof Element && (sectionElement = __patin$098171ed70511ce0af2cca16, true)))
    return;
  sectionElement.scrollIntoView(true);
}
function navigateTo(path, hash, updateHistory, resetScroll) {
  let currentPath = getCurrentPathRef();
  let currentHash = getCurrentHashRef();
  if (currentPath === null || currentHash === null)
    return;
  let normalizedPath = normalizePath(path);
  let normalizedHash = normalizeHash(hash);
  if (currentPath.value === normalizedPath && currentHash.value === normalizedHash) {
    if (normalizedHash.length > 0)
      queueScrollToHashAnchor(normalizedHash);
    else if (resetScroll)
      window.scrollTo(0, 0);
    return;
  }
  currentPath.value = normalizedPath;
  currentHash.value = normalizedHash;
  if (updateHistory) {
    let url = buildUrl(normalizedPath, normalizedHash);
    window.history.pushState(url, "", url);
  }
  syncDocumentState(normalizedPath);
  if (normalizedHash.length > 0)
    queueScrollToHashAnchor(normalizedHash);
  else if (resetScroll)
    window.scrollTo(0, 0);
}
function syncLocationStateFromBrowser() {
  let location = document.location;
  let currentPath = getCurrentPathRef();
  let currentHash = getCurrentHashRef();
  if (location === null || currentPath === null || currentHash === null)
    return;
  let normalizedPath = normalizePath(location.pathname);
  let normalizedHash = normalizeHash(location.hash);
  currentPath.value = normalizedPath;
  currentHash.value = normalizedHash;
  syncDocumentState(normalizedPath);
  if (normalizedHash.length > 0)
    queueScrollToHashAnchor(normalizedHash);
}
function shouldAllowBrowserDefault(mouseEvent) {
  return mouseEvent.button !== 0 || mouseEvent.ctrlKey || mouseEvent.metaKey || mouseEvent.shiftKey || mouseEvent.altKey;
}
function onRouteClick(mouseEvent) {
  let anchor;
  if (shouldAllowBrowserDefault(mouseEvent))
    return;
  if (!(mouseEvent.currentTarget instanceof HTMLAnchorElement && (anchor = mouseEvent.currentTarget, true)))
    return;
  mouseEvent.preventDefault();
  navigateTo(anchor.pathname, "", true, true);
}
function onTocClick(mouseEvent) {
  let anchor;
  if (shouldAllowBrowserDefault(mouseEvent))
    return;
  if (!(mouseEvent.currentTarget instanceof HTMLAnchorElement && (anchor = mouseEvent.currentTarget, true)))
    return;
  mouseEvent.preventDefault();
  navigateTo(anchor.pathname, anchor.hash, true, true);
}
function onSectionPermalinkClick(mouseEvent) {
  let buttonElement;
  if (!(mouseEvent.currentTarget instanceof HTMLButtonElement && (buttonElement = mouseEvent.currentTarget, true)))
    return;
  mouseEvent.preventDefault();
  let currentPath = getCurrentPathRef();
  if (currentPath === null)
    return;
  let sectionId = normalizeHash(buttonElement.value);
  if (sectionId.length === 0)
    return;
  resetPermalinkFeedback();
  navigateTo(currentPath.value, sectionId, true, true);
  let location = document.location;
  let sectionUrl = buildUrl(currentPath.value, sectionId);
  let sectionShareUrl = sectionUrl;
  if (location !== null)
    sectionShareUrl = location.origin + sectionUrl;
  try {
    let clipboard = window.navigator.clipboard;
    if (clipboard === null) {
      showPermalinkReady(sectionId);
      return;
    }
    Promise.resolve(clipboard.writeText(sectionShareUrl)).then(() => {
      showCopiedSection(sectionId);
      return;
    }, () => {
      showPermalinkReady(sectionId);
      return;
    });
  } catch {
    showPermalinkReady(sectionId);
  }
}
function onNavFilterInput(event) {
  let inputElement;
  if (!(event.currentTarget instanceof HTMLInputElement && (inputElement = event.currentTarget, true)))
    return;
  setNavFilter(inputElement.value);
}
function clearNavFilter(mouseEvent) {
  mouseEvent.preventDefault();
  setNavFilter("");
}
function onHashChange(event) {
  syncLocationStateFromBrowser();
  return 0;
}
function onPopState(event) {
  syncLocationStateFromBrowser();
  return 0;
}
function deploymentBody() {
  return h("div", { class: "doc-body" }, [pageSection("build-output", "Build output", [h("p", "Wiki still emits static ESM modules into `wwwroot/jazor`, but those modules now back a product-facing documentation shell instead of a sample-only landing page."), codeBlock("Key artifacts", "src/Wiki/wwwroot/jazor/main.mjs\nsrc/Wiki/wwwroot/jazor/components/wiki-home.mjs\nsrc/Wiki/wwwroot/jazor/jazor-manifest.json")]), pageSection("route-fallback", "Route fallback", [h("p", "The host maps unknown document paths back to `index.html`. That makes routes like `/guides/getting-started` refresh-safe while keeping the hosting model static and simple."), h("ul", [h("li", "Static assets resolve normally through `UseStaticFiles()`."), h("li", "Unknown docs paths fall through to the frontend entry page."), h("li", "Health remains a real backend endpoint at `/health`.")])]), pageSection("operational-checks", "Operational checks", [h("p", "The minimum release discipline for Wiki is build, route, and entry verification. This is what keeps the site from silently drifting back into sample-only quality."), codeBlock("Recommended verification", ".\\src\\Wiki\\verify-smoke.ps1 -BuildLocal"), callout("Dependency note", "CDN-backed Vue and Vuetify remain acceptable for MVP, but productization should decide whether to lock, mirror, or localize those assets.")])]);
}
function createRouteClickEvents() {
  return { onClick: onRouteClick };
}
function createNavFilterInputEvents() {
  return { onInput: onNavFilterInput };
}
function createClearNavFilterEvents() {
  return { onClick: clearNavFilter };
}
function createTocClickEvents() {
  return { onClick: onTocClick };
}
function createSectionPermalinkEvents() {
  return { onClick: onSectionPermalinkClick };
}
function headerLink(path, label) {
  return h("a", {
    class: "header-link",
    href: path,
    ...createRouteClickEvents()
  }, label);
}
function navGroup(title, links) {
  return h("section", { class: "nav-group" }, [h("p", { class: "nav-group-title" }, title), h("div", { class: "nav-group-links" }, links)]);
}
function navLink(path, title, summary, currentPath) {
  let className = "nav-link";
  if (path === currentPath)
    className = "nav-link nav-link-active";
  return h("a", {
    class: className,
    href: path,
    ...createRouteClickEvents()
  }, [h("span", { class: "nav-link-title" }, title), h("span", { class: "nav-link-summary" }, summary)]);
}
function tocRail(title, links) {
  return h("aside", { class: "toc-rail" }, [h("div", { class: "rail-card toc-card" }, [h("p", { class: "rail-kicker" }, title), h("p", { class: "rail-copy" }, "Stable anchors are part of the docs contract. Treat them as user-facing entry points."), h("div", { class: "toc-links" }, links)])]);
}
function emptyTocRail() {
  return h("aside", { class: "toc-rail toc-rail-empty" }, [h("div", { class: "rail-card toc-card" }, [h("p", { class: "rail-kicker" }, "Missing page"), h("p", { class: "rail-copy" }, "The requested route is not registered in the current Wiki route map.")])]);
}
function tocLink(path, id, title, currentHash) {
  let className = "toc-link";
  if (currentHash === id)
    className = "toc-link toc-link-active";
  return h("a", {
    class: className,
    href: path + "#" + id,
    ...createTocClickEvents()
  }, title);
}
function pagerLink(direction, path, title) {
  return h("a", {
    class: "pager-link",
    href: path,
    ...createRouteClickEvents()
  }, [h("span", { class: "pager-direction" }, direction), h("span", { class: "pager-title" }, title)]);
}
function emptyPagerSlot() {
  return h("div", { class: "pager-slot" }, "");
}
function pageSection(id, title, content) {
  let className = "doc-section";
  if (getCurrentHashRef()?.value === id)
    className = "doc-section doc-section-active";
  let permalinkLabel = "Copy link";
  let permalinkClassName = "section-permalink";
  let permalinkTitle = "Copy direct link to this section";
  if (getCopiedSectionRef()?.value === id) {
    permalinkLabel = "Copied";
    permalinkClassName = "section-permalink section-permalink-copied";
    permalinkTitle = "Direct link copied to clipboard";
  }
  else if (getPermalinkReadySectionRef()?.value === id) {
    permalinkLabel = "Link ready";
    permalinkClassName = "section-permalink section-permalink-ready";
    permalinkTitle = "Direct link is ready in the address bar; clipboard copy was not available";
  }
  return h("section", { id: id, class: className }, [h("div", { class: "section-anchor" }, id), h("div", { class: "section-title-row" }, [h("h2", title), h("button", {
    class: permalinkClassName,
    type: "button",
    value: id,
    title: permalinkTitle,
    ...createSectionPermalinkEvents()
  }, permalinkLabel)]), h("div", { class: "section-body" }, content)]);
}
function metricCard(value, title, summary) {
  return h("article", { class: "metric-card" }, [h("p", { class: "metric-value" }, value), h("h3", { class: "metric-title" }, title), h("p", { class: "metric-summary" }, summary)]);
}
function checkCard(title, summary) {
  return h("article", { class: "check-card" }, [h("h3", { class: "check-title" }, title), h("p", { class: "check-summary" }, summary)]);
}
function callout(title, summary) {
  return h("div", { class: "callout" }, [h("p", { class: "callout-title" }, title), h("p", { class: "callout-summary" }, summary)]);
}
function codeBlock(label, code) {
  return h("div", { class: "code-frame" }, [h("div", { class: "code-label" }, label), h("pre", { class: "code-block" }, code)]);
}
function gettingStartedBody() {
  return h("div", { class: "doc-body" }, [pageSection("boot-the-site", "Boot the site locally", [h("p", "The local loop is intentionally short. Build, emit, and run the static host from the repository root."), codeBlock("Local commands", "dotnet build .\\src\\Wiki\\Wiki.csproj\n.\\src\\Wiki\\serve.ps1 -Build\n.\\src\\Wiki\\verify-smoke.ps1 -BuildLocal"), h("p", "The smoke script is part of the contract now. A real route is not considered valid until build output and route fallback are both verified.")]), pageSection("route-model", "Understand the route model", [h("p", "Wiki now uses real URL paths with server fallback, so routes can be refreshed or opened directly in the browser."), h("ul", [h("li", "`/` for the overview page"), h("li", "`/guides/getting-started` for local workflow"), h("li", "`/guides/content-model` for page authoring rules"), h("li", "`/engineering/h-function-authoring` for the H-function contract"), h("li", "`/operations/deployment` for build and hosting details")])]), pageSection("add-a-page", "Add a page safely", [h("p", "A new page is introduced by adding one route constant, one route-contract entry, one nav entry, one dedicated page file, and one TOC branch. The point is not to invent a mini CMS inside the codebase."), codeBlock("Minimum page shape", "private const string NewPagePath = \"/guides/new-page\";\n\nprivate static IVNode NewPageBody()\n    => H(\"div\", \"...\");"), h("p", "After the route exists, add TOC entries and pager links, then rerun the smoke script.")]), pageSection("verify-the-result", "Verify the result", [h("p", "For Wiki, verification is deliberately operational. Build output, route availability, and shell stability matter more than screenshot-only review."), h("ul", [h("li", "Confirm `main.mjs` and `components/wiki-home.mjs` exist after build."), h("li", "Confirm all registered docs routes return the frontend shell through route fallback."), h("li", "Confirm emitted module text still contains expected route identifiers and page labels.")])])]);
}
function hFunctionAuthoringBody() {
  return h("div", { class: "doc-body" }, [pageSection("layout-composition", "Layout composition", [h("p", "H functions are the production surface here because they keep the rendered structure explicit while staying inside the same typed ecosystem as the rest of the project."), codeBlock("Section composition", "private static IVNode PageSection(string id, string title, IVNode[] content)\n    => H(\"section\", new VueObject { Id = id, Class = \"doc-section\" },\n    [\n        H(\"div\", new VueObject { Class = \"section-anchor\" }, id),\n        H(\"h2\", title),\n        H(\"div\", new VueObject { Class = \"section-body\" }, content)\n    ]);")]), pageSection("production-rules", "Production rules for H authoring", [h("ul", [h("li", "Route and metadata shape come first; visual polish sits on top of a stable shell."), h("li", "Prefer semantic HTML nodes and typed props over stringly-typed DOM manipulation."), h("li", "Keep helper methods focused on one visual concept so the page source stays readable."), h("li", "If a page needs richer interaction later, add it intentionally rather than hiding it inside layout helpers.")])]), pageSection("why-this-works", "Why this works for a real project", [h("p", "The shell is where H functions deliver the most value: route-aware layout, reusable structure, consistent page chrome, and type-checked authoring inside the same codebase as the rest of the product."), callout("Service over purity", "The site optimizes for usability first: H owns the shell because that is the part users and maintainers need to stay consistent.")])]);
}
function overviewBody() {
  return h("div", { class: "doc-body" }, [pageSection("what-ships-now", "What ships now", [h("p", "Wiki now runs as a real docs shell instead of a single-page playground. The primary contract is a stable static host, explicit document routes, and H-function-authored layout."), h("div", { class: "metric-grid" }, [metricCard("5", "Core routes", "Overview, guides, engineering, and operations pages ship as first-class entry points."), metricCard("1", "Static host", "ASP.NET Core serves assets, health checks, and fallback routing with one small host."), metricCard("100%", "Shell in H", "Navigation, hero, article sections, TOC, and pager all live on the H-function authoring surface.")])]), pageSection("why-this-exists", "Why this exists", [h("p", "The old Wiki proved that Jazor could emit Vue modules. The new Wiki proves that the H-function path can carry a production-facing information architecture, not just a demo panel."), h("ul", [h("li", "The site itself is now a product surface, not just a compiler sample."), h("li", "Navigation, page discovery, route entry, and deployment guidance are treated as product contracts."), h("li", "The content model stays explicit so maintainers can evolve it without a hidden pipeline.")])]), pageSection("mvp-boundary", "MVP boundary", [callout("Included now", "Real routes, multi-page docs, left navigation with local page filtering, right-side table of contents, and previous/next page flow."), h("ul", [h("li", "Included: a production-oriented docs shell, code-first pages, and smoke-verifiable routes."), h("li", "Deferred: markdown ingestion, editable content management, comments, and user-specific state."), h("li", "Deferred: external search service and non-CDN asset packaging.")])]), pageSection("site-structure", "Site structure", [h("p", "The site is intentionally small and explicit. The production proof point is not an abstraction layer; it is that a maintainable docs site can live directly on the H-function authoring surface."), codeBlock("Current production surface", "src/Wiki/\n  Program.cs\n  AppModule.cs\n  WikiHomeModule.cs\n  WikiHomeModule.RouteContract.cs\n  WikiHomeModule.Elements.cs\n  WikiHomeModule.Overview.cs\n  WikiHomeModule.GettingStarted.cs\n  WikiHomeModule.ContentModel.cs\n  WikiHomeModule.HFunctionAuthoring.cs\n  WikiHomeModule.Deployment.cs\n  wwwroot/index.html\n  wwwroot/site.css\n  verify-smoke.ps1")])]);
}
const totalPageCount = 5;
function isKnownPage(currentPath) {
  return getPageIndex(currentPath) >= 0;
}
function getPageIndex(currentPath) {
  if (currentPath === "/")
    return 0;
  if (currentPath === "/guides/getting-started")
    return 1;
  if (currentPath === "/guides/content-model")
    return 2;
  if (currentPath === "/engineering/h-function-authoring")
    return 3;
  if (currentPath === "/operations/deployment")
    return 4;
  return -1;
}
function getPagePath(pageIndex) {
  if (pageIndex === 0)
    return "/";
  if (pageIndex === 1)
    return "/guides/getting-started";
  if (pageIndex === 2)
    return "/guides/content-model";
  if (pageIndex === 3)
    return "/engineering/h-function-authoring";
  if (pageIndex === 4)
    return "/operations/deployment";
  return "";
}
function matchesPageFilter(currentPath, filterText) {
  if (filterText.length === 0)
    return true;
  return _d52d7114d5c1b839(currentPath, filterText, 5) || _d52d7114d5c1b839(getPageGroup(currentPath), filterText, 5) || _d52d7114d5c1b839(getPageTitle(currentPath), filterText, 5) || _d52d7114d5c1b839(getPageSummary(currentPath), filterText, 5) || _d52d7114d5c1b839(getPageStatus(currentPath), filterText, 5);
}
function getPageGroup(currentPath) {
  let pageIndex = getPageIndex(currentPath);
  if (pageIndex <= 2)
    return "Foundation";
  if (pageIndex === 3)
    return "Engineering";
  return "Operations";
}
function getPageTitle(currentPath) {
  let pageIndex = getPageIndex(currentPath);
  if (pageIndex === 0)
    return "Overview";
  if (pageIndex === 1)
    return "Getting Started";
  if (pageIndex === 2)
    return "Content Model";
  if (pageIndex === 3)
    return "H-Function Authoring";
  return "Deployment";
}
function getPageSummary(currentPath) {
  let pageIndex = getPageIndex(currentPath);
  if (pageIndex === 0)
    return "A production-oriented docs shell for Jazor, authored entirely with ECMAScript.Vue3 H functions.";
  if (pageIndex === 1)
    return "Run the site locally, understand the route model, and validate the emitted Wiki host end to end.";
  if (pageIndex === 2)
    return "Code-first page metadata, explicit sections, and a navigation contract that stays readable in C#.";
  if (pageIndex === 3)
    return "Why H functions are the production authoring surface for this Wiki, and the conventions that keep it maintainable.";
  return "Build outputs, fallback routing, smoke verification, and the static delivery contract for Wiki.";
}
function getPageStatus(currentPath) {
  let pageIndex = getPageIndex(currentPath);
  if (pageIndex === 0)
    return "Real Project MVP";
  if (pageIndex === 1)
    return "Foundation";
  if (pageIndex === 2)
    return "Authoring";
  if (pageIndex === 3)
    return "Engineering";
  return "Operations";
}
function getPreviousPath(currentPath) {
  let pageIndex = getPageIndex(currentPath);
  if (pageIndex <= 0)
    return "";
  return getPagePath(pageIndex - 1);
}
function getNextPath(currentPath) {
  let pageIndex = getPageIndex(currentPath);
  if (pageIndex < 0 || pageIndex >= 5 - 1)
    return "";
  return getPagePath(pageIndex + 1);
}
function TocRail(currentPath, currentHash) {
  let pageIndex = getPageIndex(currentPath);
  if (pageIndex === 0) {
    return tocRail("On this page", [tocLink("/", "what-ships-now", "What ships now", currentHash), tocLink("/", "why-this-exists", "Why this exists", currentHash), tocLink("/", "mvp-boundary", "MVP boundary", currentHash), tocLink("/", "site-structure", "Site structure", currentHash)]);
  }
  if (pageIndex === 1) {
    return tocRail("On this page", [tocLink("/guides/getting-started", "boot-the-site", "Boot the site", currentHash), tocLink("/guides/getting-started", "route-model", "Route model", currentHash), tocLink("/guides/getting-started", "add-a-page", "Add a page", currentHash), tocLink("/guides/getting-started", "verify-the-result", "Verify the result", currentHash)]);
  }
  if (pageIndex === 2) {
    return tocRail("On this page", [tocLink("/guides/content-model", "page-contract", "Page contract", currentHash), tocLink("/guides/content-model", "navigation-contract", "Navigation contract", currentHash), tocLink("/guides/content-model", "editing-rules", "Editing rules", currentHash)]);
  }
  if (pageIndex === 3) {
    return tocRail("On this page", [tocLink("/engineering/h-function-authoring", "layout-composition", "Layout composition", currentHash), tocLink("/engineering/h-function-authoring", "production-rules", "Production rules", currentHash), tocLink("/engineering/h-function-authoring", "why-this-works", "Why this works", currentHash)]);
  }
  return tocRail("On this page", [tocLink("/operations/deployment", "build-output", "Build output", currentHash), tocLink("/operations/deployment", "route-fallback", "Route fallback", currentHash), tocLink("/operations/deployment", "operational-checks", "Operational checks", currentHash)]);
}
//# sourceMappingURL=wiki-home.mjs.map
