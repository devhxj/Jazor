using System.Text.Json;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp;

namespace Jazor.VueHost.Frontend.Deno.Protocol;

internal sealed class DenoFrontendRequestEnvelope
{
    public string Id { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;

    public object? Payload { get; set; }
}

internal sealed class DenoFrontendResponseEnvelope
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

    public SemanticContext? FrontendContext { get; init; }

    public IReadOnlyList<ArtifactRecord>? FrontendArtifacts { get; init; }
}

internal class DenoTemplateRequest : DenoTemplateDocumentRequest
{
    public required LspPosition Position { get; init; }
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
