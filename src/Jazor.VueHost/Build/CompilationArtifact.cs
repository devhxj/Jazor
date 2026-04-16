namespace Jazor.VueHost.Build;

internal sealed class CompilationArtifact
{
    public required string SourcePath { get; init; }

    public required string JavaScript { get; init; }

    public string? Css { get; init; }

    public string? SourceMap { get; init; }

    public IReadOnlyList<string> Dependencies { get; init; } = [];

    public IReadOnlyList<string> Exports { get; init; } = [];
}
