using Jazor.Emit;

namespace Jazor.VueHost.DevServer;

internal sealed class CompilationResult
{
    public required string ContentType { get; init; }

    public required string Content { get; init; }

    public string? SourceMap { get; init; }

    public string? ModuleSignature { get; init; }

    public RazorVueManifestEntry? HotReloadManifestEntry { get; init; }

    public string? StyleContent { get; init; }

    public IReadOnlyList<CompiledStyleFragment> StyleFragments { get; init; } = [];

    public IReadOnlyList<string> Dependencies { get; init; } = [];

    public IReadOnlyList<string> EmbeddedStyleDependencies { get; init; } = [];

    public IReadOnlyDictionary<string, string> CssModuleMappings { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    public IReadOnlyList<string> Diagnostics { get; init; } = [];

    public bool IsError { get; init; }

    public string? ErrorMessage { get; init; }

    public bool SupportsHmr { get; init; }
}
