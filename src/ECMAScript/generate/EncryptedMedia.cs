namespace ECMAScript;

/// <summary>
/// MediaKeySystemConfiguration
/// </summary>
[ECMAScript]
[Description("@#MediaKeySystemConfiguration")]
public record MediaKeySystemConfiguration(
    [property: Description("@#label")]string? Label = default,
	[property: Description("@#initDataTypes")]string[]? InitDataTypes = default,
	[property: Description("@#audioCapabilities")]MediaKeySystemMediaCapability[]? AudioCapabilities = default,
	[property: Description("@#videoCapabilities")]MediaKeySystemMediaCapability[]? VideoCapabilities = default,
	[property: Description("@#distinctiveIdentifier")]MediaKeysRequirement DistinctiveIdentifier = MediaKeysRequirement.Optional,
	[property: Description("@#persistentState")]MediaKeysRequirement PersistentState = MediaKeysRequirement.Optional,
	[property: Description("@#sessionTypes")]string[]? SessionTypes = default);

/// <summary>
/// MediaKeySystemMediaCapability
/// </summary>
[ECMAScript]
[Description("@#MediaKeySystemMediaCapability")]
public record MediaKeySystemMediaCapability(
    [property: Description("@#contentType")]string? ContentType = default,
	[property: Description("@#encryptionScheme")]string? EncryptionScheme = null,
	[property: Description("@#robustness")]string? Robustness = default);

/// <summary>
/// MediaKeysPolicy
/// </summary>
[ECMAScript]
[Description("@#MediaKeysPolicy")]
public record MediaKeysPolicy(
    [property: Description("@#minHdcpVersion")]string? MinHdcpVersion = default);

/// <summary>
/// MediaKeyMessageEventInit
/// </summary>
[ECMAScript]
[Description("@#MediaKeyMessageEventInit")]
public record MediaKeyMessageEventInit(
    [property: Description("@#messageType")]MediaKeyMessageType? MessageType = default,
	[property: Description("@#message")]ArrayBuffer? Message = default): EventInit;

/// <summary>
/// MediaEncryptedEventInit
/// </summary>
[ECMAScript]
[Description("@#MediaEncryptedEventInit")]
public record MediaEncryptedEventInit(
    [property: Description("@#initDataType")]string? InitDataType = default,
	[property: Description("@#initData")]ArrayBuffer? InitData = null): EventInit;

/// <summary>
/// MediaKeySystemAccess
/// </summary>
[ECMAScript]
[Description("@#MediaKeySystemAccess")]
public class MediaKeySystemAccess
{
    /// <summary>
    /// keySystem
    /// </summary>
    [Description("@#keySystem")]
    public extern string KeySystem { get; }

    /// <summary>
    /// getConfiguration
    /// </summary>
    [Description("@#getConfiguration")]
    public extern MediaKeySystemConfiguration GetConfiguration();

    /// <summary>
    /// createMediaKeys
    /// </summary>
    [Description("@#createMediaKeys")]
    public extern PromiseResult<MediaKeys> CreateMediaKeys();
}

/// <summary>
/// MediaKeys
/// </summary>
[ECMAScript]
[Description("@#MediaKeys")]
public class MediaKeys
{
    /// <summary>
    /// createSession
    /// </summary>
    /// <param name="sessionType">sessionType</param>
    [Description("@#createSession")]
    public extern MediaKeySession CreateSession(MediaKeySessionType sessionType = MediaKeySessionType.Temporary);

    /// <summary>
    /// getStatusForPolicy
    /// </summary>
    /// <param name="policy">policy</param>
    [Description("@#getStatusForPolicy")]
    public extern PromiseResult<MediaKeyStatus> GetStatusForPolicy(MediaKeysPolicy? policy = default);

    /// <summary>
    /// setServerCertificate
    /// </summary>
    /// <param name="serverCertificate">serverCertificate</param>
    [Description("@#setServerCertificate")]
    public extern PromiseResult<bool> SetServerCertificate(IBufferSource serverCertificate);
}

