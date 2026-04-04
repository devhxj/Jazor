namespace Jazor.RazorVue.Analysis.Descriptor;

public sealed record VuePropDescriptor(
    string Name,
    string PublicName,
    string TypeName,
    bool Required,
    bool AcceptsBinding,
    string? DefaultExpression,
    VuePropKind Kind);

public enum VuePropKind
{
    Normal,
    Model,
    HtmlLike,
    LibrarySpecific
}

