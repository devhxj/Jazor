namespace Jazor.VueHost.DevServer;

internal interface IFrontendModuleCompiler
{
    ValueTask<FrontendModuleCompilation?> CompileSfcAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken);

    ValueTask<FrontendModuleCompilation?> CompileTypeScriptAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken);
}

internal sealed class FrontendModuleCompilation
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

internal sealed class NullFrontendModuleCompiler : IFrontendModuleCompiler
{
    public ValueTask<FrontendModuleCompilation?> CompileSfcAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<FrontendModuleCompilation?>(null);

    public ValueTask<FrontendModuleCompilation?> CompileTypeScriptAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<FrontendModuleCompilation?>(null);
}
