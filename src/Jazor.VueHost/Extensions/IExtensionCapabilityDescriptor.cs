namespace Jazor.VueHost.Extensions;

internal interface IExtensionCapabilityDescriptor
{
    IReadOnlySet<string> ProvidedCapabilities { get; }
}
