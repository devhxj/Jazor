namespace Jazor.Artifacts;

/// <summary>
/// Publishes RazorVue's optional runtime modules through Emit's structural provider contract.
/// 仅声明 direct render 所需的小型 helper；不会恢复旧 render-context 或 builder bridge。
/// </summary>
internal static class RuntimeProviderCatalog
{
    internal const int SchemaVersion = 1;
    internal const string ProviderId = "jazor.vue";

    private static readonly RuntimeModule[] Modules =
    [
        new(
            "Jazor.RazorVue.Runtime.raw-markup.mjs",
            "Jazor.RazorVue.Runtime.raw-markup",
            "@jazor/vue-runtime/raw-markup.mjs",
            []),
        new(
            "Jazor.RazorVue.Runtime.cascading.mjs",
            "Jazor.RazorVue.Runtime.cascading",
            "@jazor/vue-runtime/cascading.mjs",
            []),
        new(
            "Jazor.RazorVue.Runtime.blazor-routing.mjs",
            "Jazor.RazorVue.Runtime.blazor-routing",
            "@jazor/vue-runtime/blazor-routing.mjs",
            ["@jazor/vue-runtime/routes.mjs"]),
    ];

    private static readonly ImportMapContribution[] ImportMapEntries =
    [
        new("@jazor/vue-runtime/", "@jazor/vue-runtime/")
    ];

    internal static System.Collections.IEnumerable GetModules()
        => Modules;

    internal static System.Collections.IEnumerable GetImportMapEntries()
        => ImportMapEntries;

    /// <summary>Describes one embedded ESM resource and its static provider dependencies.</summary>
    private sealed class RuntimeModule(
        string resourceName,
        string id,
        string relativePath,
        string[] dependencies)
    {
        public string ResourceName { get; } = resourceName;

        public string Id { get; } = id;

        public string RelativePath { get; } = relativePath;

        public string[] Dependencies { get; } = dependencies;
    }

    /// <summary>Contributes one import-map entry when this provider is retained by Emit.</summary>
    private sealed class ImportMapContribution(string specifier, string artifactPath)
    {
        public string Specifier { get; } = specifier;

        public string ArtifactPath { get; } = artifactPath;
    }
}
