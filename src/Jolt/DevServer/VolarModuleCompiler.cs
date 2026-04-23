namespace Jolt.DevServer;

internal interface IVolarModuleCompiler
{
    ValueTask<VolarModuleCompilation?> CompileSfcAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken);

    ValueTask<VolarModuleCompilation?> CompileTypeScriptAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken);

    ValueTask<CssModuleCompilation?> CompileCssModuleAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<CssModuleCompilation?>(null);
}

internal sealed class VolarModuleCompilation
{
    public required string JavaScript { get; init; }

    public string? SourceMap { get; init; }

    public string? StyleContent { get; init; }

    public IReadOnlyList<CompiledStyleFragment> StyleFragments { get; init; } = [];

    public IReadOnlyList<string> Dependencies { get; init; } = [];

    public IReadOnlyList<string> EmbeddedStyleDependencies { get; init; } = [];

    public bool SupportsHmr { get; init; }
}

internal sealed class CompiledStyleFragment
{
    public required string Content { get; init; }

    public string? SourcePath { get; init; }

    public int? SourceLineStart { get; init; }

    public int? SourceLineCount { get; init; }
}

internal sealed class CssModuleCompilation
{
    public required string CssContent { get; init; }

    public IReadOnlyDictionary<string, string> Mappings { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

internal sealed class NullVolarModuleCompiler : IVolarModuleCompiler
{
    public ValueTask<VolarModuleCompilation?> CompileSfcAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<VolarModuleCompilation?>(null);

    public ValueTask<VolarModuleCompilation?> CompileTypeScriptAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<VolarModuleCompilation?>(null);
}
