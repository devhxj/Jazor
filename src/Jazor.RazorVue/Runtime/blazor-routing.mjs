import { defineComponent, h, inject, onUnmounted, provide, reactive } from "vue";
import { routes } from "@jazor/vue-runtime/routes.mjs";
import { CreateNavigationManager } from "Microsoft/AspNetCore/Components/NavigationManagerModule.js";

const navigationServiceKey = "jazor:service:Microsoft.AspNetCore.Components.NavigationManager";
const noNavigation = {
    get version() { return 0; },
    navigateTo() {},
    toBaseRelativePath(uri) { return toBaseRelativePath(uri); },
};

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

function renderRoute(routeData, defaultLayout) {
    if (!routeData?.component) return null;
    const page = h(routeData.component, routeData.parameters);
    const layout = routeData.layout || defaultLayout;
    return layout
        ? h(layout, null, {
            ChildContent: () => page,
            default: () => page,
        })
        : page;
}

function toBaseRelativePath(uri) {
    const location = readLocation();
    try {
        const resolved = new URL(uri, location.baseUri);
        const base = new URL(location.baseUri);
        if (resolved.origin !== base.origin)
            throw new Error("Navigation URI is outside the current base origin.");

        let pathname = resolved.pathname;
        const basePath = base.pathname.endsWith("/") ? base.pathname : `${base.pathname}/`;
        if (pathname.startsWith(basePath))
            pathname = pathname.slice(basePath.length);
        else
            pathname = pathname.replace(/^\/+/, "");
        return `${pathname}${resolved.search}${resolved.hash}`;
    } catch {
        return uri;
    }
}

function getSlot(slots, name) {
    return slots[name] || slots[name.charAt(0).toLowerCase() + name.slice(1)];
}

/** Standard Blazor Router authoring surface backed by the generated route catalog. */
export const Router = defineComponent({
    name: "JazorBlazorRouter",
    setup(props, context) {
        const state = reactive({ version: 0 });
        const refresh = () => { state.version++; };
        const navigation = CreateNavigationManager(refresh);
        provide(navigationServiceKey, navigation);
        const onPopState = () => navigation.notifyLocationChanged(false);
        globalThis.addEventListener?.("popstate", onPopState);
        globalThis.addEventListener?.("hashchange", onPopState);
        onUnmounted?.(() => {
            globalThis.removeEventListener?.("popstate", onPopState);
            globalThis.removeEventListener?.("hashchange", onPopState);
        });

        return () => {
            state.version;
            const location = readLocation();
            // Route templates are application-relative. Strip the configured base path before
            // matching so a deployment under `/admin/` behaves exactly like the same app at `/`.
            const match = findRoute(getBaseRelativeRoutePath(location));
            if (!match) {
                const notFound = getSlot(context.slots, "NotFound");
                return notFound ? notFound() : null;
            }

            const routeData = createRouteData(match, location.search);
            const found = getSlot(context.slots, "Found");
            return found ? found(routeData) : renderRoute(routeData, null);
        };
    },
});

/** Renders a standard RouteData supplied by Router's Found template. */
export const RouteView = defineComponent({
    name: "JazorBlazorRouteView",
    props: { RouteData: { default: null }, __jazorDefaultLayout: { default: null } },
    setup(props) {
        return () => renderRoute(props.RouteData, props.__jazorDefaultLayout);
    },
});

/** Applies a standard Blazor layout type to child content. */
export const LayoutView = defineComponent({
    name: "JazorBlazorLayoutView",
    props: { __jazorLayout: { default: null } },
    setup(props, context) {
        return () => {
            const content = getSlot(context.slots, "ChildContent") || context.slots.default;
            if (!props.__jazorLayout) return content ? content() : null;
            return h(props.__jazorLayout, null, {
                ChildContent: content,
                default: content,
            });
        };
    },
});

/** Standard NavLink that observes Router navigation without exposing Vue Router to Razor. */
export const NavLink = defineComponent({
    name: "JazorBlazorNavLink",
    props: {
        Href: { default: "" },
        ActiveClass: { default: "active" },
        Match: { default: 0 },
        AdditionalAttributes: { default: null },
    },
    setup(props, context) {
        const navigation = inject(navigationServiceKey, noNavigation);
        return () => {
            navigation.version;
            const href = props.Href || "";
            const current = getBaseRelativeRoutePath(readLocation());
            const target = normalizePath(toBaseRelativePath(href).split("?")[0]);
            const exact = props.Match === 1 || props.Match === "All";
            const active = exact ? current === target : current === target || current.startsWith(`${target}/`);
            const attributes = { ...(props.AdditionalAttributes || {}) };
            const currentClass = attributes.class || attributes.Class || "";
            attributes.href = href;
            attributes.class = active
                ? [currentClass, props.ActiveClass || "active"].filter(Boolean).join(" ")
                : currentClass;
            attributes.onClick = event => {
                if (event?.defaultPrevented || event?.metaKey || event?.ctrlKey || event?.shiftKey || event?.altKey) return;
                event?.preventDefault?.();
                navigation.navigateTo(href);
            };
            const content = getSlot(context.slots, "ChildContent") || context.slots.default;
            return h("a", attributes, content ? content() : null);
        };
    },
});
