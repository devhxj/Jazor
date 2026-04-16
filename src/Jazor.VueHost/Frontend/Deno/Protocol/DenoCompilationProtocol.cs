namespace Jazor.VueHost.Frontend.Deno.Protocol;

internal sealed class DenoSfcCompileRequest
{
    public required string DocumentPath { get; init; }

    public required string SfcText { get; init; }

    public required string Filename { get; init; }
}

internal sealed class DenoSfcCompileResult
{
    public required string JsContent { get; init; }

    public string? JsSourceMap { get; init; }

    public string? CssContent { get; init; }

    public IReadOnlyList<DenoSfcStyleFragmentResult> StyleFragments { get; init; } = [];

    public IReadOnlyList<string> Diagnostics { get; init; } = [];

    public bool SupportsHmr { get; init; }
}

internal sealed class DenoSfcStyleFragmentResult
{
    public required string CssContent { get; init; }

    public string? SourcePath { get; init; }

    public int? SourceLineStart { get; init; }

    public int? SourceLineCount { get; init; }
}

internal sealed class DenoTypeScriptCompileRequest
{
    public required string DocumentPath { get; init; }

    public required string Text { get; init; }

    public required string Filename { get; init; }
}

internal sealed class DenoTypeScriptCompileResult
{
    public required string JsContent { get; init; }

    public string? JsSourceMap { get; init; }

    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}
