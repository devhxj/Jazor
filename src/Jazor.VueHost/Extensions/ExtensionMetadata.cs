namespace Jazor.VueHost.Extensions;

internal sealed record ExtensionMetadata(
    string Id,
    string Name,
    string Version,
    string? Description = null,
    string? Author = null,
    IReadOnlyList<string>? Dependencies = null);
