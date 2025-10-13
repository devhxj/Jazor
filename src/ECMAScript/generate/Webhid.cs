namespace ECMAScript;

/// <summary>
/// HIDDeviceRequestOptions
/// </summary>
[ECMAScript]
[Description("@#HIDDeviceRequestOptions")]
public record HIDDeviceRequestOptions(
    [property: Description("@#filters")]HIDDeviceFilter[]? Filters = default,
	[property: Description("@#exclusionFilters")]HIDDeviceFilter[]? ExclusionFilters = default);

/// <summary>
/// HIDDeviceFilter
/// </summary>
[ECMAScript]
[Description("@#HIDDeviceFilter")]
public record HIDDeviceFilter(
    [property: Description("@#vendorId")]uint VendorId = default,
	[property: Description("@#productId")]ushort ProductId = default,
	[property: Description("@#usagePage")]ushort UsagePage = default,
	[property: Description("@#usage")]ushort Usage = default);

/// <summary>
/// HIDConnectionEventInit
/// </summary>
[ECMAScript]
[Description("@#HIDConnectionEventInit")]
public record HIDConnectionEventInit(
    [property: Description("@#device")]HIDDevice? Device = default): EventInit;

/// <summary>
/// HIDInputReportEventInit
/// </summary>
[ECMAScript]
[Description("@#HIDInputReportEventInit")]
public record HIDInputReportEventInit(
    [property: Description("@#device")]HIDDevice? Device = default,
	[property: Description("@#reportId")]byte ReportId = default,
	[property: Description("@#data")]DataView? Data = default): EventInit;

/// <summary>
/// HIDCollectionInfo
/// </summary>
[ECMAScript]
[Description("@#HIDCollectionInfo")]
public record HIDCollectionInfo(
    [property: Description("@#usagePage")]ushort UsagePage = default,
	[property: Description("@#usage")]ushort Usage = default,
	[property: Description("@#type")]byte Type = default,
	[property: Description("@#children")]HIDCollectionInfo[]? Children = default,
	[property: Description("@#inputReports")]HIDReportInfo[]? InputReports = default,
	[property: Description("@#outputReports")]HIDReportInfo[]? OutputReports = default,
	[property: Description("@#featureReports")]HIDReportInfo[]? FeatureReports = default);

/// <summary>
/// HIDReportInfo
/// </summary>
[ECMAScript]
[Description("@#HIDReportInfo")]
public record HIDReportInfo(
    [property: Description("@#reportId")]byte ReportId = default,
	[property: Description("@#items")]HIDReportItem[]? Items = default);

/// <summary>
/// HIDReportItem
/// </summary>
[ECMAScript]
[Description("@#HIDReportItem")]
public record HIDReportItem(
    [property: Description("@#isAbsolute")]bool IsAbsolute = default,
	[property: Description("@#isArray")]bool IsArray = default,
	[property: Description("@#isBufferedBytes")]bool IsBufferedBytes = default,
	[property: Description("@#isConstant")]bool IsConstant = default,
	[property: Description("@#isLinear")]bool IsLinear = default,
	[property: Description("@#isRange")]bool IsRange = default,
	[property: Description("@#isVolatile")]bool IsVolatile = default,
	[property: Description("@#hasNull")]bool HasNull = default,
	[property: Description("@#hasPreferredState")]bool HasPreferredState = default,
	[property: Description("@#wrap")]bool Wrap = default,
	[property: Description("@#usages")]uint[]? Usages = default,
	[property: Description("@#usageMinimum")]uint UsageMinimum = default,
	[property: Description("@#usageMaximum")]uint UsageMaximum = default,
	[property: Description("@#reportSize")]ushort ReportSize = default,
	[property: Description("@#reportCount")]ushort ReportCount = default,
	[property: Description("@#unitExponent")]sbyte UnitExponent = default,
	[property: Description("@#unitSystem")]HIDUnitSystem? UnitSystem = default,
	[property: Description("@#unitFactorLengthExponent")]sbyte UnitFactorLengthExponent = default,
	[property: Description("@#unitFactorMassExponent")]sbyte UnitFactorMassExponent = default,
	[property: Description("@#unitFactorTimeExponent")]sbyte UnitFactorTimeExponent = default,
	[property: Description("@#unitFactorTemperatureExponent")]sbyte UnitFactorTemperatureExponent = default,
	[property: Description("@#unitFactorCurrentExponent")]sbyte UnitFactorCurrentExponent = default,
	[property: Description("@#unitFactorLuminousIntensityExponent")]sbyte UnitFactorLuminousIntensityExponent = default,
	[property: Description("@#logicalMinimum")]int LogicalMinimum = default,
	[property: Description("@#logicalMaximum")]int LogicalMaximum = default,
	[property: Description("@#physicalMinimum")]int PhysicalMinimum = default,
	[property: Description("@#physicalMaximum")]int PhysicalMaximum = default,
	[property: Description("@#strings")]string[]? Strings = default);

