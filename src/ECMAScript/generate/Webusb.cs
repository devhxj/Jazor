namespace ECMAScript;

/// <summary>
/// USBDeviceFilter
/// </summary>
[ECMAScript]
[Description("@#USBDeviceFilter")]
public record USBDeviceFilter(
    [property: Description("@#vendorId")]ushort VendorId = default,
	[property: Description("@#productId")]ushort ProductId = default,
	[property: Description("@#classCode")]byte ClassCode = default,
	[property: Description("@#subclassCode")]byte SubclassCode = default,
	[property: Description("@#protocolCode")]byte ProtocolCode = default,
	[property: Description("@#serialNumber")]string? SerialNumber = default);

/// <summary>
/// USBDeviceRequestOptions
/// </summary>
[ECMAScript]
[Description("@#USBDeviceRequestOptions")]
public record USBDeviceRequestOptions(
    [property: Description("@#filters")]USBDeviceFilter[]? Filters = default,
	[property: Description("@#exclusionFilters")]USBDeviceFilter[]? ExclusionFilters = default);

/// <summary>
/// USBConnectionEventInit
/// </summary>
[ECMAScript]
[Description("@#USBConnectionEventInit")]
public record USBConnectionEventInit(
    [property: Description("@#device")]USBDevice? Device = default): EventInit;

/// <summary>
/// USBControlTransferParameters
/// </summary>
[ECMAScript]
[Description("@#USBControlTransferParameters")]
public record USBControlTransferParameters(
    [property: Description("@#requestType")]USBRequestType? RequestType = default,
	[property: Description("@#recipient")]USBRecipient? Recipient = default,
	[property: Description("@#request")]byte Request = default,
	[property: Description("@#value")]ushort Value = default,
	[property: Description("@#index")]ushort Index = default);

/// <summary>
/// USBBlocklistEntry
/// </summary>
[ECMAScript]
[Description("@#USBBlocklistEntry")]
public record USBBlocklistEntry(
    [property: Description("@#idVendor")]ushort IdVendor = default,
	[property: Description("@#idProduct")]ushort IdProduct = default,
	[property: Description("@#bcdDevice")]ushort BcdDevice = default);

/// <summary>
/// USBPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#USBPermissionDescriptor")]
public record USBPermissionDescriptor(
    [property: Description("@#filters")]USBDeviceFilter[]? Filters = default,
	[property: Description("@#exclusionFilters")]USBDeviceFilter[]? ExclusionFilters = default): PermissionDescriptor;

/// <summary>
/// AllowedUSBDevice
/// </summary>
[ECMAScript]
[Description("@#AllowedUSBDevice")]
public record AllowedUSBDevice(
    [property: Description("@#vendorId")]byte VendorId = default,
	[property: Description("@#productId")]byte ProductId = default,
	[property: Description("@#serialNumber")]string? SerialNumber = default);

/// <summary>
/// USBPermissionStorage
/// </summary>
[ECMAScript]
[Description("@#USBPermissionStorage")]
public record USBPermissionStorage(
    [property: Description("@#allowedDevices")]AllowedUSBDevice[]? AllowedDevices = default);

/// <summary>
/// USB
/// </summary>
[ECMAScript]
[Description("@#USB")]
public class USB : EventTarget
{
    /// <summary>
    /// onconnect
    /// </summary>
    [Description("@#onconnect")]
    public extern EventHandler Onconnect { get; set; }

    /// <summary>
    /// ondisconnect
    /// </summary>
    [Description("@#ondisconnect")]
    public extern EventHandler Ondisconnect { get; set; }

    /// <summary>
    /// getDevices
    /// </summary>
    [Description("@#getDevices")]
    public extern PromiseResult<USBDevice[]> GetDevices();

    /// <summary>
    /// requestDevice
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#requestDevice")]
    public extern PromiseResult<USBDevice> RequestDevice(USBDeviceRequestOptions options);
}

/// <summary>
/// USBConnectionEvent
/// </summary>
[ECMAScript]
[Description("@#USBConnectionEvent")]
public class USBConnectionEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern USBConnectionEvent(string type, USBConnectionEventInit eventInitDict);

    /// <summary>
    /// device
    /// </summary>
    [Description("@#device")]
    public extern USBDevice Device { get; }
}

/// <summary>
/// USBInTransferResult
/// </summary>
[ECMAScript]
[Description("@#USBInTransferResult")]
public class USBInTransferResult
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="status">status</param>
    /// <param name="data">data</param>
    public extern USBInTransferResult(USBTransferStatus status, DataView? data);

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern DataView? Data { get; }

    /// <summary>
    /// status
    /// </summary>
    [Description("@#status")]
    public extern USBTransferStatus Status { get; }
}

