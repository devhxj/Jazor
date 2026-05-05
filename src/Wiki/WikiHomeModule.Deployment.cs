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
                H("p", "The host maps unknown document paths back to `index.html`. That makes routes like `/guides/getting-started` refresh-safe while keeping the hosting model static and simple."),
                H("ul",
                [
                    H("li", "In development, `/jazor/*` resolves from the explicit project-local emit mount before the web root is consulted."),
                    H("li", "In publish output, `/jazor/*` resolves from `wwwroot/jazor` through normal static hosting."),
                    H("li", "Unknown docs paths fall through to the frontend entry page."),
                    H("li", "Health remains a real backend endpoint at `/health`.")
                ])
            ]),
            PageSection("operational-checks", "Operational checks",
            [
                H("p", "The minimum release discipline for Wiki is build, route, and entry verification. This is what keeps the site from silently drifting back into sample-only quality."),
                CodeBlock("Recommended verification", """
.\src\Wiki\verify-smoke.ps1 -BuildLocal
.\src\Wiki\verify-smoke.ps1 -Publish
.\src\Wiki\serve.ps1 -Publish
"""),
                H("ul",
                [
                    H("li", "Local smoke proves the development mount serves `/jazor/*` from the project-local emit directory."),
                    H("li", "Publish smoke proves production serves `/jazor/*` from `wwwroot/jazor` without a shadow root `jazor/` directory overriding it."),
                    H("li", "Published preview starts the actual published host so manual browser checks can use the same directory shape that production will deploy.")
                ]),
                Callout("Dependency note", "Vue 3 is vendored locally at `wwwroot/vendor/`. The site runs fully offline with no CDN dependencies.")
            ])
        ]);
}
