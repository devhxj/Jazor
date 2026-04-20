namespace Jolt.Extensions;

internal interface IExtensionCapabilityDescriptor
{
    IReadOnlySet<string> ProvidedCapabilities { get; }
}
