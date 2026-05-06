namespace Jazor.RazorVue.Descriptor;

internal sealed record VueEmitDescriptor(
    string Name,
    string PayloadTypeName,
    string? RazorAlias,
    VueEmitKind Kind);
