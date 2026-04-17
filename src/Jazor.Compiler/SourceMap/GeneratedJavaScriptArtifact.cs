namespace Jazor.Compiler;

public sealed record GeneratedJavaScriptArtifact(
    string Content,
    string? SourceMapContent,
    string JsHash,
    string? MapHash);