/// <summary>
/// USBOutTransferResult
/// </summary>
[ECMAScript]
[Description("@#USBOutTransferResult")]
public class USBOutTransferResult
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="status">status</param>
    /// <param name="bytesWritten">bytesWritten</param>
    public extern USBOutTransferResult(USBTransferStatus status, uint bytesWritten);

    /// <summary>
    /// bytesWritten
    /// </summary>
    [Description("@#bytesWritten")]
    public extern uint BytesWritten { get; }

    /// <summary>
    /// status
    /// </summary>
    [Description("@#status")]
    public extern USBTransferStatus Status { get; }
}

/// <summary>
/// USBIsochronousInTransferPacket
/// </summary>
[ECMAScript]
[Description("@#USBIsochronousInTransferPacket")]
public class USBIsochronousInTransferPacket
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="status">status</param>
    /// <param name="data">data</param>
    public extern USBIsochronousInTransferPacket(USBTransferStatus status, DataView? data);

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern DataView? Data { get; }

    /// <summary>
    /// status
    /// </summary>
    [Description("@#status")]
    public extern USBTransferStatus Status { get; }
}

/// <summary>
/// USBIsochronousInTransferResult
/// </summary>
[ECMAScript]
[Description("@#USBIsochronousInTransferResult")]
public class USBIsochronousInTransferResult
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="packets">packets</param>
    /// <param name="data">data</param>
    public extern USBIsochronousInTransferResult(USBIsochronousInTransferPacket[] packets, DataView? data);

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern DataView? Data { get; }

    /// <summary>
    /// packets
    /// </summary>
    [Description("@#packets")]
    public extern FrozenSet<USBIsochronousInTransferPacket> Packets { get; }
}

/// <summary>
/// USBIsochronousOutTransferPacket
/// </summary>
[ECMAScript]
[Description("@#USBIsochronousOutTransferPacket")]
public class USBIsochronousOutTransferPacket
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="status">status</param>
    /// <param name="bytesWritten">bytesWritten</param>
    public extern USBIsochronousOutTransferPacket(USBTransferStatus status, uint bytesWritten);

    /// <summary>
    /// bytesWritten
    /// </summary>
    [Description("@#bytesWritten")]
    public extern uint BytesWritten { get; }

    /// <summary>
    /// status
    /// </summary>
    [Description("@#status")]
    public extern USBTransferStatus Status { get; }
}

/// <summary>
/// USBIsochronousOutTransferResult
/// </summary>
[ECMAScript]
[Description("@#USBIsochronousOutTransferResult")]
public class USBIsochronousOutTransferResult
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="packets">packets</param>
    public extern USBIsochronousOutTransferResult(USBIsochronousOutTransferPacket[] packets);

    /// <summary>
    /// packets
    /// </summary>
    [Description("@#packets")]
    public extern FrozenSet<USBIsochronousOutTransferPacket> Packets { get; }
}

/// <summary>
/// USBDevice
/// </summary>
[ECMAScript]
[Description("@#USBDevice")]
public class USBDevice
{
    /// <summary>
    /// usbVersionMajor
    /// </summary>
    [Description("@#usbVersionMajor")]
    public extern byte UsbVersionMajor { get; }

    /// <summary>
    /// usbVersionMinor
    /// </summary>
    [Description("@#usbVersionMinor")]
    public extern byte UsbVersionMinor { get; }

    /// <summary>
    /// usbVersionSubminor
    /// </summary>
    [Description("@#usbVersionSubminor")]
    public extern byte UsbVersionSubminor { get; }

    /// <summary>
    /// deviceClass
    /// </summary>
    [Description("@#deviceClass")]
    public extern byte DeviceClass { get; }

    /// <summary>
    /// deviceSubclass
    /// </summary>
    [Description("@#deviceSubclass")]
    public extern byte DeviceSubclass { get; }

    /// <summary>
    /// deviceProtocol
    /// </summary>
    [Description("@#deviceProtocol")]
    public extern byte DeviceProtocol { get; }

    /// <summary>
    /// vendorId
    /// </summary>
    [Description("@#vendorId")]
    public extern ushort VendorId { get; }

    /// <summary>
    /// productId
    /// </summary>
    [Description("@#productId")]
    public extern ushort ProductId { get; }

    /// <summary>
    /// deviceVersionMajor
    /// </summary>
    [Description("@#deviceVersionMajor")]
    public extern byte DeviceVersionMajor { get; }

