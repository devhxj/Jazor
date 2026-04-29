namespace ECMAScript.Internal.VueContracts.Protocol;

public sealed class HostCapabilityDescriptor(
	string name,
	string? description)
{
	public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

	public string? Description { get; } = description;
}

public sealed class GetHostInfoResponse(
	string hostName,
	string protocolVersion,
	IReadOnlyList<HostCapabilityDescriptor> capabilities)
{
	public string HostName { get; } = hostName ?? throw new ArgumentNullException(nameof(hostName));

	public string ProtocolVersion { get; } = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));

	public IReadOnlyList<HostCapabilityDescriptor> Capabilities { get; } = capabilities ?? throw new ArgumentNullException(nameof(capabilities));
}

public sealed class PingResponse(
	string message,
	string protocolVersion)
{
	public string Message { get; } = message ?? throw new ArgumentNullException(nameof(message));

	public string ProtocolVersion { get; } = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
}
