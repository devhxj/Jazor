namespace ECMAScript;

/// <summary>
/// NDEFMessageInit
/// </summary>
[ECMAScript]
[Description("@#NDEFMessageInit")]
public record NDEFMessageInit(
    [property: Description("@#records")]NDEFRecordInit[]? Records = default);

/// <summary>
/// NDEFRecordInit
/// </summary>
[ECMAScript]
[Description("@#NDEFRecordInit")]
public record NDEFRecordInit(
    [property: Description("@#recordType")]string? RecordType = default,
	[property: Description("@#mediaType")]string? MediaType = default,
	[property: Description("@#id")]string? Id = default,
	[property: Description("@#encoding")]string? Encoding = default,
	[property: Description("@#lang")]string? Lang = default,
	[property: Description("@#data")]object? Data = default);

/// <summary>
/// NDEFReadingEventInit
/// </summary>
[ECMAScript]
[Description("@#NDEFReadingEventInit")]
public record NDEFReadingEventInit(
    [property: Description("@#serialNumber")]string? SerialNumber = "",
	[property: Description("@#message")]NDEFMessageInit? Message = default): EventInit;

/// <summary>
/// NDEFWriteOptions
/// </summary>
[ECMAScript]
[Description("@#NDEFWriteOptions")]
public record NDEFWriteOptions(
    [property: Description("@#overwrite")]bool Overwrite = true,
	[property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// NDEFMakeReadOnlyOptions
/// </summary>
[ECMAScript]
[Description("@#NDEFMakeReadOnlyOptions")]
public record NDEFMakeReadOnlyOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// NDEFScanOptions
/// </summary>
[ECMAScript]
[Description("@#NDEFScanOptions")]
public record NDEFScanOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// NDEFMessage
/// </summary>
[ECMAScript]
[Description("@#NDEFMessage")]
public class NDEFMessage
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="messageInit">messageInit</param>
    public extern NDEFMessage(NDEFMessageInit messageInit);

    /// <summary>
    /// records
    /// </summary>
    [Description("@#records")]
    public extern FrozenSet<NDEFRecord> Records { get; }
}

/// <summary>
/// NDEFRecord
/// </summary>
[ECMAScript]
[Description("@#NDEFRecord")]
public class NDEFRecord
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="recordInit">recordInit</param>
    public extern NDEFRecord(NDEFRecordInit recordInit);

    /// <summary>
    /// recordType
    /// </summary>
    [Description("@#recordType")]
    public extern string RecordType { get; }

    /// <summary>
    /// mediaType
    /// </summary>
    [Description("@#mediaType")]
    public extern string? MediaType { get; }

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string? Id { get; }

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern DataView? Data { get; }

    /// <summary>
    /// encoding
    /// </summary>
    [Description("@#encoding")]
    public extern string? Encoding { get; }

    /// <summary>
    /// lang
    /// </summary>
    [Description("@#lang")]
    public extern string? Lang { get; }

    /// <summary>
    /// toRecords
    /// </summary>
    [Description("@#toRecords")]
    public extern NDEFRecord[]? ToRecords();
}

/// <summary>
/// NDEFReader
/// </summary>
[ECMAScript]
[Description("@#NDEFReader")]
public class NDEFReader : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern NDEFReader();

    /// <summary>
    /// onreading
    /// </summary>
    [Description("@#onreading")]
    public extern EventHandler Onreading { get; set; }

    /// <summary>
    /// onreadingerror
    /// </summary>
    [Description("@#onreadingerror")]
    public extern EventHandler Onreadingerror { get; set; }

    /// <summary>
    /// scan
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#scan")]
    public extern PromiseResult<object> Scan(NDEFScanOptions? options = default);

    /// <summary>
    /// write
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="options">options</param>
    [Description("@#write")]
    public extern PromiseResult<object> Write(NDEFMessageSource message, NDEFWriteOptions? options = default);

    /// <summary>
    /// makeReadOnly
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#makeReadOnly")]
    public extern PromiseResult<object> MakeReadOnly(NDEFMakeReadOnlyOptions? options = default);
}

/// <summary>
/// NDEFReadingEvent
/// </summary>
[ECMAScript]
[Description("@#NDEFReadingEvent")]
public class NDEFReadingEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="readingEventInitDict">readingEventInitDict</param>
    public extern NDEFReadingEvent(string type, NDEFReadingEventInit readingEventInitDict);

    /// <summary>
    /// serialNumber
    /// </summary>
    [Description("@#serialNumber")]
    public extern string SerialNumber { get; }

    /// <summary>
    /// message
    /// </summary>
    [Description("@#message")]
    public extern NDEFMessage Message { get; }
}