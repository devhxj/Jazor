namespace Jazor.Artifacts;

/// <summary>
/// Vue's implementation of Emit's structural runtime-provider contract. It owns the
/// embedded resource names, static dependency closure, and import-map prefix; Emit only
/// consumes the neutral data shape exposed here.
/// </summary>
internal static class RuntimeProviderCatalog
{
    internal const int SchemaVersion = 1;
    internal const string ProviderId = "jazor.vue";

    private static readonly RuntimeModule[] Modules =
    [
        new(
            "Jazor.RazorVue.Runtime.render-context.mjs",
            "Jazor.RazorVue.Runtime.render-context",
            "@jazor/vue-runtime/render-context.mjs",
            ["@jazor/vue-runtime/render-context-core.mjs"]),
        new(
            "Jazor.RazorVue.Runtime.render-context-core.mjs",
            "Jazor.RazorVue.Runtime.render-context-core",
            "@jazor/vue-runtime/render-context-core.mjs",
            [])
    ];

    private static readonly ImportMapContribution[] ImportMapEntries =
    [
        new("@jazor/vue-runtime/", "@jazor/vue-runtime/")
    ];

    internal static System.Collections.IEnumerable GetModules()
        => Modules;

    internal static System.Collections.IEnumerable GetImportMapEntries()
        => ImportMapEntries;

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

    private sealed class ImportMapContribution(string specifier, string artifactPath)
    {
        public string Specifier { get; } = specifier;

        public string ArtifactPath { get; } = artifactPath;
    }
}
