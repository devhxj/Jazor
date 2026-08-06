// Loads library-owned stylesheets declared by VueLibraryComponent without relying on a bundler CSS import transform.

const loadedUrls = new Set();

export function ensureLibraryStyles(styleUrls) {
    if (typeof document === "undefined")
        return;

    for (const candidate of styleUrls) {
        if (typeof candidate !== "string")
            continue;

        const href = candidate.trim();
        if (href.length === 0 || loadedUrls.has(href))
            continue;

        if (hasStylesheet(href)) {
            loadedUrls.add(href);
            continue;
        }

        const link = document.createElement("link");
        link.setAttribute("rel", "stylesheet");
        link.setAttribute("href", href);
        document.head.appendChild(link);
        loadedUrls.add(href);
    }
}

function hasStylesheet(href) {
    const resolvedHref = new URL(href, document.baseURI).href;
    for (const link of document.querySelectorAll('link[rel="stylesheet"]')) {
        if (link.getAttribute("href") === href || link.href === resolvedHref)
            return true;
    }

    return false;
}
