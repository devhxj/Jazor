using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode DeploymentBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("build-output", "Build output",
            [
                H("p", "Wiki still emits static ESM modules into `wwwroot/jazor`, but those modules now back a product-facing documentation shell instead of a sample-only landing page."),
                CodeBlock("Key artifacts", """
src/Wiki/wwwroot/jazor/main.mjs
src/Wiki/wwwroot/jazor/components/wiki-home.mjs
src/Wiki/wwwroot/jazor/jazor-manifest.json
""")
            ]),
            PageSection("route-fallback", "Route fallback",
            [
                H("p", "The host maps unknown document paths back to `index.html`. That makes routes like `/guides/getting-started` refresh-safe while keeping the hosting model static and simple."),
                H("ul",
                [
                    H("li", "Static assets resolve normally through `UseStaticFiles()`."),
                    H("li", "Unknown docs paths fall through to the frontend entry page."),
                    H("li", "Health remains a real backend endpoint at `/health`.")
                ])
            ]),
            PageSection("operational-checks", "Operational checks",
            [
                H("p", "The minimum release discipline for Wiki is build, route, and entry verification. This is what keeps the site from silently drifting back into sample-only quality."),
                CodeBlock("Recommended verification", """
.\src\Wiki\verify-smoke.ps1 -BuildLocal
"""),
                Callout("Dependency note", "CDN-backed Vue and Vuetify remain acceptable for MVP, but productization should decide whether to lock, mirror, or localize those assets.")
            ])
        ]);
}
