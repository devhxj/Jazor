namespace ECMAScript;

/// <summary>
/// SerialPortRequestOptions
/// </summary>
[ECMAScript]
[Description("@#SerialPortRequestOptions")]
public record SerialPortRequestOptions(
    [property: Description("@#filters")]SerialPortFilter[]? Filters = default,
	[property: Description("@#allowedBluetoothServiceClassIds")]BluetoothServiceUUID[]? AllowedBluetoothServiceClassIds = default);

/// <summary>
/// SerialPortFilter
/// </summary>
[ECMAScript]
[Description("@#SerialPortFilter")]
public record SerialPortFilter(
    [property: Description("@#usbVendorId")]ushort UsbVendorId = default,
	[property: Description("@#usbProductId")]ushort UsbProductId = default,
	[property: Description("@#bluetoothServiceClassId")]BluetoothServiceUUID? BluetoothServiceClassId = default);

/// <summary>
/// SerialPortInfo
/// </summary>
[ECMAScript]
[Description("@#SerialPortInfo")]
public record SerialPortInfo(
    [property: Description("@#usbVendorId")]ushort UsbVendorId = default,
	[property: Description("@#usbProductId")]ushort UsbProductId = default,
	[property: Description("@#bluetoothServiceClassId")]BluetoothServiceUUID? BluetoothServiceClassId = default);

/// <summary>
/// SerialOptions
/// </summary>
[ECMAScript]
[Description("@#SerialOptions")]
public record SerialOptions(
    [property: Description("@#baudRate")]uint BaudRate = default,
	[property: Description("@#dataBits")]byte DataBits = 8,
	[property: Description("@#stopBits")]byte StopBits = 1,
	[property: Description("@#parity")]ParityType Parity = ParityType.None,
	[property: Description("@#bufferSize")]uint BufferSize = 255,
	[property: Description("@#flowControl")]FlowControlType FlowControl = FlowControlType.None);

/// <summary>
/// SerialOutputSignals
/// </summary>
[ECMAScript]
[Description("@#SerialOutputSignals")]
public record SerialOutputSignals(
    [property: Description("@#dataTerminalReady")]bool DataTerminalReady = default,
	[property: Description("@#requestToSend")]bool RequestToSend = default,
	[property: Description("@#break")]bool Break = default);

/// <summary>
/// SerialInputSignals
/// </summary>
[ECMAScript]
[Description("@#SerialInputSignals")]
public record SerialInputSignals(
    [property: Description("@#dataCarrierDetect")]bool DataCarrierDetect = default,
	[property: Description("@#clearToSend")]bool ClearToSend = default,
	[property: Description("@#ringIndicator")]bool RingIndicator = default,
	[property: Description("@#dataSetReady")]bool DataSetReady = default);

/// <summary>
/// Serial
/// </summary>
[ECMAScript]
[Description("@#Serial")]
public class Serial : EventTarget
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
    /// getPorts
    /// </summary>
    [Description("@#getPorts")]
    public extern PromiseResult<SerialPort[]> GetPorts();

    /// <summary>
    /// requestPort
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#requestPort")]
    public extern PromiseResult<SerialPort> RequestPort(SerialPortRequestOptions? options = default);
}

/// <summary>
/// SerialPort
/// </summary>
[ECMAScript]
[Description("@#SerialPort")]
public class SerialPort : EventTarget
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
    /// connected
    /// </summary>
    [Description("@#connected")]
    public extern bool Connected { get; }

    /// <summary>
    /// readable
    /// </summary>
    [Description("@#readable")]
    public extern ReadableStream Readable { get; }

    /// <summary>
    /// writable
    /// </summary>
    [Description("@#writable")]
    public extern WritableStream Writable { get; }

    /// <summary>
    /// getInfo
    /// </summary>
    [Description("@#getInfo")]
    public extern SerialPortInfo GetInfo();

    /// <summary>
    /// open
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#open")]
    public extern PromiseResult<object> Open(SerialOptions options);

    /// <summary>
    /// setSignals
    /// </summary>
    /// <param name="signals">signals</param>
    [Description("@#setSignals")]
    public extern PromiseResult<object> SetSignals(SerialOutputSignals? signals = default);

    /// <summary>
    /// getSignals
    /// </summary>
    [Description("@#getSignals")]
    public extern PromiseResult<SerialInputSignals> GetSignals();

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
}