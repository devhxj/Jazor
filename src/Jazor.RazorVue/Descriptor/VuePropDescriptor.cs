namespace Jazor.RazorVue.Descriptor;

internal sealed record VuePropDescriptor(
    string Name,
    string PublicName,
    string TypeName,
    bool Required,
    bool AcceptsBinding,
    string? DefaultExpression,
    VuePropDefaultSource DefaultSource,
    VuePropKind Kind,
    bool CaptureUnmatchedValues);

internal enum VuePropDefaultSource
{
    None,
    AuthoringOverride,
    PropertyInitializer
}
