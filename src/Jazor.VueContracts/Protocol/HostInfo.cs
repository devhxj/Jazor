namespace Jazor.VueContracts.Protocol;

public sealed class HostCapabilityDescriptor
{
    public HostCapabilityDescriptor(
        string name,
        string? description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
    }

    public string Name { get; }

    public string? Description { get; }
}

public sealed class GetHostInfoResponse
{
    public GetHostInfoResponse(
        string hostName,
        string protocolVersion,
        IReadOnlyList<HostCapabilityDescriptor> capabilities)
    {
        HostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
        ProtocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
        Capabilities = capabilities ?? throw new ArgumentNullException(nameof(capabilities));
    }

    public string HostName { get; }

    public string ProtocolVersion { get; }

    public IReadOnlyList<HostCapabilityDescriptor> Capabilities { get; }
}

public sealed class PingResponse
{
    public PingResponse(
        string message,
        string protocolVersion)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        ProtocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
    }

    public string Message { get; }

    public string ProtocolVersion { get; }
}
