import { onUnmounted, provide, reactive } from "vue";
import { routes } from "@jazor/vue-runtime/routes.mjs";
import { CreateNavigationManager, HandleHistoryNavigation } from "Microsoft/AspNetCore/Components/NavigationManagerModule.js";

const navigationServiceKey = "jazor:service:Microsoft.AspNetCore.Components.NavigationManager";
function readLocation() {
    const location = globalThis.location;
    if (!location) {
        return { pathname: "/", search: "", hash: "", href: "/", baseUri: "/" };
    }

    const fallbackHref = `${location.origin || ""}${location.pathname || "/"}${location.search || ""}${location.hash || ""}` || "/";
    let parsed;
    try {
        parsed = new URL(location.href || fallbackHref, fallbackHref);
    } catch {
        parsed = null;
    }

    const pathname = parsed?.pathname || location.pathname || "/";
    const search = parsed?.search || location.search || "";
    const hash = parsed?.hash || location.hash || "";
    const href = parsed?.href || location.href || `${pathname}${search}${hash}`;
    return {
        pathname: normalizePath(pathname),
        search,
        hash,
        href,
        baseUri: getBaseUri(href),
    };
}

function getBaseUri(currentHref) {
    const baseElement = globalThis.document?.querySelector?.("base[href]");
    const declaredBase = baseElement?.getAttribute?.("href") || "/";
    try {
        const base = new URL(declaredBase, currentHref || "/");
        let pathname = base.pathname || "/";
        if (!pathname.endsWith("/")) pathname += "/";
        base.pathname = pathname;
        base.search = "";
        base.hash = "";
        return base.href;
    } catch {
        return "/";
    }
}

function normalizePath(path) {
    if (!path) return "/";
    const normalized = path.startsWith("/") ? path : `/${path}`;
    return normalized.length > 1 && normalized.endsWith("/") ? normalized.slice(0, -1) : normalized;
}

function getBaseRelativeRoutePath(location) {
    const pathname = normalizePath(location?.pathname || "/");
    try {
        const base = new URL(location?.baseUri || "/", location?.href || "/");
        const basePath = normalizePath(base.pathname || "/");
        if (basePath === "/") return pathname;
        if (pathname === basePath) return "/";
        const prefix = basePath.endsWith("/") ? basePath : `${basePath}/`;
        return pathname.startsWith(prefix)
            ? `/${pathname.slice(prefix.length)}`
            : pathname;
    } catch {
        return pathname;
    }
}

function decode(value) {
    try {
        return decodeURIComponent(value);
    } catch {
        return value;
    }
}

function parseTemplate(template, path) {
    const templateParts = normalizePath(template).split("/").filter(Boolean);
    const pathParts = normalizePath(path).split("/").filter(Boolean);
    const values = {};
    let pathIndex = 0;

    for (let templateIndex = 0; templateIndex < templateParts.length; templateIndex++) {
        const segment = templateParts[templateIndex];
        const parameter = /^\{(\*)?([^}:?]+)(?::[^}?]+)?(\?)?\}$/.exec(segment);
        if (!parameter) {
            if ((pathParts[pathIndex] || "").toLowerCase() !== segment.toLowerCase()) return null;
            pathIndex++;
            continue;
        }

        const [, catchAll, name, optional] = parameter;
        if (catchAll) {
            values[name] = pathParts.slice(pathIndex).map(decode).join("/");
            pathIndex = pathParts.length;
            continue;
        }

        if (pathIndex >= pathParts.length) {
            if (!optional) return null;
            values[name] = undefined;
            continue;
        }

        values[name] = decode(pathParts[pathIndex]);
        pathIndex++;
    }

    return pathIndex === pathParts.length ? values : null;
}

function routeScore(route) {
    return route.template.split("/").reduce((score, segment) =>
        score + (segment.startsWith("{") ? 1 : 10), 0);
}

function findRoute(path) {
    let winner = null;
    for (const route of routes) {
        const values = parseTemplate(route.template, path);
        if (values === null) continue;
        if (!winner || routeScore(route) > routeScore(winner.route)) winner = { route, values };
    }
    return winner;
}

