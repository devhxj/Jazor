namespace Jazor.RazorVue.Descriptor;

public sealed record VueEmitDescriptor(
    string Name,
    string PayloadTypeName,
    string? RazorAlias,
    VueEmitKind Kind);

public enum VueEmitKind
{
    Normal,
    ModelUpdate,
    LifecycleLike,
    LibrarySpecific
}

