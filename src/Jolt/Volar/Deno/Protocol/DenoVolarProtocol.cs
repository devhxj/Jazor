using System.Text.Json;
using Jolt.Lsp;
using Jazor.Common.VueContracts.Protocol;

namespace Jolt.Volar.Deno.Protocol;

internal sealed class DenoVolarRequestEnvelope
{
    public string Id { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;

    public object? Payload { get; set; }
}

internal sealed class DenoVolarResponseEnvelope
{
    public string Id { get; set; } = string.Empty;

    public bool Success { get; set; }

    public JsonElement? Result { get; set; }

    public string? Error { get; set; }
}

internal class DenoTemplateDocumentRequest
{
    public required string DocumentPath { get; init; }

    public required string Text { get; init; }

    public SemanticContext? VolarContext { get; init; }

    public IReadOnlyList<ArtifactRecord>? VolarArtifacts { get; init; }
}

internal class DenoTemplateRequest : DenoTemplateDocumentRequest
{
    public required LspPosition Position { get; init; }
}

internal class DenoTemplateRangeRequest : DenoTemplateDocumentRequest
{
    public required LspRange Range { get; init; }
}

internal sealed class DenoTemplateDiagnosticRequest : DenoTemplateDocumentRequest
{
}

internal sealed class DenoTemplateSemanticTokensRequest : DenoTemplateDocumentRequest
{
}

internal sealed class DenoTemplateReferenceRequest : DenoTemplateRequest
{
    public bool IncludeDeclaration { get; init; }
}

internal sealed class DenoTemplateRenameRequest : DenoTemplateRequest
{
    public required string NewName { get; init; }
}
