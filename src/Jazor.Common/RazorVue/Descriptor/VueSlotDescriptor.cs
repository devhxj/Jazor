using System.Collections.Immutable;

namespace Jazor.RazorVue.Descriptor;

internal sealed record VueSlotDescriptor(
    string Name,
    string PublicName,
    bool IsDefault,
    ImmutableArray<VueSlotParameterDescriptor> Parameters,
    bool Required);

internal sealed record VueSlotParameterDescriptor(
    string Name,
    string TypeName);