/// <summary>
/// MediaKeySession
/// </summary>
[ECMAScript]
[Description("@#MediaKeySession")]
public class MediaKeySession : EventTarget
{
    /// <summary>
    /// sessionId
    /// </summary>
    [Description("@#sessionId")]
    public extern string SessionId { get; }

    /// <summary>
    /// expiration
    /// </summary>
    [Description("@#expiration")]
    public extern double Expiration { get; }

    /// <summary>
    /// closed
    /// </summary>
    [Description("@#closed")]
    public extern PromiseResult<MediaKeySessionClosedReason> Closed { get; }

    /// <summary>
    /// keyStatuses
    /// </summary>
    [Description("@#keyStatuses")]
    public extern MediaKeyStatusMap KeyStatuses { get; }

    /// <summary>
    /// onkeystatuseschange
    /// </summary>
    [Description("@#onkeystatuseschange")]
    public extern EventHandler Onkeystatuseschange { get; set; }

    /// <summary>
    /// onmessage
    /// </summary>
    [Description("@#onmessage")]
    public extern EventHandler Onmessage { get; set; }

    /// <summary>
    /// generateRequest
    /// </summary>
    /// <param name="initDataType">initDataType</param>
    /// <param name="initData">initData</param>
    [Description("@#generateRequest")]
    public extern PromiseResult<object> GenerateRequest(string initDataType, IBufferSource initData);

    /// <summary>
    /// load
    /// </summary>
    /// <param name="sessionId">sessionId</param>
    [Description("@#load")]
    public extern PromiseResult<bool> Load(string sessionId);

    /// <summary>
    /// update
    /// </summary>
    /// <param name="response">response</param>
    [Description("@#update")]
    public extern PromiseResult<object> Update(IBufferSource response);

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern PromiseResult<object> Close();

    /// <summary>
    /// remove
    /// </summary>
    [Description("@#remove")]
    public extern PromiseResult<object> Remove();
}

/// <summary>
/// MediaKeyStatusMap
/// </summary>
[ECMAScript]
[Description("@#MediaKeyStatusMap")]
public class MediaKeyStatusMap : IEnumerable<(IBufferSource, MediaKeyStatus)>
{
    extern IEnumerator<(IBufferSource, MediaKeyStatus)> IEnumerable<(IBufferSource, MediaKeyStatus)>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern uint Size { get; }

    /// <summary>
    /// has
    /// </summary>
    /// <param name="keyId">keyId</param>
    [Description("@#has")]
    public extern bool Has(IBufferSource keyId);

    /// <summary>
    /// get
    /// </summary>
    /// <param name="keyId">keyId</param>
    [Description("@#get")]
    public extern Either<MediaKeyStatus, object> Get(IBufferSource keyId);
}

/// <summary>
/// MediaKeyMessageEvent
/// </summary>
[ECMAScript]
[Description("@#MediaKeyMessageEvent")]
public class MediaKeyMessageEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern MediaKeyMessageEvent(string type, MediaKeyMessageEventInit eventInitDict);

    /// <summary>
    /// messageType
    /// </summary>
    [Description("@#messageType")]
    public extern MediaKeyMessageType MessageType { get; }

    /// <summary>
    /// message
    /// </summary>
    [Description("@#message")]
    public extern ArrayBuffer Message { get; }
}

/// <summary>
/// MediaEncryptedEvent
/// </summary>
[ECMAScript]
[Description("@#MediaEncryptedEvent")]
public class MediaEncryptedEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern MediaEncryptedEvent(string type, MediaEncryptedEventInit eventInitDict);

    /// <summary>
    /// initDataType
    /// </summary>
    [Description("@#initDataType")]
    public extern string InitDataType { get; }

    /// <summary>
    /// initData
    /// </summary>
    [Description("@#initData")]
    public extern ArrayBuffer? InitData { get; }
}