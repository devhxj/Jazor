using System.Collections.Immutable;

namespace Jazor.RazorVue.Descriptor;

public sealed record VueSlotDescriptor(
    string Name,
    string PublicName,
    bool IsDefault,
    ImmutableArray<VueSlotParameterDescriptor> Parameters,
    bool Required);

public sealed record VueSlotParameterDescriptor(
    string Name,
    string TypeName);

