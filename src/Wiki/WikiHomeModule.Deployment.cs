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
                H("p", "Wiki now emits static ESM modules into the project-local `jazor/` directory first, then materializes that same output into `wwwroot/jazor` at publish time for production hosting."),
                CodeBlock("Key artifacts", """
src/Wiki/jazor/main.mjs
src/Wiki/jazor/components/wiki-home.mjs
src/Wiki/jazor/jazor-manifest.json
""")
            ]),
            PageSection("route-fallback", "Route fallback",
            [
                H("p", "The host serves the same frontend shell for real docs routes and unknown document paths, but it does not collapse them into one HTTP status. Known routes stay `200`, while unknown docs paths return a recoverable `404` shell with route-aware metadata."),
                H("ul",
                [
                    H("li", "In development, `/jazor/*` resolves from the explicit project-local emit mount before the web root is consulted."),
                    H("li", "In publish output, `/jazor/*` resolves from `wwwroot/jazor` through normal static hosting."),
                    H("li", "Unknown docs paths still fall through to the frontend entry page so the shell can suggest recovery routes."),
                    H("li", "The first HTML response now carries route-correct `<title>`, description, canonical URL, Open Graph tags, and Twitter tags before client-side hydration."),
                    H("li", "Utility routes such as `/search` are intentionally emitted as `noindex, nofollow`, while `sitemap.xml` lists canonical content pages only."),
                    H("li", "Health remains a real backend endpoint at `/health`.")
                ])
            ]),
            PageSection("operational-checks", "Operational checks",
            [
                H("p", "The minimum release discipline for Wiki is build, route, and entry verification. This is what keeps the site from silently drifting back into sample-only quality."),
                CodeBlock("Recommended verification", """
.\src\Wiki\verify-smoke.ps1 -BuildLocal
.\src\Wiki\verify-browser.ps1 -BuildLocal
.\src\Wiki\verify-smoke.ps1 -Publish
.\src\Wiki\verify-browser.ps1 -Publish
.\src\Wiki\serve.ps1 -Publish
"""),
                H("ul",
                [
                    H("li", "Local smoke proves the development mount serves `/jazor/*` from the project-local emit directory."),
                    H("li", "Publish smoke proves production serves `/jazor/*` from `wwwroot/jazor` without a shadow root `jazor/` directory overriding it, and that first-response metadata, robots directives, sitemap contents, and security headers stay correct."),
                    H("li", "Browser verification proves the mounted shell still matches those first-response contracts after SPA navigation and stateful interaction."),
                    H("li", "Published preview starts the actual published host so manual browser checks can use the same directory shape that production will deploy.")
                ]),
                Callout("Dependency note", "Vue 3 is vendored locally at `wwwroot/vendor/`. The site runs fully offline with no CDN dependencies.")
            ])
        ]);
}