function toValue(value, kind) {
    if (value === undefined || value === null || value === "") return value;
    if (kind === "number") return Number(value);
    if (kind === "boolean") return value === true || String(value).toLowerCase() === "true";
    return value;
}

function createRouteData(match, search) {
    const parameters = {};
    const query = new URLSearchParams(search || "");
    for (const parameter of match.route.parameters) {
        parameters[parameter.prop] = toValue(match.values[parameter.name], parameter.kind);
    }
    for (const parameter of match.route.queries) {
        parameters[parameter.prop] = query.has(parameter.name)
            ? toValue(query.get(parameter.name), parameter.kind)
            : undefined;
    }
    return {
        component: match.route.component,
        layout: match.route.layout,
        parameters,
        template: match.route.template,
    };
}

/**
 * Installs the browser NavigationManager host for a Vue setup scope.
 *
 * Route matching returns data only; rendering remains an application-owned component contract,
 * so this framing helper does not recreate Microsoft's Router/RouteView/LayoutView/NavLink UI.
 * 在 Vue setup 生命周期内调用，host 只提供导航服务和 route metadata，不导出标准组件替代品。
 */
export function createNavigationHost(onChange) {
    const state = reactive({ version: 0 });
    let navigation;
    const readHistoryState = () => {
        const value = globalThis.history?.state;
        return typeof value === "string" ? value : null;
    };
    let acceptedLocation = { ...readLocation(), historyState: readHistoryState() };
    const refresh = () => {
        state.version++;
        // Internal NavigateTo commits the accepted browser position before the
        // refresh callback runs; keep restoration anchored to that position.
        acceptedLocation = { ...readLocation(), historyState: readHistoryState() };
        if (typeof onChange === "function") onChange(navigation);
    };
    navigation = CreateNavigationManager(refresh);
    provide(navigationServiceKey, navigation);

    let historyDispatch = 0;
    let lastHistoryEvent = null;
    const routeFromHref = href => {
        try {
            const parsed = new URL(href, acceptedLocation.href || globalThis.location?.href || "/");
            return `${parsed.pathname}${parsed.search}${parsed.hash}`;
        } catch {
            return href;
        }
    };
    const restoreAcceptedLocation = () => {
        const history = globalThis.history;
        if (!history?.replaceState) return;
        history.replaceState(
            acceptedLocation.historyState,
            "",
            routeFromHref(acceptedLocation.href));
    };
    const onHistoryChange = () => {
        const target = readLocation();
        const historyState = readHistoryState();
        // Browsers may emit both hashchange and popstate for one transition.
        // Coalesce the duplicate URL/state notification before dispatching handlers.
        const eventKey = `${target.href}\u0000${historyState ?? ""}`;
        if (eventKey === lastHistoryEvent) return;
        lastHistoryEvent = eventKey;
        const dispatch = ++historyDispatch;
        const complete = result => {
            if (dispatch !== historyDispatch || result === "stale") return;
            if (result === "prevented") {
                restoreAcceptedLocation();
                acceptedLocation = readLocation();
                return;
            } else {
                acceptedLocation = { ...target, historyState: readHistoryState() };
            }
            navigation.notifyLocationChanged(false);
        };
        HandleHistoryNavigation(
            navigation,
            target.href,
            historyState,
            complete);
    };
    globalThis.addEventListener?.("popstate", onHistoryChange);
    globalThis.addEventListener?.("hashchange", onHistoryChange);

    let disposed = false;
    const dispose = () => {
        if (disposed) return;
        disposed = true;
        historyDispatch++;
        globalThis.removeEventListener?.("popstate", onHistoryChange);
        globalThis.removeEventListener?.("hashchange", onHistoryChange);
    };
    onUnmounted?.(dispose);

    const resolveRoute = () => {
        // Read the reactive version so setup consumers re-run after browser history changes.
        state.version;
        const location = readLocation();
        const match = findRoute(getBaseRelativeRoutePath(location));
        return match ? createRouteData(match, location.search) : null;
    };

    return { navigation, resolveRoute, dispose };
}
