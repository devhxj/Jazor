namespace Jolt.VirtualDocuments.Models;

public sealed record VirtualDocumentIdentity(
    string SourceDocumentPath,
    string ProjectedDocumentPath,
    VirtualDocumentKind DocumentKind);