    /// <summary>
    /// deviceVersionMinor
    /// </summary>
    [Description("@#deviceVersionMinor")]
    public extern byte DeviceVersionMinor { get; }

    /// <summary>
    /// deviceVersionSubminor
    /// </summary>
    [Description("@#deviceVersionSubminor")]
    public extern byte DeviceVersionSubminor { get; }

    /// <summary>
    /// manufacturerName
    /// </summary>
    [Description("@#manufacturerName")]
    public extern string? ManufacturerName { get; }

    /// <summary>
    /// productName
    /// </summary>
    [Description("@#productName")]
    public extern string? ProductName { get; }

    /// <summary>
    /// serialNumber
    /// </summary>
    [Description("@#serialNumber")]
    public extern string? SerialNumber { get; }

    /// <summary>
    /// configuration
    /// </summary>
    [Description("@#configuration")]
    public extern USBConfiguration? Configuration { get; }

    /// <summary>
    /// configurations
    /// </summary>
    [Description("@#configurations")]
    public extern FrozenSet<USBConfiguration> Configurations { get; }

    /// <summary>
    /// opened
    /// </summary>
    [Description("@#opened")]
    public extern bool Opened { get; }

    /// <summary>
    /// open
    /// </summary>
    [Description("@#open")]
    public extern PromiseResult<object> Open();

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern PromiseResult<object> Close();

    /// <summary>
    /// forget
    /// </summary>
    [Description("@#forget")]
    public extern PromiseResult<object> Forget();

    /// <summary>
    /// selectConfiguration
    /// </summary>
    /// <param name="configurationValue">configurationValue</param>
    [Description("@#selectConfiguration")]
    public extern PromiseResult<object> SelectConfiguration(byte configurationValue);

    /// <summary>
    /// claimInterface
    /// </summary>
    /// <param name="interfaceNumber">interfaceNumber</param>
    [Description("@#claimInterface")]
    public extern PromiseResult<object> ClaimInterface(byte interfaceNumber);

    /// <summary>
    /// releaseInterface
    /// </summary>
    /// <param name="interfaceNumber">interfaceNumber</param>
    [Description("@#releaseInterface")]
    public extern PromiseResult<object> ReleaseInterface(byte interfaceNumber);

    /// <summary>
    /// selectAlternateInterface
    /// </summary>
    /// <param name="interfaceNumber">interfaceNumber</param>
    /// <param name="alternateSetting">alternateSetting</param>
    [Description("@#selectAlternateInterface")]
    public extern PromiseResult<object> SelectAlternateInterface(byte interfaceNumber, byte alternateSetting);

    /// <summary>
    /// controlTransferIn
    /// </summary>
    /// <param name="setup">setup</param>
    /// <param name="length">length</param>
    [Description("@#controlTransferIn")]
    public extern PromiseResult<USBInTransferResult> ControlTransferIn(USBControlTransferParameters setup, ushort length);

    /// <summary>
    /// controlTransferOut
    /// </summary>
    /// <param name="setup">setup</param>
    /// <param name="data">data</param>
    [Description("@#controlTransferOut")]
    public extern PromiseResult<USBOutTransferResult> ControlTransferOut(USBControlTransferParameters setup, IBufferSource data);

    /// <summary>
    /// clearHalt
    /// </summary>
    /// <param name="direction">direction</param>
    /// <param name="endpointNumber">endpointNumber</param>
    [Description("@#clearHalt")]
    public extern PromiseResult<object> ClearHalt(USBDirection direction, byte endpointNumber);

    /// <summary>
    /// transferIn
    /// </summary>
    /// <param name="endpointNumber">endpointNumber</param>
    /// <param name="length">length</param>
    [Description("@#transferIn")]
    public extern PromiseResult<USBInTransferResult> TransferIn(byte endpointNumber, uint length);

    /// <summary>
    /// transferOut
    /// </summary>
    /// <param name="endpointNumber">endpointNumber</param>
    /// <param name="data">data</param>
    [Description("@#transferOut")]
    public extern PromiseResult<USBOutTransferResult> TransferOut(byte endpointNumber, IBufferSource data);

    /// <summary>
    /// isochronousTransferIn
    /// </summary>
    /// <param name="endpointNumber">endpointNumber</param>
    /// <param name="packetLengths">packetLengths</param>
    [Description("@#isochronousTransferIn")]
    public extern PromiseResult<USBIsochronousInTransferResult> IsochronousTransferIn(byte endpointNumber, uint[] packetLengths);