/// <summary>
/// HID
/// </summary>
[ECMAScript]
[Description("@#HID")]
public class HID : EventTarget
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
    public extern PromiseResult<HIDDevice[]> GetDevices();

    /// <summary>
    /// requestDevice
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#requestDevice")]
    public extern PromiseResult<HIDDevice[]> RequestDevice(HIDDeviceRequestOptions options);
}

/// <summary>
/// HIDDevice
/// </summary>
[ECMAScript]
[Description("@#HIDDevice")]
public class HIDDevice : EventTarget
{
    /// <summary>
    /// oninputreport
    /// </summary>
    [Description("@#oninputreport")]
    public extern EventHandler Oninputreport { get; set; }

    /// <summary>
    /// opened
    /// </summary>
    [Description("@#opened")]
    public extern bool Opened { get; }

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
    /// productName
    /// </summary>
    [Description("@#productName")]
    public extern string ProductName { get; }

    /// <summary>
    /// collections
    /// </summary>
    [Description("@#collections")]
    public extern FrozenSet<HIDCollectionInfo> Collections { get; }

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
    /// sendReport
    /// </summary>
    /// <param name="reportId">reportId</param>
    /// <param name="data">data</param>
    [Description("@#sendReport")]
    public extern PromiseResult<object> SendReport(byte reportId, IBufferSource data);

    /// <summary>
    /// sendFeatureReport
    /// </summary>
    /// <param name="reportId">reportId</param>
    /// <param name="data">data</param>
    [Description("@#sendFeatureReport")]
    public extern PromiseResult<object> SendFeatureReport(byte reportId, IBufferSource data);

    /// <summary>
    /// receiveFeatureReport
    /// </summary>
    /// <param name="reportId">reportId</param>
    [Description("@#receiveFeatureReport")]
    public extern PromiseResult<DataView> ReceiveFeatureReport(byte reportId);
}

/// <summary>
/// HIDConnectionEvent
/// </summary>
[ECMAScript]
[Description("@#HIDConnectionEvent")]
public class HIDConnectionEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern HIDConnectionEvent(string type, HIDConnectionEventInit eventInitDict);

    /// <summary>
    /// device
    /// </summary>
    [Description("@#device")]
    public extern HIDDevice Device { get; }
}

/// <summary>
/// HIDInputReportEvent
/// </summary>
[ECMAScript]
[Description("@#HIDInputReportEvent")]
public class HIDInputReportEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern HIDInputReportEvent(string type, HIDInputReportEventInit eventInitDict);

    /// <summary>
    /// device
    /// </summary>
    [Description("@#device")]
    public extern HIDDevice Device { get; }

    /// <summary>
    /// reportId
    /// </summary>
    [Description("@#reportId")]
    public extern byte ReportId { get; }

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern DataView Data { get; }
}