namespace Playground;

internal sealed class PlaygroundExampleRepository
{
    private static readonly PlaygroundExampleDetailResponse[] Examples =
    [
        new(
            "catalog-shell",
            "Catalog shell with API-backed discovery",
            "Architecture",
            "Intermediate",
            "ASP.NET Core + RazorVue",
            "A real examples catalog with server-backed discovery, typed records, and a responsive RazorVue/Vuetify shell.",
            "This slice proves that library-mode RazorVue can front a non-trivial catalog UX without Jolt, while the host keeps API and deployment concerns explicit.",
            ["catalog", "api", "razorvue", "vuetify"],
            ["Server-owned example inventory", "Typed summary/detail split", "Shared page shell and metric cards"],
            ["Serve catalog and detail data from ASP.NET Core minimal APIs.", "Load and filter examples in the client store.", "Render a stable catalog grid and deep-linkable detail page."],
            ["Playground/Program.cs", "Playground/Pages/PlaygroundCatalogPage.razor", "playground-consumer/src/router.js"],
            "2026-05-12T00:00:00Z",
            true,
            18),
        new(
            "pinia-favorites",
            "Pinia favorites and persisted operator preferences",
            "State",
            "Intermediate",
            "Pinia",
            "Tracks saved examples, search text, and category filters through a typed client-side store with browser persistence.",
            "A formal app needs user-local preferences and predictable view restoration. This slice validates Pinia in the library-mode route instead of treating it as a demo-only add-on.",
            ["pinia", "state", "favorites", "persistence"],
            ["Single source of truth for filters", "Local persistence for saved examples", "Derived metrics for catalog UI"],
            ["Initialize Pinia before app mount.", "Persist favorites and filter state after each mutation.", "Expose derived selectors for featured and filtered example lists."],
            ["playground-consumer/src/stores/playground-store.js", "playground-consumer/src/bootstrap-app.js"],
            "2026-05-12T00:00:00Z",
            true,
            14),
        new(
            "router-deeplinks",
            "VueRoute deep links and resilient navigation",
            "Routing",
            "Advanced",
            "Vue Router",
            "Uses explicit route objects, query preservation, and fallback-safe detail navigation for a documentation-style app shell.",
            "The route layer is where library-mode integration usually becomes fragile. This slice keeps routing explicit and production-safe without coupling to Jolt-only infrastructure.",
            ["router", "deeplink", "navigation", "history"],
            ["Catalog and detail routes", "Search query retention", "404-safe fallback and host-side shell delivery"],
            ["Create web history using the deployed base path.", "Define catalog/detail routes in the consumer runtime.", "Keep host fallback handling aligned with client navigation."],
            ["playground-consumer/src/router.js", "Playground/Program.cs", "Playground/wwwroot/index.html"],
            "2026-05-12T00:00:00Z",
            true,
            12),
        new(
            "deno-pipeline",
            "DenoHost consumer pipeline for generated SFCs",
            "Tooling",
            "Advanced",
            "DenoHost",
            "Compiles emitted RazorVue SFC artifacts into browser and SSR-ready modules through a pure Deno bundling pipeline.",
            "This is the critical integration seam that replaces Vite with a repo-native DenoHost path and keeps generated .vue consumption explicit.",
            ["deno", "bundle", "ssr", "consumer"],
            ["Generated browser/SSR module trees", "Linked source maps", "Browser smoke and SSR smoke hooks"],
            ["Read RazorVue manifest from host output.", "Compile generated .vue files into local .mjs modules.", "Bundle browser entry and verify SSR/browser smoke output."],
            ["playground-consumer/scripts/lib/pipeline.ts", "playground-consumer/scripts/build.ts", "playground-consumer/scripts/smoke-ssr.ts"],
            "2026-05-12T00:00:00Z",
            false,
            22)
    ];

    public PlaygroundCatalogResponse GetCatalog()
    {
        var summaries = Examples
            .Select(static example => new PlaygroundExampleSummaryResponse(
                example.Id,
                example.Title,
                example.Category,
                example.Difficulty,
                example.Runtime,
                example.Summary,
                example.Featured,
                example.EstimatedMinutes,
                example.Tags))
            .ToArray();

        var categories = summaries
            .Select(static item => item.Category)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static item => item, StringComparer.Ordinal)
            .Prepend("All")
            .ToArray();

        return new PlaygroundCatalogResponse(summaries, categories);
    }

    public PlaygroundExampleDetailResponse? GetDetail(string id)
        => Examples.FirstOrDefault(example => string.Equals(example.Id, id, StringComparison.Ordinal));
}