    /// <summary>
    /// isochronousTransferOut
    /// </summary>
    /// <param name="endpointNumber">endpointNumber</param>
    /// <param name="data">data</param>
    /// <param name="packetLengths">packetLengths</param>
    [Description("@#isochronousTransferOut")]
    public extern PromiseResult<USBIsochronousOutTransferResult> IsochronousTransferOut(byte endpointNumber, IBufferSource data, uint[] packetLengths);

    /// <summary>
    /// reset
    /// </summary>
    [Description("@#reset")]
    public extern PromiseResult<object> Reset();
}

/// <summary>
/// USBConfiguration
/// </summary>
[ECMAScript]
[Description("@#USBConfiguration")]
public class USBConfiguration
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="device">device</param>
    /// <param name="configurationValue">configurationValue</param>
    public extern USBConfiguration(USBDevice device, byte configurationValue);

    /// <summary>
    /// configurationValue
    /// </summary>
    [Description("@#configurationValue")]
    public extern byte ConfigurationValue { get; }

    /// <summary>
    /// configurationName
    /// </summary>
    [Description("@#configurationName")]
    public extern string? ConfigurationName { get; }

    /// <summary>
    /// interfaces
    /// </summary>
    [Description("@#interfaces")]
    public extern FrozenSet<USBInterface> Interfaces { get; }
}

/// <summary>
/// USBInterface
/// </summary>
[ECMAScript]
[Description("@#USBInterface")]
public class USBInterface
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="configuration">configuration</param>
    /// <param name="interfaceNumber">interfaceNumber</param>
    public extern USBInterface(USBConfiguration configuration, byte interfaceNumber);

    /// <summary>
    /// interfaceNumber
    /// </summary>
    [Description("@#interfaceNumber")]
    public extern byte InterfaceNumber { get; }

    /// <summary>
    /// alternate
    /// </summary>
    [Description("@#alternate")]
    public extern USBAlternateInterface Alternate { get; }

    /// <summary>
    /// alternates
    /// </summary>
    [Description("@#alternates")]
    public extern FrozenSet<USBAlternateInterface> Alternates { get; }

    /// <summary>
    /// claimed
    /// </summary>
    [Description("@#claimed")]
    public extern bool Claimed { get; }
}

/// <summary>
/// USBAlternateInterface
/// </summary>
[ECMAScript]
[Description("@#USBAlternateInterface")]
public class USBAlternateInterface
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="deviceInterface">deviceInterface</param>
    /// <param name="alternateSetting">alternateSetting</param>
    public extern USBAlternateInterface(USBInterface deviceInterface, byte alternateSetting);

    /// <summary>
    /// alternateSetting
    /// </summary>
    [Description("@#alternateSetting")]
    public extern byte AlternateSetting { get; }

    /// <summary>
    /// interfaceClass
    /// </summary>
    [Description("@#interfaceClass")]
    public extern byte InterfaceClass { get; }

    /// <summary>
    /// interfaceSubclass
    /// </summary>
    [Description("@#interfaceSubclass")]
    public extern byte InterfaceSubclass { get; }

    /// <summary>
    /// interfaceProtocol
    /// </summary>
    [Description("@#interfaceProtocol")]
    public extern byte InterfaceProtocol { get; }

    /// <summary>
    /// interfaceName
    /// </summary>
    [Description("@#interfaceName")]
    public extern string? InterfaceName { get; }

    /// <summary>
    /// endpoints
    /// </summary>
    [Description("@#endpoints")]
    public extern FrozenSet<USBEndpoint> Endpoints { get; }
}

/// <summary>
/// USBEndpoint
/// </summary>
[ECMAScript]
[Description("@#USBEndpoint")]
public class USBEndpoint
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="alternate">alternate</param>
    /// <param name="endpointNumber">endpointNumber</param>
    /// <param name="direction">direction</param>
    public extern USBEndpoint(USBAlternateInterface alternate, byte endpointNumber, USBDirection direction);

    /// <summary>
    /// endpointNumber
    /// </summary>
    [Description("@#endpointNumber")]
    public extern byte EndpointNumber { get; }

    /// <summary>
    /// direction
    /// </summary>
    [Description("@#direction")]
    public extern USBDirection Direction { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern USBEndpointType Type { get; }

    /// <summary>
    /// packetSize
    /// </summary>
    [Description("@#packetSize")]
    public extern uint PacketSize { get; }
}

/// <summary>
/// USBPermissionResult
/// </summary>
[ECMAScript]
[Description("@#USBPermissionResult")]
public class USBPermissionResult : PermissionStatus
{
    /// <summary>
    /// devices
    /// </summary>
    [Description("@#devices")]
    public extern FrozenSet<USBDevice> Devices { get; set; }
}